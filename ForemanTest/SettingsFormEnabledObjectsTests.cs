using Foreman;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Forms;
using ForemanTest.Graph;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ForemanTest {
    [TestClass]
    [DoNotParallelize]
    public class SettingsFormEnabledObjectsTests : ForemanTestBase {
        [TestMethod]
        public void EnabledObjectsIconIndex_ReusesSameBitmapReference() =>
            StaTest.Run(EnabledObjectsIconIndex_ReusesSameBitmapReference_Impl);

        [TestMethod]
        public void SettingsForm_UpdateEnabledStatus_QualityVirtualListSizeDoesNotAccumulate() =>
            StaTest.Run(SettingsForm_UpdateEnabledStatus_QualityVirtualListSizeDoesNotAccumulate_Impl);

        [TestMethod]
        public void SettingsForm_LoadUnfilteredLists_DeduplicatesSharedRecipeIcons() =>
            StaTest.Run(SettingsForm_LoadUnfilteredLists_DeduplicatesSharedRecipeIcons_Impl);

        private static void EnabledObjectsIconIndex_ReusesSameBitmapReference_Impl() {
            using var iconList = new ImageList { ImageSize = new Size(24, 24), ColorDepth = ColorDepth.Depth32Bit };
            var index = new EnabledObjectsIconIndex(iconList);
            using var shared = new Bitmap(24, 24);

            int first = index.GetImageIndex(shared);
            int second = index.GetImageIndex(shared);

            Assert.AreEqual(first, second);
            Assert.AreEqual(2, index.ImageCount, "Unknown icon plus one distinct bitmap.");
        }

        private static void SettingsForm_UpdateEnabledStatus_QualityVirtualListSizeDoesNotAccumulate_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var uncommon = new QualityPrototype(ctx.Cache, "uncommon", "Uncommon", "b");
            TestDataCacheHelper.RegisterQuality(ctx.Cache, uncommon);

            using var mainForm = new MainForm();
            var options = CreateSettingsOptions(ctx);
            using var form = new SettingsForm(options, mainForm);

            ListView qualityListView = GetPrivateField<ListView>(form, "QualityListView");
            int qualityCount = GetPrivateField<List<ListViewItem>>(form, "unfilteredQualityList").Count;
            Assert.IsGreaterThanOrEqualTo(2, qualityCount, "Test setup should include multiple qualities.");

            qualityListView.VirtualListSize = 999;

            InvokePrivate(form, "UpdateEnabledStatus");
            Assert.AreEqual(qualityCount, qualityListView.VirtualListSize,
                "UpdateEnabledStatus must assign quality virtual size, not accumulate.");

            InvokePrivate(form, "UpdateEnabledStatus");
            Assert.AreEqual(qualityCount, qualityListView.VirtualListSize,
                "Repeated UpdateEnabledStatus must not inflate quality virtual size.");
        }

        private static void SettingsForm_LoadUnfilteredLists_DeduplicatesSharedRecipeIcons_Impl() {
            var ctx = GraphSessionTestHelper.CreateContext();
            using var sharedIcon = new Bitmap(24, 24);
            var color = Color.FromArgb(40, 80, 120);

            RegisterRecipeWithIcon(ctx, "§§test:recipe-a", "Recipe A", sharedIcon, color);
            RegisterRecipeWithIcon(ctx, "§§test:recipe-b", "Recipe B", sharedIcon, color);
            RegisterRecipeWithIcon(ctx, "§§test:recipe-c", "Recipe C", sharedIcon, color);

            using var mainForm = new MainForm();
            var options = CreateSettingsOptions(ctx);
            using var form = new SettingsForm(options, mainForm);

            ImageList iconList = GetPrivateField<ImageList>(form, "IconList");
            List<ListViewItem> recipeItems = GetPrivateField<List<ListViewItem>>(form, "unfilteredRecipeList");

            var ourRecipes = recipeItems.Where(i => i.Tag is IRecipe r && r.Name.StartsWith("§§test:recipe-", StringComparison.Ordinal)).ToList();
            Assert.HasCount(3, ourRecipes);
            Assert.HasCount(1, ourRecipes.Select(i => i.ImageIndex).Distinct(),
                "Recipes sharing a bitmap should share the same image index.");
            Assert.IsLessThan(ourRecipes.Count + 3, iconList.Images.Count,
                "Shared icons should not add one ImageList entry per recipe row.");
        }

        private static SettingsForm.SettingsFormOptions CreateSettingsOptions(GraphSessionTestHelper.TestContext ctx) {
            var options = new SettingsForm.SettingsFormOptions(ctx.Cache) {
                Presets = [new Preset(MainForm.DefaultPreset, true, true)]
            };
            options.SelectedPreset = options.Presets[0];
            options.QualitySteps = 5;
            options.NodeCountForSimpleView = 300;
            options.IconsOnlyIconSize = 32;
            options.SolverLowPriorityPower = 1;
            options.SolverPullConsumerNodesPower = 1;
            foreach (IRecipe recipe in ctx.Cache.Recipes.Values.Where(r => r.Enabled))
                options.EnabledObjects.Add(recipe);
            foreach (IAssembler assembler in ctx.Cache.Assemblers.Values.Where(r => r.Enabled))
                options.EnabledObjects.Add(assembler);
            foreach (IBeacon beacon in ctx.Cache.Beacons.Values.Where(r => r.Enabled))
                options.EnabledObjects.Add(beacon);
            foreach (IModule module in ctx.Cache.Modules.Values.Where(r => r.Enabled))
                options.EnabledObjects.Add(module);
            foreach (IQuality quality in ctx.Cache.Qualities.Values.Where(r => r.Enabled))
                options.EnabledObjects.Add(quality);
            return options;
        }

        private static void RegisterRecipeWithIcon(
            GraphSessionTestHelper.TestContext ctx,
            string name,
            string friendlyName,
            Bitmap icon,
            Color color) {
            var recipe = new RecipePrototype(ctx.Cache, name, friendlyName, ctx.Subgroup, "z");
            TestPrototypeFactory.SetRecipeTime(recipe, 1);
            TestPrototypeFactory.LinkRecipeAndAssembler(recipe, TestPrototypeFactory.CreateTestAssembler(ctx.Cache));
            recipe.SetIconAndColor(new IconColorPair(icon, color));
            var ore = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, name + "-ore");
            var plate = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, name + "-plate");
            recipe.InternalOneWayAddIngredient(ore, 1);
            recipe.InternalOneWayAddProduct(plate, 1, 0);
            TestDataCacheHelper.RegisterRecipe(ctx.Cache, recipe);
        }

        private static T GetPrivateField<T>(object instance, string name) where T : class {
            FieldInfo? field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field {name} should exist.");
            return (T)field!.GetValue(instance)!;
        }

        private static void InvokePrivate(object instance, string methodName) {
            MethodInfo? method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Method {methodName} should exist.");
            method!.Invoke(instance, null);
        }
    }
}
