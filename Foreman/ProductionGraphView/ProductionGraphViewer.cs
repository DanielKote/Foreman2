using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public enum NewNodeType {
        Disconnected,
        Supplier,
        Consumer
    }

    // printstyle is meant for any additional chages (from regular) for exporting to image format,
    // simple will only draw the node boxes (no icons or text) and link lines,
    // iconsonly will draw node icons instead of nodes (for zoomed view)
    public enum NodeDrawingStyle {
        Regular,
        PrintStyle,
        Simple,
        IconsOnly
    }

    [Serializable]
    public partial class ProductionGraphViewer : UserControl, ISerializable {
        private enum DragOperation {
            None,
            Item,
            Selection
        }

        // low: only names.
        // medium: assemblers, beacons, etc.
        // high: include assembler percentages
        public enum Lod {
            Low,
            Medium,
            High
        }

        public Lod LevelOfDetail { get; set; }
        public bool ArrowsOnLinks { get; set; }
        public bool IconsOnly { get; set; }
        public int IconsSize { get; set; }

        public int IconsDrawSize => ViewScale > (double) IconsSize / 96 ? 96 : (int) (IconsSize / ViewScale);

        // if the number of elements to draw is over this amount then the drawing functions will switch to simple view draws
        // (mostly for FPS during zoomed out views)
        public int
            NodeCountForSimpleView { get; set; }

        public bool ShowRecipeToolTip { get; set; }
        public bool TooltipsEnabled { get; set; }
        // used together with tooltip enabled -> if we open up an item/recipe/assembler window, this will halt tooltip show.
        private bool _subwindowOpen;
        public bool DynamicLinkWidth = false;
        public bool LockedRecipeEditPanelPosition = true;
        // if true, will add a flag for over or under supplied nodes
        public bool FlagOuSuppliedNodes = false;

        public bool SmartNodeDirection { get; set; }

        public DataCache DCache { get; set; }
        public ProductionGraph Graph { get; private set; }
        public GridManager Grid { get; private set; }
        public FloatingTooltipRenderer ToolTipRenderer { get; private set; }
        public PointingArrowRenderer ArrowRenderer { get; private set; }

        // quality of the last-edited recipe's assembler (used when placing new recipe nodes)
        public Quality LastAssemblerQuality { get; private set; }

        public GraphElement MouseDownElement { get; set; }

        public IReadOnlyDictionary<ReadOnlyBaseNode, BaseNodeElement> NodeElementDictionary => _nodeElementDictionary;

        public IReadOnlyDictionary<ReadOnlyNodeLink, LinkElement> LinkElementDictionary => _linkElementDictionary;

        public IReadOnlyCollection<BaseNodeElement> SelectedNodes => _selectedNodes;

        public Point ViewOffset { get; private set; }
        public float ViewScale { get; private set; }
        public Rectangle VisibleGraphBounds { get; private set; }

        private const int minDragDiff = 30;
        private const int minLinkWidth = 3;
        private const int maxLinkWidth = 35;

        private static readonly Pen PausedBorders = new(Color.FromArgb(255, 80, 80), 5);
        private static readonly Pen SelectionPen = new(Color.FromArgb(100, 100, 200), 2);

        private Dictionary<ReadOnlyBaseNode, BaseNodeElement> _nodeElementDictionary;
        private List<BaseNodeElement> _nodeElements;
        private Dictionary<ReadOnlyNodeLink, LinkElement> _linkElementDictionary;
        private List<LinkElement> _linkElements;
        private DraggedLinkElement _draggedLinkElement;

        private Point _mouseDownStartScreenPoint;
        // we use this to ensure that any mouse operations only count if they started on this panel
        private MouseButtons _downButtons;

        private Point _viewDragOriginPoint;
        // separate from dragOperation due to being able to drag view at all stages of dragOperation
        private bool _viewBeingDragged;

        private DragOperation _currentDragOperation = DragOperation.None;

        private Rectangle _selectionZone;
        private Point _selectionZoneOriginPoint;

        // main list of selected nodes
        private HashSet<BaseNodeElement> _selectedNodes;

        // list of nodes currently under the selection zone (which can be added/removed/replace the full list)
        private HashSet<BaseNodeElement> _currentSelectionNodes;

        private ContextMenu _rightClickMenu = new();

        public ProductionGraphViewer() {
            InitializeComponent();
            MouseWheel += ProductionGraphViewer_MouseWheel;
            Resize += ProductionGraphViewer_Resized;

            ViewOffset = new Point(Width / -2, Height / -2);
            ViewScale = 1f;
            NodeCountForSimpleView = 200;

            IconsOnly = false;
            IconsSize = 32;

            TooltipsEnabled = true;
            _subwindowOpen = false;

            Graph = new ProductionGraph();
            //Graph.ClearGraph()
            Graph.NodeAdded += Graph_NodeAdded;
            Graph.NodeDeleted += Graph_NodeDeleted;
            Graph.LinkAdded += Graph_LinkAdded;
            Graph.LinkDeleted += Graph_LinkDeleted;
            Graph.NodeValuesUpdated += Graph_NodeValuesUpdated;

            Grid = new GridManager();
            ToolTipRenderer = new FloatingTooltipRenderer(this);
            ArrowRenderer = new PointingArrowRenderer(this);

            _nodeElementDictionary = new Dictionary<ReadOnlyBaseNode, BaseNodeElement>();
            _nodeElements = [];
            _linkElementDictionary = new Dictionary<ReadOnlyNodeLink, LinkElement>();
            _linkElements = [];

            _selectedNodes = [];
            _currentSelectionNodes = [];

            UpdateGraphBounds();
            Invalidate();
        }

        public void ClearGraph() {
            DisposeLinkDrag();
            Graph.ClearGraph();

            // at this point every node element and link element has been removed.

            _selectedNodes.Clear();
            _currentSelectionNodes.Clear();
        }

        // returns first such node (in case of stacking)
        public BaseNodeElement GetNodeAtPoint(Point point) {
            //done in a 2 stage process -> first we do a rough check on the point's location (point within a node's area + 50 boundary on all sides), it goes to part 2)
            //							-> then we do a full element.containsPoint check which includes both the node and any added segments (such as item frames)

            for (var i = _nodeElements.Count - 1; i >= 0; i--) {
                var roughNodeZone = new Rectangle(_nodeElements[i].X - _nodeElements[i].Width / 2 - 50, _nodeElements[i].Y - _nodeElements[i].Height / 2 - 50,
                    _nodeElements[i].Width + 100, _nodeElements[i].Height + 100);
                if (!roughNodeZone.Contains(point))
                    continue;

                if (_nodeElements[i].ContainsPoint(point))
                    return _nodeElements[i];
            }

            return null;
        }

        //----------------------------------------------Adding new node functions (including link dragging) + Node edit

        public void StartLinkDrag(BaseNodeElement startNode, LinkType linkType, ItemQualityPair item) {
            _draggedLinkElement?.Dispose();
            _draggedLinkElement = new DraggedLinkElement(this, startNode, linkType, item);
            MouseDownElement = _draggedLinkElement;
        }

        public void DisposeLinkDrag() {
            _draggedLinkElement?.Dispose();
            _draggedLinkElement = null;
        }

        public void AddItem(Point drawOrigin, Point newLocation) {
            if (string.IsNullOrEmpty(DCache.PresetName)) {
                MessageBox.Show($"The current preset ({Properties.Settings.Default.CurrentPresetName}) is corrupt.");
                return;
            }

            _subwindowOpen = true;
            var itemChooser = new ItemChooserPanel(this, drawOrigin);
            itemChooser.ItemRequested += (o, itemRequestArgs) => { AddNewNode(drawOrigin, itemRequestArgs.Item, newLocation, NewNodeType.Disconnected); };
            itemChooser.PanelClosed += (o, e) => { _subwindowOpen = false; };

            itemChooser.Show();
        }

        public void AddNewNode(Point drawOrigin, ItemQualityPair baseItem, Point newLocation, NewNodeType nNodeType, BaseNodeElement originElement = null,
            bool offsetLocationToItemTabLevel = false) {
            if (string.IsNullOrEmpty(DCache.PresetName)) {
                DisposeLinkDrag();
                MessageBox.Show($"The current preset ({Properties.Settings.Default.CurrentPresetName}) is corrupt.");
                return;
            }

            if (nNodeType != NewNodeType.Disconnected && (originElement == null || !baseItem))
                Trace.Fail("Origin element or base item not provided for a new (linked) node");

            if (Grid.ShowGrid)
                newLocation = Grid.AlignToGrid(newLocation);

            var lastNodeWidth = 0;
            var newNodeDirection = originElement == null || !SmartNodeDirection ? Graph.DefaultNodeDirection :
                _draggedLinkElement.Type != BaseLinkElement.LineType.UShape ? originElement.DisplayedNode.NodeDirection :
                originElement.DisplayedNode.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up;

            // control key pressed -> we are making a passthrough node.

            if ((ModifierKeys & Keys.Control) == Keys.Control) {
                ProcessNodeRequest(null, new RecipeRequestArgs(NodeType.Passthrough));
                DisposeLinkDrag();
                Graph.UpdateNodeStates(false);
                Invalidate();
            } else {
                var tempRange = new FRange(0, 0, true);
                if (baseItem && baseItem.Item is Fluid { IsTemperatureDependent: true } fluid) {
                    if (nNodeType == NewNodeType.Consumer)
                        // need to check all nodes down to recipes for range of temperatures being produced
                        tempRange = LinkChecker.GetTemperatureRange(fluid, originElement.DisplayedNode, LinkType.Output, true);
                    else if (nNodeType == NewNodeType.Supplier)
                        // need to check all nodes up to recipes for range of temperatures being consumed (guaranteed to be in a SINGLE [] range)
                        tempRange = LinkChecker.GetTemperatureRange(fluid, originElement.DisplayedNode, LinkType.Input, true);
                }

                // QUALITY UPDATE

                var recipeChooser = new RecipeChooserPanel(this, drawOrigin, baseItem, tempRange, nNodeType);
                recipeChooser.RecipeRequested += ProcessNodeRequest;
                recipeChooser.PanelClosed += (o, e) => {
                    if (e.Option == IrChooserPanel.ChooserPanelCloseReason.RequiresItemSelection)
                        return;

                    _subwindowOpen = false;
                    DisposeLinkDrag();
                    Graph.UpdateNodeStates(false);
                    Invalidate();
                };

                _subwindowOpen = true;
                recipeChooser.Show();
            }

            // end of this function
            return;

            // internal helper function: called upon a successful selection of a recipe-selection screen (opened above)
            void ProcessNodeRequest(object o, RecipeRequestArgs recipeRequestArgs) {
                ReadOnlyBaseNode newNode;
                switch (recipeRequestArgs.NodeType) {
                    case NodeType.Consumer:
                        newNode = Graph.CreateConsumerNode(baseItem, newLocation);
                        FinalizeNodePosition(newNode);
                        break;
                    case NodeType.Supplier:
                        newNode = Graph.CreateSupplierNode(baseItem, newLocation);
                        FinalizeNodePosition(newNode);
                        break;
                    case NodeType.Passthrough:
                        newNode = Graph.CreatePassthroughNode(baseItem, newLocation);
                        FinalizeNodePosition(newNode);
                        break;
                    case NodeType.Spoil:
                        if (recipeRequestArgs.Direction == NodeDirection.Up) {
                            newNode = Graph.CreateSpoilNode(baseItem, baseItem.Item.SpoilResult, newLocation);
                            FinalizeNodePosition(newNode);
                        } else if (baseItem.Item.SpoilOrigins.Count == 1) {
                            // QUALITY UPDATE

                            newNode = Graph.CreateSpoilNode(new ItemQualityPair(baseItem.Item.SpoilOrigins.ElementAt(0), baseItem.Quality), baseItem.Item,
                                newLocation);
                            FinalizeNodePosition(newNode);
                        } else {
                            // need to open up an item selection window to select a given spoil origin

                            _subwindowOpen = true;
                            var itemChooser = new ItemChooserPanel(this, drawOrigin, baseItem.Item.SpoilOrigins);
                            itemChooser.ItemRequested += (oo, itemRequestArgs) => {
                                newNode = Graph.CreateSpoilNode(new ItemQualityPair(itemRequestArgs.Item.Item, baseItem.Quality), baseItem.Item, newLocation);
                                FinalizeNodePosition(newNode);
                            };
                            itemChooser.PanelClosed += (oo, e) => { _subwindowOpen = false; };
                            itemChooser.Show();
                        }

                        break;
                    case NodeType.Plant:
                        if (recipeRequestArgs.Direction == NodeDirection.Up) {
                            newNode = Graph.CreatePlantNode(baseItem.Item.PlantResult, baseItem.Quality, newLocation);
                            FinalizeNodePosition(newNode);
                        } else if (baseItem.Item.PlantOrigins.Count == 1) {
                            // QUALITY UPDATE

                            newNode = Graph.CreatePlantNode(baseItem.Item.PlantOrigins.ElementAt(0).PlantResult, DCache.DefaultQuality,
                                newLocation);
                            FinalizeNodePosition(newNode);
                        } else {
                            // need to open up an item selection window to select a given spoil origin

                            _subwindowOpen = true;
                            var itemChooser = new ItemChooserPanel(this, drawOrigin, baseItem.Item.PlantOrigins);
                            itemChooser.ItemRequested += (oo, itemRequestArgs) => {
                                newNode = Graph.CreatePlantNode(itemRequestArgs.Item.Item.PlantResult, DCache.DefaultQuality, newLocation);
                                FinalizeNodePosition(newNode);
                            };
                            itemChooser.PanelClosed += (oo, e) => { _subwindowOpen = false; };
                            itemChooser.Show();
                        }

                        break;
                    case NodeType.Recipe:
                        var rNode = Graph.CreateRecipeNode(recipeRequestArgs.Recipe, newLocation);
                        newNode = rNode;
                        if ((nNodeType == NewNodeType.Consumer && !recipeRequestArgs.Recipe.Recipe.IngredientSet.ContainsKey(baseItem.Item))
                            || (nNodeType == NewNodeType.Supplier && !recipeRequestArgs.Recipe.Recipe.ProductSet.ContainsKey(baseItem.Item))
                            || (nNodeType == NewNodeType.Disconnected && baseItem && !recipeRequestArgs.Recipe.Recipe.IngredientSet.ContainsKey(baseItem.Item)
                                && !recipeRequestArgs.Recipe.Recipe.ProductSet.ContainsKey(baseItem.Item))
                        ) {
                            AssemblerSelector.Style style;
                            switch (Graph.AssemblerSelector.DefaultSelectionStyle) {
                                case AssemblerSelector.Style.Best:
                                case AssemblerSelector.Style.BestBurner:
                                case AssemblerSelector.Style.BestNonBurner:
                                    style = AssemblerSelector.Style.BestBurner;
                                    break;
                                case AssemblerSelector.Style.Worst:
                                case AssemblerSelector.Style.WorstBurner:
                                case AssemblerSelector.Style.WorstNonBurner:
                                default:
                                    style = AssemblerSelector.Style.WorstBurner;
                                    break;
                            }

                            var assemblerOptions = Graph.AssemblerSelector.GetOrderedAssemblerList(recipeRequestArgs.Recipe.Recipe, style);

                            var controller = (RecipeNodeController) Graph.RequestNodeController(rNode);
                            if (nNodeType == NewNodeType.Consumer ||
                                (nNodeType == NewNodeType.Disconnected && assemblerOptions.Any(a => a.Fuels.Contains(baseItem.Item)))) {
                                controller.SetAssembler(new AssemblerQualityPair(assemblerOptions.First(a => a.Fuels.Contains(baseItem.Item)),
                                    Graph.DefaultAssemblerQuality));
                                controller.SetFuel(baseItem.Item);
                            } else if (nNodeType == NewNodeType.Supplier || (nNodeType == NewNodeType.Disconnected &&
                                assemblerOptions.Any(a => a.Fuels.Contains(baseItem.Item.FuelOrigin)))) {
                                controller.SetAssembler(new AssemblerQualityPair(assemblerOptions.First(a => a.Fuels.Contains(baseItem.Item.FuelOrigin)),
                                    Graph.DefaultAssemblerQuality));
                                controller.SetFuel(baseItem.Item.FuelOrigin);
                            }
                        }

                        FinalizeNodePosition(newNode);
                        break;
                }
            }

            // internal helper function: once a node has been created it will be placed where it needs to be
            // and all intermediate states (ex: dragged item line) finalized
            void FinalizeNodePosition(ReadOnlyBaseNode newNode) {
                // this is the offset to take into account multiple recipe additions (holding shift while selecting recipe).
                // First node isn't shifted, all subsequent ones are 'attempted' to be spaced.
                // should be updated once the node graphics are updated (so that the node size doesn't depend as much on the text)

                var newNodeElement = NodeElementDictionary[newNode];
                var offsetDistance = lastNodeWidth / 2;
                // effectively: this recipe width
                lastNodeWidth = newNodeElement.Width;
                if (offsetDistance > 0) {
                    offsetDistance += lastNodeWidth / 2;
                    var newOffsetDistance = Grid.AlignToGrid(offsetDistance);
                    if (newOffsetDistance < offsetDistance)
                        newOffsetDistance += Grid.CurrentGridUnit;
                    offsetDistance = newOffsetDistance;
                }

                newLocation = new Point(newLocation.X + offsetDistance, newLocation.Y);

                var yOffset = offsetLocationToItemTabLevel
                    ? nNodeType == NewNodeType.Consumer ? -newNodeElement.Height / 2 : nNodeType == NewNodeType.Supplier ? newNodeElement.Height / 2 : 0
                    : 0;
                yOffset *= newNodeDirection == NodeDirection.Up ? 1 : -1;
                Graph.RequestNodeController(newNode).SetLocation(new Point(newLocation.X, newLocation.Y + yOffset));

                if (originElement != null)
                    Graph.RequestNodeController(newNode).SetDirection(newNodeDirection);

                if (nNodeType == NewNodeType.Consumer)
                    Graph.CreateLink(originElement.DisplayedNode, newNode, baseItem);
                else if (nNodeType == NewNodeType.Supplier)
                    Graph.CreateLink(newNode, originElement.DisplayedNode, baseItem);

                DisposeLinkDrag();
                Graph.UpdateNodeValues();
                Graph.UpdateNodeStates(false);
                Invalidate();
            }
        }

        public void AddPassthroughNodesFromSelection(LinkType linkType, Size offset) {
            var newPassthroughNodes = new List<BaseNodeElement>();
            foreach (PassthroughNodeElement passthroughNode in _selectedNodes) {
                var newNodeDirection = !SmartNodeDirection ? Graph.DefaultNodeDirection :
                    _draggedLinkElement.Type != BaseLinkElement.LineType.UShape ? passthroughNode.DisplayedNode.NodeDirection :
                    passthroughNode.DisplayedNode.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up;

                var passthroughItem = ((ReadOnlyPassthroughNode) passthroughNode.DisplayedNode).PassthroughItem;

                var yOffset = linkType == LinkType.Input ? passthroughNode.Height / 2 : -passthroughNode.Height / 2;
                yOffset *= newNodeDirection == NodeDirection.Up ? 1 : -1;
                yOffset += offset.Height;

                var newNode = Graph.CreatePassthroughNode(passthroughItem,
                    new Point(passthroughNode.Location.X + offset.Width, passthroughNode.Location.Y + yOffset));
                var controller = (PassthroughNodeController) Graph.RequestNodeController(newNode);
                controller.SetDirection(newNodeDirection);

                if (linkType == LinkType.Input)
                    Graph.CreateLink(newNode, passthroughNode.DisplayedNode, passthroughItem);
                else
                    Graph.CreateLink(passthroughNode.DisplayedNode, newNode, passthroughItem);

                newPassthroughNodes.Add(_nodeElementDictionary[newNode]);
            }

            SetSelection(newPassthroughNodes);

            DisposeLinkDrag();
            Graph.UpdateNodeStates(false);
            Invalidate();
        }

        public void TryDeleteSelectedNodes() {
            var proceed = true;
            if (_selectedNodes.Count > 10)
                proceed = MessageBox.Show($"You are deleting {_selectedNodes.Count} nodes. \nAre you sure?", "Confirm delete.", MessageBoxButtons.YesNo) ==
                    DialogResult.Yes;
            if (!proceed)
                return;

            foreach (var node in _selectedNodes.ToList())
                Graph.DeleteNode(node.DisplayedNode);
            _selectedNodes.Clear();
            Graph.UpdateNodeValues();
        }

        public void FlipSelectedNodes() {
            foreach (var node in _selectedNodes.ToList())
                Graph.RequestNodeController(node.DisplayedNode)
                    .SetDirection(node.DisplayedNode.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up);
            Invalidate();
        }

        public void SetSelectedPassthroughNodesSimpleDraw(bool simpleDraw) {
            foreach (PassthroughNodeElement node in _selectedNodes.Where(n => n is PassthroughNodeElement).ToList())
                ((PassthroughNodeController) Graph.RequestNodeController(node.DisplayedNode)).SetSimpleDraw(simpleDraw);
            Invalidate();
        }

        public void EditNode(BaseNodeElement bNodeElement) {
            if (bNodeElement is RecipeNodeElement rNodeElement) {
                EditRecipeNode(rNodeElement);
                return;
            }

            _subwindowOpen = true;
            Control editPanel = new EditFlowPanel(bNodeElement.DisplayedNode, this);

            // offset view if necessary to ensure entire window will be seen (with 25 pixels boundary)

            var screenOriginPoint = GraphToScreen(new Point(bNodeElement.X - bNodeElement.Width / 2, bNodeElement.Y));
            screenOriginPoint = new Point(screenOriginPoint.X - editPanel.Width, screenOriginPoint.Y - editPanel.Height / 2);
            var offset = new Point(
                Math.Min(Math.Max(0, 25 - screenOriginPoint.X), Width - screenOriginPoint.X - editPanel.Width - bNodeElement.Width - 25),
                Math.Min(Math.Max(0, 25 - screenOriginPoint.Y), Height - screenOriginPoint.Y - editPanel.Height - 25));

            ViewOffset = Point.Add(ViewOffset, new Size((int) (offset.X / ViewScale), (int) (offset.Y / ViewScale)));
            UpdateGraphBounds();
            Invalidate();

            // open up the edit panel

            var fttc = new FloatingTooltipControl(editPanel, Direction.Right,
                new Point(bNodeElement.X - bNodeElement.Width / 2, bNodeElement.Y), this, true, false);
            fttc.Closing += (s, e) => {
                _subwindowOpen = false;
                //bNodeElement.Update();
                Graph.UpdateNodeValues();
            };
        }

        public void EditRecipeNode(RecipeNodeElement rNodeElement) {
            _subwindowOpen = true;
            var rNode = (ReadOnlyRecipeNode) rNodeElement.DisplayedNode;
            Control editPanel = new EditRecipePanel(rNode, this);
            var recipePanel = new RecipePanel([rNode.BaseRecipe.Recipe]);

            if (LockedRecipeEditPanelPosition) {
                editPanel.Location = new Point(15, 15);
                recipePanel.Location = new Point(editPanel.Location.X + editPanel.Width + 5, editPanel.Location.Y);
            } else {
                // offset view if necessary to ensure entire window will be seen (with 25 pixels boundary).
                // Additionally, we want the tooltips to start 100 pixels above the arrow point instead of based in the center of the control
                // (due to the dynamically changing height of the recipe option panel)

                var recipeEditPanelOriginPoint = ToolTipRenderer
                    .GetTooltipScreenBounds(GraphToScreen(new Point(rNodeElement.X - rNodeElement.Width / 2, rNodeElement.Y)), editPanel.Size,
                        Direction.Right).Location;
                recipeEditPanelOriginPoint.Y += editPanel.Height / 2 - 125;
                recipeEditPanelOriginPoint.X -= recipePanel.Width + 5;
                var offset = new Point(
                    Math.Min(Math.Max(0, 25 - recipeEditPanelOriginPoint.X), Width - recipeEditPanelOriginPoint.X - editPanel.Width),
                    Math.Min(Math.Max(0, 25 - recipeEditPanelOriginPoint.Y), Height - recipeEditPanelOriginPoint.Y - editPanel.Height - 25));

                editPanel.Location = Point.Add(recipeEditPanelOriginPoint, (Size) offset);
                recipePanel.Location = new Point(editPanel.Location.X + editPanel.Width + 5, editPanel.Location.Y);

                ViewOffset = Point.Add(ViewOffset, new Size((int) (offset.X / ViewScale), (int) (offset.Y / ViewScale)));
                UpdateGraphBounds(false);
                Invalidate();
            }

            // add the visible recipe to the right of the node

            new FloatingTooltipControl(recipePanel, Direction.Left, new Point(rNodeElement.X + rNodeElement.Width / 2, rNodeElement.Y), this, true, true);
            var fttc = new FloatingTooltipControl(editPanel, Direction.Right,
                new Point(rNodeElement.X - rNodeElement.Width / 2, rNodeElement.Y), this, true, true);
            fttc.Closing += (s, e) => {
                _subwindowOpen = false;
                rNodeElement.RequestStateUpdate();
                Graph.UpdateNodeValues();
            };
        }

        //----------------------------------------------Selection functions

        private void SetSelection(IEnumerable<BaseNodeElement> newSelection) {
            foreach (var element in _selectedNodes)
                element.Highlighted = false;

            _selectedNodes.Clear();
            _selectedNodes.UnionWith(newSelection);

            foreach (var element in _selectedNodes)
                element.Highlighted = true;
        }

        private void UpdateSelection() {
            foreach (var element in _nodeElements)
                element.Highlighted = false;

            if ((ModifierKeys & Keys.Alt) != 0) { // remove zone
                foreach (var selectedNode in _selectedNodes)
                    selectedNode.Highlighted = true;
                foreach (var newlySelectedNode in _currentSelectionNodes)
                    newlySelectedNode.Highlighted = false;
            } else if ((ModifierKeys & Keys.Control) != 0) { // add zone
                foreach (var selectedNode in _selectedNodes)
                    selectedNode.Highlighted = true;
                foreach (var newlySelectedNode in _currentSelectionNodes)
                    newlySelectedNode.Highlighted = true;
            } else { // add zone (additive with ctrl or simple selection)
                foreach (var newlySelectedNode in _currentSelectionNodes)
                    newlySelectedNode.Highlighted = true;
            }
        }

        public void ClearSelection() {
            foreach (var element in _nodeElements)
                element.Highlighted = false;
            _selectedNodes.Clear();
            _currentSelectionNodes.Clear();
            Invalidate();
        }

        public void AlignSelected() {
            foreach (var ne in _selectedNodes)
                ne.SetLocation(Grid.AlignToGrid(ne.Location));
            Invalidate();
        }

        //----------------------------------------------Paint functions

        protected IEnumerable<GraphElement> GetPaintingOrder() {
            if (_draggedLinkElement != null)
                yield return _draggedLinkElement;
            foreach (var element in _linkElements)
                yield return element;
            foreach (var element in _nodeElements)
                yield return element;
        }

        public void UpdateNodeVisuals() {
            try {
                foreach (var node in _nodeElements)
                    node.RequestStateUpdate();
            } catch (OverflowException) {
                // Same as when working out node values, there's not really much to do here...
                // Maybe I could show a tooltip saying the numbers are too big or something...
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            e.Graphics.ResetTransform();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.Clear(BackColor);
            e.Graphics.TranslateTransform(Width / 2, Height / 2);
            e.Graphics.ScaleTransform(ViewScale, ViewScale);
            e.Graphics.TranslateTransform(ViewOffset.X, ViewOffset.Y);

            Paint(e.Graphics);
        }

        public new void Paint(Graphics graphics, bool fullGraph = false) {
            // update visibility of all elements

            if (fullGraph)
                foreach (var element in GetPaintingOrder())
                    element.UpdateVisibility(Graph.Bounds);
            else
                foreach (var element in GetPaintingOrder())
                    element.UpdateVisibility(VisibleGraphBounds);

            // ensure width of selection is correct

            SelectionPen.Width = 2 / ViewScale;

            // grid

            if (!fullGraph)
                Grid.Paint(graphics, ViewScale, VisibleGraphBounds, _currentDragOperation == DragOperation.Item ? MouseDownElement as BaseNodeElement : null);

            // process link element widths

            if (DynamicLinkWidth) {
                double itemMax = 0;
                double fluidMax = 0;
                foreach (var element in _linkElements) {
                    // §§ is the foreman added special items (currently just §§heat). ignore them

                    if (element.Item.Item is Fluid && !element.Item.Item.Name.StartsWith("§§"))
                        fluidMax = Math.Max(fluidMax, element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item));
                    else
                        itemMax = Math.Max(itemMax, element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item));
                }

                itemMax += itemMax == 0 ? 1 : 0;
                fluidMax += fluidMax == 0 ? 1 : 0;

                foreach (var element in _linkElements) {
                    if (element.Item.Item is Fluid)
                        element.LinkWidth = (float) Math.Min(minLinkWidth + (maxLinkWidth - minLinkWidth) * (element.DisplayedLink.Throughput / fluidMax),
                            maxLinkWidth);
                    else
                        element.LinkWidth = (float) Math.Min(minLinkWidth + (maxLinkWidth - minLinkWidth) * (element.DisplayedLink.Throughput / itemMax),
                            maxLinkWidth);
                }
            } else {
                foreach (var element in _linkElements)
                    element.LinkWidth = minLinkWidth;
            }

            // run any pre-paint functions

            foreach (var element in GetPaintingOrder())
                element.PrePaint();

            //paint all elements (nodes & lines)
            var visibleElements = GetPaintingOrder().Count(e => e.Visible && e is BaseNodeElement);
            foreach (var element in GetPaintingOrder()) {
                // if view scale is 0.2, then the text, images, etc. being drawn are ~1/5th the size:
                // aka: ~6x6 pixel images, etc. Use simple draw. Also, simple draw if too many objects

                element.Paint(graphics,
                    fullGraph ? NodeDrawingStyle.PrintStyle :
                    IconsOnly ? NodeDrawingStyle.IconsOnly :
                    visibleElements > NodeCountForSimpleView || ViewScale < 0.2 ? NodeDrawingStyle.Simple :
                    NodeDrawingStyle.Regular);
            }

            // selection zone

            if (_currentDragOperation == DragOperation.Selection && !fullGraph) {
                graphics.DrawRectangle(SelectionPen, _selectionZone);
                var pConsumption = _currentSelectionNodes.Where(n => n.DisplayedNode is ReadOnlyRecipeNode).Sum(n =>
                    ((ReadOnlyRecipeNode) n.DisplayedNode).GetTotalAssemblerElectricalConsumption() +
                    ((ReadOnlyRecipeNode) n.DisplayedNode).GetTotalBeaconElectricalConsumption());
                var pProduction = _currentSelectionNodes.Where(n => n.DisplayedNode is ReadOnlyRecipeNode)
                    .Sum(n => ((ReadOnlyRecipeNode) n.DisplayedNode).GetTotalGeneratorElectricalProduction());
                var recipeNodeCount = _currentSelectionNodes.Count(n => n.DisplayedNode is ReadOnlyRecipeNode);
                var buildingCount = (int) Math.Ceiling(_currentSelectionNodes.Where(n => n.DisplayedNode is ReadOnlyRecipeNode)
                    .Sum(n => ((ReadOnlyRecipeNode) n.DisplayedNode).ActualSetValue));
                var beaconCount = _currentSelectionNodes.Where(n => n.DisplayedNode is ReadOnlyRecipeNode)
                    .Sum(n => ((ReadOnlyRecipeNode) n.DisplayedNode).GetTotalBeacons());

                ToolTipRenderer.AddExtraToolTip(new TooltipInfo() {
                    Text =
                        $"Power consumption: {GraphicsStuff.DoubleToEnergy(pConsumption, "W")}\nPower production: {GraphicsStuff.DoubleToEnergy(pProduction, "W")}\nRecipe count: {recipeNodeCount}\nBuilding count: {buildingCount}\nBeacon count: {beaconCount}",
                    Direction = Direction.None, ScreenLocation = new Point(10, 10)
                });
            }

            // everything below will be drawn directly on the screen instead of scaled/shifted based on graph

            graphics.ResetTransform();

            if (!fullGraph) {
                // warning/error arrows

                ArrowRenderer.Paint(graphics, Graph);

                // floating tooltips

                ToolTipRenderer.Paint(graphics, TooltipsEnabled && !_subwindowOpen && _currentDragOperation == DragOperation.None && !_viewBeingDragged);
                ToolTipRenderer.ClearExtraToolTips();

                //paused border

                // graph null check is purely for design view
                if (Graph is { PauseUpdates: true })
                    graphics.DrawRectangle(PausedBorders, 0, 0, Width - 3, Height - 3);
            }
        }

        //----------------------------------------------Production Graph events

        private void Graph_NodeValuesUpdated(object sender, EventArgs e) {
            UpdateNodeVisuals();
        }

        private void Graph_LinkDeleted(object sender, NodeLinkEventArgs e) {
            var supplier = _nodeElementDictionary[e.NodeLink.Supplier];
            var consumer = _nodeElementDictionary[e.NodeLink.Consumer];

            var element = _linkElementDictionary[e.NodeLink];
            _linkElementDictionary.Remove(e.NodeLink);
            _linkElements.Remove(element);
            element.Dispose();

            supplier.RequestStateUpdate();
            consumer.RequestStateUpdate();
            Invalidate();
        }

        private void Graph_LinkAdded(object sender, NodeLinkEventArgs e) {
            var supplier = _nodeElementDictionary[e.NodeLink.Supplier];
            var consumer = _nodeElementDictionary[e.NodeLink.Consumer];

            var element = new LinkElement(this, e.NodeLink, supplier, consumer);
            _linkElementDictionary.Add(e.NodeLink, element);
            _linkElements.Add(element);

            supplier.RequestStateUpdate();
            consumer.RequestStateUpdate();
            Invalidate();
        }

        private void Graph_NodeDeleted(object sender, NodeEventArgs e) {
            var element = _nodeElementDictionary[e.Node];
            _nodeElementDictionary.Remove(e.Node);
            _nodeElements.Remove(element);
            _selectedNodes.Remove(element);
            element.Dispose();
            Invalidate();
        }

        private void Graph_NodeAdded(object sender, NodeEventArgs e) {
            BaseNodeElement element = null;
            switch (e.Node) {
                case ReadOnlySupplierNode supplierNode:
                    element = new SupplierNodeElement(this, supplierNode);
                    break;
                case ReadOnlyConsumerNode consumerNode:
                    element = new ConsumerNodeElement(this, consumerNode);
                    break;
                case ReadOnlyPassthroughNode passthroughNode:
                    element = new PassthroughNodeElement(this, passthroughNode);
                    break;
                case ReadOnlyRecipeNode recipeNode:
                    element = new RecipeNodeElement(this, recipeNode);
                    break;
                case ReadOnlySpoilNode spoilNode:
                    element = new SpoilNodeElement(this, spoilNode);
                    break;
                case ReadOnlyPlantNode plantNode:
                    element = new PlantNodeElement(this, plantNode);
                    break;
                default:
                    Trace.Fail("Unexpected node type created in graph.");
                    break;
            }

            _nodeElementDictionary.Add(e.Node, element);
            _nodeElements.Add(element);
            Invalidate();
        }

        //----------------------------------------------Mouse events

        private void ProductionGraphViewer_MouseDown(object sender, MouseEventArgs e) {
            _downButtons |= e.Button;

            ToolTipRenderer.ClearFloatingControls();
            ActiveControl = null; //helps panels like IRChooserPanel (for item/recipe choosing) close when we click on the graph

            _mouseDownStartScreenPoint = MousePosition;
            var graphLocation = ScreenToGraph(e.Location);

            var clickedElement = (GraphElement) _draggedLinkElement ?? GetNodeAtPoint(ScreenToGraph(e.Location));
            clickedElement?.MouseDown(graphLocation, e.Button);

            if (e.Button is MouseButtons.Middle or MouseButtons.Right) {
                _viewDragOriginPoint = graphLocation;
            } else if (e.Button == MouseButtons.Left && clickedElement == null) //selection
            {
                _selectionZoneOriginPoint = graphLocation;
                _selectionZone = new Rectangle();
                if ((ModifierKeys & Keys.Control) == 0 &&
                    (ModifierKeys & Keys.Alt) == 0) //clear all selected nodes if we arent using modifier keys
                {
                    foreach (var ne in _selectedNodes)
                        ne.Highlighted = false;
                    _selectedNodes.Clear();
                }
            }
        }

        private void ProductionGraphViewer_MouseUp(object sender, MouseEventArgs e) {
            _downButtons &= ~e.Button;

            ToolTipRenderer.ClearFloatingControls();
            var graphLocation = ScreenToGraph(e.Location);
            var element = (GraphElement) _draggedLinkElement ?? GetNodeAtPoint(graphLocation);

            switch (e.Button) {
                case MouseButtons.Right:
                    if (_viewBeingDragged)
                        _viewBeingDragged = false;
                    else if (_currentDragOperation == DragOperation.None && element == null) { // right-click on an empty space -> show add item/recipe menu
                        var screenPoint = new Point(e.Location.X - 150, 15);

                        // want to position the recipe selector such that it is well visible.
                        screenPoint.X = Math.Max(15, Math.Min(Width - 650, screenPoint.X));

                        _rightClickMenu.MenuItems.Clear();
                        _rightClickMenu.MenuItems.Add(new MenuItem("Add Item",
                            (o, ee) => { AddItem(screenPoint, ScreenToGraph(e.Location)); }));
                        _rightClickMenu.MenuItems.Add(new MenuItem("Add Recipe",
                            (o, ee) => {
                                AddNewNode(screenPoint, new ItemQualityPair("adding disconnected recipe"), ScreenToGraph(e.Location), NewNodeType.Disconnected);
                            }));
                        _rightClickMenu.Show(this, e.Location);
                    } else if (_currentDragOperation != DragOperation.Selection)
                        element?.MouseUp(graphLocation, e.Button, _currentDragOperation == DragOperation.Item);

                    break;

                case MouseButtons.Middle:
                    _viewBeingDragged = false;
                    break;

                // finished selecting the given zone (process selected nodes)
                case MouseButtons.Left:
                    if (_currentDragOperation == DragOperation.Selection) {
                        if ((ModifierKeys & Keys.Alt) != 0) { // removal zone processing
                            _selectedNodes.ExceptWith(_currentSelectionNodes);
                        } else {
                            if ((ModifierKeys & Keys.Control) == 0) // if we aren't using control, then we are just selecting
                                _selectedNodes.Clear();
                            _selectedNodes.UnionWith(_currentSelectionNodes);
                        }

                        _currentSelectionNodes.Clear();
                    }
                    // this is a release of a left click (non-drag operation) -> modify selection if clicking on node & using modifier keys
                    else if (_currentDragOperation == DragOperation.None && MouseDownElement is BaseNodeElement clickedNode) {
                        if ((ModifierKeys & Keys.Alt) != 0) { // remove
                            _selectedNodes.Remove(clickedNode);
                            clickedNode.Highlighted = false;
                            MouseDownElement = null;
                            Invalidate();
                        } else if ((ModifierKeys & Keys.Control) != 0) { // add if unselected, remove if selected
                            if (clickedNode.Highlighted)
                                _selectedNodes.Remove(clickedNode);
                            else
                                _selectedNodes.Add(clickedNode);

                            clickedNode.Highlighted = !clickedNode.Highlighted;
                            MouseDownElement = null;
                            Invalidate();
                        } else if (!_viewBeingDragged) { // left click without modifier keys -> pass click to node
                            clickedNode.MouseUp(graphLocation, e.Button, false);
                        }
                    } else if (!_viewBeingDragged)
                        element?.MouseUp(graphLocation, e.Button, _currentDragOperation == DragOperation.Item);


                    _currentDragOperation = DragOperation.None;
                    MouseDownElement = null;
                    break;
            }
        }

        private void ProductionGraphViewer_MouseMove(object sender, MouseEventArgs e) {
            // only care about those buttons that were pressed down on this control.
            // This is also the best place to update mouse changes done outside the control
            // (ex: clicking down, dragging outside the window, letting go, moving mouse back into window)

            _downButtons &= MouseButtons;

            var graphLocation = ScreenToGraph(e.Location);

            // don't care about element mouse move operations during selection operation
            if (_currentDragOperation != DragOperation.Selection) {
                var element = _draggedLinkElement ?? MouseDownElement;
                element?.MouseMoved(graphLocation);
            }

            switch (_currentDragOperation) {
                // check for minimal distance to be considered a drag operation
                case DragOperation.None:
                    var dragDiff = Point.Subtract(MousePosition, (Size) _mouseDownStartScreenPoint);
                    if (dragDiff.X * dragDiff.X + dragDiff.Y * dragDiff.Y > minDragDiff) {
                        if ((_downButtons & MouseButtons.Middle) == MouseButtons.Middle || (_downButtons & MouseButtons.Right) == MouseButtons.Right)
                            _viewBeingDragged = true;

                        // there is an item under the mouse during drag
                        if (MouseDownElement != null)
                            _currentDragOperation = DragOperation.Item;
                        else if ((_downButtons & MouseButtons.Left) != 0)
                            _currentDragOperation = DragOperation.Selection;
                    }

                    break;

                // dragging a group
                case DragOperation.Item:
                    if (_selectedNodes.Contains(MouseDownElement)) {
                        var startPoint = MouseDownElement.Location;
                        var element = MouseDownElement;
                        MouseDownElement.Dragged(graphLocation);
                        if (element == MouseDownElement) {
                            var endPoint = MouseDownElement.Location;
                            if (startPoint != endPoint)
                                foreach (var node in _selectedNodes.Where(node => node != MouseDownElement))
                                    node.SetLocation(new Point(node.X + endPoint.X - startPoint.X, node.Y + endPoint.Y - startPoint.Y));
                            Invalidate();
                        }
                    } else { // dragging single item
                        MouseDownElement.Dragged(graphLocation);
                        Invalidate();
                    }

                    // accept middle mouse button for view dragging purposes (while dragging item or selection)

                    if ((_downButtons & MouseButtons.Middle) == MouseButtons.Middle)
                        _viewBeingDragged = true;
                    break;

                case DragOperation.Selection:
                    _selectionZone = new Rectangle(Math.Min(_selectionZoneOriginPoint.X, graphLocation.X),
                        Math.Min(_selectionZoneOriginPoint.Y, graphLocation.Y), Math.Abs(_selectionZoneOriginPoint.X - graphLocation.X),
                        Math.Abs(_selectionZoneOriginPoint.Y - graphLocation.Y));
                    _currentSelectionNodes.Clear();
                    _currentSelectionNodes.UnionWith(_nodeElements.Where(element => element.IntersectsWithZone(_selectionZone, -20, -20)));

                    UpdateSelection();

                    // accept middle mouse button for view dragging purposes (while dragging item or selection)

                    if ((_downButtons & MouseButtons.Middle) == MouseButtons.Middle)
                        _viewBeingDragged = true;
                    break;
            }

            // dragging view (can happen during any drag operation)

            if (_viewBeingDragged) {
                // new Point(ViewOffset.X + (int)((graph_location.X - lastMouseDragPoint.X) / ViewScale), ViewOffset.Y + (int)((graph_location.Y - lastMouseDragPoint.Y) / ViewScale));
                ViewOffset = Point.Add(ViewOffset,
                    (Size) Point.Subtract(graphLocation,
                        (Size) _viewDragOriginPoint)
                );
                // only hard limit the graph bounds if we aren't dragging an object
                UpdateGraphBounds(MouseDownElement == null);
            }

            Invalidate();
        }

        private void ProductionGraphViewer_MouseWheel(object sender, MouseEventArgs e) {
            // currently have a control created within this viewer active (ex: recipe chooser) -> don't want to scroll then
            if (ContainsFocus && !Focused)
                return;

            ToolTipRenderer.ClearFloatingControls();

            var oldZoomCenter = ScreenToGraph(e.Location);

            if (e.Delta > 0)
                ViewScale *= 1.1f;
            else
                ViewScale /= 1.1f;

            ViewScale = Math.Max(ViewScale, 0.01f);
            ViewScale = Math.Min(ViewScale, 2f);

            var newZoomCenter = ScreenToGraph(e.Location);
            ViewOffset = new Point(ViewOffset.X + newZoomCenter.X - oldZoomCenter.X, ViewOffset.Y + newZoomCenter.Y - oldZoomCenter.Y);

            UpdateGraphBounds();
            Invalidate();
        }

        private void ProductionGraphViewer_KeyDown(object sender, KeyEventArgs e) {
            if (_currentDragOperation == DragOperation.None) {
                if (e.KeyCode is Keys.C or Keys.X && (e.Modifiers & Keys.Control) == Keys.Control) { // copy or cut
                    var stringBuilder = new StringBuilder();
                    var writer = new JsonTextWriter(new StringWriter(stringBuilder));

                    Graph.SerializeNodeIdSet = [];
                    Graph.SerializeNodeIdSet.UnionWith(_selectedNodes.Select(n => n.DisplayedNode.NodeId));

                    var serializer = JsonSerializer.Create();
                    serializer.Formatting = Formatting.None;
                    serializer.Serialize(writer, Graph);

                    Graph.SerializeNodeIdSet.Clear();
                    Graph.SerializeNodeIdSet = null;

                    Clipboard.SetText(stringBuilder.ToString());

                    if (e.KeyCode == Keys.X) { // cut
                        foreach (var node in _selectedNodes.ToList())
                            Graph.DeleteNode(node.DisplayedNode);
                    }
                } else if (e.KeyCode == Keys.V && (e.Modifiers & Keys.Control) == Keys.Control) { // paste
                    try {
                        var json = JObject.Parse(Clipboard.GetText());
                        ImportNodesFromJson(json, ScreenToGraph(PointToClient(Cursor.Position)), false);
                    } catch {
                        // clipboard string wasn't a proper json object, or didn't process properly. Likely answer: was a clip NOT from foreman.
                        Console.WriteLine("Non-Foreman paste detected.");
                    }
                }
            } else if (_currentDragOperation == DragOperation.Selection) // possible changes to selection type
                UpdateSelection();

            var lockDragAxis = (ModifierKeys & Keys.Shift) != 0;
            if (Grid.LockDragToAxis != lockDragAxis) {
                Grid.LockDragToAxis = lockDragAxis;
                Grid.DragOrigin = Grid.AlignToGrid(MouseDownElement?.Location ?? new Point());
                if (_currentDragOperation == DragOperation.Item)
                    MouseDownElement?.Dragged(ScreenToGraph(PointToClient(MousePosition)));
            }

            Invalidate();
        }

        private void ProductionGraphViewer_KeyUp(object sender, KeyEventArgs e) {
            if (_currentDragOperation == DragOperation.None) {
                switch (e.KeyCode) {
                    case Keys.Delete:
                        TryDeleteSelectedNodes();
                        e.Handled = true;
                        break;
                }
            } else if (_currentDragOperation == DragOperation.Selection) // possible changes to selection type
                UpdateSelection();

            var lockDragAxis = (ModifierKeys & Keys.Shift) != 0;
            if (Grid.LockDragToAxis != lockDragAxis) {
                Grid.LockDragToAxis = lockDragAxis;
                Grid.DragOrigin = Grid.AlignToGrid(MouseDownElement?.Location ?? new Point());
                if (_currentDragOperation == DragOperation.Item)
                    MouseDownElement?.Dragged(ScreenToGraph(PointToClient(MousePosition)));
            }

            Invalidate();
        }

        //----------------------------------------------Keyboard events

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) { // arrow keys to move the current selection
            var processed = true;
            var moveUnit = Grid.CurrentGridUnit > 0 ? Grid.CurrentGridUnit : 6;
            var panUnit = (int) (10 / ViewScale);
            if ((ModifierKeys & Keys.Shift) == Keys.Shift) { // large move
                moveUnit = Grid.CurrentMajorGridUnit > Grid.CurrentGridUnit ? Grid.CurrentMajorGridUnit : moveUnit * 4;
                panUnit *= 5;
            }

            switch (keyData & Keys.KeyCode) {
                case Keys.Left: {
                    foreach (var node in _selectedNodes)
                        node.SetLocation(new Point(node.X - moveUnit, node.Y));
                    break;
                }
                case Keys.Right: {
                    foreach (var node in _selectedNodes)
                        node.SetLocation(new Point(node.X + moveUnit, node.Y));
                    break;
                }
                case Keys.Up: {
                    foreach (var node in _selectedNodes)
                        node.SetLocation(new Point(node.X, node.Y - moveUnit));
                    break;
                }
                case Keys.Down: {
                    foreach (var node in _selectedNodes)
                        node.SetLocation(new Point(node.X, node.Y + moveUnit));
                    break;
                }
                case Keys.W when !_subwindowOpen:
                    ViewOffset += new Size(0, panUnit);
                    UpdateGraphBounds();
                    break;
                case Keys.A when !_subwindowOpen:
                    ViewOffset += new Size(panUnit, 0);
                    UpdateGraphBounds();
                    break;
                case Keys.S when !_subwindowOpen:
                    ViewOffset += new Size(0, -panUnit);
                    UpdateGraphBounds();
                    break;
                case Keys.D when !_subwindowOpen:
                    ViewOffset += new Size(-panUnit, 0);
                    UpdateGraphBounds();
                    break;
                default:
                    processed = false;
                    break;
            }

            if (processed) {
                Invalidate();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        //----------------------------------------------Viewpoint events

        private void BGTimer_Tick(object sender, EventArgs e) {
            //if (key)
        }

        private void ProductionGraphViewer_Resized(object sender, EventArgs e) {
            UpdateGraphBounds();
            Invalidate();
        }

        private void ProductionGraphViewer_LostFocus(object sender, EventArgs e) {
            Invalidate();
        }

        public void UpdateGraphBounds(bool limitView = true) {
            if (limitView) {
                var bounds = Graph.Bounds;
                var screenCentre = ScreenToGraph(new Point(Width / 2, Height / 2));
                if (bounds.Width == 0 || bounds.Height == 0) {
                    ViewOffset = new Point(0, 0);
                } else {
                    var newX = ViewOffset.X;
                    var newY = ViewOffset.Y;
                    if (screenCentre.X < bounds.X) {
                        newX -= bounds.X - screenCentre.X;
                    }

                    if (screenCentre.Y < bounds.Y) {
                        newY -= bounds.Y - screenCentre.Y;
                    }

                    if (screenCentre.X > bounds.X + bounds.Width) {
                        newX -= bounds.X + bounds.Width - screenCentre.X;
                    }

                    if (screenCentre.Y > bounds.Y + bounds.Height) {
                        newY -= bounds.Y + bounds.Height - screenCentre.Y;
                    }

                    ViewOffset = new Point(newX, newY);
                }
            }

            VisibleGraphBounds = new Rectangle(
                (int) (-Width / (2 * ViewScale) - ViewOffset.X),
                (int) (-Height / (2 * ViewScale) - ViewOffset.Y),
                (int) (Width / ViewScale),
                (int) (Height / ViewScale));
        }

        private void ProductionGraphViewer_Resize(object sender, EventArgs e) {
            // resize can happen before tooltip is created (due to scaling)
            ToolTipRenderer?.ClearFloatingControls();
        }

        private void ProductionGraphViewer_Leave(object sender, EventArgs e) {
            ToolTipRenderer.ClearFloatingControls();
        }

        //----------------------------------------------Helper functions (point conversions, alignment, etc)

        public Point ScreenToGraph(Point point) {
            return new Point(Convert.ToInt32((point.X - Width / 2) / ViewScale - ViewOffset.X),
                Convert.ToInt32((point.Y - Height / 2) / ViewScale - ViewOffset.Y));
        }

        public Point GraphToScreen(Point point) {
            return new Point(Convert.ToInt32((point.X + ViewOffset.X) * ViewScale + Width / 2),
                Convert.ToInt32((point.Y + ViewOffset.Y) * ViewScale + Height / 2));
        }

        //----------------------------------------------Save/Load JSON functions

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            // preset options

            info.AddValue("Version", Properties.Settings.Default.ForemanVersion);
            info.AddValue("Object", "ProductionGraphViewer");
            info.AddValue("SavedPresetName", DCache.PresetName);
            info.AddValue("IncludedMods", DCache.IncludedMods.Select(m => m.Key + "|" + m.Value));

            // graph viewer options

            info.AddValue("Unit", Graph.SelectedRateUnit);
            info.AddValue("ViewOffset", ViewOffset);
            info.AddValue("ViewScale", ViewScale);

            // graph defaults (saved here instead of within the graph since they are used here, plus they aren't used during copy/paste)

            info.AddValue("ExtraProdForNonMiners", Graph.EnableExtraProductivityForNonMiners);
            info.AddValue("AssemblerSelectorStyle", Graph.AssemblerSelector.DefaultSelectionStyle);
            info.AddValue("ModuleSelectorStyle", Graph.ModuleSelector.DefaultSelectionStyle);
            info.AddValue("FuelPriorityList", Graph.FuelSelector.FuelPriority.Select(i => i.Name));

            // enabled lists

            info.AddValue("EnabledRecipes", DCache.Recipes.Values.Where(r => r.Enabled).Select(r => r.Name));
            info.AddValue("EnabledAssemblers", DCache.Assemblers.Values.Where(a => a.Enabled).Select(a => a.Name));
            info.AddValue("EnabledModules", DCache.Modules.Values.Where(m => m.Enabled).Select(m => m.Name));
            info.AddValue("EnabledBeacons", DCache.Beacons.Values.Where(b => b.Enabled).Select(b => b.Name));

            // planting results are always enabled

            // graph :)

            info.AddValue("ProductionGraph", Graph);
        }

        public void ImportNodesFromJson(JObject json, Point origin, bool loadSolverValues) {
            // NOTE: missing items & recipes may be added here!
            var newNodeCollection = Graph.InsertNodesFromJson(DCache, json, loadSolverValues);
            if (newNodeCollection == null || newNodeCollection.NewNodes.Count == 0)
                return;

            // update the locations of the new nodes to be centered around the mouse position (as opposed to wherever they were before)

            long xAve = 0;
            long yAve = 0;
            foreach (var newNode in newNodeCollection.NewNodes) {
                xAve += newNode.Location.X;
                yAve += newNode.Location.Y;
            }

            xAve /= newNodeCollection.NewNodes.Count;
            yAve /= newNodeCollection.NewNodes.Count;

            var importCenter = new Point((int) xAve, (int) yAve);
            var offset = (Size) Grid.AlignToGrid(Point.Subtract(origin, (Size) importCenter));
            foreach (var newNode in newNodeCollection.NewNodes)
                Graph.RequestNodeController(newNode).SetLocation(Point.Add(newNode.Location, offset));

            // update the selection to be just the newly imported nodes

            ClearSelection();
            foreach (var newNodeElement in newNodeCollection.NewNodes.Select(node => _nodeElementDictionary[node])) {
                _selectedNodes.Add(newNodeElement);
                newNodeElement.Highlighted = true;
            }

            Console.WriteLine(_selectedNodes.Count);

            UpdateGraphBounds();
            Graph.UpdateNodeValues();
        }

        public void LoadPreset(Preset preset) {
            using (var form = new DataLoadForm(preset)) {
                form.StartPosition = FormStartPosition.Manual;
                form.Left = ParentForm.Left + 150;
                form.Top = ParentForm.Top + 200;
                // LOAD FACTORIO DATA
                var result = form.ShowDialog();
                DCache?.Clear();
                DCache = form.GetDataCache();
                // QUALITY UPDATE
                LastAssemblerQuality = DCache.DefaultQuality;
                Graph.DefaultAssemblerQuality = DCache.DefaultQuality;
                Graph.MaxQualitySteps = 5;
                //DCache.QualityMaxChainLength;

                if (result == DialogResult.Abort) {
                    MessageBox.Show(
                        $"The current preset ({Properties.Settings.Default.CurrentPresetName}) is corrupt. Switching to the default preset (Factorio 2.0 Vanilla)");
                    Properties.Settings.Default.CurrentPresetName = MainForm.DefaultPreset;

                    using var form2 = new DataLoadForm(new Preset(MainForm.DefaultPreset, false, true));

                    form2.StartPosition = FormStartPosition.Manual;
                    form2.Left = ParentForm.Left + 150;
                    form2.Top = ParentForm.Top + 200;
                    // LOAD default preset
                    var result2 = form2.ShowDialog();
                    DCache?.Clear();
                    DCache = form2.GetDataCache();
                    if (result2 == DialogResult.Abort)
                        MessageBox.Show($"The default preset ({Properties.Settings.Default.CurrentPresetName}) is corrupt. No Preset is loaded!");
                }

                // loaded a new data cache - the old one should be collected
                // (data caches can be over 1gb in size due to icons, plus whatever was in the old graph)
                GC.Collect();
            }

            Invalidate();
        }

        public async Task LoadFromJson(JObject json, bool useFirstPreset, bool setEnablesFromJson) {
            if (json["Version"] == null || (int) json["Version"] != Properties.Settings.Default.ForemanVersion || json["Object"] == null ||
                (string) json["Object"] != "ProductionGraphViewer") {
                json = VersionUpdater.UpdateSave(json, DCache);
                // update failed
                if (json == null)
                    return;

                VersionUpdater.UpdateGraph((JObject) json["ProductionGraph"], DCache);
            }

            // grab mod list

            var modSet = new Dictionary<string, string>();
            foreach (var str in json["IncludedMods"].Select(t => (string) t).ToList()) {
                var mod = str.Split('|');
                modSet.Add(mod[0], mod[1]);
            }

            // grab include lists

            var itemNames = json["ProductionGraph"]["IncludedItems"].Select(t => (string) t).ToList();
            var assemblerNames = json["ProductionGraph"]["IncludedAssemblers"].Select(t => (string) t).ToList();
            var qualityNames = json["ProductionGraph"]["IncludedQualities"].Select(t => (string) t["Key"]).ToList();
            var recipeShorts = RecipeShort.GetSetFromJson(json["ProductionGraph"]["IncludedRecipes"]);
            var plantShorts = PlantShort.GetSetFromJson(json["ProductionGraph"]["IncludedPlantProcesses"]);

            //now - two options:
            // a) we are told to use the first preset (basically, the selected preset) - so that is the only one added to the possible Presets
            // b) we can choose preset - so go through each one and compare mod lists - ask to continue if
            // the preset list will then be checked for compatibility based on recipes, and the one with the least errors will be used.
            // any errors will prompt a message box saying that 'incompatibility was found, but proceeding anyway'.

            var allPresets = MainForm.GetValidPresetsList();
            var presetErrors = new List<PresetErrorPackage>();
            Preset chosenPreset = null;
            if (useFirstPreset) {
                chosenPreset = allPresets[0];
            } else {
                // test for the preset specified in the json save

                var savedWPreset = allPresets.FirstOrDefault(p => p.Name == (string) json["SavedPresetName"]);
                if (savedWPreset != null) {
                    var errors = await PresetProcessor.TestPreset(savedWPreset, modSet, itemNames, assemblerNames, qualityNames, recipeShorts, plantShorts);
                    if (errors is { ErrorCount: 0 }) { // no errors found here. We will then use this exact preset and not search for a different one
                        chosenPreset = savedWPreset;
                    } else { // errors found. even though the name fits, but the preset seems to be the wrong one. Proceed with searching for best-fit
                        if (errors != null)
                            presetErrors.Add(errors);
                        allPresets.Remove(savedWPreset);
                    }
                }

                // haven't found the preset, or it returned some errors (not good) -> have to search for best fit (and leave the decision to user if we have multiple)

                if (chosenPreset == null) {
                    foreach (var preset in allPresets) {
                        var errors =
                            await PresetProcessor.TestPreset(preset, modSet, itemNames, assemblerNames, qualityNames, recipeShorts, plantShorts);
                        if (errors != null)
                            presetErrors.Add(errors);
                    }

                    // show the menu to select the preferred preset

                    using var form = new PresetSelectionForm(presetErrors);
                    form.StartPosition = FormStartPosition.Manual;
                    form.Left = ParentForm.Left + 50;
                    form.Top = ParentForm.Top + 50;

                    // null check is not necessary - if we get an ok dialogresult, we know it will be set
                    if (form.ShowDialog() != DialogResult.OK || form.ChosenPreset == null)
                        return;

                    chosenPreset = form.ChosenPreset;
                    Properties.Settings.Default.CurrentPresetName = chosenPreset.Name;
                    Properties.Settings.Default.Save();
                } else if (chosenPreset.Name != Properties.Settings.Default.CurrentPresetName) {
                    // we had to switch the preset to a new one (without the user having to select a preset from a list)

                    MessageBox.Show(
                        $"Loaded graph uses a different Preset.\nPreset switched from \"{Properties.Settings.Default.CurrentPresetName}\" to \"{chosenPreset.Name}\"");
                    Properties.Settings.Default.CurrentPresetName = chosenPreset.Name;
                    Properties.Settings.Default.Save();
                }
            }

            // clear graph

            ClearGraph();

            // load new preset

            LoadPreset(chosenPreset);

            // set up graph options

            Graph.SelectedRateUnit = (ProductionGraph.RateUnit) (int) json["Unit"];
            Graph.AssemblerSelector.DefaultSelectionStyle = (AssemblerSelector.Style) (int) json["AssemblerSelectorStyle"];
            Graph.ModuleSelector.DefaultSelectionStyle = (ModuleSelector.Style) (int) json["ModuleSelectorStyle"];

            foreach (var fuelType in json["FuelPriorityList"].Select(t => (string) t)) {
                if (DCache.Items.TryGetValue(fuelType, out var item))
                    Graph.FuelSelector.UseFuel(item);
            }

            Graph.EnableExtraProductivityForNonMiners = (bool) json["ExtraProdForNonMiners"];

            // set up graph view options

            var viewOffsetString = ((string) json["ViewOffset"]).Split(',');
            ViewOffset = new Point(int.Parse(viewOffsetString[0]), int.Parse(viewOffsetString[1]));
            ViewScale = (float) json["ViewScale"];

            // update enabled statuses

            if (setEnablesFromJson) {
                foreach (var beacon in DCache.Beacons.Values)
                    beacon.Enabled = false;
                foreach (var beacon in json["EnabledBeacons"].Select(t => (string) t).ToList())
                    if (DCache.Beacons.TryGetValue(beacon, out var cacheBeacon))
                        cacheBeacon.Enabled = true;

                foreach (var assembler in DCache.Assemblers.Values)
                    assembler.Enabled = false;
                foreach (var name in json["EnabledAssemblers"].Select(t => (string) t).ToList())
                    if (DCache.Assemblers.TryGetValue(name, out var assembler))
                        assembler.Enabled = true;
                DCache.RocketAssembler.Enabled = DCache.Assemblers["rocket-silo"]?.Enabled ?? false;

                foreach (var module in DCache.Modules.Values)
                    module.Enabled = false;
                foreach (var name in json["EnabledModules"].Select(t => (string) t).ToList())
                    if (DCache.Modules.TryGetValue(name, out var module))
                        module.Enabled = true;

                foreach (var recipe in DCache.Recipes.Values)
                    recipe.Enabled = false;
                foreach (var recipe in json["EnabledRecipes"].Select(t => (string) t).ToList())
                    if (DCache.Recipes.TryGetValue(recipe, out var cacheRecipe))
                        cacheRecipe.Enabled = true;
            }

            // add all nodes

            var collection = Graph.InsertNodesFromJson(DCache, (JObject) json["ProductionGraph"], true);

            // check for old import

            if (json["OldImport"] != null) {
                foreach (ReadOnlyRecipeNode rNode in collection.NewNodes.Where(node => node is ReadOnlyRecipeNode))
                    ((RecipeNodeController) Graph.RequestNodeController(rNode)).AutoSetAssembler(AssemblerSelector.Style.BestNonBurner);
            }

            // upgrade graph & values

            UpdateGraphBounds();
            Graph.UpdateNodeValues();
            Focus();
            Invalidate();
        }

        //Stolen from the designer file
        protected override void Dispose(bool disposing) {
            ClearGraph();


            if (disposing && components != null) {
                components.Dispose();
            }

            _rightClickMenu.Dispose();

            base.Dispose(disposing);
        }
    }
}