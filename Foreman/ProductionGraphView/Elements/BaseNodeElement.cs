using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Foreman {
    public abstract class BaseNodeElement : GraphElement {
        // selection - note that this doesn't mean it is or isn't in selection
        // (at least not during drag operation - ex: dragging a not-selection over a group of selected nodes will change their highlight status,
        // but won't add them to the 'selected' set until you let go of the drag)
        public bool Highlighted = false;

        public ReadOnlyBaseNode DisplayedNode { get; private set; }

        public override int X {
            get => DisplayedNode.Location.X;
            set => Trace.Fail("Base node element location cant be set through X parameter! Use SetLocation(Point)");
        }

        public override int Y {
            get => DisplayedNode.Location.Y;
            set => Trace.Fail("Base node element location cant be set through Y parameter! Use SetLocation(Point)");
        }

        public override Point Location {
            get => DisplayedNode.Location;
            set => Trace.Fail("Base node element location cant be set through Location parameter! Use SetLocation(Point)");
        }

        public void SetLocation(Point location) {
            if (location == Location)
                return;

            GraphViewer.Graph.RequestNodeController(DisplayedNode).SetLocation(location);

            RequestStateUpdate();
            foreach (var linkedNode in DisplayedNode.InputLinks.Select(l => GraphViewer.LinkElementDictionary[l].SupplierElement))
                linkedNode.RequestStateUpdate();
            foreach (var linkedNode in DisplayedNode.OutputLinks.Select(l => GraphViewer.LinkElementDictionary[l].ConsumerElement))
                linkedNode.RequestStateUpdate();
        }

        protected abstract Brush CleanBgBrush { get; }
        private static readonly Brush ErrorBgBrush = Brushes.Coral;
        private static readonly Brush ManualRateBgFilterBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0));

        private static readonly Brush EqualFlowBorderBrush = Brushes.DarkGreen;
        private static readonly Brush OverproducingFlowBorderBrush = Brushes.DarkGoldenrod;
        private static readonly Brush UndersuppliedFlowBorderBrush = Brushes.DarkRed;

        protected static readonly Brush SelectionOverlayBrush = new SolidBrush(Color.FromArgb(100, 100, 100, 200));

        protected static readonly Brush TextBrush = Brushes.Black;
        protected static readonly Font BaseFont = new(FontFamily.GenericSansSerif, 10f);
        protected static readonly Font CounterBaseFont = new(FontFamily.GenericSansSerif, 14f);
        protected static readonly Font TitleFont = new(FontFamily.GenericSansSerif, 9.2f, FontStyle.Bold);

        protected static StringFormat TitleFormat = new() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center };
        protected static StringFormat TextFormat = new() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center };

        // most values are attempted to fit the grid (6 * 2^n) - ex: 72 = 6 * (4 + 8)

        //  96 fits grid
        protected const int BaseSimpleHeight = 96;
        // 144 fits grid
        protected const int BaseRecipeHeight = 144;
        // makes each tab be evenly spaced for grid
        protected const int TabPadding = 7;
        // (6 * 4) -> width will be divisible by this
        protected const int WidthD = 24;
        protected const int PassthroughNodeWidth = WidthD * 3;
        protected const int SpoilNodeWidth = WidthD * 6;
        protected const int MinWidth = WidthD * 6;

        // the drawn node will be smaller by this in all directions
        // (graph looks nicer if adjacent nodes have a slight gap between them)
        protected const int BorderSpacing = 1;

        protected List<ItemTabElement> InputTabs;
        protected List<ItemTabElement> OutputTabs;

        // location where the mouse click down first happened - in graph coordinates
        // (used to ensure that any drag operation begins at the start, and not at the point (+- a few pixels)
        // where the drag was officially registered as a drag and not just a mouse click.
        private Point _mouseDownLocation;

        // location of this node the moment the mouse click down first happened - in graph coordinates
        private Point _mouseDownNodeLocation;
        private bool _dragStarted;

        // these are set by the events called from the node (as well as calling for invalidation).
        // Any paint call checks for these, and if true resets them to false and calls the appropriate update functions
        private bool _nodeStateRequiresUpdate;

        // this removes the need to manually update the nodes after any change,
        // as well as not spamming update calls after every change
        // (being based on paint refresh - aka: when it actually matters)
        private bool _nodeValuesRequireUpdate;

        protected ErrorNoticeElement ErrorNotice;

        public BaseNodeElement(ProductionGraphViewer graphViewer, ReadOnlyBaseNode node) : base(graphViewer) {
            DisplayedNode = node;
            _dragStarted = false;
            DisplayedNode.NodeStateChanged += DisplayedNode_NodeStateChanged;
            DisplayedNode.NodeValuesChanged += DisplayedNode_NodeValuesChanged;

            InputTabs = [];
            OutputTabs = [];

            ErrorNotice = new ErrorNoticeElement(graphViewer, this);
            ErrorNotice.Location = new Point(-Width / 2, -Height / 2);
            ErrorNotice.SetVisibility(false);

            // first stage item tab creation - absolutely necessary in the constructor due to the creation
            // and simultaneous linking of nodes being possible (drag to new node for example).

            foreach (var item in DisplayedNode.Inputs)
                InputTabs.Add(new ItemTabElement(item, LinkType.Input, GraphViewer, this));
            foreach (var item in DisplayedNode.Outputs)
                OutputTabs.Add(new ItemTabElement(item, LinkType.Output, GraphViewer, this));
        }

        private void DisplayedNode_NodeStateChanged(object sender, EventArgs e) {
            _nodeStateRequiresUpdate = true;
            GraphViewer.Invalidate();
        }

        private void DisplayedNode_NodeValuesChanged(object sender, EventArgs e) {
            _nodeValuesRequireUpdate = true;
            GraphViewer.Invalidate();
        }

        public void RequestStateUpdate() {
            _nodeStateRequiresUpdate = true;
        }

        protected virtual void UpdateState() {
            // update error notice
            ErrorNotice.SetVisibility(DisplayedNode.State is NodeState.Error or NodeState.Warning);
            ErrorNotice.X = -Width / 2;
            ErrorNotice.Y = -Height / 2;

            UpdateTabOrder();
        }

        protected void UpdateValues() {
            // update tab values

            // for inputs, we only care to display the supply rate (guaranteed by solver to be equal to the amount consumed by recipe)
            foreach (var tab in InputTabs) {
                tab.UpdateValues(DisplayedNode.GetConsumeRate(tab.Item), 0, false);
            }

            // for outputs, we want the amount produced by the node, the amount supplied to other nodes,
            // and true if we are supplying less than producing.
            foreach (var tab in OutputTabs) {
                tab.UpdateValues(
                    DisplayedNode.GetSupplyRate(tab.Item),
                    DisplayedNode.GetSupplyUsedRate(tab.Item),
                    DisplayedNode.IsOverproducing(tab.Item)
                );
            }
        }

        private void UpdateTabOrder() {
            InputTabs = InputTabs.OrderBy(GetItemTabXHeuristic)
                .ThenBy(it => it.Item.Item.Name)
                .ThenBy(it => it.Item.Quality.Level)
                .ThenBy(it => it.Item.Quality.Name).ToList(); // then by ensures same result no matter who came first
            OutputTabs = OutputTabs.OrderBy(GetItemTabXHeuristic)
                .ThenBy(it => it.Item.Item.Name)
                .ThenBy(it => it.Item.Quality.Level)
                .ThenBy(it => it.Item.Quality.Name).ToList();

            var x = -GetIconWidths(OutputTabs) / 2;
            var y = DisplayedNode.NodeDirection == NodeDirection.Up ? -Height / 2 + 1 : Height / 2 - 1;
            foreach (var tab in OutputTabs) {
                x += TabPadding;
                tab.Location = new Point(x + tab.Width / 2, y);
                x += tab.Width;
            }

            x = -GetIconWidths(InputTabs) / 2;
            y = DisplayedNode.NodeDirection == NodeDirection.Up ? Height / 2 - 1 : -Height / 2 + 1;
            foreach (var tab in InputTabs) {
                x += TabPadding;
                tab.Location = new Point(x + tab.Width / 2, y);
                x += tab.Width;
            }
        }

        protected int GetIconWidths(List<ItemTabElement> tabs) {
            return TabPadding + tabs.Sum(tab => tab.Bounds.Width + TabPadding);
        }

        private int GetItemTabXHeuristic(ItemTabElement tab) {
            var total = 0;
            foreach (var link in tab.Links) {
                // x needs to be flipped depending on which endpoint we are calculating for.
                // y is absolute to take care of down connections.
                // slight addition in case of up connection ensures that 2 equal connections will prioritize the up over the down.

                var diff = Point.Subtract(link.Supplier.Location, (Size) link.Consumer.Location);
                total += Convert.ToInt32(Math.Atan2(tab.LinkType == LinkType.Input ? diff.X : -diff.X, diff.Y)
                    * 1000 + (diff.Y > 0 ? 1 : 0));
            }

            return total;
        }

        public ItemTabElement GetOutputLineItemTab(ItemQualityPair item) {
            if (_nodeStateRequiresUpdate)
                UpdateState();
            _nodeStateRequiresUpdate = false;

            return OutputTabs.First(it => it.Item == item);
        }

        public ItemTabElement GetInputLineItemTab(ItemQualityPair item) {
            if (_nodeStateRequiresUpdate)
                UpdateState();
            _nodeStateRequiresUpdate = false;

            return InputTabs.First(it => it.Item == item);
        }

        public override void UpdateVisibility(Rectangle graphZone, int xBorder = 0, int yBorder = 0) {
            // account for the vertical item boxes
            base.UpdateVisibility(graphZone, xBorder, yBorder + 30);
        }

        public override bool ContainsPoint(Point graphPoint) {
            if (!Visible)
                return false;
            if (base.ContainsPoint(graphPoint))
                return true;

            return SubElements.OfType<ItemTabElement>().Any(tab => tab.ContainsPoint(graphPoint))
                || ErrorNotice.ContainsPoint(graphPoint);
        }

        public override void PrePaint() {
            if (_nodeStateRequiresUpdate)
                UpdateState();
            if (_nodeStateRequiresUpdate || _nodeValuesRequireUpdate)
                UpdateValues();
            _nodeStateRequiresUpdate = false;
            _nodeValuesRequireUpdate = false;
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            // all draw operations happen in graph 0,0 origin coordinates. So we need to transform all our draw operations to the local 0,0 (center of object)
            var trans = LocalToGraph(new Point(0, 0));

            if (style == NodeDrawingStyle.IconsOnly) {
                var iconSize = GraphViewer.IconsDrawSize;
                if (NodeIcon() != null) graphics.DrawImage(NodeIcon(), trans.X - iconSize / 2, trans.Y - iconSize / 2, iconSize, iconSize);
            } else {
                // background

                var bgBrush = DisplayedNode.State == NodeState.Error ? ErrorBgBrush : CleanBgBrush;
                var borderBrush = DisplayedNode.ManualRateNotMet() && this is not SupplierNodeElement ? UndersuppliedFlowBorderBrush :
                    DisplayedNode.IsOverproducing() ? OverproducingFlowBorderBrush : EqualFlowBorderBrush;

                // flow status border
                GraphicsStuff.FillRoundRect(trans.X - Width / 2 + BorderSpacing, trans.Y - Height / 2 + BorderSpacing, Width - 2 * BorderSpacing,
                    Height - 2 * BorderSpacing, 10, graphics, borderBrush);

                var yOffset = DisplayedNode.KeyNode && this is not ConsumerNodeElement ? 15 : 0;
                var heightOffset = DisplayedNode.KeyNode ? this is ConsumerNodeElement || this is SupplierNodeElement ? 15 : 30 : 0;

                // basic background (with given background brush)

                GraphicsStuff.FillRoundRect(trans.X - Width / 2 + BorderSpacing + 3, trans.Y - Height / 2 + BorderSpacing + 3 + yOffset,
                    Width - 2 * BorderSpacing - 6, Height - 2 * BorderSpacing - 6 - heightOffset, 7, graphics, bgBrush);

                // darken background if it's a manual rate set

                if (DisplayedNode.RateType == RateType.Manual) {
                    GraphicsStuff.FillRoundRect(trans.X - Width / 2 + 3, trans.Y - Height / 2 + 3, Width - 6, Height - 6, 7, graphics,
                        ManualRateBgFilterBrush);
                }

                // supply flag

                if (GraphViewer.FlagOuSuppliedNodes && borderBrush != EqualFlowBorderBrush) {
                    GraphicsStuff.FillRoundRectTlFlag(trans.X - Width / 2 + 3, trans.Y - Height / 2 + 3, Width / 2 - 6, Height / 2 - 6, 7, graphics,
                        borderBrush);
                }

                // warning flag

                if (DisplayedNode.State == NodeState.Warning) {
                    GraphicsStuff.FillRoundRectTlFlag(trans.X - Width / 2 + 3, trans.Y - Height / 2 + 3, Width / 2 - 6, Height / 2 - 6, 7, graphics,
                        ErrorBgBrush);
                }

                // draw in all the inside details for this node

                if (style is NodeDrawingStyle.Regular or NodeDrawingStyle.PrintStyle)
                    DetailsDraw(graphics, trans);

                // highlight

                if (Highlighted)
                    GraphicsStuff.FillRoundRect(trans.X - Width / 2, trans.Y - Height / 2, Width, Height, 8, graphics, SelectionOverlayBrush);
            }
        }

        // draw the inside of the node.
        protected abstract void DetailsDraw(Graphics graphics, Point trans);
        protected abstract Bitmap NodeIcon();

        public override List<TooltipInfo> GetToolTips(Point graphPoint) {
            var element = SubElements.FirstOrDefault(it => it.ContainsPoint(graphPoint));
            var subTooltips = element?.GetToolTips(graphPoint);
            var myTooltips = GetMyToolTips(graphPoint, subTooltips == null || subTooltips.Count == 0) ?? [];

            if (subTooltips != null)
                myTooltips.AddRange(subTooltips);

            return myTooltips;
        }

        // exclusive = true means no other tooltips are shown
        protected abstract List<TooltipInfo> GetMyToolTips(Point graphPoint, bool exclusive);

        public override void MouseDown(Point graphPoint, MouseButtons button) {
            _mouseDownLocation = graphPoint;
            _mouseDownNodeLocation = new Point(X, Y);

            if (button == MouseButtons.Left)
                GraphViewer.MouseDownElement = this;
        }

        public override void MouseUp(Point graphPoint, MouseButtons button, bool wasDragged) {
            _dragStarted = false;
            GraphElement subelement = SubElements.OfType<ItemTabElement>().FirstOrDefault(it => it.ContainsPoint(graphPoint));
            if (wasDragged)
                return;

            if (subelement != null)
                subelement.MouseUp(graphPoint, button, false);
            else if (ErrorNotice.ContainsPoint(graphPoint))
                ErrorNotice.MouseUp(graphPoint, button, false);
            else
                MouseUpAction(graphPoint, button);
        }

        protected void MouseUpAction(Point graphPoint, MouseButtons button) {
            if (button == MouseButtons.Left) {
                GraphViewer.EditNode(this);
            } else if (button == MouseButtons.Right) {
                RightClickMenu.Items.Add(new ToolStripMenuItem("Delete node", null,
                    (o, e) => {
                        RightClickMenu.Close();
                        GraphViewer.Graph.DeleteNode(DisplayedNode);
                        GraphViewer.Graph.UpdateNodeValues();
                    }));
                if (GraphViewer.SelectedNodes.Count > 1 && GraphViewer.SelectedNodes.Contains(this)) {
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Delete selected nodes", null,
                        (o, e) => {
                            RightClickMenu.Close();
                            GraphViewer.TryDeleteSelectedNodes();
                        }));
                }

                RightClickMenu.Items.Add(new ToolStripSeparator());

                RightClickMenu.Items.Add(new ToolStripMenuItem("Flip node", null,
                    (o, e) => {
                        RightClickMenu.Close();
                        GraphViewer.Graph.RequestNodeController(DisplayedNode)
                            .SetDirection(DisplayedNode.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up);
                    }));
                if (GraphViewer.SelectedNodes.Count > 1 && GraphViewer.SelectedNodes.Contains(this)) {
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Flip selected nodes", null,
                        (o, e) => {
                            RightClickMenu.Close();
                            GraphViewer.FlipSelectedNodes();
                        }));
                }

                if (GraphViewer.SelectedNodes.Count > 0) {
                    RightClickMenu.Items.Add(new ToolStripSeparator());
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Clear selection", null,
                        (o, e) => {
                            RightClickMenu.Close();
                            GraphViewer.ClearSelection();
                        }));
                }

                var openInputs =
                    new HashSet<ItemQualityPair>(GraphViewer.SelectedNodes.SelectMany(n => n.InputTabs.Where(t => !t.Links.Any()).Select(t => t.Item)));
                var openOutputs =
                    new HashSet<ItemQualityPair>(GraphViewer.SelectedNodes.SelectMany(n => n.OutputTabs.Where(t => !t.Links.Any()).Select(t => t.Item)));
                var availableInputs =
                    new HashSet<ItemQualityPair>(GraphViewer.SelectedNodes.SelectMany(n => n.InputTabs.Select(t => t.Item)));
                var availableOutputs =
                    new HashSet<ItemQualityPair>(GraphViewer.SelectedNodes.SelectMany(n => n.OutputTabs.Select(t => t.Item)));
                var matchedIo = openInputs.Intersect(availableOutputs).Any();
                var matchedOi = openOutputs.Intersect(availableInputs).Any();
                if (matchedIo || matchedOi) {
                    RightClickMenu.Items.Add(new ToolStripSeparator());

                    if (matchedIo) {
                        RightClickMenu.Items.Add(new ToolStripMenuItem("Auto-connect disconnected inputs", null,
                            (o, e) => {
                                RightClickMenu.Close();

                                var openInputNodes = new Dictionary<ReadOnlyBaseNode, List<ItemQualityPair>>();
                                foreach (var node in GraphViewer.SelectedNodes.Where(n => n.InputTabs.Any(t => !t.Links.Any())))
                                    openInputNodes.Add(node.DisplayedNode, node.InputTabs.Where(t => !t.Links.Any()).Select(t => t.Item).ToList());

                                var availableOutputNodes =
                                    new Dictionary<ItemQualityPair, List<ReadOnlyBaseNode>>();
                                foreach (var node in GraphViewer.SelectedNodes.Select(n => n.DisplayedNode)
                                    .Where(n => !openInputNodes.ContainsKey(n))) {
                                    foreach (var output in node.Outputs) {
                                        if (!availableOutputNodes.ContainsKey(output))
                                            availableOutputNodes.Add(output, []);
                                        availableOutputNodes[output].Add(node);
                                    }
                                }

                                foreach (var node in openInputNodes.Keys) {
                                    foreach (var requiredInput in openInputNodes[node]) {
                                        if (!availableOutputNodes.TryGetValue(requiredInput, out var outputNode))
                                            continue;

                                        var linkNode = outputNode.OrderBy(n =>
                                            Math.Abs(node.Location.X - n.Location.X) + Math.Abs(node.Location.Y - n.Location.Y)).FirstOrDefault();
                                        if (linkNode != null)
                                            GraphViewer.Graph.CreateLink(linkNode, node, requiredInput);
                                    }
                                }

                                GraphViewer.Graph.UpdateNodeValues();
                            }));
                    }

                    if (matchedOi) {
                        RightClickMenu.Items.Add(new ToolStripMenuItem("Auto-connect disconnected outputs", null,
                            (o, e) => {
                                RightClickMenu.Close();

                                var openOutputNodes = new Dictionary<ReadOnlyBaseNode, List<ItemQualityPair>>();
                                foreach (var node in GraphViewer.SelectedNodes.Where(n => n.OutputTabs.Any(t => !t.Links.Any())))
                                    openOutputNodes.Add(node.DisplayedNode, node.OutputTabs.Where(t => !t.Links.Any()).Select(t => t.Item).ToList());

                                var availableInputNodes =
                                    new Dictionary<ItemQualityPair, List<ReadOnlyBaseNode>>();
                                foreach (var node in GraphViewer.SelectedNodes.Select(n => n.DisplayedNode)
                                    .Where(n => !openOutputNodes.ContainsKey(n))) {
                                    foreach (var input in node.Inputs) {
                                        if (!availableInputNodes.ContainsKey(input))
                                            availableInputNodes.Add(input, []);
                                        availableInputNodes[input].Add(node);
                                    }
                                }

                                foreach (var node in openOutputNodes.Keys) {
                                    foreach (var requiredOutput in openOutputNodes[node]) {
                                        if (!availableInputNodes.TryGetValue(requiredOutput, out var inputNode))
                                            continue;

                                        var linkNode = inputNode.OrderBy(n =>
                                            Math.Abs(node.Location.X - n.Location.X) + Math.Abs(node.Location.Y - n.Location.Y)).FirstOrDefault();
                                        if (linkNode != null)
                                            GraphViewer.Graph.CreateLink(node, linkNode, requiredOutput);
                                    }
                                }

                                GraphViewer.Graph.UpdateNodeValues();
                            }));
                    }
                }

                AddRClickMenuOptions(GraphViewer.SelectedNodes.Count == 0 || GraphViewer.SelectedNodes.Contains(this));

                RightClickMenu.Items.Add(new ToolStripSeparator());
                RightClickMenu.Items.Add(new ToolStripMenuItem("Copy key node status", null,
                    (o, e) => {
                        RightClickMenu.Close();
                        var stringBuilder = new StringBuilder();
                        var writer = new JsonTextWriter(new StringWriter(stringBuilder));

                        var serializer = JsonSerializer.Create();
                        serializer.Formatting = Formatting.None;
                        serializer.Serialize(writer, new Tuple<bool, string>(DisplayedNode.KeyNode, DisplayedNode.KeyNodeTitle));

                        Clipboard.SetText(stringBuilder.ToString());
                    }));

                if (GraphViewer.SelectedNodes.Count == 0 || GraphViewer.SelectedNodes.Contains(this)) {
                    try {
                        var keyNodeStatus = JObject.Parse(Clipboard.GetText());
                        if (keyNodeStatus["Item1"] != null && keyNodeStatus["Item2"] != null) {
                            var keyNode = (bool) keyNodeStatus["Item1"];
                            var keyNodeTitle = (string) keyNodeStatus["Item2"];
                            RightClickMenu.Items.Add(new ToolStripMenuItem("Paste key node status", null,
                                (o, e) => {
                                    RightClickMenu.Close();
                                    if (GraphViewer.SelectedNodes.Count == 0) {
                                        var controller = GraphViewer.Graph.RequestNodeController(DisplayedNode);
                                        controller.SetKeyNode(keyNode);
                                        controller.SetKeyNodeTitle(keyNodeTitle);
                                    } else if (GraphViewer.SelectedNodes.Contains(this)) {
                                        foreach (var node in GraphViewer.SelectedNodes) {
                                            var controller = GraphViewer.Graph.RequestNodeController(node.DisplayedNode);
                                            controller.SetKeyNode(keyNode);
                                            controller.SetKeyNodeTitle(keyNodeTitle);
                                        }
                                    }
                                }));
                        }
                    } catch {
                        // ignored
                    }
                }


                RightClickMenu.Show(GraphViewer, GraphViewer.GraphToScreen(graphPoint));
            }
        }

        protected virtual void AddRClickMenuOptions(bool nodeInSelection) { }

        public override void Dragged(Point graphPoint) {
            if (!_dragStarted) {
                ItemTabElement draggedTab = null;
                foreach (var tab in SubElements.OfType<ItemTabElement>())
                    if (tab.ContainsPoint(_mouseDownLocation))
                        draggedTab = tab;
                if (draggedTab != null)
                    GraphViewer.StartLinkDrag(this, draggedTab.LinkType, draggedTab.Item);
                else {
                    _dragStarted = true;
                }
            } else { // drag started -> proceed with dragging the node around
                var offset = (Size) Point.Subtract(graphPoint, (Size) _mouseDownLocation);
                var newLocation = GraphViewer.Grid.AlignToGrid(Point.Add(_mouseDownNodeLocation, offset));
                if (GraphViewer.Grid.LockDragToAxis) {
                    var lockedDragOffset = Point.Subtract(graphPoint, (Size) GraphViewer.Grid.DragOrigin);

                    if (Math.Abs(lockedDragOffset.X) > Math.Abs(lockedDragOffset.Y))
                        newLocation.Y = GraphViewer.Grid.DragOrigin.Y;
                    else
                        newLocation.X = GraphViewer.Grid.DragOrigin.X;
                }

                if (Location != newLocation) {
                    SetLocation(newLocation);

                    UpdateTabOrder();
                    foreach (var node in DisplayedNode.InputLinks.Select(l => l.Supplier))
                        GraphViewer.NodeElementDictionary[node].UpdateTabOrder();
                    foreach (var node in DisplayedNode.OutputLinks.Select(l => l.Consumer))
                        GraphViewer.NodeElementDictionary[node].UpdateTabOrder();
                }
            }
        }
    }
}