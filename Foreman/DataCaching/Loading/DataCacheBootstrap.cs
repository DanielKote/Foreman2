using Foreman.DataCaching.DataTypes;
using System.Drawing;
using System.IO;

namespace Foreman.DataCaching.Loading {
    /// <summary>Creates Foreman-internal helper objects that survive <see cref="DataCache.Clear"/>.</summary>
    internal static class DataCacheBootstrap {
        public static void GenerateForemanHelperObjects(DataCache owner, DataCacheStore store) {
            store.StartingTech = new TechnologyPrototype(owner, "§§t:starting_tech", "Starting Technology") {
                Tier = 0
            };

            store.ExtraFormanGroup = new GroupPrototype(owner, "§§g:extra_group", "Resource Extraction\nPower Generation\nRocket Launches", "~~~z1");
            var icon = IconCache.GetIcon(Path.Combine("Graphics", "ExtraGroupIcon.png"), 64);
            try {
                store.ExtraFormanGroup.SetIconAndColor(new IconColorPair(icon, Color.Gray));
                icon = null;
            } finally {
                icon?.Dispose();
            }

            store.ExtractionSubgroupItems = new SubgroupPrototype(owner, "§§sg:extraction_items", "1") {
                MyGroupInternal = store.ExtraFormanGroup
            };
            store.ExtraFormanGroup.SubgroupsInternal.Add(store.ExtractionSubgroupItems);

            store.ExtractionSubgroupFluids = new SubgroupPrototype(owner, "§§sg:extraction_fluids", "2") {
                MyGroupInternal = store.ExtraFormanGroup
            };
            store.ExtraFormanGroup.SubgroupsInternal.Add(store.ExtractionSubgroupFluids);

            store.ExtractionSubgroupFluidsOP = new SubgroupPrototype(owner, "§§sg:extraction_fluids_2", "3") {
                MyGroupInternal = store.ExtraFormanGroup
            };
            store.ExtraFormanGroup.SubgroupsInternal.Add(store.ExtractionSubgroupFluidsOP);

            store.EnergySubgroupBoiling = new SubgroupPrototype(owner, "§§sg:energy_boiling", "4") {
                MyGroupInternal = store.ExtraFormanGroup
            };
            store.ExtraFormanGroup.SubgroupsInternal.Add(store.EnergySubgroupBoiling);

            store.EnergySubgroupEnergy = new SubgroupPrototype(owner, "§§sg:energy_heat", "5") {
                MyGroupInternal = store.ExtraFormanGroup
            };
            store.ExtraFormanGroup.SubgroupsInternal.Add(store.EnergySubgroupEnergy);

            store.RocketLaunchSubgroup = new SubgroupPrototype(owner, "§§sg:rocket_launches", "6") {
                MyGroupInternal = store.ExtraFormanGroup
            };
            store.ExtraFormanGroup.SubgroupsInternal.Add(store.RocketLaunchSubgroup);

            store.ErrorQuality = new QualityPrototype(owner, "§§error_quality", "ERROR", "-");

            var heatIconBmp = IconCache.GetIcon(Path.Combine("Graphics", "HeatIcon.png"), 64);
            var burnerGenBmp = IconCache.GetIcon(Path.Combine("Graphics", "BurnerGeneratorIcon.png"), 64);
            var playerAssemblerBmp = IconCache.GetIcon(Path.Combine("Graphics", "PlayerAssembler.png"), 64);
            var rocketAssemblerBmp = IconCache.GetIcon(Path.Combine("Graphics", "RocketAssembler.png"), 64);

            var heatIcon = new IconColorPair(heatIconBmp, Color.DarkRed);
            var burnerGeneratorIcon = new IconColorPair(burnerGenBmp, Color.DarkRed);
            var playerAssemblerIcon = new IconColorPair(playerAssemblerBmp, Color.Gray);
            var rocketAssemblerIcon = new IconColorPair(rocketAssemblerBmp, Color.Gray);
            store.HeatItem = new ItemPrototype(owner, "§§i:heat", "Heat (1MJ)", new SubgroupPrototype(owner, "-", "-"), "-");
            store.HeatRecipe = new RecipePrototype(owner, "§§r:h:heat-generation", "Heat Generation", store.EnergySubgroupEnergy, "1");
            try {
                store.HeatItem.SetIconAndColor(heatIcon);
                store.HeatRecipe.SetIconAndColor(heatIcon);
                heatIconBmp = null;
            } finally {
                heatIconBmp?.Dispose();
            }
            store.HeatItem.FuelValue = 1000000;

            store.HeatRecipe.InternalOneWayAddProduct(store.HeatItem, 1, 0);
            store.HeatItem.ProductionRecipesInternal.Add(store.HeatRecipe);
            store.HeatRecipe.Time = 1;

            store.BurnerRecipe = new RecipePrototype(owner, "§§r:h:burner-electicity", "Burner Generator", store.EnergySubgroupEnergy, "2");
            try {
                store.BurnerRecipe.SetIconAndColor(burnerGeneratorIcon);
                burnerGenBmp = null;
            } finally {
                burnerGenBmp?.Dispose();
            }
            store.BurnerRecipe.Time = 1;

            store.PlayerAssembler = new AssemblerPrototype(owner, "§§a:player-assembler", "Player", EntityType.Assembler, EnergySource.Void) {
                EnergyDrainInternal = 0
            };
            try {
                store.PlayerAssembler.SetIconAndColor(playerAssemblerIcon);
                playerAssemblerBmp = null;
            } finally {
                playerAssemblerBmp?.Dispose();
            }

            store.RocketAssembler = new AssemblerPrototype(owner, "§§a:rocket-assembler", "Rocket", EntityType.Rocket, EnergySource.Void) {
                EnergyDrainInternal = 0
            };
            try {
                store.RocketAssembler.SetIconAndColor(rocketAssemblerIcon);
                rocketAssemblerBmp = null;
            } finally {
                rocketAssemblerBmp?.Dispose();
            }

            store.ElectricityIcon = IconCache.GetIcon(Path.Combine("Graphics", "ElectricityIcon.png"), 64);

            store.MissingSubgroup = new SubgroupPrototype(owner, "§§MISSING-SG", "") {
                MyGroupInternal = new GroupPrototype(owner, "§§MISSING-G", "MISSING", "")
            };

            store.MissingAssembler = new AssemblerPrototype(owner, "§§a:MISSING-A", "missing assembler", EntityType.Assembler, EnergySource.Void, true);
        }

        public static void ClearLoadedData(DataCacheStore store) {
            store.DefaultQuality = store.ErrorQuality;

            store.IncludedMods.Clear();
            store.Technologies.Clear();
            store.Groups.Clear();
            store.Subgroups.Clear();
            store.Qualities.Clear();
            store.MissingQualities.Clear();
            store.Items.Clear();
            store.Recipes.Clear();
            store.PlantProcesses.Clear();
            store.Assemblers.Clear();
            store.Modules.Clear();
            store.Beacons.Clear();
            store.SciencePacks.Clear();
            store.SciencePackPrerequisites.Clear();

            store.MissingItems.Clear();
            store.MissingAssemblers.Clear();
            store.MissingModules.Clear();
            store.MissingBeacons.Clear();
            store.MissingRecipes.Clear();
            store.MissingPlantProcesses.Clear();

            if (store.IconCache != null) {
                foreach (var iconset in store.IconCache.Values)
                    iconset.Icon?.Dispose();
                store.IconCache.Clear();
            }

            if (store.ExtraFormanGroup is not null)
                store.Groups.Add(store.ExtraFormanGroup.Name, store.ExtraFormanGroup);
            if (store.ExtractionSubgroupItems is not null)
                store.Subgroups.Add(store.ExtractionSubgroupItems.Name, store.ExtractionSubgroupItems);
            if (store.ExtractionSubgroupFluids is not null)
                store.Subgroups.Add(store.ExtractionSubgroupFluids.Name, store.ExtractionSubgroupFluids);
            if (store.ExtractionSubgroupFluidsOP is not null)
                store.Subgroups.Add(store.ExtractionSubgroupFluidsOP.Name, store.ExtractionSubgroupFluidsOP);
            if (store.HeatItem is not null)
                store.Items.Add(store.HeatItem.Name, store.HeatItem);
            if (store.HeatRecipe is not null)
                store.Recipes.Add(store.HeatRecipe.Name, store.HeatRecipe);
            if (store.BurnerRecipe is not null)
                store.Recipes.Add(store.BurnerRecipe.Name, store.BurnerRecipe);
            if (store.StartingTech is not null)
                store.Technologies.Add(store.StartingTech.Name, store.StartingTech);
        }
    }
}
