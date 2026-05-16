using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    /// <summary>Loaded Factorio preset data (items, recipes, technologies, entities).</summary>
    public class DataCache {
        private readonly DataCacheStore _store;
        private readonly DataCacheImportHandlers _import;

        public string? PresetName { get; private set; }

        public IEnumerable<Group> AvailableGroups => _store.Groups.Values.Where(g => g.Available);
        public IEnumerable<Subgroup> AvailableSubgroups => _store.Subgroups.Values.Where(g => g.Available);
        public IEnumerable<Quality> AvailableQualities => _store.Qualities.Values.Where(g => g.Available);
        public IEnumerable<Item> AvailableItems => _store.Items.Values.Where(g => g.Available);
        public IEnumerable<Recipe> AvailableRecipes => _store.Recipes.Values.Where(g => g.Available);
        public IEnumerable<PlantProcess> AvailablePlantProcesses => _store.PlantProcesses.Values.Where(g => g.Available);

        public IReadOnlyDictionary<string, string> IncludedMods => _store.IncludedMods;
        public IReadOnlyDictionary<string, Technology> Technologies => _store.Technologies;
        public IReadOnlyDictionary<string, Group> Groups => _store.Groups;
        public IReadOnlyDictionary<string, Subgroup> Subgroups => _store.Subgroups;
        public IReadOnlyDictionary<string, Quality> Qualities => _store.Qualities;
        public IReadOnlyDictionary<string, Item> Items => _store.Items;
        public IReadOnlyDictionary<string, Recipe> Recipes => _store.Recipes;
        public IReadOnlyDictionary<string, PlantProcess> PlantProcesses => _store.PlantProcesses;
        public IReadOnlyDictionary<string, Assembler> Assemblers => _store.Assemblers;
        public IReadOnlyDictionary<string, Module> Modules => _store.Modules;
        public IReadOnlyDictionary<string, Beacon> Beacons => _store.Beacons;
        public IReadOnlyList<Item> SciencePacks => _store.SciencePacks;
        public IReadOnlyDictionary<Item, ICollection<Item>> SciencePackPrerequisites => _store.SciencePackPrerequisites;

        public Assembler? PlayerAssembler => _store.PlayerAssembler;
        public Assembler? RocketAssembler => _store.RocketAssembler;
        public Technology? StartingTech => _store.StartingTech;

        public Subgroup? MissingSubgroup => _store.MissingSubgroup;
        public IReadOnlyDictionary<string, Quality> MissingQualities => _store.MissingQualities;
        public IReadOnlyDictionary<string, Item> MissingItems => _store.MissingItems;
        public IReadOnlyDictionary<string, Assembler> MissingAssemblers => _store.MissingAssemblers;
        public IReadOnlyDictionary<string, Module> MissingModules => _store.MissingModules;
        public IReadOnlyDictionary<string, Beacon> MissingBeacons => _store.MissingBeacons;
        public IReadOnlyDictionary<RecipeShort, Recipe> MissingRecipes => _store.MissingRecipes;
        public IReadOnlyDictionary<PlantShort, PlantProcess> MissingPlantProcesses => _store.MissingPlantProcesses;

        public Quality? DefaultQuality => _store.DefaultQuality;
        public uint QualityMaxChainLength => _store.QualityMaxChainLength;

        public static Bitmap UnknownIcon => IconCache.UnknownIcon;
        public static Bitmap NoBeaconIcon {
            get {
                if (field is null)
                    field = IconCache.GetIcon(Path.Combine("Graphics", "NoBeacon.png"), 64);
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

            _store.IconCache = loadIcons
                ? await IconCache.LoadIconCache(Path.Combine(Application.StartupPath, "Presets", preset.Name + ".dat"), progress, 0, 90)
                : new Dictionary<string, IconColorPair>();

            await Task.Run(() => {
                progress.Report(new KeyValuePair<int, string>(90, "Processing Data..."));

                var entityLoader = new EntityDataLoader(this, _store, session);
                var presetLoader = new PresetDataLoader(this, _store, session);
                presetLoader.LoadFromJson(jsonData, _store.IconCache);
                entityLoader.LoadEntities(jsonData, _store.IconCache);
                presetLoader.LoadRocketLaunches(jsonData);
                entityLoader.LoadCharacter(PresetJson.EnumerateArray(jsonData, "entities").FirstOrDefault(a => PresetJson.GetString(a, "name") == "character"), session.CraftingCategories);

                if (_store.RocketAssembler is not null)
                    _store.Assemblers.Add(_store.RocketAssembler.Name, _store.RocketAssembler);

                new DataCachePostLoadProcessor(this, _store).RunAfterPresetParsed();

                progress.Report(new KeyValuePair<int, string>(98, "Finalizing..."));
                progress.Report(new KeyValuePair<int, string>(100, "Done!"));
            });
        }

        public void Clear() => DataCacheBootstrap.ClearLoadedData(_store);

        public void ProcessImportedItemsSet(IEnumerable<string> itemNames) =>
            _import.ProcessImportedItemsSet(itemNames);

        public Dictionary<string, Quality?> ProcessImportedQualitiesSet(IEnumerable<KeyValuePair<string, int>> qualityPairs) =>
            _import.ProcessImportedQualitiesSet(qualityPairs);

        public void ProcessImportedAssemblersSet(IEnumerable<string> assemblerNames) =>
            _import.ProcessImportedAssemblersSet(assemblerNames);

        public void ProcessImportedModulesSet(IEnumerable<string> moduleNames) =>
            _import.ProcessImportedModulesSet(moduleNames);

        public void ProcessImportedBeaconsSet(IEnumerable<string> beaconNames) =>
            _import.ProcessImportedBeaconsSet(beaconNames);

        public Dictionary<long, Recipe> ProcessImportedRecipesSet(IEnumerable<RecipeShort> recipeShorts) =>
            _import.ProcessImportedRecipesSet(recipeShorts);

        public Dictionary<long, PlantProcess> ProcessImportedPlantProcessesSet(IEnumerable<PlantShort> plantShorts) =>
            _import.ProcessImportedPlantProcessesSet(plantShorts);
    }
}