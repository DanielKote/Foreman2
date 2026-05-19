using Foreman.DataCaching.DataTypes;
using Foreman.DataCaching.Loading;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman.DataCaching {
    /// <summary>Loaded Factorio preset data (items, recipes, technologies, entities).</summary>
    public class DataCache {
        private readonly DataCacheStore _store;
        private readonly DataCacheImportHandlers _import;

        public string? PresetName { get; private set; }
        public int Version { get; private set; }

        public IEnumerable<IGroup> AvailableGroups => _store.Groups.Values.Where(g => g.Available);
        public IEnumerable<ISubgroup> AvailableSubgroups => _store.Subgroups.Values.Where(g => g.Available);
        public IEnumerable<IQuality> AvailableQualities => _store.Qualities.Values.Where(g => g.Available);
        public IEnumerable<IItem> AvailableItems => _store.Items.Values.Where(g => g.Available);
        public IEnumerable<IRecipe> AvailableRecipes => _store.Recipes.Values.Where(g => g.Available);
        public IEnumerable<IPlantProcess> AvailablePlantProcesses => _store.PlantProcesses.Values.Where(g => g.Available);

        public IReadOnlyDictionary<string, string> IncludedMods => _store.IncludedMods;
        public IReadOnlyDictionary<string, ITechnology> Technologies => _store.Technologies;
        public IReadOnlyDictionary<string, IGroup> Groups => _store.Groups;
        public IReadOnlyDictionary<string, ISubgroup> Subgroups => _store.Subgroups;
        public IReadOnlyDictionary<string, IQuality> Qualities => _store.Qualities;
        public IReadOnlyDictionary<string, IItem> Items => _store.Items;
        public IReadOnlyDictionary<string, IRecipe> Recipes => _store.Recipes;
        public IReadOnlyDictionary<string, IPlantProcess> PlantProcesses => _store.PlantProcesses;
        public IReadOnlyDictionary<string, IAssembler> Assemblers => _store.Assemblers;
        public IReadOnlyDictionary<string, IModule> Modules => _store.Modules;
        public IReadOnlyDictionary<string, IBeacon> Beacons => _store.Beacons;
        public IReadOnlyList<IItem> SciencePacks => _store.SciencePacks;
        public IReadOnlyDictionary<IItem, ICollection<IItem>> SciencePackPrerequisites => _store.SciencePackPrerequisites;

        public IAssembler? PlayerAssembler => _store.PlayerAssembler;
        public IAssembler? RocketAssembler => _store.RocketAssembler;
        public ITechnology? StartingTech => _store.StartingTech;

        public ISubgroup? MissingSubgroup => _store.MissingSubgroup;
        public IReadOnlyDictionary<string, IQuality> MissingQualities => _store.MissingQualities;
        public IReadOnlyDictionary<string, IItem> MissingItems => _store.MissingItems;
        public IReadOnlyDictionary<string, IAssembler> MissingAssemblers => _store.MissingAssemblers;
        public IReadOnlyDictionary<string, IModule> MissingModules => _store.MissingModules;
        public IReadOnlyDictionary<string, IBeacon> MissingBeacons => _store.MissingBeacons;
        public IReadOnlyDictionary<RecipeShort, IRecipe> MissingRecipes => _store.MissingRecipes;
        public IReadOnlyDictionary<PlantShort, IPlantProcess> MissingPlantProcesses => _store.MissingPlantProcesses;

        public IQuality? DefaultQuality => _store.DefaultQuality;
        public uint QualityMaxChainLength => _store.QualityMaxChainLength;

        public static Bitmap UnknownIcon => IconCache.UnknownIcon;
        public static Bitmap NoBeaconIcon {
            get {
                field ??= IconCache.GetIcon(Path.Combine("Graphics", "NoBeacon.png"), 64);
                return field;
            }
        }

        public DataCache(bool filterRecipes) {
            _store = new DataCacheStore(filterRecipes);
            _import = new DataCacheImportHandlers(this, _store);
            DataCacheBootstrap.GenerateForemanHelperObjects(this, _store);
            Clear();
        }

        public async Task LoadAllData(Preset preset, IProgress<KeyValuePair<int, string>> progress, bool loadIcons = true) {
            Clear();

            var session = new PresetLoadSession();
            PresetName = preset.Name;
            JsonObject jsonData = PresetProcessor.PrepPreset(preset);
            Version = PresetJson.GetInt32(jsonData, PresetExportFormat.VersionPropertyName) ?? 0;

            _store.IconCache = loadIcons
                ? await IconCache.LoadIconCache(Path.Combine(Application.StartupPath, "Presets", preset.Name + ".dat"), progress, 0, 90).ConfigureAwait(false)
                : [];

            await Task.Run(() => {
                progress.Report(new KeyValuePair<int, string>(90, "Processing Data..."));

                var entityLoader = new EntityDataLoader(this, _store, session);
                var presetLoader = new PresetDataLoader(this, _store, session);
                presetLoader.LoadFromJson(jsonData, _store.IconCache);
                entityLoader.LoadEntities(jsonData, _store.IconCache);
                entityLoader.LinkRecipesToCraftingCategoryMachines();
                presetLoader.LoadRocketLaunches(jsonData);
                entityLoader.LoadCharacter(
                    PresetJson.EnumerateArray(jsonData, "entities").FirstOrDefault(a => PresetJson.GetString(a, "name") == "character"));

                if (_store.RocketAssembler is not null)
                    _store.Assemblers.Add(_store.RocketAssembler.Name, _store.RocketAssembler);

                PresetCraftingCompatibility.FinalizeRecipeCraftingLinks(_store, session);
                new DataCachePostLoadProcessor(this, _store).RunAfterPresetParsed();

                progress.Report(new KeyValuePair<int, string>(98, "Finalizing..."));
                progress.Report(new KeyValuePair<int, string>(100, "Done!"));
            }).ConfigureAwait(false);
        }

        public void Clear() => DataCacheBootstrap.ClearLoadedData(_store);

        public void ProcessImportedItemsSet(IEnumerable<string> itemNames) =>
            _import.ProcessImportedItemsSet(itemNames);

        public Dictionary<string, IQuality?> ProcessImportedQualitiesSet(IEnumerable<KeyValuePair<string, int>> qualityPairs) =>
            _import.ProcessImportedQualitiesSet(qualityPairs);

        public void ProcessImportedAssemblersSet(IEnumerable<string> assemblerNames) =>
            _import.ProcessImportedAssemblersSet(assemblerNames);

        public void ProcessImportedModulesSet(IEnumerable<string> moduleNames) =>
            _import.ProcessImportedModulesSet(moduleNames);

        public void ProcessImportedBeaconsSet(IEnumerable<string> beaconNames) =>
            _import.ProcessImportedBeaconsSet(beaconNames);

        public Dictionary<long, IRecipe> ProcessImportedRecipesSet(IEnumerable<RecipeShort> recipeShorts) =>
            _import.ProcessImportedRecipesSet(recipeShorts);

        public Dictionary<long, IPlantProcess> ProcessImportedPlantProcessesSet(IEnumerable<PlantShort> plantShorts) =>
            _import.ProcessImportedPlantProcessesSet(plantShorts);
    }
}
