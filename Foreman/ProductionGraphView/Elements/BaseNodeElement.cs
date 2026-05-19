using Foreman.Controls;
using Foreman.DataCaching;
using Foreman.Graph;
using Foreman.Models;
using Foreman.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman.ProductionGraphView.Elements {
    public abstract class BaseNodeElement : GraphElement {
        public bool Highlighted { get; set; } //selection - note that this doesnt mean it is or isnt in selection (at least not during drag operation - ex: dragging a not-selection over a group of selected nodes will change their highlight status, but wont add them to the 'selected' set until you let go of the drag)
        public INodeViewModel ViewModel { get; }

        public override int X { get { return ViewModel.Location.X; } set { Trace.Fail("Base node element location cant be set through X parameter! Use SetLocation(Point)"); } }
        public override int Y { get { return ViewModel.Location.Y; } set { Trace.Fail("Base node element location cant be set through Y parameter! Use SetLocation(Point)"); } }
        public override Point Location { get { return ViewModel.Location; } set { Trace.Fail("Base node element location cant be set through Location parameter! Use SetLocation(Point)"); } }
        public void SetLocation(Point location) {
            if (location != Location) {
                graphViewer.Session.Editor.SetLocation(ViewModel.Id, location);

                RequestStateUpdate();
                foreach (BaseNodeElement linkedNode in ViewModel.InputLinks.Select(l => graphViewer.GetNodeElement(l.SupplierId)).OfType<BaseNodeElement>())
                    linkedNode.RequestStateUpdate();
                foreach (BaseNodeElement linkedNode in ViewModel.OutputLinks.Select(l => graphViewer.GetNodeElement(l.ConsumerId)).OfType<BaseNodeElement>())
                    linkedNode.RequestStateUpdate();
            }
        }

        protected abstract Brush CleanBgBrush { get; }
        private static readonly Brush errorBgBrush = Brushes.Coral;
        private static readonly Brush ManualRateBGFilterBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0));

        private static readonly Brush equalFlowBorderBrush = Brushes.DarkGreen;
        private static readonly Brush overproducingFlowBorderBrush = Brushes.DarkGoldenrod;
        private static readonly Brush undersuppliedFlowBorderBrush = Brushes.DarkRed;

        protected static readonly Brush selectionOverlayBrush = new SolidBrush(Color.FromArgb(100, 100, 100, 200));

        protected static readonly Brush TextBrush = Brushes.Black;
        protected static readonly Font BaseFont = new(FontFamily.GenericSansSerif, 10f);
        protected static readonly Font CounterBaseFont = new(FontFamily.GenericSansSerif, 14f);
        protected static readonly Font TitleFont = new(FontFamily.GenericSansSerif, 9.2f, FontStyle.Bold);

        private static readonly StringFormat titleFormat = new() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center };
        private static readonly StringFormat textFormat = new() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center };
        protected static StringFormat TitleFormat => titleFormat;
        protected static StringFormat TextFormat => textFormat;

        //most values are attempted to fit the grid (6 * 2^n) - ex: 72 = 6 * (4+8)
        protected const int BaseSimpleHeight = 96; // 96 fits grid
        protected const int BaseRecipeHeight = 144; //144 fits grid
        protected const int TabPadding = 7; //makes each tab be evenly spaced for grid
        protected const int WidthD = 24; //(6*4) -> width will be divisible by this
        protected const int PassthroughNodeWidth = WidthD * 3;
        protected const int SpoilNodeWidth = WidthD * 6;
        protected const int MinWidth = WidthD * 6;
        protected const int BorderSpacing = 1; //the drawn node will be smaller by this in all directions (graph looks nicer if adjacent nodes have a slight gap between them)

        protected List<ItemTabElement> InputTabs { get; set; }
        protected List<ItemTabElement> OutputTabs { get; set; }
        private Point MouseDownLocation; //location where the mouse click down first happened - in graph coordinates (used to ensure that any drag operation begins at the start, and not at the point (+- a few pixels) where the drag was officially registed as a drag and not just a mouse click.
        private Point MouseDownNodeLocation; //location of this node the moment the mouse click down first happened - in graph coordinates
        private bool DragStarted;

        private bool NodeStateRequiresUpdate; //these are set by the events called from the node (as well as calling for invalidation). Any paint call checks for these, and if true resets them to false and calls the appropriate update functions
        private bool NodeValuesRequireUpdate; //this removes the need to manually update the nodes after any change, as well as not spamming update calls after every change (being based on paint refresh - aka: when it actually matters)

        protected ErrorNoticeElement errorNotice { get; set; }
        public BaseNodeElement(ProductionGraphViewer graphViewer, INodeViewModel viewModel) : base(graphViewer) {
            ViewModel = viewModel;
            DragStarted = false;
            ViewModel.NodeStateChanged += ViewModel_NodeStateChanged;
            ViewModel.NodeValuesChanged += ViewModel_NodeValuesChanged;

            InputTabs = [];
            OutputTabs = [];

            errorNotice = new ErrorNoticeElement(graphViewer, this) {
                Location = new Point(-Width / 2, -Height / 2)
            };
            errorNotice.SetVisibility(false);

            //first stage item tab creation - absolutely necessary in the constructor due to the creation and simultaneous linking of nodes being possible (drag to new node for example).
            foreach (ItemQualityPair item in ViewModel.Inputs)
                InputTabs.Add(new ItemTabElement(item, LinkType.Input, base.graphViewer, this));
            foreach (ItemQualityPair item in ViewModel.Outputs)
                OutputTabs.Add(new ItemTabElement(item, LinkType.Output, base.graphViewer, this));
        }

        private void ViewModel_NodeStateChanged(object? sender, EventArgs e) { NodeStateRequiresUpdate = true; graphViewer.Invalidate(); }
        private void ViewModel_NodeValuesChanged(object? sender, EventArgs e) { NodeValuesRequireUpdate = true; graphViewer.Invalidate(); }

        public void RequestStateUpdate() { NodeStateRequiresUpdate = true; }

        protected virtual void UpdateState() {
            //update error notice
            errorNotice.SetVisibility(ViewModel.State == NodeState.Error || ViewModel.State == NodeState.Warning);
            errorNotice.X = -Width / 2;
            errorNotice.Y = -Height / 2;

            UpdateTabOrder();
        }

        protected virtual void UpdateValues() {
            //update tab values
            foreach (ItemTabElement tab in InputTabs)
                tab.UpdateValues(ViewModel.GetConsumeRate(tab.Item), 0, false); //for inputs we only care to display the supply rate (guaranteed by solver to be equal to the amount consumed by recipe)
            foreach (ItemTabElement tab in OutputTabs)
                tab.UpdateValues(ViewModel.GetSupplyRate(tab.Item), ViewModel.GetSupplyUsedRate(tab.Item), ViewModel.IsOverproducing(tab.Item)); //for outputs we want the amount produced by the node, the amount supplied to other nodes, and true if we are supplying less than producing.
        }

        private void UpdateTabOrder() {
            InputTabs = [.. InputTabs.OrderBy(it => GetItemTabXHeuristic(it)).ThenBy(it => it.Item.Item?.Name).ThenBy(it => it.Item.Quality?.Level).ThenBy(it => it.Item.Quality?.Name)]; //then by ensures same result no matter who came first
            OutputTabs = [.. OutputTabs.OrderBy(it => GetItemTabXHeuristic(it)).ThenBy(it => it.Item.Item?.Name).ThenBy(it => it.Item.Quality?.Level).ThenBy(it => it.Item.Quality?.Name)];

            int x = -GetIconWidths(OutputTabs) / 2;
            int y = ViewModel.NodeDirection == NodeDirection.Up ? (-Height / 2) + 1 : (Height / 2) - 1;
            foreach (ItemTabElement tab in OutputTabs) {
                x += TabPadding;
                tab.Location = new Point(x + (tab.Width / 2), y);
                x += tab.Width;
            }

            x = -GetIconWidths(InputTabs) / 2;
            y = ViewModel.NodeDirection == NodeDirection.Up ? (Height / 2) - 1 : (-Height / 2) + 1;
            foreach (ItemTabElement tab in InputTabs) {
                x += TabPadding;
                tab.Location = new Point(x + (tab.Width / 2), y);
                x += tab.Width;
            }
        }

        protected static int GetIconWidths(List<ItemTabElement> tabs) {
            int result = TabPadding;
            foreach (ItemTabElement tab in tabs)
                result += tab.Bounds.Width + TabPadding;
            return result;
        }

        private int GetItemTabXHeuristic(ItemTabElement tab) {
            int total = 0;
            foreach (INodeLinkViewModel link in tab.Links) {
                if (!graphViewer.Session.View.TryGetNode(link.SupplierId, out INodeViewModel? supplier) || supplier is null ||
                    !graphViewer.Session.View.TryGetNode(link.ConsumerId, out INodeViewModel? consumer) || consumer is null)
                    continue;
                var diff = Point.Subtract(supplier.Location, (Size)consumer.Location);
                total += Convert.ToInt32(Math.Atan2(tab.LinkType == LinkType.Input ? diff.X : -diff.X, diff.Y) * 1000 + (diff.Y > 0 ? 1 : 0)); //x needs to be flipped depending on which endpoint we are calculating for. y is absoluted to take care of down connections. slight addition in case of up connection ensures that 2 equal connections will prioritize the up over the down.
            }
            return total;
        }

        public ItemTabElement GetOutputLineItemTab(ItemQualityPair item) {
            if (NodeStateRequiresUpdate)
                UpdateState();
            NodeStateRequiresUpdate = false;

            return OutputTabs.First(it => it.Item == item);
        }
        public ItemTabElement GetInputLineItemTab(ItemQualityPair item) {
            if (NodeStateRequiresUpdate)
                UpdateState();
            NodeStateRequiresUpdate = false;

            return InputTabs.First(it => it.Item == item);
        }

        public override void UpdateVisibility(Rectangle graphZone, int xborder = 0, int yborder = 0) {
            base.UpdateVisibility(graphZone, xborder, yborder + 30); //account for the vertical item boxes
        }

        public override bool ContainsPoint(Point graphPoint) {
            if (!Visible)
                return false;
            if (base.ContainsPoint(graphPoint))
                return true;

            foreach (ItemTabElement tab in SubElements.OfType<ItemTabElement>())
                if (tab.ContainsPoint(graphPoint))
                    return true;
            return errorNotice.ContainsPoint(graphPoint);
        }

        public override void PrePaint() {
            if (NodeStateRequiresUpdate)
                UpdateState();
            if (NodeStateRequiresUpdate || NodeValuesRequireUpdate)
                UpdateValues();
            NodeStateRequiresUpdate = false;
            NodeValuesRequireUpdate = false;
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            Point trans = LocalToGraph(new Point(0, 0)); //all draw operations happen in graph 0,0 origin coordinates. So we need to transform all our draw operations to the local 0,0 (center of object)
            if (style == NodeDrawingStyle.IconsOnly) {
                int iconSize = graphViewer.IconsDrawSize;
                if (NodeIcon() is Bitmap nodeIcon)
                    graphics.DrawImage(nodeIcon, trans.X - (iconSize / 2), trans.Y - (iconSize / 2), iconSize, iconSize);
            } else {
                //background
                Brush bgBrush = ViewModel.State == NodeState.Error ? errorBgBrush : CleanBgBrush;
                Brush borderBrush = ViewModel.ManualRateNotMet() && !(this is SupplierNodeElement) ? undersuppliedFlowBorderBrush : ViewModel.IsOverproducing() ? overproducingFlowBorderBrush : equalFlowBorderBrush;

                GraphicsStuff.FillRoundRect(trans.X - (Width / 2) + BorderSpacing, trans.Y - (Height / 2) + BorderSpacing, Width - (2 * BorderSpacing), Height - (2 * BorderSpacing), 10, graphics, borderBrush); //flow status border

                int yoffset = (ViewModel.KeyNode && !(this is ConsumerNodeElement)) ? 15 : 0;
                int heightOffset = ViewModel.KeyNode ? (this is ConsumerNodeElement || this is SupplierNodeElement) ? 15 : 30 : 0;
                GraphicsStuff.FillRoundRect(trans.X - (Width / 2) + BorderSpacing + 3, trans.Y - (Height / 2) + BorderSpacing + 3 + yoffset, Width - (2 * BorderSpacing) - 6, Height - (2 * BorderSpacing) - 6 - heightOffset, 7, graphics, bgBrush); //basic background (with given background brush)
                if (ViewModel.RateType == RateType.Manual)
                    GraphicsStuff.FillRoundRect(trans.X - (Width / 2) + 3, trans.Y - (Height / 2) + 3, Width - 6, Height - 6, 7, graphics, ManualRateBGFilterBrush); //darken background if its a manual rate set

                if (graphViewer.FlagOUSuppliedNodes && borderBrush != equalFlowBorderBrush)
                    GraphicsStuff.FillRoundRectTLFlag(trans.X - (Width / 2) + 3, trans.Y - (Height / 2) + 3, Width / 2 - 6, Height / 2 - 6, 7, graphics, borderBrush); //supply flag
                if (ViewModel.State == NodeState.Warning)
                    GraphicsStuff.FillRoundRectTLFlag(trans.X - (Width / 2) + 3, trans.Y - (Height / 2) + 3, Width / 2 - 6, Height / 2 - 6, 7, graphics, errorBgBrush); //warning flag

                //draw in all the inside details for this node
                if (style == NodeDrawingStyle.Regular || style == NodeDrawingStyle.PrintStyle)
                    DetailsDraw(graphics, trans);

                //highlight
                if (Highlighted)
                    GraphicsStuff.FillRoundRect(trans.X - (Width / 2), trans.Y - (Height / 2), Width, Height, 8, graphics, selectionOverlayBrush);
            }
        }

        protected abstract void DetailsDraw(Graphics graphics, Point trans); //draw the inside of the node.
        protected abstract Bitmap? NodeIcon();

        public override List<TooltipInfo> GetToolTips(Point graphPoint) {
            var element = SubElements.FirstOrDefault(it => it.ContainsPoint(graphPoint));
            var subTooltips = element?.GetToolTips(graphPoint) ?? null;
            var myTooltips = GetMyToolTips(graphPoint, subTooltips == null || subTooltips.Count == 0);

            if (subTooltips != null)
                myTooltips.AddRange(subTooltips);

            return myTooltips;
        }

        protected abstract List<TooltipInfo> GetMyToolTips(Point graphPoint, bool exclusive); //exclusive = true means no other tooltips are shown

        protected static List<TooltipInfo> ExclusiveHelpTooltip(string text, bool exclusive) =>
            exclusive ? [new TooltipInfo { Text = text, Direction = Direction.None, ScreenLocation = new Point(10, 10) }] : [];

        public override void MouseDown(Point graphPoint, MouseButtons button) {
            MouseDownLocation = graphPoint;
            MouseDownNodeLocation = new Point(X, Y);

            if (button == MouseButtons.Left)
                graphViewer.MouseDownElement = this;
        }

        public override void MouseUp(Point graphPoint, MouseButtons button, bool wasDragged) {
            DragStarted = false;
            var subelement = SubElements.OfType<ItemTabElement>().FirstOrDefault(it => it.ContainsPoint(graphPoint));
            if (!wasDragged) {
                if (subelement is not null)
                    subelement.MouseUp(graphPoint, button, false);
                else if (errorNotice.ContainsPoint(graphPoint))
                    errorNotice.MouseUp(graphPoint, button, false);
                else
                    MouseUpAction(graphPoint, button);
            }
        }

        protected virtual void MouseUpAction(Point graphPoint, MouseButtons button) {
            if (button == MouseButtons.Left) {
                graphViewer.EditNode(this);
            } else if (button == MouseButtons.Right) {
                RightClickMenu.Items.Add(new ToolStripMenuItem("Delete node", null,
                        new EventHandler((o, e) => {
                            RightClickMenu.Close();
                            graphViewer.Session.Editor.DeleteNode(ViewModel.Id);
                            graphViewer.Graph.UpdateNodeValues();
                        })));
                if (graphViewer.SelectedNodes.Count > 1 && graphViewer.SelectedNodes.Contains(this)) {
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Delete selected nodes", null,
                        new EventHandler((o, e) => {
                            RightClickMenu.Close();
                            graphViewer.TryDeleteSelectedNodes();
                        })));
                }

                RightClickMenu.Items.Add(new ToolStripSeparator());

                RightClickMenu.Items.Add(new ToolStripMenuItem("Flip node", null,
                    new EventHandler((o, e) => {
                        RightClickMenu.Close();
                        graphViewer.Session.Editor.SetDirection(ViewModel.Id, ViewModel.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up);
                    })));
                if (graphViewer.SelectedNodes.Count > 1 && graphViewer.SelectedNodes.Contains(this)) {
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Flip selected nodes", null,
                        new EventHandler((o, e) => {
                            RightClickMenu.Close();
                            graphViewer.FlipSelectedNodes();
                        })));
                }

                if (graphViewer.SelectedNodes.Count > 0) {
                    RightClickMenu.Items.Add(new ToolStripSeparator());
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Clear selection", null,
                        new EventHandler((o, e) => {
                            RightClickMenu.Close();
                            graphViewer.ClearSelection();
                        })));
                }

                var openInputs = new HashSet<ItemQualityPair>(graphViewer.SelectedNodes.SelectMany(n => n.InputTabs.Where(t => !t.Links.Any()).Select(t => t.Item)));
                var openOutputs = new HashSet<ItemQualityPair>(graphViewer.SelectedNodes.SelectMany(n => n.OutputTabs.Where(t => !t.Links.Any()).Select(t => t.Item)));
                var availableInputs = new HashSet<ItemQualityPair>(graphViewer.SelectedNodes.SelectMany(n => n.InputTabs.Select(t => t.Item)));
                var availableOutputs = new HashSet<ItemQualityPair>(graphViewer.SelectedNodes.SelectMany(n => n.OutputTabs.Select(t => t.Item)));
                bool matchedIO = openInputs.Intersect(availableOutputs).Any();
                bool matchedOI = openOutputs.Intersect(availableInputs).Any();
                if (matchedIO || matchedOI) {
                    RightClickMenu.Items.Add(new ToolStripSeparator());

                    if (matchedIO) {
                        RightClickMenu.Items.Add(new ToolStripMenuItem("Auto-connect disconnected inputs", null,
                            new EventHandler((o, e) => {
                                RightClickMenu.Close();

                                var openInputNodes = new Dictionary<NodeId, List<ItemQualityPair>>();
                                foreach (BaseNodeElement node in graphViewer.SelectedNodes.Where(n => n.InputTabs.Any(t => !t.Links.Any())))
                                    openInputNodes.Add(node.ViewModel.Id, [.. node.InputTabs.Where(t => !t.Links.Any()).Select(t => t.Item)]);

                                var availableOutputNodes = new Dictionary<ItemQualityPair, List<NodeId>>();
                                foreach (BaseNodeElement element in graphViewer.SelectedNodes.Where(n => !openInputNodes.ContainsKey(n.ViewModel.Id))) {
                                    foreach (ItemQualityPair output in element.ViewModel.Outputs) {
                                        if (!availableOutputNodes.ContainsKey(output))
                                            availableOutputNodes.Add(output, []);
                                        availableOutputNodes[output].Add(element.ViewModel.Id);
                                    }
                                }

                                foreach (KeyValuePair<NodeId, List<ItemQualityPair>> openInput in openInputNodes) {
                                    if (!graphViewer.Session.View.TryGetNode(openInput.Key, out INodeViewModel? consumerVm) || consumerVm is null)
                                        continue;
                                    foreach (ItemQualityPair requiredInput in openInput.Value) {
                                        if (!availableOutputNodes.TryGetValue(requiredInput, out List<NodeId>? suppliers))
                                            continue;
                                        NodeId supplierId = suppliers
                                            .OrderBy(id => {
                                                return !graphViewer.Session.View.TryGetNode(id, out INodeViewModel? supplierVm) || supplierVm is null
                                                    ? int.MaxValue
                                                    : Math.Abs(consumerVm.Location.X - supplierVm.Location.X) + Math.Abs(consumerVm.Location.Y - supplierVm.Location.Y);
                                            })
                                            .First();
                                        if (supplierId.IsValid)
                                            graphViewer.Session.Editor.CreateLink(supplierId, openInput.Key, requiredInput);
                                    }
                                }

                                graphViewer.Graph.UpdateNodeValues();
                            })));
                    }
                    if (matchedOI) {
                        RightClickMenu.Items.Add(new ToolStripMenuItem("Auto-connect disconnected outputs", null,
                            new EventHandler((o, e) => {
                                RightClickMenu.Close();

                                var openOutputNodes = new Dictionary<NodeId, List<ItemQualityPair>>();
                                foreach (BaseNodeElement node in graphViewer.SelectedNodes.Where(n => n.OutputTabs.Any(t => !t.Links.Any())))
                                    openOutputNodes.Add(node.ViewModel.Id, [.. node.OutputTabs.Where(t => !t.Links.Any()).Select(t => t.Item)]);

                                var availableInputNodes = new Dictionary<ItemQualityPair, List<NodeId>>();
                                foreach (BaseNodeElement element in graphViewer.SelectedNodes.Where(n => !openOutputNodes.ContainsKey(n.ViewModel.Id))) {
                                    foreach (ItemQualityPair input in element.ViewModel.Inputs) {
                                        if (!availableInputNodes.ContainsKey(input))
                                            availableInputNodes.Add(input, []);
                                        availableInputNodes[input].Add(element.ViewModel.Id);
                                    }
                                }

                                foreach (KeyValuePair<NodeId, List<ItemQualityPair>> openOutput in openOutputNodes) {
                                    if (!graphViewer.Session.View.TryGetNode(openOutput.Key, out INodeViewModel? supplierVm) || supplierVm is null)
                                        continue;
                                    foreach (ItemQualityPair requiredOutput in openOutput.Value) {
                                        if (!availableInputNodes.TryGetValue(requiredOutput, out List<NodeId>? consumers))
                                            continue;
                                        NodeId consumerId = consumers
                                            .OrderBy(id => {
                                                return !graphViewer.Session.View.TryGetNode(id, out INodeViewModel? consumerVm) || consumerVm is null
                                                    ? int.MaxValue
                                                    : Math.Abs(supplierVm.Location.X - consumerVm.Location.X) + Math.Abs(supplierVm.Location.Y - consumerVm.Location.Y);
                                            })
                                            .First();
                                        if (consumerId.IsValid)
                                            graphViewer.Session.Editor.CreateLink(openOutput.Key, consumerId, requiredOutput);
                                    }
                                }

                                graphViewer.Graph.UpdateNodeValues();
                            })));
                    }
                }

                AddRClickMenuOptions(graphViewer.SelectedNodes.Count == 0 || graphViewer.SelectedNodes.Contains(this));

                RightClickMenu.Items.Add(new ToolStripSeparator());
                RightClickMenu.Items.Add(new ToolStripMenuItem("Copy key node status", null,
                    new EventHandler((o, e) => {
                        RightClickMenu.Close();
                        Clipboard.SetText(GraphSaveCodec.WriteKeyNodeClipboardToString(
                            ViewModel.KeyNode, ViewModel.KeyNodeTitle,
                            writeIndented: false));

                    })));

                if (graphViewer.SelectedNodes.Count == 0 || graphViewer.SelectedNodes.Contains(this)) {
                    try {
                        if (GraphSaveCodec.ReadKeyNodeClipboard(Clipboard.GetText()) is KeyNodeClipboardSaveData keyNodeStatus) {
                            RightClickMenu.Items.Add(new ToolStripMenuItem("Paste key node status", null,
                                new EventHandler((o, e) => {
                                    RightClickMenu.Close();
                                    if (graphViewer.SelectedNodes.Count == 0) {
                                        if (graphViewer.Session.Editor.RequestNodeController(ViewModel.Id) is BaseNodeController controller) {
                                            controller.SetKeyNode(keyNodeStatus.KeyNode);
                                            controller.SetKeyNodeTitle(keyNodeStatus.Title);
                                        }
                                    } else if (graphViewer.SelectedNodes.Contains(this)) {
                                        foreach (BaseNodeElement node in graphViewer.SelectedNodes) {
                                            if (graphViewer.Session.Editor.RequestNodeController(node.ViewModel.Id) is BaseNodeController controller) {
                                                controller.SetKeyNode(keyNodeStatus.KeyNode);
                                                controller.SetKeyNodeTitle(keyNodeStatus.Title);
                                            }
                                        }
                                    }
                                })));
                        }
                    } catch (Exception ex) { ErrorLogging.LogException(ex, "Failed to apply clipboard node options"); }
                }


                RightClickMenu.Show(graphViewer, graphViewer.GraphToScreen(graphPoint));
            }
        }

        protected virtual void AddRClickMenuOptions(bool nodeInSelection) { }

        public override void Dragged(Point graphPoint) {
            if (!DragStarted) {
                ItemTabElement? draggedTab = null;
                foreach (ItemTabElement tab in SubElements.OfType<ItemTabElement>())
                    if (tab.ContainsPoint(MouseDownLocation))
                        draggedTab = tab;
                if (draggedTab != null)
                    graphViewer.StartLinkDrag(this, draggedTab.LinkType, draggedTab.Item);
                else {
                    DragStarted = true;
                }
            } else //drag started -> proceed with dragging the node around
              {
                var offset = (Size)Point.Subtract(graphPoint, (Size)MouseDownLocation);
                Point newLocation = graphViewer.Grid.AlignToGrid(Point.Add(MouseDownNodeLocation, offset));
                if (graphViewer.Grid.LockDragToAxis) {
                    var lockedDragOffset = Point.Subtract(graphPoint, (Size)graphViewer.Grid.DragOrigin);

                    if (Math.Abs(lockedDragOffset.X) > Math.Abs(lockedDragOffset.Y))
                        newLocation.Y = graphViewer.Grid.DragOrigin.Y;
                    else
                        newLocation.X = graphViewer.Grid.DragOrigin.X;
                }

                if (Location != newLocation) {
                    SetLocation(newLocation);

                    this.UpdateTabOrder();
                    foreach (BaseNodeElement? inputLinkedNode in ViewModel.InputLinks.Select(l => graphViewer.GetNodeElement(l.SupplierId)).OfType<BaseNodeElement>())
                        inputLinkedNode.UpdateTabOrder();
                    foreach (BaseNodeElement? outputLinkedNode in ViewModel.OutputLinks.Select(l => graphViewer.GetNodeElement(l.ConsumerId)).OfType<BaseNodeElement>())
                        outputLinkedNode.UpdateTabOrder();
                }
            }
        }
    }
}
