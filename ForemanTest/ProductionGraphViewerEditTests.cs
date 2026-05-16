using Foreman;
using Foreman.Graph;
using ForemanTest.Graph;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ForemanTest {
    [TestClass]
    [DoNotParallelize]
    public class ProductionGraphViewerEditTests {
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
            Assert.IsInstanceOfType(element, typeof(RecipeNodeElement));
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
            var panel = new Panel { Size = new Size(200, 100), Location = new Point(30, 40) };
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
            ItemQualityPair disconnectedRecipeAnchor = new ItemQualityPair("adding disconnected recipe");
            Assert.IsFalse(disconnectedRecipeAnchor, "Add Recipe uses an empty item-quality sentinel.");

            try {
                viewer.AddNewNode(new Point(10, 10), disconnectedRecipeAnchor, new Point(200, 150), NewNodeType.Disconnected);
                RecipeChooserPanel? chooser = viewer.Controls.OfType<RecipeChooserPanel>().FirstOrDefault();
                Assert.IsNotNull(chooser, "AddNewNode should open a recipe chooser for disconnected placement.");

                Recipe recipe = CreateTestRecipeDefinition(ctx);
                int nodesBefore = viewer.Session.View.Nodes.Count;

                SelectRecipeInChooser(chooser, recipe);

                Assert.AreEqual(nodesBefore + 1, viewer.Session.View.Nodes.Count,
                    "Selecting a recipe from the disconnected chooser should add a recipe node.");
                Assert.IsTrue(viewer.Session.View.Nodes.OfType<IRecipeNodeViewModel>().Any(),
                    "The new node should be a recipe view model.");
            } finally {
                viewer.ToolTipRenderer.ClearFloatingControls();
            }
        }

        private static void SelectRecipeInChooser(RecipeChooserPanel chooser, Recipe recipe) {
            MethodInfo? mouseUp = typeof(RecipeChooserPanel).GetMethod("IRButton_MouseUp", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(mouseUp, "RecipeChooserPanel.IRButton_MouseUp should exist.");
            var recipeButton = new Button { Tag = recipe };
            mouseUp.Invoke(chooser, new object[] { recipeButton, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0) });
        }

        private static Recipe CreateTestRecipeDefinition(GraphSessionTestHelper.TestContext ctx) {
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
            Recipe recipe = CreateTestRecipeDefinition(ctx);
            return viewer.Session.Editor.CreateRecipeNode(new RecipeQualityPair(recipe, ctx.Quality), location);
        }

        private static void AssertFloatingPanelsOnScreen(ProductionGraphViewer viewer) {
            const int margin = EditPanelScreenLayout.DefaultMargin;
            foreach (Control panel in viewer.Controls.Cast<Control>().Where(c => c.Visible)) {
                Rectangle bounds = panel.Bounds;
                Assert.IsTrue(EditPanelScreenLayout.FitsViewer(bounds, viewer.Width, viewer.Height, margin),
                    $"Panel {panel.GetType().Name} at {bounds} should be fully inside the viewer.");
            }
        }
    }
}