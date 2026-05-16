using System.Drawing;
using System.IO;

namespace Foreman {
    /// <summary>Creates Foreman-internal helper objects that survive <see cref="DataCache.Clear"/>.</summary>
    internal static class DataCacheBootstrap {
        public static void GenerateForemanHelperObjects(DataCache owner, DataCacheStore store) {
            store.StartingTech = new TechnologyPrototype(owner, "§§t:starting_tech", "Starting Technology");
            store.StartingTech.Tier = 0;

            store.ExtraFormanGroup = new GroupPrototype(owner, "§§g:extra_group", "Resource Extraction\nPower Generation\nRocket Launches", "~~~z1");
            store.ExtraFormanGroup.SetIconAndColor(new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "ExtraGroupIcon.png"), 64), Color.Gray));

            store.ExtractionSubgroupItems = new SubgroupPrototype(owner, "§§sg:extraction_items", "1");
            store.ExtractionSubgroupItems.myGroup = store.ExtraFormanGroup;
            store.ExtraFormanGroup.subgroups.Add(store.ExtractionSubgroupItems);

            store.ExtractionSubgroupFluids = new SubgroupPrototype(owner, "§§sg:extraction_fluids", "2");
            store.ExtractionSubgroupFluids.myGroup = store.ExtraFormanGroup;
            store.ExtraFormanGroup.subgroups.Add(store.ExtractionSubgroupFluids);

            store.ExtractionSubgroupFluidsOP = new SubgroupPrototype(owner, "§§sg:extraction_fluids_2", "3");
            store.ExtractionSubgroupFluidsOP.myGroup = store.ExtraFormanGroup;
            store.ExtraFormanGroup.subgroups.Add(store.ExtractionSubgroupFluidsOP);

            store.EnergySubgroupBoiling = new SubgroupPrototype(owner, "§§sg:energy_boiling", "4");
            store.EnergySubgroupBoiling.myGroup = store.ExtraFormanGroup;
            store.ExtraFormanGroup.subgroups.Add(store.EnergySubgroupBoiling);

            store.EnergySubgroupEnergy = new SubgroupPrototype(owner, "§§sg:energy_heat", "5");
            store.EnergySubgroupEnergy.myGroup = store.ExtraFormanGroup;
            store.ExtraFormanGroup.subgroups.Add(store.EnergySubgroupEnergy);

            store.RocketLaunchSubgroup = new SubgroupPrototype(owner, "§§sg:rocket_launches", "6");
            store.RocketLaunchSubgroup.myGroup = store.ExtraFormanGroup;
            store.ExtraFormanGroup.subgroups.Add(store.RocketLaunchSubgroup);

            store.ErrorQuality = new QualityPrototype(owner, "§§error_quality", "ERROR", "-");

            IconColorPair heatIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "HeatIcon.png"), 64), Color.DarkRed);
            IconColorPair burnerGeneratorIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "BurnerGeneratorIcon.png"), 64), Color.DarkRed);
            IconColorPair playerAssemblerIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "PlayerAssembler.png"), 64), Color.Gray);
            IconColorPair rocketAssemblerIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "RocketAssembler.png"), 64), Color.Gray);
            store.HeatItem = new ItemPrototype(owner, "§§i:heat", "Heat (1MJ)", new SubgroupPrototype(owner, "-", "-"), "-");
            store.HeatItem.SetIconAndColor(heatIcon);
            store.HeatItem.FuelValue = 1000000;

            store.HeatRecipe = new RecipePrototype(owner, "§§r:h:heat-generation", "Heat Generation", store.EnergySubgroupEnergy, "1");
            store.HeatRecipe.SetIconAndColor(heatIcon);
            store.HeatRecipe.InternalOneWayAddProduct(store.HeatItem, 1, 0);
            store.HeatItem.productionRecipes.Add(store.HeatRecipe);
            store.HeatRecipe.Time = 1;

            store.BurnerRecipe = new RecipePrototype(owner, "§§r:h:burner-electicity", "Burner Generator", store.EnergySubgroupEnergy, "2");
            store.BurnerRecipe.SetIconAndColor(burnerGeneratorIcon);
            store.BurnerRecipe.Time = 1;

            store.PlayerAssembler = new AssemblerPrototype(owner, "§§a:player-assembler", "Player", EntityType.Assembler, EnergySource.Void);
            store.PlayerAssembler.energyDrain = 0;
            store.PlayerAssembler.SetIconAndColor(playerAssemblerIcon);

            store.RocketAssembler = new AssemblerPrototype(owner, "§§a:rocket-assembler", "Rocket", EntityType.Rocket, EnergySource.Void);
            store.RocketAssembler.energyDrain = 0;
            store.RocketAssembler.SetIconAndColor(rocketAssemblerIcon);

            store.ElectricityIcon = IconCache.GetIcon(Path.Combine("Graphics", "ElectricityIcon.png"), 64);

            store.MissingSubgroup = new SubgroupPrototype(owner, "§§MISSING-SG", "");
            store.MissingSubgroup.myGroup = new GroupPrototype(owner, "§§MISSING-G", "MISSING", "");

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