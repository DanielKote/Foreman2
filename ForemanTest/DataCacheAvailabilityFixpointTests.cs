using Foreman;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.DataCaching.Loading;
using ForemanTest.Graph;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Windows.Forms;

namespace ForemanTest {
    [TestClass]
    public class DataCacheAvailabilityFixpointTests : ForemanTestBase {
        [TestMethod]
        public void ItemAvailabilityFixpoint_SpoilResultStaysAvailable_WithoutCycling() {
            var ctx = GraphSessionTestHelper.CreateContext();
            DataCacheStore store = TestDataCacheHelper.RequireStore(ctx.Cache);

            ItemPrototype fresh = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "fresh-fish");
            ItemPrototype spoiled = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "spoiled-fish");
            fresh.Available = true;
            spoiled.Available = true;
            spoiled.SpoilOriginsInternal.Add(fresh);

            AssemblerPrototype assembler = TestPrototypeFactory.CreateTestAssembler(ctx.Cache);
            TestDataCacheHelper.RegisterAssembler(ctx.Cache, assembler);
            ((AssemblerPrototype)assembler).Available = true;

            var makeFresh = new RecipePrototype(ctx.Cache, "make-fresh-fish", "Catch", ctx.Subgroup, "z") {
                Available = true
            };
            makeFresh.InternalOneWayAddProduct(fresh, 1, 0);
            fresh.ProductionRecipesInternal.Add(makeFresh);
            TestPrototypeFactory.LinkRecipeAndAssembler(makeFresh, assembler);
            TestDataCacheHelper.RegisterRecipe(ctx.Cache, makeFresh);

            var burnRecipe = new RecipePrototype(ctx.Cache, "burn-spoiled-fish", "Burn", ctx.Subgroup, "z") {
                Available = true
            };
            burnRecipe.InternalOneWayAddIngredient(spoiled, 1);
            spoiled.ConsumptionRecipesInternal.Add(burnRecipe);

            TestPrototypeFactory.LinkRecipeAndAssembler(burnRecipe, assembler);
            TestDataCacheHelper.RegisterRecipe(ctx.Cache, burnRecipe);

            ItemAvailabilityFixpoint.Run(store);

            Assert.IsTrue(spoiled.Available, "Spoil results with an available origin should remain available.");
            Assert.IsFalse(ItemAvailabilityFixpoint.LastRunDetectedCycle);
            Assert.IsLessThanOrEqualTo(4, ItemAvailabilityFixpoint.LastIterationCount,
                "Fixpoint should converge quickly when spoil/plant rules are consistent.");
        }

        [TestMethod]
        public void ItemAvailabilityFixpoint_ShouldRemainAvailable_IncludesSpoilAndPlantOrigins() {
            var ctx = GraphSessionTestHelper.CreateContext();
            ItemPrototype origin = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "origin");
            ItemPrototype spoilResult = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "spoil-result");
            origin.Available = true;
            spoilResult.SpoilOriginsInternal.Add(origin);

            Assert.IsTrue(ItemAvailabilityFixpoint.ShouldRemainAvailable(spoilResult));
        }

        [TestMethod]
        public void PresetDataLoader_PostProcessQuality_CyclicNextQuality_DoesNotHang() {
            var cache = new DataCache(filterRecipes: false);
            DataCacheStore store = TestDataCacheHelper.RequireStore(cache);
            var session = new PresetLoadSession();
            var loader = new PresetDataLoader(cache, store, session);

            var normal = new QualityPrototype(cache, "normal", "Normal", "a") { Enabled = true, NextProbability = 0.1 };
            var uncommon = new QualityPrototype(cache, "uncommon", "Uncommon", "b") { Enabled = true, NextProbability = 0.1 };
            store.Qualities.Add(normal.Name, normal);
            store.Qualities.Add(uncommon.Name, uncommon);
            normal.NextQuality = uncommon;
            uncommon.NextQuality = normal;
            store.DefaultQuality = normal;

            loader.PostProcessQuality();

            Assert.IsGreaterThanOrEqualTo(1u, store.QualityMaxChainLength);
            Assert.IsLessThanOrEqualTo(2u, store.QualityMaxChainLength,
                "A cyclic quality chain should truncate chain-length measurement instead of looping forever.");
        }

        [TestMethod]
        public void DataLoadForm_OnFormClosing_WarnsWhenLoadStillInProgress() =>
            StaTest.Run(DataLoadForm_OnFormClosing_WarnsWhenLoadStillInProgress_Impl);

        private static void DataLoadForm_OnFormClosing_WarnsWhenLoadStillInProgress_Impl() {
            bool warned = false;
            using (UserMessages.UseHandler((text, caption, buttons, icon) => {
                if (text.Contains("incomplete", System.StringComparison.OrdinalIgnoreCase))
                    warned = true;
                return DialogResult.OK;
            })) {
                using var form = new DataLoadForm(new Preset("test-preset", true, false));
                SetPrivateField(form, "loadInProgress", true);
                SetPrivateField(form, "loadCompleted", false);

                var args = new FormClosingEventArgs(CloseReason.UserClosing, cancel: false);
                typeof(DataLoadForm).GetMethod("OnFormClosing", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(form, [args]);

                Assert.IsTrue(warned, "Closing during load should warn about incomplete preset data.");
                Assert.IsFalse(args.Cancel, "Warning only; the user must still be able to close the dialog.");
            }
        }

        private static void SetPrivateField(object instance, string name, object value) {
            FieldInfo field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!;
            field.SetValue(instance, value);
        }
    }
}
