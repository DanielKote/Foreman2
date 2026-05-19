using Foreman;
using Foreman.Graph;
using ForemanTest.Graph;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Linq;
namespace ForemanTest {
    [TestClass]
    [DoNotParallelize]
    public class BeaconElementDrawTests : ForemanTestBase {
        [TestMethod]
        public void BeaconElement_Draw_WithMoreThanSixBeaconModules_DoesNotThrow() =>
            StaTest.Run(BeaconElement_Draw_WithMoreThanSixBeaconModules_DoesNotThrow_Impl);

        private static void BeaconElement_Draw_WithMoreThanSixBeaconModules_DoesNotThrow_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            const int beaconModuleSlots = 8;
            const int moduleCount = 7;

            BeaconPrototype beacon = CreateTestBeacon(ctx, beaconModuleSlots);
            ModulePrototype module = CreateTestSpeedModule(ctx);
            WireModuleForBeaconRecipe(ctx, beacon, module);
            Recipe recipe = CreateBeaconCapableRecipe(ctx, module);

            using var viewer = new ProductionGraphViewer {
                DCache = ctx.Cache,
                Size = new Size(800, 600),
                LevelOfDetail = ProductionGraphViewer.LOD.High,
            };
            viewer.Graph.DefaultAssemblerQuality = ctx.Quality;
            viewer.ApplySaveUi(new GraphViewerUiSaveData { ViewOffset = Point.Empty, ViewScale = 1f }, ctx.Cache, setEnablesFromJson: false);

            NodeId recipeId = viewer.Session.Editor.CreateRecipeNode(new RecipeQualityPair(recipe, ctx.Quality), new Point(0, 0));
            Assert.IsTrue(viewer.NodeElementDictionary.TryGetValue(recipeId, out BaseNodeElement? nodeElement));
            Assert.IsInstanceOfType(nodeElement, typeof(RecipeNodeElement));
            var recipeElement = (RecipeNodeElement)nodeElement!;

            var nodeController = viewer.Session.Editor.RequestNodeController(recipeId);
            Assert.IsInstanceOfType(nodeController, typeof(RecipeNodeController));
            var controller = (RecipeNodeController)nodeController!;
            controller.SetBeacon(new BeaconQualityPair(beacon, ctx.Quality));
            var modulePair = new ModuleQualityPair(module, ctx.Quality);
            for (int i = 0; i < moduleCount; i++)
                controller.AddBeaconModule(modulePair);

            Assert.AreEqual(moduleCount, recipeElement.RecipeViewModel.BeaconModules.Count,
                "Test setup should place the node in the >6 module circle-drawing path.");

            BeaconElement beaconElement = recipeElement.SubElements.OfType<BeaconElement>().Single();
            beaconElement.SetVisibility(true);

            using var bitmap = new Bitmap(240, 80);
            using var graphics = Graphics.FromImage(bitmap);
            beaconElement.Paint(graphics, NodeDrawingStyle.Regular);
            beaconElement.GetToolTips(new Point(0, 0));
        }

        private static BeaconPrototype CreateTestBeacon(GraphSessionTestHelper.TestContext ctx, int moduleSlots) {
            var beacon = new BeaconPrototype(ctx.Cache, "§§test:beacon-many-slots", "Test Beacon", EnergySource.Electric);
            beacon.ModuleSlots = moduleSlots;
            beacon.energyConsumption[ctx.Quality] = 1000;
            TestDataCacheHelper.RegisterBeacon(ctx.Cache, beacon);
            return beacon;
        }

        private static ModulePrototype CreateTestSpeedModule(GraphSessionTestHelper.TestContext ctx) {
            const string name = "§§test:speed-module";
            TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, name);
            var module = new ModulePrototype(ctx.Cache, name, "Test Speed Module") { SpeedBonus = 0.5 };
            TestDataCacheHelper.RegisterModule(ctx.Cache, module);
            return module;
        }

        private static void WireModuleForBeaconRecipe(
            GraphSessionTestHelper.TestContext ctx,
            BeaconPrototype beacon,
            ModulePrototype module) {
            Assembler assembler = TestPrototypeFactory.CreateTestAssembler(ctx.Cache);
            TestDataCacheHelper.RegisterAssembler(ctx.Cache, assembler);
            ((AssemblerPrototype)assembler).modules.Add(module);
            beacon.modules.Add(module);
            module.assemblers.Add((AssemblerPrototype)assembler);
            module.beacons.Add(beacon);
        }

        private static Recipe CreateBeaconCapableRecipe(GraphSessionTestHelper.TestContext ctx, ModulePrototype module) {
            var recipe = new RecipePrototype(ctx.Cache, "§§test:beacon-recipe", "Beacon Recipe", ctx.Subgroup, "z");
            TestPrototypeFactory.SetRecipeTime(recipe, 1);
            Assembler assembler = TestPrototypeFactory.CreateTestAssembler(ctx.Cache);
            if (!ctx.Cache.Assemblers.ContainsKey(assembler.Name))
                TestDataCacheHelper.RegisterAssembler(ctx.Cache, assembler);
            TestPrototypeFactory.LinkRecipeAndAssembler(recipe, assembler);
            recipe.beaconModules.Add(module);
            recipe.assemblerModules.Add(module);
            ((AssemblerPrototype)assembler).modules.Add(module);
            module.recipes.Add(recipe);

            var ore = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "beacon-test-ore");
            var plate = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "beacon-test-plate");
            recipe.InternalOneWayAddIngredient(ore, 1);
            recipe.InternalOneWayAddProduct(plate, 1, 0);
            TestDataCacheHelper.RegisterRecipe(ctx.Cache, recipe);
            return recipe;
        }
    }
}