using Foreman;
using Foreman.Controls;
using Foreman.DataCaching.DataTypes;
using Foreman.DataCaching.Loading;
using Foreman.Graph;
using Foreman.Models;
using Foreman.ProductionGraphView;
using Foreman.ProductionGraphView.Elements;
using Foreman.Serialization;
using ForemanTest.Graph;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using static Foreman.ProductionGraphView.EditPanelViewportLayout;

namespace ForemanTest {
    [TestClass]
    [DoNotParallelize]
    public class ProductionGraphViewerEditTests : ForemanTestBase {
        private const int ViewerWidth = 1200;
        private const int ViewerHeight = 800;

        [TestMethod]
        public void EditNode_DoesNotChangeViewOffset_WhenPanelsWouldClip() =>
            StaTest.Run(EditNode_DoesNotChangeViewOffset_WhenPanelsWouldClip_Impl);

        [TestMethod]
        public void EditRecipeNode_DoesNotChangeViewOffset_WhenPanelsWouldClip() =>
            StaTest.Run(EditRecipeNode_DoesNotChangeViewOffset_WhenPanelsWouldClip_Impl);

        [TestMethod]
        public void FloatingTooltipControl_UseControlLocation_PreservesPreplacedPanelLocation() =>
            StaTest.Run(FloatingTooltipControl_UseControlLocation_PreservesPreplacedPanelLocation_Impl);

        [TestMethod]
        public void AddDisconnectedRecipe_CreatesRecipeNode_WithoutBaseItem() =>
            StaTest.Run(AddDisconnectedRecipe_CreatesRecipeNode_WithoutBaseItem_Impl);

        [TestMethod]
        public void ItemChooser_ClosesOnGraphClick_AndSelectsWithoutException() =>
            StaTest.Run(ItemChooser_ClosesOnGraphClick_AndSelectsWithoutException_Impl);

        [TestMethod]
        public void ItemChooser_SizeMatchesContentAfterShow() =>
            StaTest.Run(ItemChooser_SizeMatchesContentAfterShow_Impl);

        [TestMethod]
        public void ItemChooser_StaysFullyVisibleWhenOpenedNearViewerEdge() =>
            StaTest.Run(ItemChooser_StaysFullyVisibleWhenOpenedNearViewerEdge_Impl);

        [TestMethod]
        public void RecipeChooser_FooterButtonsFitPanelWidth() =>
            StaTest.Run(RecipeChooser_FooterButtonsFitPanelWidth_Impl);

        [TestMethod]
        public void RecipeChooser_FooterButtonsScaleWithViewerSize() =>
            StaTest.Run(RecipeChooser_FooterButtonsScaleWithViewerSize_Impl);

        [TestMethod]
        public void RecipeChooser_HeaderAndFooterControlsFitPanelWidth() =>
            StaTest.Run(RecipeChooser_HeaderAndFooterControlsFitPanelWidth_Impl);

        [TestMethod]
        public void RecipeChooser_RecipeOnlyCheckboxFitsPanelWidthOnShortViewer() =>
            StaTest.Run(RecipeChooser_RecipeOnlyCheckboxFitsPanelWidthOnShortViewer_Impl);

        [TestMethod]
        public void RecipeChooser_ManyGroupsFitsViewportWithFooterVisible() =>
            StaTest.Run(RecipeChooser_ManyGroupsFitsViewportWithFooterVisible_Impl);

        [TestMethod]
        public void EditRecipePanel_HeightFitsViewerAndScrollsWhenContentIsTaller() =>
            StaTest.Run(EditRecipePanel_HeightFitsViewerAndScrollsWhenContentIsTaller_Impl);

        [TestMethod]
        public void EditRecipePanel_VerticalScrollbarDoesNotOverlapContent() =>
            StaTest.Run(EditRecipePanel_VerticalScrollbarDoesNotOverlapContent_Impl);

        [TestMethod]
        public void EditFlowPanel_HeightFitsShortViewer() =>
            StaTest.Run(EditFlowPanel_HeightFitsShortViewer_Impl);

        [TestMethod]
        public void MouseDown_OnAlreadySelectedNode_PreservesMultiSelection() =>
            StaTest.Run(MouseDown_OnAlreadySelectedNode_PreservesMultiSelection_Impl);

        [TestMethod]
        public void MouseDown_OnUnselectedNode_ClearsExistingSelection() =>
            StaTest.Run(MouseDown_OnUnselectedNode_ClearsExistingSelection_Impl);

        private static void EditNode_DoesNotChangeViewOffset_WhenPanelsWouldClip_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(120, 300));
            Point viewBefore = viewer.ViewOffset;

            NodeId id = viewer.Session.Editor.CreateSupplierNode(ctx.Item("iron"), new Point(0, 420));
            Assert.IsTrue(viewer.NodeElementDictionary.TryGetValue(id, out BaseNodeElement? element));
            Assert.IsNotNull(element);

            try {
                viewer.EditNode(element);
                Assert.AreEqual(viewBefore, viewer.ViewOffset, "Opening a flow edit panel must not pan the graph viewport.");
                AssertFloatingPanelsOnScreen(viewer);
            } finally {
                viewer.ToolTipRenderer.ClearFloatingControls();
            }
        }

        private static void EditRecipeNode_DoesNotChangeViewOffset_WhenPanelsWouldClip_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(80, 250));
            NodeId recipeId = CreateTestRecipeNode(ctx, viewer, new Point(0, 450));
            Assert.IsTrue(viewer.NodeElementDictionary.TryGetValue(recipeId, out BaseNodeElement? element));
            Assert.IsInstanceOfType<RecipeNodeElement>(element);
            Assert.IsNotNull(element);

            Point viewBefore = viewer.ViewOffset;
            try {
                viewer.EditRecipeNode((RecipeNodeElement)element);
                Assert.AreEqual(viewBefore, viewer.ViewOffset, "Opening recipe edit panels must not pan the graph viewport.");
                AssertFloatingPanelsOnScreen(viewer);
            } finally {
                viewer.ToolTipRenderer.ClearFloatingControls();
            }
        }

        private static void FloatingTooltipControl_UseControlLocation_PreservesPreplacedPanelLocation_Impl() {
            using var viewer = new ProductionGraphViewer { Size = new Size(ViewerWidth, ViewerHeight) };
            using var panel = new Panel { Size = new Size(200, 100), Location = new Point(30, 40) };
            Point expected = panel.Location;
            var tooltip = new FloatingTooltipControl(panel, Direction.Right, new Point(0, 0), viewer, showOverride: true, useControlLocation: true);
            try {
                Assert.AreEqual(expected, panel.Location,
                    "Edit panels must keep their clamped screen position; the tooltip must not re-layout over them.");
            } finally {
                tooltip.Dispose();
            }
        }

        private static void AddDisconnectedRecipe_CreatesRecipeNode_WithoutBaseItem_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            TestDataCacheHelper.SetPresetName(ctx.Cache, "test-preset");
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));
            var disconnectedRecipeAnchor = new ItemQualityPair(/*"adding disconnected recipe"*/);
            Assert.IsFalse(disconnectedRecipeAnchor, "Add IRecipe uses an empty item-quality sentinel.");

            try {
                viewer.AddNewNode(new Point(10, 10), disconnectedRecipeAnchor, new Point(200, 150), NewNodeType.Disconnected);
                RecipeChooserPanel? chooser = viewer.Controls.OfType<RecipeChooserPanel>().FirstOrDefault();
                Assert.IsNotNull(chooser, "AddNewNode should open a recipe chooser for disconnected placement.");
                Assert.IsTrue(chooser.Width >= 200 && chooser.Height >= 200,
                    "Recipe chooser should have a visible size after Show().");
                Assert.IsTrue(chooser.Visible && !chooser.IsDisposed,
                    "Recipe chooser should stay open after focusing the filter box.");

                RecipePrototype recipe = CreateTestRecipeDefinition(ctx);
                int nodesBefore = viewer.Session.View.Nodes.Count;

                SelectRecipeInChooser(chooser, recipe);

                Assert.HasCount(nodesBefore + 1, viewer.Session.View.Nodes,
                    "Selecting a recipe from the disconnected chooser should add a recipe node.");
                Assert.IsNotEmpty(viewer.Session.View.Nodes.OfType<IRecipeNodeViewModel>(),
                    "The new node should be a recipe view model.");
            } finally {
                viewer.ToolTipRenderer.ClearFloatingControls();
            }
        }

        private static void EditRecipePanel_HeightFitsViewerAndScrollsWhenContentIsTaller_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));
            viewer.Size = new Size(900, 320);
            NodeId recipeId = CreateTestRecipeNode(ctx, viewer, new Point(0, 200));
            Assert.IsTrue(viewer.NodeElementDictionary.TryGetValue(recipeId, out BaseNodeElement? element));
            Assert.IsInstanceOfType<RecipeNodeElement>(element);

            try {
                viewer.EditRecipeNode((RecipeNodeElement)element);
                EditRecipePanel? editPanel = viewer.Controls.OfType<EditRecipePanel>().FirstOrDefault();
                Assert.IsNotNull(editPanel);

                int maxHeight = viewer.ClientSize.Height - EditPanelScreenLayout.DefaultMargin * 2;
                Assert.IsLessThanOrEqualTo(maxHeight, editPanel.Height,
                    $"Edit panel height {editPanel.Height} should not exceed viewer chrome ({maxHeight}px).");

                Panel? scrollHost = editPanel.Controls.Find(ScrollHostName, false).OfType<Panel>().FirstOrDefault();
                Assert.IsNotNull(scrollHost, "Edit recipe panel should host content in a scroll viewport.");
                Assert.IsTrue(scrollHost.AutoScroll);

                Control content = scrollHost.Controls[0];
                Assert.IsGreaterThan(scrollHost.ClientSize.Height, content.Height,
                    "Recipe editor content should remain full height inside the capped viewport.");
                Assert.IsTrue(scrollHost.VerticalScroll.Visible,
                    "A vertical scrollbar should appear when recipe editor content exceeds the viewer height.");
                Assert.IsLessThanOrEqualTo(scrollHost.ClientSize.Width, content.Width,
                    "Recipe editor content should fit the viewport width beside the vertical scrollbar.");
            } finally {
                viewer.ToolTipRenderer.ClearFloatingControls();
            }
        }

        private static void EditRecipePanel_VerticalScrollbarDoesNotOverlapContent_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));
            viewer.Size = new Size(900, 320);
            NodeId recipeId = CreateTestRecipeNode(ctx, viewer, new Point(0, 200));
            Assert.IsTrue(viewer.NodeElementDictionary.TryGetValue(recipeId, out BaseNodeElement? element));
            Assert.IsInstanceOfType<RecipeNodeElement>(element);

            try {
                viewer.EditRecipeNode((RecipeNodeElement)element);
                EditRecipePanel? editPanel = viewer.Controls.OfType<EditRecipePanel>().FirstOrDefault();
                Assert.IsNotNull(editPanel);

                Panel scrollHost = editPanel.Controls.Find(ScrollHostName, false).OfType<Panel>().First();
                Control content = scrollHost.Controls[0];
                int naturalContentWidth = MeasureUnconstrainedContentWidth(content);
                AssertVerticalScrollbarDoesNotOverlapContent(editPanel, scrollHost, content, naturalContentWidth);
            } finally {
                viewer.ToolTipRenderer.ClearFloatingControls();
            }
        }

        private static void EditFlowPanel_HeightFitsShortViewer_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));
            viewer.Size = new Size(900, 280);
            NodeId id = viewer.Session.Editor.CreateSupplierNode(ctx.Item("iron"), new Point(0, 200));
            Assert.IsTrue(viewer.NodeElementDictionary.TryGetValue(id, out BaseNodeElement? element));
            Assert.IsNotNull(element);

            try {
                viewer.EditNode(element);
                EditFlowPanel? editPanel = viewer.Controls.OfType<EditFlowPanel>().FirstOrDefault();
                Assert.IsNotNull(editPanel);

                int maxHeight = viewer.ClientSize.Height - EditPanelScreenLayout.DefaultMargin * 2;
                Assert.IsLessThanOrEqualTo(maxHeight, editPanel.Height,
                    $"Flow edit panel height {editPanel.Height} should fit inside the graph viewer.");
                Assert.IsNotNull(editPanel.Controls.Find(ScrollHostName, false).FirstOrDefault());
            } finally {
                viewer.ToolTipRenderer.ClearFloatingControls();
            }
        }

        private static void ItemChooser_StaysFullyVisibleWhenOpenedNearViewerEdge_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            TestDataCacheHelper.SetPresetName(ctx.Cache, "test-preset");
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));
            viewer.Size = new Size(520, 560);

            viewer.AddItem(new Point(460, 500), new Point(200, 150));
            AssertFloatingPanelsOnScreen(viewer);
        }

        private static void SeedManyChooserGroups(GraphSessionTestHelper.TestContext ctx, int groupCount) {
            DataCacheStore store = TestDataCacheHelper.RequireStore(ctx.Cache);
            AssemblerPrototype assembler = TestPrototypeFactory.CreateTestAssembler(ctx.Cache);
            for (int i = 0; i < groupCount; i++) {
                var group = new GroupPrototype(ctx.Cache, $"§§t:g{i}", $"G{i}", $"{i:D4}");
                var subgroup = new SubgroupPrototype(ctx.Cache, $"§§t:s{i}", "0") { MyGroupInternal = group };
                group.SubgroupsInternal.Add(subgroup);
                ItemPrototype item = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, $"t-item{i}");
                var recipe = new RecipePrototype(ctx.Cache, $"§§t:r{i}", $"R{i}", subgroup, "0");
                TestPrototypeFactory.SetRecipeTime(recipe, 1);
                TestPrototypeFactory.LinkRecipeAndAssembler(recipe, assembler);
                recipe.InternalOneWayAddIngredient(item, 1);
                recipe.InternalOneWayAddProduct(item, 1, 0);
                item.ConsumptionRecipesInternal.Add(recipe);
                item.ProductionRecipesInternal.Add(recipe);
                subgroup.ItemsInternal.Add(item);
                subgroup.RecipesInternal.Add(recipe);
                store.Groups[group.Name] = group;
                store.Subgroups[subgroup.Name] = subgroup;
                TestDataCacheHelper.RegisterRecipe(ctx.Cache, recipe);
            }
        }

        private static void RecipeChooser_ManyGroupsFitsViewportWithFooterVisible_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            SeedManyChooserGroups(ctx, 24);
            TestDataCacheHelper.SetPresetName(ctx.Cache, "test-preset");
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));
            viewer.CreateControl();
            viewer.Size = new Size(900, 380);
            viewer.PerformLayout();

            viewer.AddItem(new Point(20, 20), new Point(200, 150));
            viewer.PerformLayout();
            Application.DoEvents();

            ItemChooserPanel? itemChooser = viewer.Controls.OfType<ItemChooserPanel>().FirstOrDefault();
            Assert.IsNotNull(itemChooser);
            SelectItemInChooser(itemChooser, ctx.Item("t-item0"));

            RecipeChooserPanel? chooser = viewer.Controls.OfType<RecipeChooserPanel>().FirstOrDefault();
            Assert.IsNotNull(chooser);
            MethodInfo? refreshBounds = typeof(IRChooserPanel).GetMethod("RefreshViewerBounds", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(refreshBounds);
            refreshBounds.Invoke(chooser, null);
            Application.DoEvents();

            const int margin = EditPanelScreenLayout.DefaultMargin;
            int maxPanelHeight = viewer.ClientSize.Height - margin * 2;
            Assert.IsTrue(chooser.Height <= maxPanelHeight,
                $"Recipe chooser height {chooser.Height} should fit a {viewer.ClientSize.Height}px-tall viewer (max content {maxPanelHeight}px).");

            Button passThrough = GetChooserButton(chooser, "AddPassthroughButton");
            Assert.IsTrue(passThrough.Visible);
            Assert.IsTrue(passThrough.Bottom <= chooser.ClientSize.Height,
                $"Pass-Through bottom ({passThrough.Bottom}) should be inside the panel ({chooser.ClientSize.Height}px).");

            FlowLayoutPanel groups = GetGroupsPanel(chooser);
            Assert.IsTrue(groups.Visible, "Recipe chooser should show category groups when the item has matching recipes.");
            Assert.IsTrue(groups.Height > 20,
                $"With many categories, groups panel height {groups.Height} should show a category strip (or scroll when capped).");
            Assert.IsTrue(groups.Controls.Count >= 20,
                "Seeded mod-style groups should populate the category strip.");

            AssertFloatingPanelsOnScreen(viewer);
        }

        private static void SelectItemInChooser(ItemChooserPanel chooser, ItemQualityPair item) {
            MethodInfo? mouseUp = typeof(ItemChooserPanel).GetMethod("IRButtonMouseUp", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(mouseUp);
            using var button = new Button { Tag = item.Item };
            mouseUp.Invoke(chooser, [button, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)]);
        }

        private static FlowLayoutPanel GetGroupsPanel(IRChooserPanel chooser) {
            FieldInfo? field = typeof(IRChooserPanel).GetField("groupsPanel", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            return (FlowLayoutPanel)field.GetValue(chooser)!;
        }

        private static void RecipeChooser_HeaderAndFooterControlsFitPanelWidth_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            TestDataCacheHelper.SetPresetName(ctx.Cache, "test-preset");
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));

            NodeId supplierId = viewer.Session.Editor.CreateSupplierNode(ctx.Item("iron"), new Point(100, 100));
            Assert.IsTrue(viewer.NodeElementDictionary.TryGetValue(supplierId, out BaseNodeElement? supplier));
            Assert.IsNotNull(supplier);

            viewer.AddNewNode(new Point(10, 10), ctx.Item("iron"), new Point(300, 100), NewNodeType.Consumer, supplier);
            RecipeChooserPanel? chooser = viewer.Controls.OfType<RecipeChooserPanel>().FirstOrDefault();
            Assert.IsNotNull(chooser);

            Button passThrough = GetChooserButton(chooser, "AddPassthroughButton");
            CheckBox showHidden = GetChooserCheckBox(chooser, "ShowHiddenCheckBox");
            Assert.IsTrue(passThrough.Visible);
            Assert.IsLessThanOrEqualTo(chooser.ClientSize.Width, passThrough.Right,
                $"Pass-Through button right ({passThrough.Right}) should fit in panel width ({chooser.ClientSize.Width}).");
            Assert.IsLessThanOrEqualTo(chooser.ClientSize.Width, showHidden.Right,
                $"Show-hidden checkbox right ({showHidden.Right}) should fit in panel width ({chooser.ClientSize.Width}).");
            AssertFloatingPanelsOnScreen(viewer);
        }

        private static CheckBox GetChooserCheckBox(IRChooserPanel chooser, string name) {
            FieldInfo? field = typeof(IRChooserPanel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"IRChooserPanel.{name} field should exist.");
            return (CheckBox)field.GetValue(chooser)!;
        }

        private static void RecipeChooser_RecipeOnlyCheckboxFitsPanelWidthOnShortViewer_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            TestDataCacheHelper.SetPresetName(ctx.Cache, "test-preset");
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));
            viewer.Size = new Size(600, 280);

            var disconnectedAnchor = new ItemQualityPair();
            viewer.AddNewNode(new Point(10, 10), disconnectedAnchor, new Point(300, 100), NewNodeType.Disconnected);
            RecipeChooserPanel? chooser = viewer.Controls.OfType<RecipeChooserPanel>().FirstOrDefault();
            Assert.IsNotNull(chooser);

            MethodInfo? refreshBounds = typeof(IRChooserPanel).GetMethod("RefreshViewerBounds", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(refreshBounds);
            refreshBounds.Invoke(chooser, null);

            CheckBox recipeOnly = GetChooserCheckBox(chooser, "RecipeNameOnlyFilterCheckBox");
            Assert.IsTrue(recipeOnly.Visible, "Recipe Only filter should be shown on the full recipe chooser.");
            Assert.IsLessThanOrEqualTo(chooser.ClientSize.Width, recipeOnly.Right,
                $"Recipe Only checkbox right ({recipeOnly.Right}) should fit in panel width ({chooser.ClientSize.Width}) on a short viewer.");
            AssertFloatingPanelsOnScreen(viewer);
        }

        private static void RecipeChooser_FooterButtonsScaleWithViewerSize_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            TestDataCacheHelper.SetPresetName(ctx.Cache, "test-preset");
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));

            NodeId supplierId = viewer.Session.Editor.CreateSupplierNode(ctx.Item("iron"), new Point(100, 100));
            Assert.IsTrue(viewer.NodeElementDictionary.TryGetValue(supplierId, out BaseNodeElement? supplier));
            Assert.IsNotNull(supplier);

            viewer.AddNewNode(new Point(10, 10), ctx.Item("iron"), new Point(300, 100), NewNodeType.Consumer, supplier);
            RecipeChooserPanel? chooser = viewer.Controls.OfType<RecipeChooserPanel>().FirstOrDefault();
            Assert.IsNotNull(chooser);

            MethodInfo? refreshBounds = typeof(IRChooserPanel).GetMethod("RefreshViewerBounds", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(refreshBounds);

            Button passThrough = GetChooserButton(chooser, "AddPassthroughButton");
            Assert.IsTrue(passThrough.Visible);

            int designFooter = ChooserLayout.Scale(chooser, ChooserLayout.DesignFooterButtonHeightPixels);
            int minFooter = ChooserLayout.Scale(chooser, ChooserLayout.DesignMinFooterButtonHeightPixels);
            int designCell = ChooserLayout.Scale(chooser, ChooserLayout.DesignCellPixels);
            float designFont = passThrough.Font.Size;
            float minFont = designFont * ChooserLayout.DesignMinFooterButtonFontSizePoints
                / ChooserLayout.DesignFooterButtonFontSizePoints;

            (int Width, int Height)[] viewerSizes = [(1200, 800), (600, 360), (320, 240)];
            int[] buttonHeights = new int[viewerSizes.Length];
            float[] fontSizes = new float[viewerSizes.Length];
            int[] cellSizes = new int[viewerSizes.Length];
            for (int i = 0; i < viewerSizes.Length; i++) {
                (int viewerWidth, int viewerHeight) = viewerSizes[i];
                viewer.Size = new Size(viewerWidth, viewerHeight);
                refreshBounds.Invoke(chooser, null);

                ChooserIconGrid iconGrid = GetChooserIconGrid(chooser);
                int cellSize = iconGrid.TargetCellSize;
                int expectedHeight = ChooserLayout.FooterButtonHeightForCell(cellSize, designFooter, minFooter);
                float expectedFont = ChooserLayout.FooterButtonFontSizeForCell(cellSize, designCell, designFont, minFont);
                cellSizes[i] = cellSize;
                buttonHeights[i] = passThrough.Height;
                fontSizes[i] = passThrough.Font.Size;

                Assert.IsFalse(passThrough.AutoSize,
                    $"At viewer {viewerWidth}x{viewerHeight}, footer buttons should use explicit layout sizing.");
                Assert.AreEqual(expectedHeight, passThrough.Height,
                    $"At viewer {viewerWidth}x{viewerHeight}, footer height should track icon cell size {cellSize}px.");
                Assert.AreEqual(expectedFont, passThrough.Font.Size, 0.05f,
                    $"At viewer {viewerWidth}x{viewerHeight}, footer font should track icon cell size {cellSize}px.");
                Assert.IsLessThanOrEqualTo(chooser.ClientSize.Height, passThrough.Bottom,
                    $"At viewer {viewerWidth}x{viewerHeight}, Pass-Through bottom ({passThrough.Bottom}) should fit panel height ({chooser.ClientSize.Height}).");
                Assert.IsLessThanOrEqualTo(chooser.ClientSize.Width, passThrough.Right,
                    $"At viewer {viewerWidth}x{viewerHeight}, Pass-Through right ({passThrough.Right}) should fit panel width ({chooser.ClientSize.Width}).");
            }

            Assert.IsTrue(cellSizes[0] >= cellSizes[^1],
                $"Icon cell size should not grow on shorter viewers (tall={cellSizes[0]}px, short={cellSizes[^1]}px).");
            Assert.IsTrue(buttonHeights[0] >= buttonHeights[^1],
                $"Footer button height should track cell shrink (tall={buttonHeights[0]}px, short={buttonHeights[^1]}px).");
            Assert.IsTrue(cellSizes[0] > cellSizes[^1] && buttonHeights[0] > buttonHeights[^1],
                $"Short viewer should shrink icon cells and footer buttons (cells {cellSizes[0]}→{cellSizes[^1]}, buttons {buttonHeights[0]}→{buttonHeights[^1]}).");
            Assert.IsTrue(fontSizes[0] > fontSizes[^1],
                $"Footer font should shrink on shorter viewers (tall={fontSizes[0]}, short={fontSizes[^1]}).");
            AssertFloatingPanelsOnScreen(viewer);
        }

        private static ChooserIconGrid GetChooserIconGrid(IRChooserPanel chooser) {
            FieldInfo? field = typeof(IRChooserPanel).GetField("iconGrid", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "IRChooserPanel.iconGrid field should exist.");
            return (ChooserIconGrid)field.GetValue(chooser)!;
        }

        private static void RecipeChooser_FooterButtonsFitPanelWidth_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            TestDataCacheHelper.SetPresetName(ctx.Cache, "test-preset");
            var seed = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "seed");
            var spoiled = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "spoiled");
            GraphSessionTestHelper.WireSpoilChain(seed, spoiled, ctx.Quality);
            GraphSessionTestHelper.CreatePlantProcess(ctx, "seed", "crop");

            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));
            ItemQualityPair keyItem = ctx.Item("seed");
            viewer.AddNewNode(new Point(10, 10), keyItem, new Point(200, 150), NewNodeType.Disconnected);

            RecipeChooserPanel? chooser = viewer.Controls.OfType<RecipeChooserPanel>().FirstOrDefault();
            Assert.IsNotNull(chooser);

            FlowLayoutPanel rowB = GetNodeOptionsRowB(chooser);
            Button addPlant = GetChooserButton(chooser, "AddPlantButton");
            Assert.IsTrue(addPlant.Visible, "Plant button should be visible for a plantable seed item.");
            Assert.IsLessThanOrEqualTo(rowB.ClientSize.Width, addPlant.Right,
                $"AddPlantButton right edge ({addPlant.Right}) should fit inside row B width ({rowB.ClientSize.Width}).");
            AssertFloatingPanelsOnScreen(viewer);
        }

        private static FlowLayoutPanel GetNodeOptionsRowB(IRChooserPanel chooser) {
            FieldInfo? field = typeof(IRChooserPanel).GetField("nodeOptionsRowB", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "IRChooserPanel.nodeOptionsRowB field should exist.");
            return (FlowLayoutPanel)field.GetValue(chooser)!;
        }

        private static Button GetChooserButton(IRChooserPanel chooser, string name) {
            FieldInfo? field = typeof(IRChooserPanel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"IRChooserPanel.{name} field should exist.");
            return (Button)field.GetValue(chooser)!;
        }

        private static void ItemChooser_SizeMatchesContentAfterShow_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            TestDataCacheHelper.SetPresetName(ctx.Cache, "test-preset");
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));

            viewer.AddItem(new Point(10, 10), new Point(200, 150));
            viewer.PerformLayout();
            Application.DoEvents();
            ItemChooserPanel? chooser = viewer.Controls.OfType<ItemChooserPanel>().FirstOrDefault();
            Assert.IsNotNull(chooser);

            const int margin = EditPanelScreenLayout.DefaultMargin;
            int maxPanelHeight = viewer.ClientSize.Height - margin * 2;
            Assert.IsTrue(chooser.Width >= 200 && chooser.Height >= 200,
                "Chooser should have a usable size after Show().");
            Assert.IsTrue(chooser.Height <= maxPanelHeight,
                $"Chooser height {chooser.Height} should not exceed the graph viewer viewport ({maxPanelHeight}px).");
            Assert.IsNull(chooser.Controls.Find(EditPanelViewportLayout.ScrollHostName, false).FirstOrDefault(),
                "Chooser should shrink to fit the viewport instead of using a panel scrollbar.");
            Assert.AreEqual(chooser.Size, chooser.MaximumSize,
                "Chooser should size tightly to its content.");
            AssertFloatingPanelsOnScreen(viewer);
        }

        private static void MouseDown_OnAlreadySelectedNode_PreservesMultiSelection_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));

            NodeId id1 = viewer.Session.Editor.CreateSupplierNode(ctx.Item("iron"), new Point(100, 100));
            NodeId id2 = viewer.Session.Editor.CreateSupplierNode(ctx.Item("copper"), new Point(300, 100));
            Assert.IsTrue(viewer.NodeElementDictionary.TryGetValue(id1, out BaseNodeElement? node1));
            Assert.IsTrue(viewer.NodeElementDictionary.TryGetValue(id2, out BaseNodeElement? node2));
            Assert.IsNotNull(node1);
            Assert.IsNotNull(node2);

            SetViewerSelection(viewer, node1, node2);
            Assert.AreEqual(2, viewer.SelectedNodes.Count);

            InvokeViewerMouseDown(viewer, viewer.GraphToScreen(new Point(node1.X, node1.Y)));

            Assert.AreEqual(2, viewer.SelectedNodes.Count);
            Assert.IsTrue(node1.Highlighted);
            Assert.IsTrue(node2.Highlighted);
        }

        private static void MouseDown_OnUnselectedNode_ClearsExistingSelection_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));

            NodeId id1 = viewer.Session.Editor.CreateSupplierNode(ctx.Item("iron"), new Point(100, 100));
            NodeId id2 = viewer.Session.Editor.CreateSupplierNode(ctx.Item("copper"), new Point(300, 100));
            NodeId id3 = viewer.Session.Editor.CreateSupplierNode(ctx.Item("plate"), new Point(500, 100));
            Assert.IsTrue(viewer.NodeElementDictionary.TryGetValue(id1, out BaseNodeElement? node1));
            Assert.IsTrue(viewer.NodeElementDictionary.TryGetValue(id2, out BaseNodeElement? node2));
            Assert.IsTrue(viewer.NodeElementDictionary.TryGetValue(id3, out BaseNodeElement? node3));
            Assert.IsNotNull(node1);
            Assert.IsNotNull(node2);
            Assert.IsNotNull(node3);

            SetViewerSelection(viewer, node1, node2);
            InvokeViewerMouseDown(viewer, viewer.GraphToScreen(new Point(node3.X, node3.Y)));

            Assert.AreEqual(0, viewer.SelectedNodes.Count);
            Assert.IsFalse(node1.Highlighted);
            Assert.IsFalse(node2.Highlighted);
        }

        private static void ItemChooser_ClosesOnGraphClick_AndSelectsWithoutException_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            TestDataCacheHelper.SetPresetName(ctx.Cache, "test-preset");
            using var viewer = CreateViewer(ctx, lockedRecipeEditor: false, viewOffset: new Point(0, 0));

            viewer.AddItem(new Point(10, 10), new Point(200, 150));
            ItemChooserPanel? chooser = viewer.Controls.OfType<ItemChooserPanel>().FirstOrDefault();
            Assert.IsNotNull(chooser);

            MethodInfo? viewerMouseDown = typeof(ProductionGraphViewer).GetMethod(
                "ProductionGraphViewer_MouseDown", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(viewerMouseDown);
            viewerMouseDown.Invoke(viewer, [viewer, new MouseEventArgs(MouseButtons.Left, 1, 500, 500, 0)]);
            Assert.IsTrue(chooser.IsDisposed, "Clicking empty graph space should close the item chooser.");

            viewer.AddItem(new Point(10, 10), new Point(200, 150));
            chooser = viewer.Controls.OfType<ItemChooserPanel>().FirstOrDefault();
            Assert.IsNotNull(chooser);

            IItem item = ctx.Item("iron").Item!;
            using var button = new Button { Tag = item };
            var mouseUp = typeof(ItemChooserPanel).GetMethod("IRButtonMouseUp", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(mouseUp);
            mouseUp.Invoke(chooser, [button, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)]);
            Assert.IsTrue(chooser.IsDisposed, "Selecting an item should close the chooser without throwing.");
        }

        private static void SelectRecipeInChooser(RecipeChooserPanel chooser, RecipePrototype recipe) {
            MethodInfo? mouseUp = typeof(RecipeChooserPanel).GetMethod("IRButtonMouseUp", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(mouseUp, "RecipeChooserPanel.IRButtonMouseUp should exist.");
            using var recipeButton = new Button { Tag = recipe };
            mouseUp.Invoke(chooser, [recipeButton, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)]);
        }

        private static RecipePrototype CreateTestRecipeDefinition(GraphSessionTestHelper.TestContext ctx) {
            var recipe = new RecipePrototype(ctx.Cache, "test-disconnected-recipe", "Test Recipe", ctx.Subgroup, "z");
            TestPrototypeFactory.SetRecipeTime(recipe, 1);
            TestPrototypeFactory.LinkRecipeAndAssembler(recipe, TestPrototypeFactory.CreateTestAssembler(ctx.Cache));
            TestDataCacheHelper.RegisterRecipe(ctx.Cache, recipe);
            var ore = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "ore");
            var plate = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "plate");
            recipe.InternalOneWayAddIngredient(ore, 1);
            recipe.InternalOneWayAddProduct(plate, 1, 0);
            return recipe;
        }

        private static void SetViewerSelection(ProductionGraphViewer viewer, params BaseNodeElement[] nodes) {
            MethodInfo? setSelection = typeof(ProductionGraphViewer).GetMethod(
                "SetSelection", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(setSelection);
            setSelection.Invoke(viewer, [nodes]);
        }

        private static void InvokeViewerMouseDown(ProductionGraphViewer viewer, Point clientLocation) {
            MethodInfo? viewerMouseDown = typeof(ProductionGraphViewer).GetMethod(
                "ProductionGraphViewer_MouseDown", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(viewerMouseDown);
            viewerMouseDown.Invoke(viewer, [viewer, new MouseEventArgs(MouseButtons.Left, 1, clientLocation.X, clientLocation.Y, 0)]);
        }

        private static ProductionGraphViewer CreateViewer(
            GraphSessionTestHelper.TestContext ctx,
            bool lockedRecipeEditor,
            Point viewOffset) {
            var viewer = new ProductionGraphViewer {
                DCache = ctx.Cache,
                Size = new Size(ViewerWidth, ViewerHeight),
                LockedRecipeEditPanelPosition = lockedRecipeEditor,
            };
            viewer.Graph.DefaultAssemblerQuality = ctx.Quality;
            viewer.ApplySaveUi(new GraphViewerUiSaveData {
                ViewOffset = viewOffset,
                ViewScale = 1f,
            }, ctx.Cache, setEnablesFromJson: false);
            return viewer;
        }

        private static NodeId CreateTestRecipeNode(GraphSessionTestHelper.TestContext ctx, ProductionGraphViewer viewer, Point location) {
            RecipePrototype recipe = CreateTestRecipeDefinition(ctx);
            return viewer.Session.Editor.CreateRecipeNode(new RecipeQualityPair(recipe, ctx.Quality), location);
        }

        private static void AssertVerticalScrollbarDoesNotOverlapContent(
            UserControl editPanel,
            Panel scrollHost,
            Control content,
            int naturalContentWidth) {
            Assert.IsTrue(scrollHost.VerticalScroll.Visible,
                "Test requires a visible vertical scrollbar.");

            int scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
            Assert.IsGreaterThanOrEqualTo(naturalContentWidth + scrollBarWidth, editPanel.Width,
                $"Edit panel width {editPanel.Width} should reserve {scrollBarWidth}px beside natural content width {naturalContentWidth}px.");
            Assert.IsGreaterThanOrEqualTo(naturalContentWidth - 2, content.Width,
                $"Content width {content.Width} should not be squeezed below natural width {naturalContentWidth}px.");
            Assert.IsLessThanOrEqualTo(scrollHost.ClientSize.Width, content.Width,
                "Content should fit inside the scroll viewport client area.");
        }

        private static int MeasureUnconstrainedContentWidth(Control content) {
            Size previousMaximum = content.MaximumSize;
            try {
                content.MaximumSize = Size.Empty;
                return EditPanelViewportLayout.MeasureContentSize(content).Width;
            } finally {
                content.MaximumSize = previousMaximum;
            }
        }

        private static void AssertFloatingPanelsOnScreen(ProductionGraphViewer viewer) {
            const int margin = EditPanelScreenLayout.DefaultMargin;
            foreach (Control panel in viewer.Controls.Cast<Control>().Where(c => c.Visible)) {
                Rectangle bounds = panel.Bounds;
                Assert.IsTrue(EditPanelScreenLayout.FitsViewer(bounds, viewer.ClientSize.Width, viewer.ClientSize.Height, margin),
                    $"Panel {panel.GetType().Name} at {bounds} should be fully inside the viewer.");
            }
        }
    }
}
