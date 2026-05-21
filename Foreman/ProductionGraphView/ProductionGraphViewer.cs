using Foreman.Controls;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Graph;
using Foreman.Models;
using Foreman.Models.Nodes;
using Foreman.ProductionGraphView;
using Foreman.ProductionGraphView.Annotations;
using Foreman.ProductionGraphView.Elements;
using Foreman.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public enum NewNodeType { Disconnected, Supplier, Consumer }
    public enum NodeDrawingStyle { Regular, PrintStyle, Simple, IconsOnly } //printstyle is meant for any additional chages (from regular) for exporting to image format, simple will only draw the node boxes (no icons or text) and link lines, iconsonly will draw node icons instead of nodes (for zoomed view)

    public partial class ProductionGraphViewer : UserControl {
        private enum DragOperation { None, Item, Selection, DrawShape }
        public enum LOD { Low, Medium, High } //low: only names. medium: assemblers, beacons, etc. high: include assembler percentages

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LOD LevelOfDetail { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ArrowsOnLinks { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IconsOnly { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IconsSize { get; set; }
        public int IconsDrawSize { get { return ViewScale > ((double)IconsSize / 96) ? 96 : (int)(IconsSize / ViewScale); } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int NodeCountForSimpleView { get; set; } //if the number of elements to draw is over this amount then the drawing functions will switch to simple view draws (mostly for FPS during zoomed out views)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowRecipeToolTip { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool TooltipsEnabled { get; set; }
        private bool SubwindowOpen; //used together with tooltip enabled -> if we open up an item/recipe/assembler window, this will halt tooltip show.
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DynamicLinkWidth { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool LockedRecipeEditPanelPosition { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool FlagOUSuppliedNodes { get; set; } //if true, will add a flag for over or under supplied nodes

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SmartNodeDirection { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataCache? DCache { get; set; }
        public ProductionGraph Graph { get; private set; }
        public GridManager Grid { get; private set; }
        public FloatingTooltipRenderer ToolTipRenderer { get; private set; }
        public PointingArrowRenderer ArrowRenderer { get; private set; }

        public IQuality? LastAssemblerQuality { get; private set; } //quality of the last-edited recipe's assembler (used when placing new recipe nodes)

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GraphElement? MouseDownElement { get; set; }

        public ProductionGraphSession Session { get; private set; }
        public IReadOnlyDictionary<NodeId, BaseNodeElement> NodeElementDictionary { get { return nodeElementDictionary; } }
        public IReadOnlyDictionary<LinkId, LinkElement> LinkElementDictionary { get { return linkElementDictionary; } }

        public IReadOnlyCollection<BaseNodeElement> SelectedNodes { get { return selectedNodes; } }

        public Point ViewOffset { get; private set; }
        public float ViewScale { get; private set; }
        public Rectangle VisibleGraphBounds { get; private set; }

        private const int minDragDiff = 30;
        private const int minLinkWidth = 3;
        private const int maxLinkWidth = 35;

        private static readonly Pen pausedBorders = new(Color.FromArgb(255, 80, 80), 5);
        private static readonly Pen selectionPen = new(Color.FromArgb(100, 100, 200), 2);

        private readonly Dictionary<NodeId, BaseNodeElement> nodeElementDictionary;
        private readonly List<BaseNodeElement> nodeElements;
        private readonly Dictionary<LinkId, LinkElement> linkElementDictionary;
        private readonly List<LinkElement> linkElements;
        private DraggedLinkElement? draggedLinkElement;

        private Point mouseDownStartScreenPoint;
        private MouseButtons downButtons; //we use this to ensure that any mouse operations only count if they started on this panel

        private Point ViewDragOriginPoint;
        private bool viewBeingDragged; //separate from dragOperation due to being able to drag view at all stages of dragOperation

        private DragOperation currentDragOperation = DragOperation.None;

        private Rectangle SelectionZone;
        private Point SelectionZoneOriginPoint;

        private readonly HashSet<BaseNodeElement> selectedNodes; //main list of selected nodes
        private readonly HashSet<BaseNodeElement> currentSelectionNodes; //list of nodes currently under the selection zone (which can be added/removed/replace the full list)

        private readonly ContextMenuStrip rightClickMenu = new();

        public ProductionGraphViewer() {
            InitializeComponent();
            MouseWheel += new MouseEventHandler(ProductionGraphViewer_MouseWheel);
            Resize += new EventHandler(ProductionGraphViewer_Resized);

            ViewOffset = new Point(Width / -2, Height / -2);
            ViewScale = 1f;
            NodeCountForSimpleView = 200;

            IconsOnly = false;
            IconsSize = 32;

            TooltipsEnabled = true;
            SubwindowOpen = false;

            Graph = new ProductionGraph();
            Session = new ProductionGraphSession(Graph);
            Session.Attach();
            Session.NodeViewModelAdded += Session_NodeViewModelAdded;
            Session.NodeViewModelRemoved += Session_NodeViewModelRemoved;
            Session.LinkViewModelAdded += Session_LinkViewModelAdded;
            Session.LinkViewModelRemoved += Session_LinkViewModelRemoved;
            Session.NodeValuesUpdated += (_, _) => UpdateNodeVisuals();
            Session.GraphCleared += Session_GraphCleared;

            Grid = new GridManager();
            ToolTipRenderer = new FloatingTooltipRenderer(this);
            ArrowRenderer = new PointingArrowRenderer(this);

            nodeElementDictionary = [];
            nodeElements = [];
            linkElementDictionary = [];
            linkElements = [];

            selectedNodes = [];
            currentSelectionNodes = [];

            UpdateGraphBounds();
            Invalidate();
        }

        public void ClearGraph() {
            DisposeLinkDrag();
            Graph.ClearGraph();
            //at this point every node element and link element has been removed.

            ClearAnnotations();
            selectedNodes.Clear();
            currentSelectionNodes.Clear();
        }

        public BaseNodeElement? GetNodeAtPoint(Point point) //returns first such node (in case of stacking)
        {
            //done in a 2 stage process -> first we do a rough check on the point's location (point within a node's area + 50 boundary on all sides), it goes to part 2)
            //							-> then we do a full element.containsPoint check which includes both the node and any added segments (such as item frames)

            for (int i = nodeElements.Count - 1; i >= 0; i--) {
                var roughNodeZone = new Rectangle(nodeElements[i].X - nodeElements[i].Width / 2 - 50, nodeElements[i].Y - nodeElements[i].Height / 2 - 50, nodeElements[i].Width + 100, nodeElements[i].Height + 100);
                if (roughNodeZone.Contains(point))
                    if (nodeElements[i].ContainsPoint(point))
                        return nodeElements[i];
            }
            return null;
        }

        //----------------------------------------------Adding new node functions (including link dragging) + Node edit

        public void StartLinkDrag(BaseNodeElement startNode, LinkType linkType, ItemQualityPair item) {
            draggedLinkElement?.Dispose();
            draggedLinkElement = new DraggedLinkElement(this, startNode, linkType, item);
            MouseDownElement = draggedLinkElement;
        }

        public void DisposeLinkDrag() {
            draggedLinkElement?.Dispose();
            draggedLinkElement = null;
        }

        public void AddItem(Point drawOrigin, Point newLocation) {
            if (DCache is not DataCache cache || string.IsNullOrEmpty(cache.PresetName)) {
                UserMessages.Show("The current preset (" + Properties.Settings.Default.CurrentPresetName + ") is corrupt.");
                return;
            }

            SubwindowOpen = true;
            var itemChooser = new ItemChooserPanel(this, drawOrigin);
            try {
                itemChooser.ItemRequested += (o, itemRequestArgs) => {
                    AddNewNode(drawOrigin, itemRequestArgs.Item, newLocation, NewNodeType.Disconnected);
                };
                itemChooser.PanelClosed += (o, e) => { SubwindowOpen = false; };

                itemChooser.Show();
                itemChooser = null;
            } finally {
                itemChooser?.Dispose();
            }
        }

        public void AddNewNode(Point drawOrigin, ItemQualityPair baseItem, Point newLocation, NewNodeType nNodeType, BaseNodeElement? originElement = null, bool offsetLocationToItemTabLevel = false) {
            if (DCache is not DataCache cache || string.IsNullOrEmpty(cache.PresetName)) {
                DisposeLinkDrag();
                UserMessages.Show("The current preset (" + Properties.Settings.Default.CurrentPresetName + ") is corrupt.");
                return;
            }

            if ((nNodeType != NewNodeType.Disconnected) && (originElement == null || !baseItem))
                Trace.Fail("Origin element or base item not provided for a new (linked) node");

            if (Grid.ShowGrid)
                newLocation = Grid.AlignToGrid(newLocation);

            int lastNodeWidth = 0;
            var newNodeDirection = originElement == null || !SmartNodeDirection
                ? Graph.DefaultNodeDirection
                : draggedLinkElement != null
                ? draggedLinkElement.Type != BaseLinkElement.LineType.UShape ? originElement.ViewModel.NodeDirection :
                    originElement.ViewModel.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up
                : Graph.DefaultNodeDirection;

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control) //control key pressed -> we are making a passthrough node.
            {
                ProcessNodeRequest(null, new RecipeRequestEventArgs(NodeType.Passthrough));
                DisposeLinkDrag();
                Graph.UpdateNodeStates(false);
                Invalidate();
            } else {
                var tempRange = new FRange(0, 0, true);
                if (baseItem && baseItem.Item is IFluid fluid && fluid.IsTemperatureDependent &&
                    originElement is BaseNodeElement originForTemp) {
                    if (nNodeType == NewNodeType.Consumer) //need to check all nodes down to recipes for range of temperatures being produced
                        tempRange = LinkChecker.GetTemperatureRange(fluid, originForTemp.ViewModel, LinkType.Output, true, Session);
                    else if (nNodeType == NewNodeType.Supplier) //need to check all nodes up to recipes for range of temperatures being consumed (guaranteed to be in a SINGLE [] range)
                        tempRange = LinkChecker.GetTemperatureRange(fluid, originForTemp.ViewModel, LinkType.Input, true, Session);
                }

                var recipeChooser = new RecipeChooserPanel(this, drawOrigin, baseItem, tempRange, nNodeType); //QUALITY UPDATE
                try {
                    recipeChooser.RecipeRequested += ProcessNodeRequest;
                    recipeChooser.PanelClosed += (o, e) => {
                        if (e.Option != IRChooserPanel.ChooserPanelCloseReason.RequiresItemSelection) {
                            SubwindowOpen = false;
                            DisposeLinkDrag();
                            Graph.UpdateNodeStates(false);
                            Invalidate();
                        }
                    };

                    SubwindowOpen = true;
                    recipeChooser.Show();
                    recipeChooser = null;
                } finally {
                    recipeChooser?.Dispose();
                }
            }
            return; //end of this function

            //internal helper funtion: called upon a successfull selection of a recipe-selection screen (opened above)
            void ProcessNodeRequest(object? o, RecipeRequestEventArgs recipeRequestArgs) {
                NodeId newNodeId = NodeId.Invalid;
                IItem? itemNN = baseItem.Item;
                IQuality? qualityNN = baseItem.Quality;
                if (recipeRequestArgs.NodeType != NodeType.Recipe && (itemNN is null || qualityNN is null)) {
                    Trace.Fail("ProcessNodeRequest called without a valid base item");
                    return;
                }
                switch (recipeRequestArgs.NodeType) {
                    case NodeType.Consumer:
                        newNodeId = Session.Editor.CreateConsumerNode(baseItem, newLocation);
                        FinalizeNodePosition(newNodeId);
                        break;
                    case NodeType.Supplier:
                        newNodeId = Session.Editor.CreateSupplierNode(baseItem, newLocation);
                        FinalizeNodePosition(newNodeId);
                        break;
                    case NodeType.Passthrough:
                        newNodeId = Session.Editor.CreatePassthroughNode(baseItem, newLocation);
                        FinalizeNodePosition(newNodeId);
                        break;
                    case NodeType.Spoil:
                        if (itemNN is not IItem spoilItem || qualityNN is not IQuality spoilQuality) {
                            Trace.Fail("Spoil node request without a valid base item.");
                            break;
                        }
                        if (recipeRequestArgs.Direction == NodeDirection.Up) {
                            if (spoilItem.SpoilResult is not IItem spoilOutput) {
                                Trace.Fail("Spoil node request for item with no spoil result.");
                                break;
                            }
                            newNodeId = Session.Editor.CreateSpoilNode(baseItem, spoilOutput, newLocation);
                            FinalizeNodePosition(newNodeId);
                        } else if (spoilItem.SpoilOrigins.Count == 1) {
                            newNodeId = Session.Editor.CreateSpoilNode(new ItemQualityPair(spoilItem.SpoilOrigins.ElementAt(0), spoilQuality), spoilItem, newLocation); //QUALITY UPDATE
                            FinalizeNodePosition(newNodeId);
                        } else {
                            //need to open up an item selection window to select a given spoil origin
                            SubwindowOpen = true;
                            var itemChooser = new ItemChooserPanel(this, drawOrigin, spoilItem.SpoilOrigins);
                            try {
                                itemChooser.ItemRequested += (oo, itemRequestArgs) => {
                                    if (itemRequestArgs.Item is { Item: IItem spoilOriginItem, Quality: IQuality spoilOriginQuality }) {
                                        newNodeId = Session.Editor.CreateSpoilNode(new ItemQualityPair(spoilOriginItem, spoilOriginQuality), spoilItem, newLocation);
                                        FinalizeNodePosition(newNodeId);
                                    }
                                };
                                itemChooser.PanelClosed += (oo, e) => { SubwindowOpen = false; };
                                itemChooser.Show();
                                itemChooser = null;
                            } finally {
                                itemChooser?.Dispose();
                            }
                        }
                        break;
                    case NodeType.Plant:
                        if (itemNN is not IItem plantItem || qualityNN is not IQuality plantQuality) {
                            Trace.Fail("Plant node request without a valid base item.");
                            break;
                        }
                        if (recipeRequestArgs.Direction == NodeDirection.Up) {
                            if (plantItem.PlantResult is not IPlantProcess plantProcessUp) {
                                Trace.Fail("Plant node request for item with no plant result.");
                                break;
                            }
                            newNodeId = Session.Editor.CreatePlantNode(plantProcessUp, plantQuality, newLocation);
                            FinalizeNodePosition(newNodeId);
                        } else if (plantItem.PlantOrigins.Count == 1) {
                            IItem plantOriginItem = plantItem.PlantOrigins.ElementAt(0);
                            if (plantOriginItem.PlantResult is not IPlantProcess plantProcessSingle || cache.DefaultQuality is not IQuality defaultPlantQuality) {
                                Trace.Fail("Plant origin missing process or default quality.");
                                break;
                            }
                            newNodeId = Session.Editor.CreatePlantNode(plantProcessSingle, defaultPlantQuality, newLocation); //QUALITY UPDATE
                            FinalizeNodePosition(newNodeId);
                        } else {
                            //need to open up an item selection window to select a given spoil origin
                            SubwindowOpen = true;
                            var itemChooser = new ItemChooserPanel(this, drawOrigin, plantItem.PlantOrigins);
                            try {
                                itemChooser.ItemRequested += (oo, itemRequestArgs) => {
                                    if (itemRequestArgs.Item.Item?.PlantResult is not IPlantProcess plantFromChooser || cache.DefaultQuality is not IQuality dqChooser) {
                                        Trace.Fail("Plant selection missing process or default quality.");
                                        return;
                                    }
                                    newNodeId = Session.Editor.CreatePlantNode(plantFromChooser, dqChooser, newLocation);
                                    FinalizeNodePosition(newNodeId);
                                };
                                itemChooser.PanelClosed += (oo, e) => { SubwindowOpen = false; };
                                itemChooser.Show();
                                itemChooser = null;
                            } finally {
                                itemChooser?.Dispose();
                            }
                        }
                        break;
                    case NodeType.Recipe:
                        if (!recipeRequestArgs.Recipe || recipeRequestArgs.Recipe.Recipe is not IRecipe recipeDef) {
                            Trace.Fail("Recipe request missing recipe definition.");
                            break;
                        }
                        newNodeId = Session.Editor.CreateRecipeNode(recipeRequestArgs.Recipe, newLocation);
                        if ((nNodeType == NewNodeType.Consumer && itemNN is not null && !recipeDef.IngredientSet.ContainsKey(itemNN)) ||
                            (nNodeType == NewNodeType.Supplier && itemNN is not null && !recipeDef.ProductSet.ContainsKey(itemNN)) ||
                            (nNodeType == NewNodeType.Disconnected && baseItem && itemNN is not null && !recipeDef.IngredientSet.ContainsKey(itemNN) && !recipeDef.ProductSet.ContainsKey(itemNN))) {
                            var style = Graph.AssemblerSelector.DefaultSelectionStyle switch {
                                AssemblerSelector.Style.Best or AssemblerSelector.Style.BestBurner or AssemblerSelector.Style.BestNonBurner => AssemblerSelector.Style.BestBurner,
                                _ => AssemblerSelector.Style.WorstBurner,
                            };
                            List<IAssembler> assemblerOptions = AssemblerSelector.GetOrderedAssemblerList(recipeDef, style);

                            if (Session.Editor.RequestNodeController(newNodeId) is not RecipeNodeController controller) {
                                Trace.Fail("Recipe node has no controller.");
                                break;
                            }
                            if (Graph.DefaultAssemblerQuality is not IQuality defAssyQuality) {
                                Trace.Fail("Default assembler quality is not set on the graph.");
                                break;
                            }
                            if ((nNodeType == NewNodeType.Consumer) || (nNodeType == NewNodeType.Disconnected && assemblerOptions.Any(a => a.Fuels.Contains(itemNN)))) {
                                controller.SetAssembler(new AssemblerQualityPair(assemblerOptions.First(a => a.Fuels.Contains(itemNN)), defAssyQuality));
                                controller.SetFuel(itemNN);
                            } else if (nNodeType == NewNodeType.Supplier || (nNodeType == NewNodeType.Disconnected && assemblerOptions.Any(a => a.Fuels.Contains(itemNN.FuelOrigin)))) {
                                controller.SetAssembler(new AssemblerQualityPair(assemblerOptions.First(a => a.Fuels.Contains(itemNN.FuelOrigin)), defAssyQuality));
                                controller.SetFuel(itemNN.FuelOrigin);
                            }
                        }
                        FinalizeNodePosition(newNodeId);
                        break;
                }
            }

            //internal helper funtion: once a node has been created it will be placed where it needs to be and all intermediate states (ex: dragged item line) finalized
            void FinalizeNodePosition(NodeId newNodeId) {
                //this is the offset to take into account multiple recipe additions (holding shift while selecting recipe). First node isnt shifted, all subsequent ones are 'attempted' to be spaced.
                //should be updated once the node graphics are updated (so that the node size doesnt depend as much on the text)
                BaseNodeElement newNodeElement = NodeElementDictionary[newNodeId];
                int offsetDistance = lastNodeWidth / 2;
                lastNodeWidth = newNodeElement.Width; //effectively: this recipe width
                if (offsetDistance > 0) {
                    offsetDistance += (lastNodeWidth / 2);
                    int newOffsetDistance = Grid.AlignToGrid(offsetDistance);
                    if (newOffsetDistance < offsetDistance)
                        newOffsetDistance += Grid.CurrentGridUnit;
                    offsetDistance = newOffsetDistance;
                }
                newLocation = new Point(newLocation.X + offsetDistance, newLocation.Y);

                int yoffset = offsetLocationToItemTabLevel ? (nNodeType == NewNodeType.Consumer ? -newNodeElement.Height / 2 : nNodeType == NewNodeType.Supplier ? newNodeElement.Height / 2 : 0) : 0;
                yoffset *= newNodeDirection == NodeDirection.Up ? 1 : -1;
                if (Session.Editor.RequestNodeController(newNodeId) is not BaseNodeController placementController) {
                    Trace.Fail("New node has no controller for placement.");
                    return;
                }
                placementController.SetLocation(new Point(newLocation.X, newLocation.Y + yoffset));

                if (originElement != null)
                    placementController.SetDirection(newNodeDirection);

                if (nNodeType == NewNodeType.Consumer && originElement is BaseNodeElement originConsumer)
                    Session.Editor.CreateLink(originConsumer.ViewModel.Id, newNodeId, baseItem);
                else if (nNodeType == NewNodeType.Supplier && originElement is BaseNodeElement originSupplier)
                    Session.Editor.CreateLink(newNodeId, originSupplier.ViewModel.Id, baseItem);

                DisposeLinkDrag();
                Graph.UpdateNodeValues();
                Graph.UpdateNodeStates(false);
                Invalidate();
            }
        }

        public void AddPassthroughNodesFromSelection(LinkType linkType, Size offset) {
            var newPassthroughNodes = new List<BaseNodeElement>();
            foreach (var passthroughNode in selectedNodes.OfType<PassthroughNodeElement>()) {
                var newNodeDirection = !SmartNodeDirection
                    ? Graph.DefaultNodeDirection
                    : draggedLinkElement != null
                    ? draggedLinkElement.Type != BaseLinkElement.LineType.UShape ? passthroughNode.ViewModel.NodeDirection :
                    passthroughNode.ViewModel.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up
                    : Graph.DefaultNodeDirection;

                ItemQualityPair passthroughItem = ((IPassthroughNodeViewModel)passthroughNode.ViewModel).PassthroughItem;

                int yoffset = linkType == LinkType.Input ? passthroughNode.Height / 2 : -passthroughNode.Height / 2;
                yoffset *= newNodeDirection == NodeDirection.Up ? 1 : -1;
                yoffset += offset.Height;

                NodeId newNodeId = Session.Editor.CreatePassthroughNode(passthroughItem, new Point(passthroughNode.Location.X + offset.Width, passthroughNode.Location.Y + yoffset));
                if (Session.Editor.RequestNodeController(newNodeId) is PassthroughNodeController controller)
                    controller.SetDirection(newNodeDirection);

                if (linkType == LinkType.Input)
                    Session.Editor.CreateLink(newNodeId, passthroughNode.ViewModel.Id, passthroughItem);
                else
                    Session.Editor.CreateLink(passthroughNode.ViewModel.Id, newNodeId, passthroughItem);

                if (GetNodeElement(newNodeId) is BaseNodeElement newElement)
                    newPassthroughNodes.Add(newElement);
            }
            SetSelection(newPassthroughNodes);

            DisposeLinkDrag();
            Graph.UpdateNodeStates(false);
            Invalidate();
        }

        public int AutoconnectDisconnectedInputs() {
            int linksCreated = GraphAutoconnect.ConnectDisconnectedInputs(Session);
            if (linksCreated > 0) {
                Graph.UpdateNodeStates(false);
                Invalidate();
            }
            return linksCreated;
        }

        public void TryDeleteSelectedNodes() {
            bool proceed = true;
            if (selectedNodes.Count > 10)
                proceed = (UserMessages.Show("You are deleting " + selectedNodes.Count + " nodes. \nAre you sure?", "Confirm delete.", MessageBoxButtons.YesNo) == DialogResult.Yes);
            if (proceed) {
                foreach (BaseNodeElement node in selectedNodes.ToList())
                    Session.Editor.DeleteNode(node.ViewModel.Id);
                selectedNodes.Clear();
                Graph.UpdateNodeValues();
            }
        }

        public void FlipSelectedNodes() {
            foreach (BaseNodeElement node in selectedNodes.ToList()) {
                if (Session.Editor.RequestNodeController(node.ViewModel.Id) is BaseNodeController flipController)
                    flipController.SetDirection(node.ViewModel.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up);
            }
            Invalidate();
        }

        public void SetSelectedPassthroughNodesSimpleDraw(bool simpleDraw) {
            foreach (var node in selectedNodes.OfType<PassthroughNodeElement>()) {
                if (Session.Editor.RequestNodeController(node.ViewModel.Id) is PassthroughNodeController passthroughController)
                    passthroughController.SetSimpleDraw(simpleDraw);
            }
            Invalidate();
        }

        private void PlaceFloatingPanels(Rectangle desiredBounds, params Control[] panels) =>
            EditPanelScreenLayout.ShiftControlsToFit(
                desiredBounds, ClientSize.Width, ClientSize.Height, EditPanelScreenLayout.DefaultMargin, panels);

        public void EditNode(BaseNodeElement bNodeElement) {
            if (bNodeElement is RecipeNodeElement rNodeElement) {
                EditRecipeNode(rNodeElement);
                return;
            }

            SubwindowOpen = true;
            var editPanel = new EditFlowPanel(bNodeElement.ViewModel, this);
            editPanel.ApplyViewportBounds();
            var graphAnchor = new Point(bNodeElement.X - (bNodeElement.Width / 2), bNodeElement.Y);
            Rectangle panelRect = FloatingTooltipRenderer.getTooltipScreenBounds(GraphToScreen(graphAnchor), editPanel.Size, Direction.Right);
            editPanel.Location = panelRect.Location;
            PlaceFloatingPanels(panelRect, editPanel);

            var fttc = new FloatingTooltipControl(editPanel, Direction.Right, graphAnchor, this, true, true);
            try {
                fttc.Closing += (s, e) => {
                    SubwindowOpen = false;
                    Graph.UpdateNodeValues();
                };
                if (ToolTipRenderer.ContainsTooltip(fttc))
                    fttc = null;
            } finally {
                fttc?.Dispose();
            }
        }

        public void EditRecipeNode(RecipeNodeElement rNodeElement) {
            SubwindowOpen = true;
            if (rNodeElement.ViewModel is not RecipeNodeViewModel recipeVm)
                return;
            if (recipeVm.BaseRecipe.Recipe is not IRecipe baseRecipe)
                return;
            var editPanel = new EditRecipePanel(recipeVm, this);
            editPanel.ApplyViewportBounds();
            var recipePanel = new RecipePanel([baseRecipe]);
            var leftAnchor = new Point(rNodeElement.X - (rNodeElement.Width / 2), rNodeElement.Y);
            var rightAnchor = new Point(rNodeElement.X + (rNodeElement.Width / 2), rNodeElement.Y);

            if (LockedRecipeEditPanelPosition) {
                editPanel.Location = new Point(15, 15);
                recipePanel.Location = new Point(editPanel.Location.X + editPanel.Width + 5, editPanel.Location.Y);
                var recipeTooltip = new FloatingTooltipControl(recipePanel, Direction.Left, rightAnchor, this, true, true);
                var editTooltip = new FloatingTooltipControl(editPanel, Direction.Right, leftAnchor, this, true, true);
                try {
                    editTooltip.Closing += (s, e) => { SubwindowOpen = false; rNodeElement.RequestStateUpdate(); Graph.UpdateNodeValues(); };
                    if (ToolTipRenderer.ContainsTooltip(recipeTooltip))
                        recipeTooltip = null;
                    if (ToolTipRenderer.ContainsTooltip(editTooltip))
                        editTooltip = null;
                } finally {
                    recipeTooltip?.Dispose();
                    editTooltip?.Dispose();
                }
                return;
            }

            Rectangle editRect = FloatingTooltipRenderer.getTooltipScreenBounds(GraphToScreen(leftAnchor), editPanel.Size, Direction.Right);
            Rectangle recipeRect = FloatingTooltipRenderer.getTooltipScreenBounds(GraphToScreen(rightAnchor), recipePanel.Size, Direction.Left);
            editPanel.Location = editRect.Location;
            recipePanel.Location = recipeRect.Location;
            PlaceFloatingPanels(Rectangle.Union(editRect, recipeRect), editPanel, recipePanel);

            var recipeTooltip2 = new FloatingTooltipControl(recipePanel, Direction.Left, rightAnchor, this, true, true);
            var fttc = new FloatingTooltipControl(editPanel, Direction.Right, leftAnchor, this, true, true);
            try {
                fttc.Closing += (s, e) => { SubwindowOpen = false; rNodeElement.RequestStateUpdate(); Graph.UpdateNodeValues(); };
                if (ToolTipRenderer.ContainsTooltip(recipeTooltip2))
                    recipeTooltip2 = null;
                if (ToolTipRenderer.ContainsTooltip(fttc))
                    fttc = null;
            } finally {
                recipeTooltip2?.Dispose();
                fttc?.Dispose();
            }
        }

        //----------------------------------------------Selection functions

        private void SetSelection(IEnumerable<BaseNodeElement> newSelection) {
            foreach (BaseNodeElement element in selectedNodes)
                element.Highlighted = false;

            selectedNodes.Clear();
            selectedNodes.UnionWith(newSelection);

            foreach (BaseNodeElement element in selectedNodes)
                element.Highlighted = true;
        }

        private void UpdateSelection() {
            foreach (BaseNodeElement element in nodeElements)
                element.Highlighted = false;

            if ((Control.ModifierKeys & Keys.Alt) != 0) //remove zone
            {
                foreach (BaseNodeElement selectedNode in selectedNodes)
                    selectedNode.Highlighted = true;
                foreach (BaseNodeElement newlySelectedNode in currentSelectionNodes)
                    newlySelectedNode.Highlighted = false;
            } else if ((Control.ModifierKeys & Keys.Control) != 0)  //add zone
              {
                foreach (BaseNodeElement selectedNode in selectedNodes)
                    selectedNode.Highlighted = true;
                foreach (BaseNodeElement newlySelectedNode in currentSelectionNodes)
                    newlySelectedNode.Highlighted = true;
            } else //add zone (additive with ctrl or simple selection)
              {
                foreach (BaseNodeElement newlySelectedNode in currentSelectionNodes)
                    newlySelectedNode.Highlighted = true;
            }
        }

        public void ClearSelection() {
            foreach (BaseNodeElement element in nodeElements)
                element.Highlighted = false;
            selectedNodes.Clear();
            currentSelectionNodes.Clear();
            ClearAnnotationSelection();
            Invalidate();
        }

        public void AlignSelected() {
            foreach (BaseNodeElement ne in selectedNodes)
                ne.SetLocation(Grid.AlignToGrid(ne.Location));
            Invalidate();
        }

        //----------------------------------------------Paint functions

        protected IEnumerable<GraphElement> GetPaintingOrder() {
            foreach (AnnotationElement element in annotationElements)
                yield return element;
            if (draggedLinkElement != null)
                yield return draggedLinkElement;
            foreach (LinkElement element in linkElements)
                yield return element;
            foreach (BaseNodeElement element in nodeElements)
                yield return element;
        }

        public void UpdateNodeVisuals() {
            try {
                foreach (BaseNodeElement node in nodeElements)
                    node.RequestStateUpdate();
            } catch (OverflowException ex) {
                ErrorLogging.LogException(ex, "UpdateNodeVisuals overflow while refreshing node elements");
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            e.Graphics.ResetTransform();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.Clear(this.BackColor);
            e.Graphics.TranslateTransform(Width / 2, Height / 2);
            e.Graphics.ScaleTransform(ViewScale, ViewScale);
            e.Graphics.TranslateTransform(ViewOffset.X, ViewOffset.Y);

            Paint(e.Graphics, false);
        }

        public new void Paint(Graphics graphics, bool FullGraph = false) {
            //update visibility of all elements
            if (FullGraph) {
                foreach (GraphElement element in GetPaintingOrder())
                    element.UpdateVisibility(Graph.Bounds);
                foreach (AnnotationElement ann in annotationElements)
                    ann.ForceVisible();
            } else
                foreach (GraphElement element in GetPaintingOrder())
                    element.UpdateVisibility(VisibleGraphBounds);

            //ensure width of selection is correct
            selectionPen.Width = 2 / ViewScale;

            //grid
            if (!FullGraph)
                Grid.Paint(graphics, ViewScale, VisibleGraphBounds, (currentDragOperation == DragOperation.Item) ? MouseDownElement as BaseNodeElement : null);

            //process link element widths
            if (DynamicLinkWidth) {
                double itemMax = 0;
                double fluidMax = 0;
                foreach (LinkElement element in linkElements) {
                    if (element.ConsumerElement is not BaseNodeElement consumerElement)
                        continue;
                    if (element.Item.Item is IFluid linkFluid && !linkFluid.Name.StartsWith("§§", StringComparison.Ordinal)) //§§ is the foreman added special items (currently just §§heat). ignore them
                        fluidMax = Math.Max(fluidMax, consumerElement.ViewModel.GetConsumeRate(element.Item));
                    else if (element.Item.Item is not null)
                        itemMax = Math.Max(itemMax, consumerElement.ViewModel.GetConsumeRate(element.Item));
                }
                itemMax += itemMax == 0 ? 1 : 0;
                fluidMax += fluidMax == 0 ? 1 : 0;

                foreach (LinkElement element in linkElements) {
                    element.LinkWidth = element.Item.Item is IFluid
                        ? (float)Math.Min((minLinkWidth + (maxLinkWidth - minLinkWidth) * (element.ViewModel.Throughput / fluidMax)), maxLinkWidth)
                        : (float)Math.Min((minLinkWidth + (maxLinkWidth - minLinkWidth) * (element.ViewModel.Throughput / itemMax)), maxLinkWidth);
                }
            } else {
                foreach (LinkElement element in linkElements)
                    element.LinkWidth = minLinkWidth;
            }

            //run any pre-paint functions
            foreach (GraphElement elemnent in GetPaintingOrder())
                elemnent.PrePaint();

            //paint all elements (nodes & lines)
            int visibleElements = GetPaintingOrder().Count(e => e.Visible && e is BaseNodeElement);
            foreach (GraphElement element in GetPaintingOrder())
                element.Paint(graphics, FullGraph ? NodeDrawingStyle.PrintStyle : IconsOnly ? NodeDrawingStyle.IconsOnly : (visibleElements > NodeCountForSimpleView || ViewScale < 0.2) ? NodeDrawingStyle.Simple : NodeDrawingStyle.Regular); //if viewscale is 0.2, then the text, images, etc being drawn are ~1/5th the size: aka: ~6x6 pixel images, etc. Use simple draw. Also simple draw if too many objects

            if (currentDragOperation == DragOperation.DrawShape && !FullGraph && SelectionZone.Width > 0 && SelectionZone.Height > 0) {
                drawShapePen.Width = 2 / ViewScale;
                graphics.DrawRectangle(drawShapePen, SelectionZone);
            }

            //selection zone
            if (currentDragOperation == DragOperation.Selection && !FullGraph) {
                graphics.DrawRectangle(selectionPen, SelectionZone);
                double pConsumption = currentSelectionNodes.OfType<RecipeNodeElement>().Sum(n => n.RecipeViewModel.GetTotalAssemblerElectricalConsumption() + n.RecipeViewModel.GetTotalBeaconElectricalConsumption());
                double pProduction = currentSelectionNodes.OfType<RecipeNodeElement>().Sum(n => n.RecipeViewModel.GetTotalGeneratorElectricalProduction());
                int recipeNodeCount = currentSelectionNodes.OfType<RecipeNodeElement>().Count();
                int buildingCount = (int)Math.Ceiling(currentSelectionNodes.OfType<RecipeNodeElement>().Sum(n => n.ViewModel.ActualSetValue));
                int beaconCount = currentSelectionNodes.OfType<RecipeNodeElement>().Sum(n => n.RecipeViewModel.GetTotalBeacons());

                ToolTipRenderer.AddExtraToolTip(new TooltipInfo() { Text = string.Format(DisplayCulture.Format, "Power consumption: {0}\nPower production: {1}\nRecipe count: {2}\nBuilding count: {3}\nBeacon count: {4}", GraphicsStuff.DoubleToEnergy(pConsumption, "W"), GraphicsStuff.DoubleToEnergy(pProduction, "W"), recipeNodeCount, buildingCount, beaconCount), Direction = Direction.None, ScreenLocation = new Point(10, 10) });
            }

            //everything below will be drawn directly on the screen instead of scaled/shifted based on graph
            graphics.ResetTransform();

            if (!FullGraph) {
                //warning/error arrows
                ArrowRenderer.Paint(graphics, Graph);

                //floating tooltips
                ToolTipRenderer.Paint(graphics, TooltipsEnabled && !SubwindowOpen && currentDragOperation == DragOperation.None && !viewBeingDragged);
                ToolTipRenderer.ClearExtraToolTips();

                //paused border
                if (Graph != null && Graph.PauseUpdates) //graph null check is purely for design view
                    graphics.DrawRectangle(pausedBorders, 0, 0, Width - 3, Height - 3);
            }
        }

        //----------------------------------------------Production Graph events

        public BaseNodeElement? GetNodeElement(NodeId id) =>
            nodeElementDictionary.TryGetValue(id, out BaseNodeElement? element) ? element : null;

        public LinkElement? GetLinkElement(LinkId id) =>
            linkElementDictionary.TryGetValue(id, out LinkElement? element) ? element : null;

        private void Session_GraphCleared(object? sender, EventArgs e) {
            foreach (BaseNodeElement element in nodeElements.ToList())
                element.Dispose();
            foreach (LinkElement element in linkElements.ToList())
                element.Dispose();
            nodeElementDictionary.Clear();
            nodeElements.Clear();
            linkElementDictionary.Clear();
            linkElements.Clear();
            ClearAnnotations();
            selectedNodes.Clear();
            Invalidate();
        }

        private void Session_LinkViewModelRemoved(object? sender, LinkViewModelEventArgs e) {
            if (!linkElementDictionary.TryGetValue(e.ViewModel.Id, out LinkElement? element))
                return;

            linkElementDictionary.Remove(e.ViewModel.Id);
            linkElements.Remove(element);
            element.Dispose();

            GetNodeElement(e.ViewModel.SupplierId)?.RequestStateUpdate();
            GetNodeElement(e.ViewModel.ConsumerId)?.RequestStateUpdate();
            Invalidate();
        }

        private void Session_LinkViewModelAdded(object? sender, LinkViewModelEventArgs e) {
            INodeLinkViewModel link = e.ViewModel;
            if (GetNodeElement(link.SupplierId) is not BaseNodeElement supplier ||
                GetNodeElement(link.ConsumerId) is not BaseNodeElement consumer)
                return;

            var element = new LinkElement(this, link, supplier, consumer);
            linkElementDictionary.Add(link.Id, element);
            linkElements.Add(element);

            supplier.RequestStateUpdate();
            consumer.RequestStateUpdate();
            Invalidate();
        }

        private void Session_NodeViewModelRemoved(object? sender, NodeViewModelEventArgs e) {
            if (!nodeElementDictionary.TryGetValue(e.ViewModel.Id, out BaseNodeElement? element))
                return;

            nodeElementDictionary.Remove(e.ViewModel.Id);
            nodeElements.Remove(element);
            selectedNodes.Remove(element);
            element.Dispose();
            Invalidate();
        }

        private void Session_NodeViewModelAdded(object? sender, NodeViewModelEventArgs e) {
            BaseNodeElement? element = CreateNodeElement(e.ViewModel);
            try {
                if (element is null) {
                    Trace.Fail("Unexpected node type created in graph.");
                    return;
                }
                nodeElementDictionary.Add(e.ViewModel.Id, element);
                nodeElements.Add(element);
                element = null;
                Invalidate();
            } finally {
                element?.Dispose();
            }
        }

        private BaseNodeElement? CreateNodeElement(INodeViewModel viewModel) => viewModel switch {
            ISupplierNodeViewModel supplier => new SupplierNodeElement(this, supplier),
            IConsumerNodeViewModel consumer => new ConsumerNodeElement(this, consumer),
            IPassthroughNodeViewModel passthrough => new PassthroughNodeElement(this, passthrough),
            RecipeNodeViewModel recipe => new RecipeNodeElement(this, recipe),
            ISpoilNodeViewModel spoil => new SpoilNodeElement(this, spoil),
            IPlantNodeViewModel plant => new PlantNodeElement(this, plant),
            _ => null,
        };

        //----------------------------------------------Mouse events

        private void ProductionGraphViewer_MouseDown(object? sender, MouseEventArgs e) {
            downButtons |= e.Button;

            ToolTipRenderer.ClearFloatingControls();
            ActiveControl = null; //helps panels like IRChooserPanel (for item/recipe choosing) close when we click on the graph
            foreach (IRChooserPanel chooser in Controls.OfType<IRChooserPanel>().ToArray())
                chooser.CloseIfClickOutside(e.Location);

            mouseDownStartScreenPoint = Control.MousePosition;
            Point graph_location = ScreenToGraph(e.Location);

            GraphElement? clickedElement = (GraphElement?)draggedLinkElement
                ?? (GraphElement?)GetNodeAtPoint(graph_location)
                ?? GetAnnotationAtPoint(graph_location);

            // Before element MouseDown — that sets MouseDownElement and the modal dialog can swallow MouseUp.
            if (Annotation_OnMouseDownDoubleClick(e, clickedElement))
                return;

            if (Annotation_OnMouseDown(e, graph_location, ref clickedElement))
                return;

            clickedElement?.MouseDown(graph_location, e.Button);

            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Right)) {
                ViewDragOriginPoint = graph_location;
            } else if (e.Button == MouseButtons.Left && clickedElement is not AnnotationElement) {
                SelectionZoneOriginPoint = graph_location;
                SelectionZone = new Rectangle();
                if ((Control.ModifierKeys & Keys.Control) == 0 && (Control.ModifierKeys & Keys.Alt) == 0) {
                    bool keepGroupSelection = clickedElement is BaseNodeElement clickedNode && selectedNodes.Contains(clickedNode);
                    if (!keepGroupSelection) {
                        foreach (BaseNodeElement ne in selectedNodes)
                            ne.Highlighted = false;
                        selectedNodes.Clear();
                        ClearAnnotationSelection();
                    }
                }
            }
        }

        private void ProductionGraphViewer_MouseUp(object? sender, MouseEventArgs e) {
            downButtons &= ~e.Button;

            ToolTipRenderer.ClearFloatingControls();
            Point graph_location = ScreenToGraph(e.Location);
            GraphElement? element = (GraphElement?)draggedLinkElement
                ?? (GraphElement?)GetNodeAtPoint(graph_location)
                ?? GetAnnotationAtPoint(graph_location);

            switch (e.Button) {
                case MouseButtons.Right:
                    if (viewBeingDragged)
                        viewBeingDragged = false;
                    else if (currentDragOperation == DragOperation.None && element == null) //right click on an empty space -> show add item/recipe menu
                    {
                        Point screenPoint = e.Location;

                        rightClickMenu.Items.Clear();
                        rightClickMenu.Items.Add(new ToolStripMenuItem("Add Item", null,
                            new EventHandler((o, ee) => {
                                AddItem(screenPoint, ScreenToGraph(e.Location));
                            })));
                        rightClickMenu.Items.Add(new ToolStripMenuItem("Add Recipe", null,
                            new EventHandler((o, ee) => {
                                AddNewNode(screenPoint, new ItemQualityPair(/*"adding disconnected recipe"*/), ScreenToGraph(e.Location), NewNodeType.Disconnected);
                            })));
                        Annotation_AppendContextMenuItems(graph_location);
                        rightClickMenu.Show(this, e.Location);
                    } else if (currentDragOperation != DragOperation.Selection)
                        element?.MouseUp(graph_location, e.Button, (currentDragOperation == DragOperation.Item));
                    break;
                case MouseButtons.Middle:
                    viewBeingDragged = false;
                    break;
                case MouseButtons.Left:
                    if (Annotation_FinishDrawShape()) {
                        currentDragOperation = DragOperation.None;
                        MouseDownElement = null;
                        break;
                    }

                    //finished selecting the given zone (process selected nodes)
                    if (currentDragOperation == DragOperation.Selection) {
                        if ((Control.ModifierKeys & Keys.Alt) != 0) //removal zone processing
                            selectedNodes.ExceptWith(currentSelectionNodes);
                        else {
                            if ((Control.ModifierKeys & Keys.Control) == 0) //if we arent using control, then we are just selecting
                                selectedNodes.Clear();
                            selectedNodes.UnionWith(currentSelectionNodes);
                        }
                        currentSelectionNodes.Clear();
                        CommitAnnotationLassoSelection();
                    }
                    //this is a release of a left click (non-drag operation) -> modify selection if clicking on node & using modifier keys
                    else if (currentDragOperation == DragOperation.None && MouseDownElement is BaseNodeElement clickedNode) {
                        if ((Control.ModifierKeys & Keys.Alt) != 0) //remove
                        {
                            selectedNodes.Remove(clickedNode);
                            clickedNode.Highlighted = false;
                            MouseDownElement = null;
                            Invalidate();
                        } else if ((Control.ModifierKeys & Keys.Control) != 0) //add if unselected, remove if selected
                          {
                            if (clickedNode.Highlighted)
                                selectedNodes.Remove(clickedNode);
                            else
                                selectedNodes.Add(clickedNode);

                            clickedNode.Highlighted = !clickedNode.Highlighted;
                            MouseDownElement = null;
                            Invalidate();
                        } else if (!viewBeingDragged) //left click without modifier keys -> pass click to node
                          {
                            clickedNode.MouseUp(graph_location, e.Button, false);
                        }
                    } else {
                        Annotation_OnMouseUpLeft(element, viewBeingDragged);
                        if (!viewBeingDragged)
                            element?.MouseUp(graph_location, e.Button, (currentDragOperation == DragOperation.Item));
                    }

                    currentDragOperation = DragOperation.None;
                    MouseDownElement = null;
                    break;
            }
        }

        private void ProductionGraphViewer_MouseMove(object? sender, MouseEventArgs e) {
            downButtons &= Control.MouseButtons; //only care about those buttons that were pressed down on this control. This is also the best place to update mouse changes done outside the control (ex: clicking down, dragging outside the window, letting go, moving mouse back into window)

            Point graph_location = ScreenToGraph(e.Location);

            if (currentDragOperation != DragOperation.Selection && currentDragOperation != DragOperation.DrawShape) {
                GraphElement? element = (GraphElement?)draggedLinkElement ?? MouseDownElement;
                element?.MouseMoved(graph_location);
            }

            switch (currentDragOperation) {
                case DragOperation.None: //check for minimal distance to be considered a drag operation
                    var dragDiff = Point.Subtract(Control.MousePosition, (Size)mouseDownStartScreenPoint);
                    if (dragDiff.X * dragDiff.X + dragDiff.Y * dragDiff.Y > minDragDiff) {
                        if ((downButtons & MouseButtons.Middle) == MouseButtons.Middle || (downButtons & MouseButtons.Right) == MouseButtons.Right)
                            viewBeingDragged = true;

                        if (MouseDownElement != null && !inDrawShapeMode)
                            currentDragOperation = DragOperation.Item;
                        else if ((downButtons & MouseButtons.Left) != 0)
                            currentDragOperation = inDrawShapeMode ? DragOperation.DrawShape : DragOperation.Selection;
                    }
                    break;

                case DragOperation.Item:
                    if (MouseDownElement is GraphElement dragTarget) {
                        if (dragTarget is BaseNodeElement groupDragNode && selectedNodes.Contains(groupDragNode)) {
                            Point startPoint = groupDragNode.Location;
                            GraphElement element = groupDragNode;
                            dragTarget.Dragged(graph_location);
                            if (element == groupDragNode) {
                                Point endPoint = groupDragNode.Location;
                                if (startPoint != endPoint) {
                                    int dx = endPoint.X - startPoint.X;
                                    int dy = endPoint.Y - startPoint.Y;
                                    foreach (BaseNodeElement node in selectedNodes.Where(node => node != groupDragNode))
                                        node.SetLocation(new Point(node.X + dx, node.Y + dy));
                                    foreach (AnnotationElement ann in selectedAnnotations)
                                        ann.Location = new Point(ann.X + dx, ann.Y + dy);
                                }
                                Invalidate();
                            }
                        } else if (dragTarget is AnnotationElement) {
                            Annotation_OnItemDrag(graph_location);
                            Invalidate();
                        } else {
                            dragTarget.Dragged(graph_location);
                            Invalidate();
                        }
                    }

                    //accept middle mouse button for view dragging purposes (while dragging item or selection)
                    if ((downButtons & MouseButtons.Middle) == MouseButtons.Middle)
                        viewBeingDragged = true;
                    break;

                case DragOperation.DrawShape:
                    SelectionZone = new Rectangle(
                        Math.Min(SelectionZoneOriginPoint.X, graph_location.X),
                        Math.Min(SelectionZoneOriginPoint.Y, graph_location.Y),
                        Math.Abs(SelectionZoneOriginPoint.X - graph_location.X),
                        Math.Abs(SelectionZoneOriginPoint.Y - graph_location.Y));
                    break;

                case DragOperation.Selection:
                    SelectionZone = new Rectangle(Math.Min(SelectionZoneOriginPoint.X, graph_location.X), Math.Min(SelectionZoneOriginPoint.Y, graph_location.Y), Math.Abs(SelectionZoneOriginPoint.X - graph_location.X), Math.Abs(SelectionZoneOriginPoint.Y - graph_location.Y));
                    currentSelectionNodes.Clear();
                    currentSelectionNodes.UnionWith(nodeElements.Where(element => element.IntersectsWithZone(SelectionZone, -20, -20)));
                    UpdateSelection();
                    UpdateAnnotationLassoPreview();

                    //accept middle mouse button for view dragging purposes (while dragging item or selection)
                    if ((downButtons & MouseButtons.Middle) == MouseButtons.Middle)
                        viewBeingDragged = true;
                    break;
            }

            //dragging view (can happen during any drag operation)
            if (viewBeingDragged) {
                ViewOffset = Point.Add(ViewOffset, (Size)Point.Subtract(graph_location, (Size)ViewDragOriginPoint));// new Point(ViewOffset.X + (int)((graph_location.X - lastMouseDragPoint.X) / ViewScale), ViewOffset.Y + (int)((graph_location.Y - lastMouseDragPoint.Y) / ViewScale));
                UpdateGraphBounds(MouseDownElement == null); //only hard limit the graph bounds if we arent dragging an object
            }

            Annotation_UpdateCursor(graph_location);
            Invalidate();
        }

        private void ProductionGraphViewer_MouseWheel(object? sender, MouseEventArgs e) {
            if (ContainsFocus && !this.Focused) //currently have a control created within this viewer active (ex: recipe chooser) -> dont want to scroll then
                return;

            ToolTipRenderer.ClearFloatingControls();

            Point oldZoomCenter = ScreenToGraph(e.Location);

            if (e.Delta > 0)
                ViewScale *= 1.1f;
            else
                ViewScale /= 1.1f;

            ViewScale = Math.Max(ViewScale, 0.01f);
            ViewScale = Math.Min(ViewScale, 2f);

            Point newZoomCenter = ScreenToGraph(e.Location);
            ViewOffset = new Point(ViewOffset.X + newZoomCenter.X - oldZoomCenter.X, ViewOffset.Y + newZoomCenter.Y - oldZoomCenter.Y);

            UpdateGraphBounds();
            Invalidate();
        }

        private void ProductionGraphViewer_KeyDown(object? sender, KeyEventArgs e) {
            Annotation_OnKeyDown(e);
            if (e.Handled)
                return;

            if (currentDragOperation == DragOperation.None) {
                if ((e.KeyCode == Keys.C || e.KeyCode == Keys.X) && (e.Modifiers & Keys.Control) == Keys.Control) //copy or cut
                {
                    Graph.SerializeNodeIdSet = [];
                    Graph.SerializeNodeIdSet.UnionWith(selectedNodes.Select(n => n.ViewModel.Id.Value));

                    string fragmentJson = GraphSaveCodec.WriteProductionGraphToString(Graph, writeIndented: false);

                    Graph.SerializeNodeIdSet.Clear();
                    Graph.SerializeNodeIdSet = null;

                    if (selectedAnnotations.Count > 0) {
                        fragmentJson = AnnotationClipboardCodec.MergeAnnotationsIntoFragment(
                            fragmentJson,
                            selectedAnnotations.Select(a => a.ToSaveData()));
                    }

                    Clipboard.SetText(fragmentJson);

                    if (e.KeyCode == Keys.X) {
                        foreach (BaseNodeElement node in selectedNodes.ToList())
                            Session.Editor.DeleteNode(node.ViewModel.Id);
                        foreach (AnnotationElement ann in selectedAnnotations.ToList())
                            RemoveAnnotationElement(ann);
                        selectedAnnotations.Clear();
                    }
                } else if (e.KeyCode == Keys.V && (e.Modifiers & Keys.Control) == Keys.Control) //paste
                  {
                    try {
                        ImportNodesFromFragment(Clipboard.GetText(), ScreenToGraph(PointToClient(Cursor.Position)), applySolverSettings: false);
                    } catch (Exception ex) { ErrorLogging.LogException(ex, "Non-Foreman paste or invalid clipboard JSON"); }
                }
            } else if (currentDragOperation == DragOperation.Selection) {
                UpdateSelection();
                UpdateAnnotationLassoPreview();
            }

            bool lockDragAxis = (Control.ModifierKeys & Keys.Shift) != 0;
            if (Grid.LockDragToAxis != lockDragAxis) {
                Grid.LockDragToAxis = lockDragAxis;
                Grid.DragOrigin = Grid.AlignToGrid(MouseDownElement?.Location ?? new Point());
                if (currentDragOperation == DragOperation.Item)
                    MouseDownElement?.Dragged(ScreenToGraph(PointToClient(Control.MousePosition)));
            }
            Invalidate();
        }

        private void ProductionGraphViewer_KeyUp(object? sender, KeyEventArgs e) {
            if (currentDragOperation == DragOperation.None) {
                switch (e.KeyCode) {
                    case Keys.Delete:
                        if (selectedAnnotations.Count > 0)
                            TryDeleteSelection();
                        else
                            TryDeleteSelectedNodes();
                        e.Handled = true;
                        break;
                }
            } else if (currentDragOperation == DragOperation.Selection) {
                UpdateSelection();
                UpdateAnnotationLassoPreview();
            }

            bool lockDragAxis = (Control.ModifierKeys & Keys.Shift) != 0;
            if (Grid.LockDragToAxis != lockDragAxis) {
                Grid.LockDragToAxis = lockDragAxis;
                Grid.DragOrigin = Grid.AlignToGrid(MouseDownElement?.Location ?? new Point());
                if (currentDragOperation == DragOperation.Item)
                    MouseDownElement?.Dragged(ScreenToGraph(PointToClient(Control.MousePosition)));
            }
            Invalidate();
        }

        //----------------------------------------------Keyboard events

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) //arrow keys to move the current selection
        {
            bool processed = true;
            int moveUnit = (Grid.CurrentGridUnit > 0) ? Grid.CurrentGridUnit : 6;
            int panUnit = (int)(10 / ViewScale);
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) //large move
            {
                moveUnit = (Grid.CurrentMajorGridUnit > Grid.CurrentGridUnit) ? Grid.CurrentMajorGridUnit : moveUnit * 4;
                panUnit *= 5;
            }

            if ((keyData & Keys.KeyCode) == Keys.Left) {
                Annotation_MoveSelection(-moveUnit, 0);
                foreach (BaseNodeElement node in selectedNodes)
                    node.SetLocation(new Point(node.X - moveUnit, node.Y));
            } else if ((keyData & Keys.KeyCode) == Keys.Right) {
                Annotation_MoveSelection(moveUnit, 0);
                foreach (BaseNodeElement node in selectedNodes)
                    node.SetLocation(new Point(node.X + moveUnit, node.Y));
            } else if ((keyData & Keys.KeyCode) == Keys.Up) {
                Annotation_MoveSelection(0, -moveUnit);
                foreach (BaseNodeElement node in selectedNodes)
                    node.SetLocation(new Point(node.X, node.Y - moveUnit));
            } else if ((keyData & Keys.KeyCode) == Keys.Down) {
                Annotation_MoveSelection(0, moveUnit);
                foreach (BaseNodeElement node in selectedNodes)
                    node.SetLocation(new Point(node.X, node.Y + moveUnit));
            } else if ((keyData & Keys.KeyCode) == Keys.W && !SubwindowOpen) {
                ViewOffset += new Size(0, panUnit);
                UpdateGraphBounds();
            } else if ((keyData & Keys.KeyCode) == Keys.A && !SubwindowOpen) {
                ViewOffset += new Size(panUnit, 0);
                UpdateGraphBounds();
            } else if ((keyData & Keys.KeyCode) == Keys.S && !SubwindowOpen) {
                ViewOffset += new Size(0, -panUnit);
                UpdateGraphBounds();
            } else if ((keyData & Keys.KeyCode) == Keys.D && !SubwindowOpen) {
                ViewOffset += new Size(-panUnit, 0);
                UpdateGraphBounds();
            } else
                processed = false;

            if (processed) {
                Invalidate();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        //----------------------------------------------Viewpoint events

        private void BGTimer_Tick(object? sender, EventArgs e) {
            //if (key)
        }

        private void ProductionGraphViewer_Resized(object? sender, EventArgs e) {
            UpdateGraphBounds();
            Invalidate();
        }

        private void ProductionGraphViewer_LostFocus(object? sender, EventArgs e) {
            Invalidate();
        }

        public void UpdateGraphBounds(bool limitView = true) {
            if (limitView) {
                Rectangle bounds = Graph.Bounds;
                Point screenCentre = ScreenToGraph(new Point(Width / 2, Height / 2));
                if (bounds.Width == 0 || bounds.Height == 0) {
                    ViewOffset = new Point(0, 0);
                } else {
                    int newX = ViewOffset.X;
                    int newY = ViewOffset.Y;
                    if (screenCentre.X < bounds.X) { newX -= bounds.X - screenCentre.X; }
                    if (screenCentre.Y < bounds.Y) { newY -= bounds.Y - screenCentre.Y; }
                    if (screenCentre.X > bounds.X + bounds.Width) { newX -= bounds.X + bounds.Width - screenCentre.X; }
                    if (screenCentre.Y > bounds.Y + bounds.Height) { newY -= bounds.Y + bounds.Height - screenCentre.Y; }
                    ViewOffset = new Point(newX, newY);
                }
            }

            VisibleGraphBounds = new Rectangle(
                (int)(-Width / (2 * ViewScale) - ViewOffset.X),
                (int)(-Height / (2 * ViewScale) - ViewOffset.Y),
                (int)(Width / ViewScale),
                (int)(Height / ViewScale));
        }

        private void ProductionGraphViewer_Resize(object? sender, EventArgs e) {
            ToolTipRenderer?.ClearFloatingControls(); //resize can happen before tooltip is created (due to scaling)
        }

        private void ProductionGraphViewer_Leave(object? sender, EventArgs e) {
            ToolTipRenderer.ClearFloatingControls();
        }

        //----------------------------------------------Helper functions (point conversions, alignment, etc)

        public Point ScreenToGraph(Point point) {
            return new Point(Convert.ToInt32(((point.X - Width / 2) / ViewScale) - ViewOffset.X), Convert.ToInt32(((point.Y - Height / 2) / ViewScale) - ViewOffset.Y));
        }

        public Point GraphToScreen(Point point) {
            return new Point(Convert.ToInt32(((point.X + ViewOffset.X) * ViewScale) + Width / 2), Convert.ToInt32(((point.Y + ViewOffset.Y) * ViewScale) + Height / 2));
        }

        //----------------------------------------------Save/Load JSON functions

        internal void ApplySaveUi(GraphViewerUiSaveData ui, DataCache cache, bool setEnablesFromJson) {
            Graph.SelectedRateUnit = ui.Unit;
            Graph.AssemblerSelector.DefaultSelectionStyle = ui.AssemblerSelectorStyle;
            Graph.ModuleSelector.DefaultSelectionStyle = ui.ModuleSelectorStyle;
            Graph.EnableExtraProductivityForNonMiners = ui.ExtraProdForNonMiners;
            ViewOffset = ui.ViewOffset;
            ViewScale = ui.ViewScale;

            foreach (string fuelType in ui.FuelPriorityList) {
                if (cache.Items.TryGetValue(fuelType, out IItem? fuelItem) && fuelItem is not null)
                    Graph.FuelSelector.UseFuel(fuelItem);
            }

            if (setEnablesFromJson) {
                ApplyEnabledList(cache.Beacons.Values, cache.Beacons, ui.EnabledBeacons, (b, e) => b.Enabled = e);
                ApplyEnabledList(cache.Assemblers.Values, cache.Assemblers, ui.EnabledAssemblers, (a, e) => a.Enabled = e);
                cache.RocketAssembler?.Enabled = cache.Assemblers.TryGetValue("rocket-silo", out IAssembler? silo) && silo?.Enabled == true;
                ApplyEnabledList(cache.Modules.Values, cache.Modules, ui.EnabledModules, (m, e) => m.Enabled = e);
                ApplyEnabledList(cache.Recipes.Values, cache.Recipes, ui.EnabledRecipes, (r, e) => r.Enabled = e);
            }
        }

        private static void ApplyEnabledList<T>(
            IEnumerable<T> all,
            IReadOnlyDictionary<string, T> byName,
            IReadOnlyList<string> enabledNames,
            Action<T, bool> setEnabled) where T : class {
            foreach (T item in all)
                setEnabled(item, false);
            foreach (string name in enabledNames) {
                if (byName.TryGetValue(name, out T? entry))
                    setEnabled(entry, true);
            }
        }

        public void ImportNodesFromFragment(string json, Point origin, bool applySolverSettings) {
            if (DCache is null)
                return;
            ProductionGraphSaveDocument? document = GraphSaveCodec.ReadGraphPayload(json);
            if (document is null) {
                ErrorLogging.LogLine("ImportNodesFromFragment: clipboard JSON is not a current-format graph or viewer save fragment");
                return;
            }
            ImportNodesFromDocument(document, origin, applySolverSettings);

            if (AnnotationClipboardCodec.ReadAnnotations(json) is IReadOnlyList<AnnotationSaveData> clipboardAnnotations)
                ImportAnnotationsAtOrigin(clipboardAnnotations, origin);
        }

        public void ImportNodesFromDocument(ProductionGraphSaveDocument document, Point origin, bool applySolverSettings) {
            if (DCache is not DataCache cache)
                return;

            ProductionGraph.NewNodeBatch newNodeCollection = Graph.InsertNodesFromDocument(cache, document, applySolverSettings); //NOTE: missing items & recipes may be added here!
            if (newNodeCollection == null || newNodeCollection.NewNodes.Count == 0)
                return;

            //update the locations of the new nodes to be centered around the mouse position (as opposed to wherever they were before)
            long xAve = 0;
            long yAve = 0;
            foreach (BaseNode newNode in newNodeCollection.NewNodes) {
                xAve += newNode.Location.X;
                yAve += newNode.Location.Y;
            }
            xAve /= newNodeCollection.NewNodes.Count;
            yAve /= newNodeCollection.NewNodes.Count;

            var importCenter = new Point((int)xAve, (int)yAve);
            var offset = (Size)Grid.AlignToGrid(Point.Subtract(origin, (Size)importCenter));
            foreach (BaseNode newNode in newNodeCollection.NewNodes) {
                if (Graph.RequestNodeController(newNode) is BaseNodeController importController)
                    importController.SetLocation(Point.Add(newNode.Location, offset));
            }

            //update the selection to be just the newly imported nodes
            ClearSelection();
            foreach (BaseNode importedNode in newNodeCollection.NewNodes) {
                INodeViewModel? importedVm = Session.View.Nodes.FirstOrDefault(vm => vm.Id.Value == importedNode.NodeID);
                if (importedVm is not null && nodeElementDictionary.TryGetValue(importedVm.Id, out BaseNodeElement? newNodeElement)) {
                    selectedNodes.Add(newNodeElement);
                    newNodeElement.Highlighted = true;
                }
            }
            Console.WriteLine(selectedNodes.Count);

            UpdateGraphBounds();
            Graph.UpdateNodeValues();
        }

        public void LoadPreset(Preset preset) {
            Form? ownerForm = ParentForm ?? FindForm();
            using (var form = new DataLoadForm(preset)) {
                form.StartPosition = FormStartPosition.Manual;
                if (ownerForm is not null) {
                    form.Left = ownerForm.Left + 150;
                    form.Top = ownerForm.Top + 200;
                }
                DialogResult result = form.ShowDialog(); //LOAD FACTORIO DATA
                DCache?.Clear();
                DCache = form.GetDataCache();
                if (DCache is DataCache primaryCache) {
                    LastAssemblerQuality = primaryCache.DefaultQuality; //QUALITY UPDATE
                    Graph.DefaultAssemblerQuality = primaryCache.DefaultQuality;
                    Graph.MaxQualitySteps = 5; //DCache.QualityMaxChainLength;
                }

                if (result == DialogResult.OK)
                    PresetExportFormat.ShowOutdatedWarningIfNeeded(DCache);

                if (result == DialogResult.Abort) {
                    UserMessages.Show("The current preset (" + Properties.Settings.Default.CurrentPresetName + ") is corrupt. Switching to the default preset (Factorio 2.0 Vanilla)");
                    Properties.Settings.Default.CurrentPresetName = MainForm.DefaultPreset;
                    using var form2 = new DataLoadForm(new Preset(MainForm.DefaultPreset, false, true));
                    form2.StartPosition = FormStartPosition.Manual;
                    if (ownerForm is not null) {
                        form2.Left = ownerForm.Left + 150;
                        form2.Top = ownerForm.Top + 200;
                    }
                    DialogResult result2 = form2.ShowDialog(); //LOAD default preset
                    DCache?.Clear();
                    DCache = form2.GetDataCache();
                    if (result2 == DialogResult.Abort)
                        UserMessages.Show("The default preset (" + Properties.Settings.Default.CurrentPresetName + ") is corrupt. No Preset is loaded!");
                }
                GC.Collect(); //loaded a new data cache - the old one should be collected (data caches can be over 1gb in size due to icons, plus whatever was in the old graph)
            }
            Invalidate();
        }

        private static void ShowCannotLoadSave(string message) {
            UserMessages.Show(message, "Cannot load save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public async Task ReloadGraphForCurrentPreset() {
            GraphViewerSaveDocument saveState = GraphSaveCodec.BuildViewer(this);
            await LoadFromSaveDocument(saveState, useFirstPreset: true, setEnablesFromJson: false).ConfigureAwait(false);
        }

        public async Task LoadFromJson(string json, bool useFirstPreset, bool setEnablesFromJson) {
            GraphViewerSaveDocument? saveDocument = GraphSaveCodec.ReadViewer(json);
            if (saveDocument is null) {
                await this.InvokeOnUiThreadAsync(() =>
                    ShowCannotLoadSave(
                        "This save file is too old or corrupt. Try opening it in the previous Foreman release and saving it again, then open the new file here."))
                    .ConfigureAwait(false);
                return;
            }
            await LoadFromSaveDocument(saveDocument, useFirstPreset, setEnablesFromJson).ConfigureAwait(false);
        }

        public async Task LoadFromSaveDocument(GraphViewerSaveDocument saveDocument, bool useFirstPreset, bool setEnablesFromJson) {
            Preset? chosenPreset = await ResolveChosenPresetAsync(saveDocument, useFirstPreset).ConfigureAwait(false);
            if (chosenPreset is null)
                return;

            await this.InvokeOnUiThreadAsync(() => ApplyLoadedSaveDocument(chosenPreset, saveDocument, setEnablesFromJson)).ConfigureAwait(false);
        }

        private async Task<Preset?> ResolveChosenPresetAsync(GraphViewerSaveDocument saveDocument, bool useFirstPreset) {
            ProductionGraphSaveDocument productionGraph = saveDocument.ProductionGraph;
            var modSet = new Dictionary<string, string>(saveDocument.IncludedMods);
            var itemNames = productionGraph.IncludedItems.ToList();
            var assemblerNames = productionGraph.IncludedAssemblers.ToList();
            var qualityNames = productionGraph.IncludedQualities.Select(q => q.Key).ToList();
            var recipeShorts = productionGraph.IncludedRecipes.ToList();
            var plantShorts = productionGraph.IncludedPlantProcesses.ToList();

            List<Preset>? allPresets = null;
            await this.InvokeOnUiThreadAsync(() => allPresets = MainForm.GetValidPresetsList()).ConfigureAwait(false);
            if (allPresets is null || allPresets.Count == 0)
                return null;

            var presetErrors = new List<PresetErrorPackage>();
            Preset? chosenPreset = null;
            if (useFirstPreset)
                chosenPreset = allPresets[0];
            else {
                string? savedPresetName = saveDocument.SavedPresetName;
                Preset? savedWPreset = savedPresetName is not null
                    ? allPresets.FirstOrDefault(p => p.Name == savedPresetName)
                    : null;
                if (savedWPreset is not null) {
                    PresetErrorPackage? errors = await PresetProcessor.TestPreset(savedWPreset, modSet, itemNames, qualityNames, recipeShorts, plantShorts).ConfigureAwait(false);
                    if (errors is not null && errors.ErrorCount == 0)
                        chosenPreset = savedWPreset;
                    else {
                        if (errors is not null)
                            presetErrors.Add(errors);
                        allPresets.Remove(savedWPreset);
                    }
                }

                if (chosenPreset is null) {
                    foreach (Preset preset in allPresets) {
                        PresetErrorPackage? errors = await PresetProcessor.TestPreset(preset, modSet, itemNames, qualityNames, recipeShorts, plantShorts).ConfigureAwait(false);
                        if (errors is not null)
                            presetErrors.Add(errors);
                    }

                    Preset? dialogChoice = null;
                    await this.InvokeOnUiThreadAsync(() => {
                        Form? ownerForm = ParentForm ?? FindForm();
                        using var form = new PresetSelectionForm(presetErrors);
                        form.StartPosition = FormStartPosition.Manual;
                        if (ownerForm is not null) {
                            form.Left = ownerForm.Left + 50;
                            form.Top = ownerForm.Top + 50;
                        }

                        if (form.ShowDialog() == DialogResult.OK && form.ChosenPreset is not null) {
                            dialogChoice = form.ChosenPreset;
                            Properties.Settings.Default.CurrentPresetName = dialogChoice.Name;
                            Properties.Settings.Default.Save();
                        }
                    }).ConfigureAwait(false);

                    if (dialogChoice is null)
                        return null;
                    chosenPreset = dialogChoice;
                } else if (chosenPreset.Name != Properties.Settings.Default.CurrentPresetName) {
                    string previousPresetName = Properties.Settings.Default.CurrentPresetName;
                    string newPresetName = chosenPreset.Name;
                    await this.InvokeOnUiThreadAsync(() => {
                        UserMessages.Show(string.Format(DisplayCulture.Format, "Loaded graph uses a different Preset.\nPreset switched from \"{0}\" to \"{1}\"", previousPresetName, newPresetName));
                        Properties.Settings.Default.CurrentPresetName = newPresetName;
                        Properties.Settings.Default.Save();
                    }).ConfigureAwait(false);
                }
            }

            return chosenPreset;
        }

        private void ApplyLoadedSaveDocument(Preset chosenPreset, GraphViewerSaveDocument saveDocument, bool setEnablesFromJson) {
            ProductionGraphSaveDocument productionGraph = saveDocument.ProductionGraph;

            ClearGraph();
            LoadPreset(chosenPreset);

            if (DCache is not DataCache cache) {
                ShowCannotLoadSave("The preset data could not be loaded, so the save cannot be opened.");
                return;
            }

            if (saveDocument.Ui is not null)
                ApplySaveUi(saveDocument.Ui, cache, setEnablesFromJson);

            LoadAnnotationsFromSave(saveDocument.Annotations, saveDocument.AnnotationDpi);

            ProductionGraph.NewNodeBatch collection = GraphSaveLoader.LoadProductionGraph(Graph, cache, productionGraph, applySolverSettings: true);
            if (collection.NewNodes.Count == 0 && productionGraph.Nodes.Count > 0) {
                ShowCannotLoadSave("The production graph in this save could not be loaded (nodes failed to import).");
                return;
            }

            if (saveDocument.Ui?.OldImport == true)
                foreach (RecipeNode rNode in collection.NewNodes.OfType<RecipeNode>()) {
                    if (Graph.RequestNodeController(rNode) is RecipeNodeController rnc)
                        rnc.AutoSetAssembler(AssemblerSelector.Style.BestNonBurner);
                }

            UpdateGraphBounds();
            Graph.UpdateNodeValues();
            Focus();
            Invalidate();
        }

        //Stolen from the designer file
        protected override void Dispose(bool disposing) {
            ClearGraph();


            if (disposing && (components != null)) {
                components.Dispose();
            }

            rightClickMenu.Dispose();

            base.Dispose(disposing);
        }
    }
}
