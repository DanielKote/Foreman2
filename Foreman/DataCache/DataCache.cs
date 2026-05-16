using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public class DataCache {
        public string? PresetName { get; private set; }

        public IEnumerable<Group> AvailableGroups { get { return groups.Values.Where(g => g.Available); } }
        public IEnumerable<Subgroup> AvailableSubgroups { get { return subgroups.Values.Where(g => g.Available); } }
        public IEnumerable<Quality> AvailableQualities { get { return qualities.Values.Where(g => g.Available); } }
        public IEnumerable<Item> AvailableItems { get { return items.Values.Where(g => g.Available); } }
        public IEnumerable<Recipe> AvailableRecipes { get { return recipes.Values.Where(g => g.Available); } }
        public IEnumerable<PlantProcess> AvailablePlantProcesses { get { return plantProcesses.Values.Where(g => g.Available); } }

        //mods: <name, version>
        //others: <name, object>

        public IReadOnlyDictionary<string, string> IncludedMods { get { return includedMods; } }
        public IReadOnlyDictionary<string, Technology> Technologies { get { return technologies; } }
        public IReadOnlyDictionary<string, Group> Groups { get { return groups; } }
        public IReadOnlyDictionary<string, Subgroup> Subgroups { get { return subgroups; } }
        public IReadOnlyDictionary<string, Quality> Qualities { get { return qualities; } }
        public IReadOnlyDictionary<string, Item> Items { get { return items; } }
        public IReadOnlyDictionary<string, Recipe> Recipes { get { return recipes; } }
        public IReadOnlyDictionary<string, PlantProcess> PlantProcesses { get { return plantProcesses; } }
        public IReadOnlyDictionary<string, Assembler> Assemblers { get { return assemblers; } }
        public IReadOnlyDictionary<string, Module> Modules { get { return modules; } }
        public IReadOnlyDictionary<string, Beacon> Beacons { get { return beacons; } }
        public IReadOnlyList<Item> SciencePacks { get { return sciencePacks; } }
        public IReadOnlyDictionary<Item, ICollection<Item>> SciencePackPrerequisites { get { return sciencePackPrerequisites; } }

        public Assembler? PlayerAssembler { get { return playerAssembler; } }
        public Assembler? RocketAssembler { get { return rocketAssembler; } }
        public Technology? StartingTech { get { return startingTech; } }

        //missing objects are not linked properly and just have the minimal values necessary to function. They are just placeholders, and cant actually be added to graph except while importing. They are also not solved for.
        public Subgroup? MissingSubgroup { get { return missingSubgroup; } }
        public IReadOnlyDictionary<string, Quality> MissingQualities { get { return missingQualities; } }
        public IReadOnlyDictionary<string, Item> MissingItems { get { return missingItems; } }
        public IReadOnlyDictionary<string, Assembler> MissingAssemblers { get { return missingAssemblers; } }
        public IReadOnlyDictionary<string, Module> MissingModules { get { return missingModules; } }
        public IReadOnlyDictionary<string, Beacon> MissingBeacons { get { return missingBeacons; } }
        public IReadOnlyDictionary<RecipeShort, Recipe> MissingRecipes { get { return missingRecipes; } }
        public IReadOnlyDictionary<PlantShort, PlantProcess> MissingPlantProcesses { get { return missingPlantProcesses; } }

        public Quality? DefaultQuality { get; private set; }
        public uint QualityMaxChainLength { get; private set; }
        private Quality? ErrorQuality;

        public static Bitmap UnknownIcon => IconCache.UnknownIcon;
        public static Bitmap NoBeaconIcon {
            get {
                if (field is null)
                    field = IconCache.GetIcon(Path.Combine("Graphics", "NoBeacon.png"), 64);
                return field;
            }
        }

        private Dictionary<string, string> includedMods; //name : version
        private Dictionary<string, Technology> technologies;
        private Dictionary<string, Group> groups;
        private Dictionary<string, Subgroup> subgroups;
        private Dictionary<string, Quality> qualities;
        private Dictionary<string, Item> items;
        private Dictionary<string, Recipe> recipes;
        private Dictionary<string, PlantProcess> plantProcesses;
        private Dictionary<string, Assembler> assemblers;
        private Dictionary<string, Module> modules;
        private Dictionary<string, Beacon> beacons;
        private List<Item> sciencePacks;
        private Dictionary<Item, ICollection<Item>> sciencePackPrerequisites;

        private Dictionary<string, Quality> missingQualities;
        private Dictionary<string, Item> missingItems;
        private Dictionary<string, Assembler> missingAssemblers;
        private Dictionary<string, Module> missingModules;
        private Dictionary<string, Beacon> missingBeacons;
        private Dictionary<RecipeShort, Recipe> missingRecipes;
        private Dictionary<PlantShort, PlantProcess> missingPlantProcesses;

        private GroupPrototype? extraFormanGroup;
        private SubgroupPrototype? extractionSubgroupItems;
        private SubgroupPrototype? extractionSubgroupFluids;
        private SubgroupPrototype? extractionSubgroupFluidsOP; //offshore pumps
        private SubgroupPrototype? energySubgroupBoiling; //water to steam (boilers)
        private SubgroupPrototype? energySubgroupEnergy; //heat production (heat consumption is processed as 'fuel'), steam consumption, burning to energy
        private SubgroupPrototype? rocketLaunchSubgroup; //any rocket launch recipes will go here

        private ItemPrototype? HeatItem;
        private RecipePrototype? HeatRecipe;
        private RecipePrototype? BurnerRecipe; //for burner-generators

        private Bitmap? ElectricityIcon;

        private AssemblerPrototype? playerAssembler; //for hand crafting. Because Fk automation, thats why.
        private AssemblerPrototype? rocketAssembler; //for those rocket recipes

        private SubgroupPrototype? missingSubgroup;
        private TechnologyPrototype? startingTech;
        private AssemblerPrototype? missingAssembler; //missing recipes will have this set as their one and only assembler.

        private readonly bool UseRecipeBWLists;
        private static readonly Regex[] recipeWhiteList = { new Regex("^empty-barrel$") }; //whitelist takes priority over blacklist
        private static readonly Regex[] recipeBlackList = { new Regex("-barrel$"), new Regex("^deadlock-packrecipe-"), new Regex("^deadlock-unpackrecipe-"), new Regex("^deadlock-plastic-packaging$") };
        private static readonly KeyValuePair<string, Regex>[] recyclingItemNameBlackList = { new KeyValuePair<string, Regex>("barrel", new Regex("-barrel$")) };

        private Dictionary<string, IconColorPair>? iconCache;

        private static readonly double MaxTemp = 10000000; //some mods set the temperature ranges as 'way too high' and expect factorio to handle it (it does). Since we prefer to show temperature ranges we will define any temp beyond these as no limit
        private static readonly double MinTemp = -MaxTemp;

        public DataCache(bool filterRecipes) //if true then the read recipes will be filtered by the white and black lists above. In most cases this is desirable (why bother with barreling, etc???), but if the user want to use them, then so be it.
        {
            UseRecipeBWLists = filterRecipes;

            includedMods = new Dictionary<string, string>();
            technologies = new Dictionary<string, Technology>();
            groups = new Dictionary<string, Group>();
            subgroups = new Dictionary<string, Subgroup>();
            qualities = new Dictionary<string, Quality>();
            items = new Dictionary<string, Item>();
            recipes = new Dictionary<string, Recipe>();
            plantProcesses = new Dictionary<string, PlantProcess>();
            assemblers = new Dictionary<string, Assembler>();
            modules = new Dictionary<string, Module>();
            beacons = new Dictionary<string, Beacon>();
            sciencePacks = new List<Item>();
            sciencePackPrerequisites = new Dictionary<Item, ICollection<Item>>();

            missingQualities = new Dictionary<string, Quality>();
            missingItems = new Dictionary<string, Item>();
            missingAssemblers = new Dictionary<string, Assembler>();
            missingModules = new Dictionary<string, Module>();
            missingBeacons = new Dictionary<string, Beacon>();
            missingRecipes = new Dictionary<RecipeShort, Recipe>(new RecipeShortNaInPrComparer());
            missingPlantProcesses = new Dictionary<PlantShort, PlantProcess>();

            GenerateHelperObjects();
            Clear();
        }

        private void GenerateHelperObjects() {
            startingTech = new TechnologyPrototype(this, "§§t:starting_tech", "Starting Technology");
            startingTech.Tier = 0;

            extraFormanGroup = new GroupPrototype(this, "§§g:extra_group", "Resource Extraction\nPower Generation\nRocket Launches", "~~~z1");
            extraFormanGroup.SetIconAndColor(new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "ExtraGroupIcon.png"), 64), Color.Gray));

            extractionSubgroupItems = new SubgroupPrototype(this, "§§sg:extraction_items", "1");
            extractionSubgroupItems.myGroup = extraFormanGroup;
            extraFormanGroup.subgroups.Add(extractionSubgroupItems);

            extractionSubgroupFluids = new SubgroupPrototype(this, "§§sg:extraction_fluids", "2");
            extractionSubgroupFluids.myGroup = extraFormanGroup;
            extraFormanGroup.subgroups.Add(extractionSubgroupFluids);

            extractionSubgroupFluidsOP = new SubgroupPrototype(this, "§§sg:extraction_fluids_2", "3");
            extractionSubgroupFluidsOP.myGroup = extraFormanGroup;
            extraFormanGroup.subgroups.Add(extractionSubgroupFluidsOP);

            energySubgroupBoiling = new SubgroupPrototype(this, "§§sg:energy_boiling", "4");
            energySubgroupBoiling.myGroup = extraFormanGroup;
            extraFormanGroup.subgroups.Add(energySubgroupBoiling);

            energySubgroupEnergy = new SubgroupPrototype(this, "§§sg:energy_heat", "5");
            energySubgroupEnergy.myGroup = extraFormanGroup;
            extraFormanGroup.subgroups.Add(energySubgroupEnergy);

            rocketLaunchSubgroup = new SubgroupPrototype(this, "§§sg:rocket_launches", "6");
            rocketLaunchSubgroup.myGroup = extraFormanGroup;
            extraFormanGroup.subgroups.Add(rocketLaunchSubgroup);

            ErrorQuality = new QualityPrototype(this, "§§error_quality", "ERROR", "-");

            IconColorPair heatIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "HeatIcon.png"), 64), Color.DarkRed);
            IconColorPair burnerGeneratorIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "BurnerGeneratorIcon.png"), 64), Color.DarkRed);
            IconColorPair playerAssemblerIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "PlayerAssembler.png"), 64), Color.Gray);
            IconColorPair rocketAssemblerIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "RocketAssembler.png"), 64), Color.Gray);
            HeatItem = new ItemPrototype(this, "§§i:heat", "Heat (1MJ)", new SubgroupPrototype(this, "-", "-"), "-"); //we dont want heat to appear as an item in the lists, so just give it a blank subgroup.
            HeatItem.SetIconAndColor(heatIcon);
            HeatItem.FuelValue = 1000000; //1MJ - nice amount

            HeatRecipe = new RecipePrototype(this, "§§r:h:heat-generation", "Heat Generation", energySubgroupEnergy, "1");
            HeatRecipe.SetIconAndColor(heatIcon);
            HeatRecipe.InternalOneWayAddProduct(HeatItem, 1, 0);
            HeatItem.productionRecipes.Add(HeatRecipe);
            HeatRecipe.Time = 1;

            BurnerRecipe = new RecipePrototype(this, "§§r:h:burner-electicity", "Burner Generator", energySubgroupEnergy, "2");
            BurnerRecipe.SetIconAndColor(burnerGeneratorIcon);
            BurnerRecipe.Time = 1;

            playerAssembler = new AssemblerPrototype(this, "§§a:player-assembler", "Player", EntityType.Assembler, EnergySource.Void);
            playerAssembler.energyDrain = 0;
            playerAssembler.SetIconAndColor(playerAssemblerIcon);

            rocketAssembler = new AssemblerPrototype(this, "§§a:rocket-assembler", "Rocket", EntityType.Rocket, EnergySource.Void);
            rocketAssembler.energyDrain = 0;
            rocketAssembler.SetIconAndColor(rocketAssemblerIcon);

            ElectricityIcon = IconCache.GetIcon(Path.Combine("Graphics", "ElectricityIcon.png"), 64);

            missingSubgroup = new SubgroupPrototype(this, "§§MISSING-SG", "");
            missingSubgroup.myGroup = new GroupPrototype(this, "§§MISSING-G", "MISSING", "");

            missingAssembler = new AssemblerPrototype(this, "§§a:MISSING-A", "missing assembler", EntityType.Assembler, EnergySource.Void, true);
        }

        public async Task LoadAllData(Preset preset, IProgress<KeyValuePair<int, string>> progress, bool loadIcons = true) {
            Clear();
            //return;

            Dictionary<string, List<RecipePrototype>> craftingCategories = new Dictionary<string, List<RecipePrototype>>();
            Dictionary<string, List<ModulePrototype>> moduleCategories = new Dictionary<string, List<ModulePrototype>>();
            Dictionary<string, List<RecipePrototype>> resourceCategories = new Dictionary<string, List<RecipePrototype>>();
            resourceCategories.Add("<<foreman_resource_category_water_tile>>", new List<RecipePrototype>()); //the water resources
            Dictionary<string, List<ItemPrototype>> fuelCategories = new Dictionary<string, List<ItemPrototype>>();
            fuelCategories.Add("§§fc:liquids", new List<ItemPrototype>()); //the liquid fuels category
            Dictionary<Item, string> burnResults = new Dictionary<Item, string>();
            Dictionary<Item, string> spoilResults = new Dictionary<Item, string>();
            Dictionary<Quality, string> nextQualities = new Dictionary<Quality, string>();
            List<Recipe> miningWithFluidRecipes = new List<Recipe>();

            PresetName = preset.Name;
            JObject jsonData = PresetProcessor.PrepPreset(preset);
            if (jsonData == null)
                return;

            iconCache = loadIcons ? await IconCache.LoadIconCache(Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".dat" }), progress, 0, 90) : new Dictionary<string, IconColorPair>();

            await Task.Run(() => {
                progress.Report(new KeyValuePair<int, string>(90, "Processing Data...")); //this is SUPER quick, so we dont need to worry about timing stuff here

                //process each section (order is rather important here)
                foreach (var objJToken in jsonData["mods"]?.ToList() ?? [])
                    ProcessMod(objJToken);

                foreach (var objJToken in jsonData["subgroups"]?.ToList() ?? [])
                    ProcessSubgroup(objJToken);

                foreach (var objJToken in jsonData["groups"]?.ToList() ?? [])
                    ProcessGroup(objJToken, iconCache);

                foreach (var objToken in jsonData["qualities"]?.ToList() ?? [])
                    ProcessQuality(objToken, iconCache, nextQualities);
                foreach (QualityPrototype quality in qualities.Values.Cast<QualityPrototype>())
                    ProcessQualityLink(quality, nextQualities);
                PostProcessQuality();

                foreach (var objJToken in jsonData["fluids"]?.ToList() ?? [])
                    ProcessFluid(objJToken, iconCache, fuelCategories);

                foreach (var objJToken in jsonData["items"]?.ToList() ?? [])
                    ProcessItem(objJToken, iconCache, fuelCategories, burnResults, spoilResults); //items after fluids to take care of duplicates (if name exists in fluid and in item set, then only the fluid is counted)
                foreach (ItemPrototype item in items.Values.Cast<ItemPrototype>())
                    ProcessBurnItem(item, burnResults); //link up any items with burn remains
                foreach (var objJToken in jsonData["items"]?.ToList() ?? [])
                    ProcessPlantProcess(objJToken); //process items json specifically for plant processes (items should all be populated by now)
                foreach (ItemPrototype item in items.Values.Cast<ItemPrototype>())
                    ProcessSpoilItem(item, spoilResults); //link up any items with spoil remains

                foreach (var objJToken in jsonData["modules"]?.ToList() ?? [])
                    ProcessModule(objJToken, iconCache, moduleCategories);

                foreach (var objJToken in jsonData["recipes"]?.ToList() ?? [])
                    ProcessRecipe(objJToken, iconCache, craftingCategories, moduleCategories);

                foreach (var objJToken in jsonData["resources"]?.ToList() ?? [])
                    ProcessResource(objJToken, resourceCategories, miningWithFluidRecipes);
                foreach (var objToken in jsonData["water_resources"]?.ToList() ?? [])
                    ProcessResource(objToken, resourceCategories, miningWithFluidRecipes);

                foreach (var objJToken in jsonData["technologies"]?.ToList() ?? [])
                    ProcessTechnology(objJToken, iconCache, miningWithFluidRecipes);
                foreach (var objJToken in jsonData["technologies"]?.ToList() ?? [])
                    ProcessTechnologyP2(objJToken); //required to properly link technology prerequisites

                foreach (var objJToken in jsonData["entities"]?.ToList() ?? [])
                    ProcessEntity(objJToken, iconCache, craftingCategories, resourceCategories, fuelCategories, miningWithFluidRecipes, moduleCategories);

                //process launch products (empty now - depreciated)
                foreach (var objJToken in jsonData["items"]?.Where(t => t["rocket_launch_products"] != null).ToList() ?? [])
                    ProcessRocketLaunch(objJToken);
                foreach (var objJToken in jsonData["fluids"]?.Where(t => t["rocket_launch_products"] != null).ToList() ?? [])
                    ProcessRocketLaunch(objJToken);

                //process character
                ProcessCharacter(jsonData["entities"]?.First(a => (string?)a["name"] == "character"), craftingCategories);

                //add rocket assembler
                if (rocketAssembler is not null)
                    assemblers.Add(rocketAssembler.Name, rocketAssembler);

                //remove these temporary dictionaries (no longer necessary)
                craftingCategories.Clear();
                resourceCategories.Clear();
                fuelCategories.Clear();
                burnResults.Clear();
                spoilResults.Clear();


                //sort
                foreach (GroupPrototype g in groups.Values.Cast<GroupPrototype>())
                    g.SortSubgroups();
                foreach (SubgroupPrototype sg in subgroups.Values.Cast<SubgroupPrototype>())
                    sg.SortIRs();

                //The data read by the dataCache (json preset) includes everything. We need to now process it such that any items/recipes that cant be used dont appear.
                //thus any object that has Unavailable set to true should be ignored. We will leave the option to use them to the user, but in most cases its better without them


                //delete any recipe that has no assembler. This is the only type of deletion that we will do, as we MUST enforce the 'at least 1 assembler' per recipe. The only recipes with no assemblers linked are those added to 'missing' category, and those are handled separately.
                //note that even hand crafting has been handled: there is a player assembler that has been added. So the only recipes removed here are those that literally can not be crafted.
                foreach (RecipePrototype recipe in recipes.Values.Where(r => r.Assemblers.Count == 0).ToList().Cast<RecipePrototype>()) {
                    foreach (ItemPrototype ingredient in recipe.ingredientList)
                        ingredient.consumptionRecipes.Remove(recipe);
                    foreach (ItemPrototype product in recipe.productList)
                        product.productionRecipes.Remove(recipe);
                    foreach (TechnologyPrototype tech in recipe.myUnlockTechnologies)
                        tech.unlockedRecipes.Remove(recipe);
                    foreach (ModulePrototype module in recipe.assemblerModules)
                        module.recipes.Remove(recipe);
                    recipe.mySubgroup.recipes.Remove(recipe);

                    recipes.Remove(recipe.Name);
                    ErrorLogging.LogLine(string.Format("Removal of {0} due to having no assemblers associated with it.", recipe));
                    Console.WriteLine(string.Format("Removal of {0} due to having no assemblers associated with it.", recipe));
                }

                //calculate the availability of various recipes and entities (based on their unlock technologies + entity place objects' unlock technologies)
                ProcessAvailableStatuses();

                //calculate the science packs for each technology (based on both their listed science packs, the science packs of their prerequisites, and the science packs required to research the science packs)
                ProcessSciencePacks();

                //delete any groups/subgroups without any items/recipes within them, and sort by order
                CleanupGroups();

                //check each fluid to see if all production recipe temperatures can fit within all consumption recipe ranges. if not, then the item / fluid is set to be 'temperature dependent' and requires further processing when checking link validity.
                UpdateFluidTemperatureDependencies();

#if DEBUG
                //PrintDataCache();
#endif

                progress.Report(new KeyValuePair<int, string>(98, "Finalizing..."));
                progress.Report(new KeyValuePair<int, string>(100, "Done!"));
            });
        }

        public void Clear() {
            DefaultQuality = ErrorQuality;

            includedMods.Clear();
            technologies.Clear();
            groups.Clear();
            subgroups.Clear();
            items.Clear();
            recipes.Clear();
            plantProcesses.Clear();
            assemblers.Clear();
            modules.Clear();
            beacons.Clear();

            missingItems.Clear();
            missingAssemblers.Clear();
            missingModules.Clear();
            missingBeacons.Clear();
            missingRecipes.Clear();
            missingPlantProcesses.Clear();

            if (iconCache != null) {
                foreach (var iconset in iconCache.Values)
                    iconset.Icon?.Dispose();
                iconCache.Clear();
            }

            if (extraFormanGroup is not null)
                groups.Add(extraFormanGroup.Name, extraFormanGroup);
            if (extractionSubgroupItems is not null)
                subgroups.Add(extractionSubgroupItems.Name, extractionSubgroupItems);
            if (extractionSubgroupFluids is not null)
                subgroups.Add(extractionSubgroupFluids.Name, extractionSubgroupFluids);
            if (extractionSubgroupFluidsOP is not null)
                subgroups.Add(extractionSubgroupFluidsOP.Name, extractionSubgroupFluidsOP);
            if (HeatItem is not null)
                items.Add(HeatItem.Name, HeatItem);
            if (HeatRecipe is not null)
                recipes.Add(HeatRecipe.Name, HeatRecipe);
            if (BurnerRecipe is not null)
                recipes.Add(BurnerRecipe.Name, BurnerRecipe);
            if (StartingTech is not null && startingTech is not null)
                technologies.Add(StartingTech.Name, startingTech);
        }

        //------------------------------------------------------Import processing

        public void ProcessImportedItemsSet(IEnumerable<string> itemNames) //will ensure that all items are now part of the data cache -> existing ones (regular and missing) are skipped, new ones are added to MissingItems
        {
            foreach (string iItem in itemNames) {
                if (missingSubgroup is not null && !items.ContainsKey(iItem) && !missingItems.ContainsKey(iItem)) //want to check for missing items too - in this case dont want duplicates
                {
                    ItemPrototype missingItem = new ItemPrototype(this, iItem, iItem, missingSubgroup, "", true); //just assume it isnt a fluid. we dont honestly care (no temperatures)
                    missingItems.Add(missingItem.Name, missingItem);
                }
            }
        }

        public Dictionary<string, Quality?> ProcessImportedQualitiesSet(IEnumerable<KeyValuePair<string, int>> qualityPairs) {
            //check that a quality exists in the set of qualities (missing or otherwise) that has the correct level; if not, make a new one
            Dictionary<string, Quality?> qualityMap = new();

            foreach (var quality in qualityPairs) {
                //check quality sets for any direct matches (name & level)
                if (qualities.Values.Any(q => q.Name == quality.Key && q.Level == quality.Value)) {
                    qualityMap.Add(quality.Key, qualities[quality.Key]);
                    continue;
                } else if (missingQualities.Values.Any(q => q.Name == quality.Key && q.Level == quality.Value)) {
                    qualityMap.Add(quality.Key, missingQualities[quality.Key]);
                    continue;
                }

                //check for any matching level quality in the base chain (starting from 'normal' and going until null)
                Quality? curQuality = DefaultQuality;
                while (curQuality != null) {
                    if (curQuality.Level == quality.Value)
                        break;
                    curQuality = curQuality.NextQuality;
                }
                if (curQuality != null) {
                    qualityMap.Add(quality.Key, curQuality);
                    continue;
                }

                //step 3: check if there is a quality of the same level
                curQuality = Qualities.Values.FirstOrDefault(q => q.Level == quality.Value);
                if (curQuality != null) {
                    qualityMap.Add(quality.Key, curQuality);
                    continue;
                }
                curQuality = MissingQualities.Values.FirstOrDefault(q => q.Level == quality.Value);
                if (curQuality != null) {
                    qualityMap.Add(quality.Key, curQuality);
                    continue;
                }

                //step 4: no other option, make a new quality and add it to missing qualities
                string missingQualityName = quality.Key;
                while (qualities.ContainsKey(missingQualityName) || missingQualities.ContainsKey(missingQualityName))
                    missingQualityName += "_";

                QualityPrototype missingQuality = new QualityPrototype(this, missingQualityName, quality.Key, "-", true);
                missingQualities.Add(missingQuality.Name, missingQuality);
                qualityMap.Add(quality.Key, curQuality);
            }

            return qualityMap;
        }

        public void ProcessImportedAssemblersSet(IEnumerable<string> assemblerNames) {
            foreach (string iAssembler in assemblerNames) {
                if (!assemblers.ContainsKey(iAssembler) && !missingAssemblers.ContainsKey(iAssembler)) {
                    AssemblerPrototype missingAssembler = new AssemblerPrototype(this, iAssembler, iAssembler, EntityType.Assembler, EnergySource.Void, true); //dont know, dont care about entity type we will just treat it as a void-assembler (and let fuel io + recipe figure it out)
                    missingAssemblers.Add(missingAssembler.Name, missingAssembler);
                }
            }
        }

        public void ProcessImportedModulesSet(IEnumerable<string> moduleNames) {
            foreach (string iModule in moduleNames) {
                if (!modules.ContainsKey(iModule) && !missingModules.ContainsKey(iModule)) {
                    ModulePrototype missingModule = new ModulePrototype(this, iModule, iModule, true);
                    missingModules.Add(missingModule.Name, missingModule);
                }
            }
        }

        public void ProcessImportedBeaconsSet(IEnumerable<string> beaconNames) {
            foreach (string iBeacon in beaconNames) {
                if (!beacons.ContainsKey(iBeacon) && !missingBeacons.ContainsKey(iBeacon)) {
                    BeaconPrototype missingBeacon = new BeaconPrototype(this, iBeacon, iBeacon, EnergySource.Void, true);
                    missingBeacons.Add(missingBeacon.Name, missingBeacon);
                }
            }
        }

        public Dictionary<long, Recipe> ProcessImportedRecipesSet(IEnumerable<RecipeShort> recipeShorts) //will ensure all recipes are now part of the data cache -> each one is checked against existing recipes (regular & missing), and if it doesnt exist are added to MissingRecipes. Returns a set of links of original recipeID (NOT! the noew recipeIDs) to the recipe
        {
            Dictionary<long, Recipe> recipeLinks = new();
            foreach (RecipeShort recipeShort in recipeShorts) {
                Recipe? recipe = null;

                //recipe check #1 : does its name exist in database (note: we dont quite care about extra missing recipes here - so what if we have a couple identical ones? they will combine during save/load anyway)
                bool recipeExists = recipes.ContainsKey(recipeShort.Name);
                if (recipeExists) {
                    //recipe check #2 : do the number of ingredients & products match?
                    recipe = recipes[recipeShort.Name];
                    recipeExists &= recipeShort.Ingredients.Count == recipe.IngredientList.Count;
                    recipeExists &= recipeShort.Products.Count == recipe.ProductList.Count;
                }
                if (recipeExists) {
                    //recipe check #3 : do the ingredients & products from the loaded data match the actual recipe? (names, not quantities -> this is to allow some recipes to pass; ex: normal->expensive might change the values, but importing such a recipe should just use the 'correct' quantities and soft-pass the different recipe)
                    foreach (string ingredient in recipeShort.Ingredients.Keys)
                        recipeExists &= items.ContainsKey(ingredient) && recipe is not null && recipe.IngredientSet.ContainsKey(items[ingredient]);
                    foreach (string product in recipeShort.Products.Keys)
                        recipeExists &= items.ContainsKey(product) && recipe is not null && recipe.ProductSet.ContainsKey(items[product]);
                }
                if (!recipeExists) {
                    bool missingRecipeExists = missingRecipes.ContainsKey(recipeShort);

                    if (missingRecipeExists) {
                        recipe = missingRecipes[recipeShort];
                    } else if (missingSubgroup is not null && missingAssembler is not null) {
                        RecipePrototype missingRecipe = new(this, recipeShort.Name, recipeShort.Name, missingSubgroup, "", true);
                        foreach (var ingredient in recipeShort.Ingredients) {
                            if (items.ContainsKey(ingredient.Key))
                                missingRecipe.InternalOneWayAddIngredient((ItemPrototype)items[ingredient.Key], ingredient.Value);
                            else
                                missingRecipe.InternalOneWayAddIngredient((ItemPrototype)missingItems[ingredient.Key], ingredient.Value);
                        }
                        foreach (var product in recipeShort.Products) {
                            if (items.ContainsKey(product.Key))
                                missingRecipe.InternalOneWayAddProduct((ItemPrototype)items[product.Key], product.Value, 0);
                            else
                                missingRecipe.InternalOneWayAddProduct((ItemPrototype)missingItems[product.Key], product.Value, 0);
                        }
                        missingRecipe.assemblers.Add(missingAssembler);
                        missingAssembler.recipes.Add(missingRecipe);

                        missingRecipes.Add(recipeShort, missingRecipe);
                        recipe = missingRecipe;
                    }
                }
                if (!recipeLinks.ContainsKey(recipeShort.RecipeID) && recipe is not null)
                    recipeLinks.Add(recipeShort.RecipeID, recipe);
            }
            return recipeLinks;
        }

        //pretty much a copy of the above, just for plant processes (so no ingredient list, and using different data sets)
        public Dictionary<long, PlantProcess> ProcessImportedPlantProcessesSet(IEnumerable<PlantShort> plantShorts) {
            Dictionary<long, PlantProcess> plantLinks = new Dictionary<long, PlantProcess>();
            foreach (PlantShort plantShort in plantShorts) {
                PlantProcess? pprocess = null;

                //recipe check #1 : does its name exist in database (note: we dont quite care about extra missing recipes here - so what if we have a couple identical ones? they will combine during save/load anyway)
                bool pprocessExists = plantProcesses.ContainsKey(plantShort.Name);
                if (pprocessExists) {
                    //recipe check #2 : do the number of ingredients & products match?
                    pprocess = plantProcesses[plantShort.Name];
                    pprocessExists &= plantShort.Products.Count == pprocess.ProductList.Count;
                }
                if (pprocessExists) {
                    //recipe check #3 : do the ingredients & products from the loaded data match the actual recipe? (names, not quantities -> this is to allow some recipes to pass; ex: normal->expensive might change the values, but importing such a recipe should just use the 'correct' quantities and soft-pass the different recipe)
                    foreach (string product in plantShort.Products.Keys)
                        pprocessExists &= items.ContainsKey(product) && pprocess is not null && pprocess.ProductSet.ContainsKey(items[product]);
                }
                if (!pprocessExists) {
                    bool missingPProcessExists = missingPlantProcesses.ContainsKey(plantShort);

                    if (missingPProcessExists) {
                        pprocess = missingPlantProcesses[plantShort];
                    } else {
                        PlantProcessPrototype missingPProcess = new PlantProcessPrototype(this, plantShort.Name, true);
                        foreach (var product in plantShort.Products) {
                            if (items.ContainsKey(product.Key))
                                missingPProcess.InternalOneWayAddProduct((ItemPrototype)items[product.Key], product.Value);
                            else
                                missingPProcess.InternalOneWayAddProduct((ItemPrototype)missingItems[product.Key], product.Value);
                        }

                        missingPlantProcesses.Add(plantShort, missingPProcess);
                        pprocess = missingPProcess;
                    }
                }
                if (pprocess is not null && !plantLinks.ContainsKey(plantShort.PlantID))
                    plantLinks.Add(plantShort.PlantID, pprocess);
            }
            return plantLinks;
        }

        //------------------------------------------------------Data cache load helper functions (all the process functions from LoadAllData)

        private void ProcessMod(JToken objJToken) {
            if ((string?)objJToken["name"] is string name && (string?)objJToken["version"] is string version)
                includedMods.Add(name, version);
        }

        private void ProcessSubgroup(JToken objJToken) {
            if ((string?)objJToken["name"] is not string name || (string?)objJToken["order"] is not string order)
                return;
            SubgroupPrototype subgroup = new SubgroupPrototype(
                this,
                name,
                order);

            subgroups.Add(subgroup.Name, subgroup);
        }

        private void ProcessGroup(JToken objJToken, Dictionary<string, IconColorPair> iconCache) {
            if ((string?)objJToken["name"] is not string name ||
                (string?)objJToken["localised_name"] is not string localisedName ||
                (string?)objJToken["order"] is not string order ||
                (string?)objJToken["icon_name"] is not string iconName ||
                objJToken["subgroups"] is not JToken subgroupsArr)
                return;
            GroupPrototype group = new(this, name, localisedName, order);

            if (iconCache.TryGetValue(iconName, out var icp))
                group.SetIconAndColor(icp);

            foreach (var subgroupJToken in subgroupsArr) {
                if ((string?)subgroupJToken is not string s || subgroups[s] is not SubgroupPrototype sgp)
                    continue;
                sgp.myGroup = group;
                group.subgroups.Add(sgp);
            }
            groups.Add(group.Name, group);
        }

        private void ProcessQuality(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<Quality, string> nextQualities) {
            if ((string?)objJToken["name"] is not string name ||
                (string?)objJToken["localised_name"] is not string localisedName ||
                (string?)objJToken["order"] is not string order ||
                (string?)objJToken["icon_name"] is not string iconName ||
                (bool?)objJToken["hidden"] is not bool hidden ||
                (int?)objJToken["level"] is not int level ||
                (double?)objJToken["beacon_power_multiplier"] is not double beaconPowerMult ||
                (double?)objJToken["mining_drill_resource_drain_multiplier"] is not double miningDrillResDrainMult)
                return;
            QualityPrototype quality = new(this, name, localisedName, order);

            if (iconCache.TryGetValue(iconName, out var icp))
                quality.SetIconAndColor(icp);

            quality.Available = !hidden;
            quality.Enabled = quality.Available; //can be set via science packs, but this requires modifying datacache... so later

            quality.Level = level;
            quality.BeaconPowerMultiplier = beaconPowerMult;
            quality.MiningDrillResourceDrainMultiplier = miningDrillResDrainMult;
            quality.NextProbability = (double?)objJToken["next_probability"] ?? 0;

            if (quality.NextProbability != 0 && (string?)objJToken["next"] is string next)
                nextQualities.Add(quality, next);

            qualities.Add(quality.Name, quality);
        }

        private void ProcessQualityLink(QualityPrototype quality, Dictionary<Quality, string> nextQualities) {

            if (nextQualities.ContainsKey(quality) && qualities.ContainsKey(nextQualities[quality])) {
                quality.NextQuality = qualities[nextQualities[quality]];
                ((QualityPrototype)qualities[nextQualities[quality]]).PrevQuality = quality;
            }
        }

        private void PostProcessQuality() {
            //make sure that the default quality is always enabled & available
            DefaultQuality = qualities.ContainsKey("normal") ? qualities["normal"] : ErrorQuality;
            DefaultQuality?.Enabled = true;
            (DefaultQuality as QualityPrototype)?.Available = true;

            //make available all qualities that are within the defaultquality chain
            Quality? cQuality = DefaultQuality;
            while (cQuality is not null) {
                ((QualityPrototype)cQuality).Available = cQuality.Enabled;
                cQuality = cQuality.NextQuality;
            }

            Quality currentQuality;
            uint currentChain;
            foreach (Quality quality in qualities.Values) {
                currentChain = 1;
                currentQuality = quality;
                while (currentQuality.NextQuality != null && currentQuality.NextProbability != 0) {
                    currentChain++;
                    currentQuality = currentQuality.NextQuality;
                }
                QualityMaxChainLength = Math.Max(QualityMaxChainLength, currentChain);
            }
        }

        private void ProcessFluid(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<ItemPrototype>> fuelCategories) {
            if ((string?)objJToken["name"] is not string name ||
                (string?)objJToken["localised_name"] is not string localisedName ||
                (string?)objJToken["subgroup"] is not string subgroupStr ||
                (string?)objJToken["order"] is not string order ||
                (string?)objJToken["icon_name"] is not string iconName ||
                (double?)objJToken["default_temperature"] is not double defaultTemp ||
                (double?)objJToken["heat_capacity"] is not double heatCapacity ||
                (double?)objJToken["gas_temperature"] is not double gasTemp ||
                (double?)objJToken["max_temperature"] is not double maxTemp ||
                subgroups[subgroupStr] is not SubgroupPrototype sgProto)
                return;
            FluidPrototype item = new(this, name, localisedName, sgProto, order);

            if (iconCache.TryGetValue(iconName, out var icp))
                item.SetIconAndColor(icp);

            item.DefaultTemperature = defaultTemp;
            item.SpecificHeatCapacity = heatCapacity;
            item.GasTemperature = gasTemp;
            item.MaxTemperature = maxTemp;

            if ((double?)objJToken["fuel_value"] is double fuelValue && fuelValue > 0) {
                item.FuelValue = fuelValue;
                item.PollutionMultiplier = (double?)objJToken["emissions_multiplier"] ?? 1;
                fuelCategories["§§fc:liquids"].Add(item);
            }

            items.Add(item.Name, item);
        }

        private void ProcessItem(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<ItemPrototype>> fuelCategories, Dictionary<Item, string> burnResults, Dictionary<Item, string> spoilResults) {
            if ((string?)objJToken["name"] is not string name ||
                items.ContainsKey(name) ||
                (string?)objJToken["localised_name"] is not string localisedName ||
                (string?)objJToken["subgroup"] is not string subgroupStr ||
                (string?)objJToken["order"] is not string order ||
                (int?)objJToken["stack_size"] is not int stackSize ||
                (double?)objJToken["weight"] is not double weight ||
                (double?)objJToken["ingredient_to_weight_coefficient"] is not double ingredientToWeightCoeff ||
                subgroups[subgroupStr] is not SubgroupPrototype sgProto
                ) //special handling for fluids which appear in both items & fluid lists (ex: fluid-unknown)
                return;

            ItemPrototype item = new(this, name, localisedName, sgProto, order);

            if ((string?)objJToken["icon_name"] is string iconName && iconCache.TryGetValue(iconName, out var icp))
                item.SetIconAndColor(icp);

            item.StackSize = stackSize;
            item.Weight = weight;
            item.IngredientToWeightCoefficient = ingredientToWeightCoeff;

            if ((string?)objJToken["fuel_category"] is string fuelCategory &&
                (double?)objJToken["fuel_value"] is double fuelValue &&
                fuelValue > 0) //factorio eliminates any 0fuel value fuel from the list (checked)
            {
                item.FuelValue = fuelValue;
                item.PollutionMultiplier = (double?)objJToken["fuel_emissions_multiplier"] ?? 1;

                if (!fuelCategories.ContainsKey(fuelCategory))
                    fuelCategories.Add(fuelCategory, new());
                fuelCategories[fuelCategory].Add(item);
            }
            if ((string?)objJToken["burnt_result"] is string burntResult)
                burnResults.Add(item, burntResult);
            if ((string?)objJToken["spoil_result"] is string spoilResult && objJToken["q_spoil_time"] is JToken qSpoilTime) {
                spoilResults.Add(item, spoilResult);
                foreach (JToken spoilToken in qSpoilTime)
                    if ((string?)spoilToken["quality"] is string quality && (double?)spoilToken["value"] is double value)
                        item.spoilageTimes.Add(qualities[quality], value);
            }

            items.Add(item.Name, item);
        }

        private void ProcessBurnItem(ItemPrototype item, Dictionary<Item, string> burnResults) {
            if (burnResults.ContainsKey(item)) {
                item.BurnResult = items[burnResults[item]];
                ((ItemPrototype)items[burnResults[item]]).FuelOrigin = item;
            }
        }
        private void ProcessPlantProcess(JToken objJToken) {
            if (objJToken["plant_results"] is JToken plantResults &&
                (string?)objJToken["name"] is string name &&
                (double?)objJToken["plant_growth_time"] is double growTime) {
                ItemPrototype seed = (ItemPrototype)items[name];
                PlantProcessPrototype plantProcess = new PlantProcessPrototype(
                    this,
                    seed.Name);

                plantProcess.Seed = seed;
                plantProcess.GrowTime = growTime;

                foreach (var productJToken in plantResults) {
                    if ((string?)productJToken["name"] is not string prodName)
                        continue;
                    ItemPrototype product = (ItemPrototype)items[prodName];
                    double amount = (double?)productJToken["amount"] ?? 0;
                    if (amount != 0) {
                        plantProcess.InternalOneWayAddProduct(product, amount);
                        product.plantOrigins.Add(seed);
                        seed.PlantResult = plantProcess;
                    }
                }

                plantProcesses.Add(seed.Name, plantProcess); //seed.Name = plantProcess.name, but for clarity: any searches will be done via seed's name
            }
        }
        private void ProcessSpoilItem(ItemPrototype item, Dictionary<Item, string> spoilResults) {
            if (spoilResults.ContainsKey(item)) {
                item.SpoilResult = items[spoilResults[item]];
                ((ItemPrototype)items[spoilResults[item]]).spoilOrigins.Add(item);
            }
        }

        private void ProcessModule(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<ModulePrototype>> moduleCategories) {
            if ((string?)objJToken["name"] is not string name ||
                (string?)objJToken["localised_name"] is not string localisedName ||
                (int?)objJToken["tier"] is not int tier ||
                (string?)objJToken["category"] is not string category)
                return;
            ModulePrototype module = new ModulePrototype(this, name, localisedName);

            if ((string?)objJToken["icon_name"] is string iconName && iconCache.TryGetValue(iconName, out var icp))
                module.SetIconAndColor(icp);
            else if ((string?)objJToken["icon_alt_name"] is string iconAltName && iconCache.TryGetValue(iconAltName, out var icpAlt))
                module.SetIconAndColor(icpAlt);

            module.SpeedBonus = Math.Round(((double?)objJToken["module_effects"]?["speed"] ?? default) * 1000, 0, MidpointRounding.AwayFromZero) / 1000;
            module.ProductivityBonus = Math.Round(((double?)objJToken["module_effects"]?["productivity"] ?? default) * 1000, 0, MidpointRounding.AwayFromZero) / 1000;
            module.ConsumptionBonus = Math.Round(((double?)objJToken["module_effects"]?["consumption"] ?? default) * 1000, 0, MidpointRounding.AwayFromZero) / 1000;
            module.PollutionBonus = Math.Round(((double?)objJToken["module_effects"]?["pollution"] ?? default) * 1000, 0, MidpointRounding.AwayFromZero) / 1000;
            module.QualityBonus = Math.Round(((double?)objJToken["module_effects"]?["quality"] ?? default) * 1000, 0, MidpointRounding.AwayFromZero) / 1000;

            module.Tier = tier;

            module.Category = category;
            if (moduleCategories.TryGetValue(category, out var list))
                list.Add(module);
            else
                moduleCategories.Add(category, [module]);

            modules.Add(module.Name, module);
        }

        private void ProcessRecipe(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<RecipePrototype>> craftingCategories, Dictionary<string, List<ModulePrototype>> moduleCategories) {
            if ((string?)objJToken["name"] is not string name ||
                (string?)objJToken["localised_name"] is not string localisedName ||
                (string?)objJToken["subgroup"] is not string subgroupStr ||
                (string?)objJToken["order"] is not string order ||
                (string?)objJToken["category"] is not string category)
                return;
            RecipePrototype recipe = new(this, name, localisedName, (SubgroupPrototype)subgroups[subgroupStr], order);

            recipe.Time = (double?)objJToken["energy"] ?? default;
            if ((bool?)objJToken["enabled"] is true && startingTech is not null) //due to the way the import of presets happens, enabled at this stage means the recipe is available without any research necessary (aka: available at start)
            {
                recipe.myUnlockTechnologies.Add(startingTech);
                startingTech.unlockedRecipes.Add(recipe);
            }

            if (craftingCategories.TryGetValue(category, out var list))
                list.Add(recipe);
            else
                craftingCategories.Add(category, [recipe]);

            if ((string?)objJToken["icon_name"] is string iconName && iconCache.TryGetValue(iconName, out var icp))
                recipe.SetIconAndColor(icp);
            else if ((string?)objJToken["icon_alt_name"] is string iconAltName && iconCache.TryGetValue(iconAltName, out var icpAlt))
                recipe.SetIconAndColor(icpAlt);

            recipe.HasProductivityResearch = (bool?)objJToken["prod_research"] is true;
            recipe.MaxProductivityBonus = (double?)objJToken["maximum_productivity"] ?? 1000;

            foreach (var productJToken in objJToken["products"]?.AsEnumerable() ?? []) {
                if ((string?)productJToken["name"] is not string prodName)
                    continue;
                ItemPrototype product = (ItemPrototype)items[prodName];
                double amount = (double?)productJToken["amount"] ?? 0;
                if (amount != 0 && (double?)productJToken["p_amount"] is double pAmount) {
                    if ((string?)productJToken["type"] == "fluid")
                        recipe.InternalOneWayAddProduct(product, amount, pAmount, (double?)productJToken["temperature"] ?? ((FluidPrototype)product).DefaultTemperature);
                    else
                        recipe.InternalOneWayAddProduct(product, amount, pAmount);

                    product.productionRecipes.Add(recipe);
                }
            }

            foreach (var ingredientJToken in objJToken["ingredients"]?.AsEnumerable() ?? []) {
                if ((string?)ingredientJToken["name"] is not string ingName)
                    continue;
                ItemPrototype ingredient = (ItemPrototype)items[ingName];
                double amount = (double?)ingredientJToken["amount"] ?? 0;
                if (amount != 0) {
                    double minTemp = ((string?)ingredientJToken["type"] == "fluid" && (double?)ingredientJToken["minimum_temperature"] is double min) ? min : double.NegativeInfinity;
                    double maxTemp = ((string?)ingredientJToken["type"] == "fluid" && (double?)ingredientJToken["maximum_temperature"] is double max) ? max : double.PositiveInfinity;
                    if (minTemp < MinTemp)
                        minTemp = double.NegativeInfinity;
                    if (maxTemp > MaxTemp)
                        maxTemp = double.PositiveInfinity;

                    recipe.InternalOneWayAddIngredient(ingredient, amount, minTemp, maxTemp);
                    ingredient.consumptionRecipes.Add(recipe);
                }
            }

            if (objJToken["allowed_effects"] is JToken allowedEffects) {
                recipe.AllowConsumptionBonus = (bool?)allowedEffects["consumption"] is true;
                recipe.AllowSpeedBonus = (bool?)allowedEffects["speed"] is true;
                recipe.AllowProductivityBonus = (bool?)allowedEffects["productivity"] is true;
                recipe.AllowPollutionBonus = (bool?)allowedEffects["pollution"] is true;
                recipe.AllowQualityBonus = (bool?)allowedEffects["quality"] is true;

                foreach (ModulePrototype module in modules.Values.Cast<ModulePrototype>()) {
                    bool validModule = (recipe.AllowConsumptionBonus || module.ConsumptionBonus >= 0) &&
                                        (recipe.AllowSpeedBonus || module.SpeedBonus <= 0) &&
                                        (recipe.AllowProductivityBonus || module.ProductivityBonus <= 0) &&
                                        (recipe.AllowPollutionBonus || module.PollutionBonus >= 0) &&
                                        (recipe.AllowQualityBonus || module.QualityBonus <= 0);
                    if (validModule) {
                        recipe.beaconModules.Add(module);
                    }
                }

                if (objJToken["allowed_module_categories"] is not JToken allowedModCats || allowedModCats.Count() == 0) {
                    foreach (ModulePrototype module in modules.Values.Cast<ModulePrototype>()) {
                        bool validModule = (recipe.AllowConsumptionBonus || module.ConsumptionBonus >= 0) &&
                                            (recipe.AllowSpeedBonus || module.SpeedBonus <= 0) &&
                                            (recipe.AllowProductivityBonus || module.ProductivityBonus <= 0) &&
                                            (recipe.AllowPollutionBonus || module.PollutionBonus >= 0) &&
                                            (recipe.AllowQualityBonus || module.QualityBonus <= 0);
                        if (validModule) {
                            recipe.assemblerModules.Add(module);
                            module.recipes.Add(recipe);
                        }
                    }
                } else {
                    foreach (string moduleCategory in objJToken["allowed_module_categories"]?.Select(a => ((JProperty)a).Name) ?? []) {
                        if (moduleCategories.ContainsKey(moduleCategory)) {
                            foreach (ModulePrototype module in moduleCategories[moduleCategory]) {
                                bool validModule = (recipe.AllowConsumptionBonus || module.ConsumptionBonus >= 0) &&
                                                    (recipe.AllowSpeedBonus || module.SpeedBonus <= 0) &&
                                                    (recipe.AllowProductivityBonus || module.ProductivityBonus <= 0) &&
                                                    (recipe.AllowPollutionBonus || module.PollutionBonus >= 0) &&
                                                    (recipe.AllowQualityBonus || module.QualityBonus <= 0);
                                if (validModule) {
                                    recipe.assemblerModules.Add(module);
                                    module.recipes.Add(recipe);
                                }
                            }
                        }
                    }
                }
            }

            recipes.Add(recipe.Name, recipe);
        }

        private string GetExtractionRecipeName(string itemName) { return "§§r:e:" + itemName; }

        private void ProcessResource(JToken objJToken, Dictionary<string, List<RecipePrototype>> resourceCategories, List<Recipe> miningWithFluidRecipes) {
            var subgroup = (string?)objJToken["products"]?.First?["type"] == "fluid" ? extractionSubgroupFluids : extractionSubgroupItems;
            if (objJToken["products"]?.Count() == 0 ||
                subgroup is null ||
                (string?)objJToken["name"] is not string name ||
                (string?)objJToken["localised_name"] is not string localisedName)
                return;

            RecipePrototype recipe = new(this, GetExtractionRecipeName(name), localisedName + " Extraction", subgroup, name);

            recipe.Time = (double?)objJToken["mining_time"] ?? 0;

            foreach (var productJToken in objJToken["products"]?.AsEnumerable() ?? []) {
                if ((string?)productJToken["name"] is not string prodName || !items.ContainsKey(name) || (double?)productJToken["amount"] <= 0)
                    continue;
                ItemPrototype product = (ItemPrototype)items[prodName];
                var amount = (double?)productJToken["amount"] ?? default;
                recipe.InternalOneWayAddProduct(product, amount, amount);
                product.productionRecipes.Add(recipe);
            }

            if (recipe.productList.Count == 0) {
                recipe.mySubgroup.recipes.Remove(recipe);
                return;
            }

            if ((string?)objJToken["required_fluid"] is string requiredFluid && (double?)objJToken["fluid_amount"] is double fluidAmount && fluidAmount != 0) {
                ItemPrototype reqLiquid = (ItemPrototype)items[requiredFluid];
                recipe.InternalOneWayAddIngredient(reqLiquid, fluidAmount);
                reqLiquid.consumptionRecipes.Add(recipe);
                miningWithFluidRecipes.Add(recipe);
            }

            foreach (ModulePrototype module in modules.Values.Cast<ModulePrototype>()) //we will let the assembler sort out which module can be used with this recipe
            {
                module.recipes.Add(recipe);
                recipe.assemblerModules.Add(module);
            }

            recipe.SetIconAndColor(new IconColorPair(recipe.productList[0].Icon, recipe.productList[0].AverageColor));

            if ((string?)objJToken["resource_category"] is string category) {
                if (resourceCategories.TryGetValue(category, out var list))
                    list.Add(recipe);
                else
                    resourceCategories.Add(category, [recipe]);
            }

            //resource recipe will be processed when adding to miners (each miner that can use this recipe will have its recipe's techs added to unlock tech of the resource recipe)
            //this is for any non-fluid based resource! (fluid based item mining is locked behind research and processed in research function)
            //recipe.myUnlockTechnologies.Add(startingTech);
            //startingTech.unlockedRecipes.Add(recipe);

            recipes.Add(recipe.Name, recipe);
        }

        private void ProcessTechnology(JToken objJToken, Dictionary<string, IconColorPair> iconCache, List<Recipe> miningWithFluidRecipes) {
            if ((string?)objJToken["name"] is not string name ||
                (string?)objJToken["localised_name"] is not string localisedName)
                return;
            TechnologyPrototype technology = new TechnologyPrototype(
                this,
                name,
                localisedName);

            if ((string?)objJToken["icon_name"] is string iconName && iconCache.TryGetValue(iconName, out var icp))
                technology.SetIconAndColor(icp);

            technology.Available = (bool?)objJToken["hidden"] is not true && (bool?)objJToken["enabled"] is true; //not sure - factorio documentation states 'enabled' means 'available at start', but in this case 'enabled' being false seems to represent the technology not appearing on screen (same as hidden)??? I will just work with what tests show -> tech is available if it is enabled & not hidden.

            foreach (var recipe in objJToken["recipes"]?.Select(jt => (string?)jt).OfType<string>() ?? []) {
                if (recipes.TryGetValue(recipe, out var proto)) {
                    ((RecipePrototype)proto).myUnlockTechnologies.Add(technology);
                    technology.unlockedRecipes.Add((RecipePrototype)proto);
                }
            }

            foreach (var qualityName in objJToken["qualities"]?.Select(jt => (string?)jt).OfType<string>() ?? []) {
                if (qualities.TryGetValue(qualityName, out var quality)) {
                    ((QualityPrototype)quality).myUnlockTechnologies.Add(technology);
                    technology.unlockedQualities.Add((QualityPrototype)quality);
                }
            }

            if (objJToken["unlocks-mining-with-fluid"] is not null) {
                foreach (RecipePrototype recipe in miningWithFluidRecipes.Cast<RecipePrototype>()) {
                    recipe.myUnlockTechnologies.Add(technology);
                    technology.unlockedRecipes.Add(recipe);
                }
            }

            foreach (var ingredientJToken in objJToken["research_unit_ingredients"]?.AsEnumerable() ?? []) {
                var ingName = (string?)ingredientJToken["name"];
                var amount = (double?)ingredientJToken["amount"] ?? 0;

                if (ingName is string && amount != 0) {
                    technology.InternalOneWayAddSciPack((ItemPrototype)items[ingName], amount);
                    ((ItemPrototype)items[ingName]).consumptionTechnologies.Add(technology);
                }
            }

            technologies.Add(technology.Name, technology);
        }

        private void ProcessTechnologyP2(JToken objJToken) {
            if ((string?)objJToken["name"] is not string name)
                return;
            TechnologyPrototype technology = (TechnologyPrototype)technologies[name];
            foreach (var prerequisite in objJToken["prerequisites"]?.Select(o => (string?)o).OfType<string>() ?? []) {
                if (technologies.TryGetValue(prerequisite, out var proto)) {
                    technology.prerequisites.Add((TechnologyPrototype)proto);
                    ((TechnologyPrototype)proto).postTechs.Add(technology);
                }
            }
            if (technology.prerequisites.Count == 0 && startingTech is not null) //entire tech tree will stem from teh 'startingTech' node.
            {
                technology.prerequisites.Add(startingTech);
                startingTech.postTechs.Add(technology);
            }
        }

        private void ProcessCharacter(JToken? objJtoken, Dictionary<string, List<RecipePrototype>> craftingCategories) {
            if (objJtoken is null || playerAssembler is null)
                return;
            AssemblerAdditionalProcessing(objJtoken, playerAssembler, craftingCategories);
            assemblers.Add(playerAssembler.Name, playerAssembler);
        }

        private void ProcessEntity(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<RecipePrototype>> craftingCategories, Dictionary<string, List<RecipePrototype>> resourceCategories, Dictionary<string, List<ItemPrototype>> fuelCategories, List<Recipe> miningWithFluidRecipes, Dictionary<string, List<ModulePrototype>> moduleCategories) {
            var type = (string?)objJToken["type"];
            //character is processed later
            if (type == "character" ||
                (string?)objJToken["name"] is not string name ||
                (string?)objJToken["localised_name"] is not string localisedName)
                return;

            EntityObjectBasePrototype entity;
            EnergySource esource =
                ((string?)objJToken["fuel_type"] == "item") ? EnergySource.Burner :
                ((string?)objJToken["fuel_type"] == "fluid") ? EnergySource.FluidBurner :
                ((string?)objJToken["fuel_type"] == "electricity") ? EnergySource.Electric :
                ((string?)objJToken["fuel_type"] == "heat") ? EnergySource.Heat : EnergySource.Void;
            var etype = type switch {
                "beacon" => EntityType.Beacon,
                "mining-drill" => EntityType.Miner,
                "offshore-pump" => EntityType.OffshorePump,
                "furnace" or "assembling-machine" or "rocket-silo" => EntityType.Assembler,
                "boiler" => EntityType.Boiler,
                "generator" => EntityType.Generator,
                "burner-generator" => EntityType.BurnerGenerator,
                "reactor" => EntityType.Reactor,
                _ => EntityType.ERROR,
            };
            if (etype == EntityType.ERROR)
                Trace.Fail(string.Format("Unexpected type of entity ({0} in json data!", type));


            if (etype == EntityType.Beacon) {
                entity = new BeaconPrototype(this, name, localisedName, esource, isMissing: false);
            } else {
                entity = new AssemblerPrototype(this, name, localisedName, etype, esource, isMissing: false);
            }

            //icons
            if ((string?)objJToken["icon_name"] is string iconName && iconCache.TryGetValue(iconName, out var icp))
                entity.SetIconAndColor(icp);
            else if ((string?)objJToken["icon_alt_name"] is string iconAlt && iconCache.TryGetValue(iconAlt, out var icpAlt))
                entity.SetIconAndColor(icpAlt);

            //associated items
            foreach (var item in objJToken["items_to_place_this"]?.Select(i => (string?)i).OfType<string>() ?? [])
                if (items.ContainsKey(item))
                    entity.associatedItems.Add((ItemPrototype)items[item]);

            //base parameters
            if (objJToken["q_speed"] is JToken qSpeed) {
                foreach (JToken speedToken in qSpeed.AsEnumerable() ?? [])
                    if ((string?)speedToken["quality"] is string quality && (double?)speedToken["value"] is double value)
                        entity.speed.Add(qualities[quality], value);
            } else if ((double?)objJToken["speed"] is double speed) {
                foreach (Quality quality in qualities.Values)
                    entity.speed.Add(quality, speed);
            }

            entity.ModuleSlots = (int?)objJToken["module_inventory_size"] ?? 0;

            //modules
            if (entity.EntityType == EntityType.Assembler || entity.EntityType == EntityType.Miner || entity.EntityType == EntityType.Rocket || entity.EntityType == EntityType.Beacon) {
                if (entity is AssemblerPrototype) {
                    ((AssemblerPrototype)entity).BaseConsumptionBonus = (double?)objJToken["base_module_effects"]?["consumption"] ?? default;
                    ((AssemblerPrototype)entity).BaseSpeedBonus = (double?)objJToken["base_module_effects"]?["speed"] ?? default;
                    ((AssemblerPrototype)entity).BaseProductivityBonus = (double?)objJToken["base_module_effects"]?["productivity"] ?? default;
                    ((AssemblerPrototype)entity).BasePollutionBonus = (double?)objJToken["base_module_effects"]?["pollution"] ?? default;
                    ((AssemblerPrototype)entity).BaseQualityBonus = (double?)objJToken["base_module_effects"]?["quality"] ?? default;
                    ((AssemblerPrototype)entity).AllowModules = (bool?)objJToken["uses_module_effects"] is true;
                    ((AssemblerPrototype)entity).AllowBeacons = (bool?)objJToken["uses_beacon_effects"] is true;
                }

                if (objJToken["allowed_effects"] is JToken allowedEffects) {
                    bool allow_consumption = (bool?)allowedEffects["consumption"] is true;
                    bool allow_speed = (bool?)allowedEffects["speed"] is true;
                    bool alllow_productivity = (bool?)allowedEffects["productivity"] is true;
                    bool allow_pollution = (bool?)allowedEffects["pollution"] is true;
                    bool allow_quality = (bool?)allowedEffects["quality"] is true;

                    if (objJToken["allowed_module_categories"] is not JToken allowedModuleCats || allowedModuleCats.Count() == 0) {
                        foreach (ModulePrototype module in modules.Values.Cast<ModulePrototype>()) {
                            bool validModule = (allow_consumption || module.ConsumptionBonus >= 0) &&
                                                (allow_speed || module.SpeedBonus <= 0) &&
                                                (alllow_productivity || module.ProductivityBonus <= 0) &&
                                                (allow_pollution || module.PollutionBonus >= 0) &&
                                                (allow_quality || module.QualityBonus <= 0);
                            if (validModule) {
                                entity.modules.Add(module);
                                if (entity is AssemblerPrototype aEntity)
                                    module.assemblers.Add(aEntity);
                                else if (entity is BeaconPrototype bEntity)
                                    module.beacons.Add(bEntity);
                            }
                        }
                    } else {
                        foreach (string moduleCategory in allowedModuleCats.Select(a => ((JProperty)a).Name)) {
                            if (moduleCategories.ContainsKey(moduleCategory)) {
                                foreach (ModulePrototype module in moduleCategories[moduleCategory]) {
                                    bool validModule = (allow_consumption || module.ConsumptionBonus >= 0) &&
                                                        (allow_speed || module.SpeedBonus <= 0) &&
                                                        (alllow_productivity || module.ProductivityBonus <= 0) &&
                                                        (allow_pollution || module.PollutionBonus >= 0) &&
                                                        (allow_quality || module.QualityBonus <= 0);
                                    if (validModule) {
                                        entity.modules.Add(module);
                                        if (entity is AssemblerPrototype aEntity)
                                            module.assemblers.Add(aEntity);
                                        else if (entity is BeaconPrototype bEntity)
                                            module.beacons.Add(bEntity);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //energy types
            EntityEnergyFurtherProcessing(objJToken, entity, fuelCategories);

            //assembler / beacon specific parameters
            if (etype == EntityType.Beacon) {
                BeaconPrototype bEntity = (BeaconPrototype)entity;

                if (BeaconAdditionalProcessing(objJToken, bEntity))
                    beacons.Add(bEntity.Name, bEntity);
            } else {
                AssemblerPrototype aEntity = (AssemblerPrototype)entity;

                bool success = false;
                switch (etype) {
                    case EntityType.Assembler:
                        success = AssemblerAdditionalProcessing(objJToken, aEntity, craftingCategories);
                        break;
                    case EntityType.Boiler:
                        success = BoilerAdditionalProcessing(objJToken, aEntity);
                        break;
                    case EntityType.BurnerGenerator:
                        success = BurnerGeneratorAdditionalProcessing(objJToken, aEntity);
                        break;
                    case EntityType.Generator:
                        success = GeneratorAdditionalProcessing(objJToken, aEntity);
                        break;
                    case EntityType.Miner:
                        success = MinerAdditionalProcessing(objJToken, aEntity, resourceCategories, miningWithFluidRecipes);
                        break;
                    case EntityType.OffshorePump:
                        success = OffshorePumpAdditionalProcessing(objJToken, aEntity, resourceCategories["<<foreman_resource_category_water_tile>>"]);
                        break;
                    case EntityType.Reactor:
                        success = ReactorAdditionalProcessing(objJToken, aEntity);
                        break;
                }
                if (success)
                    assemblers.Add(aEntity.Name, aEntity);
            }
        }

        private void EntityEnergyFurtherProcessing(JToken objJToken, EntityObjectBasePrototype entity, Dictionary<string, List<ItemPrototype>> fuelCategories) {
            entity.ConsumptionEffectivity = (double?)objJToken["fuel_effectivity"] ?? 0;

            //pollution
            if (objJToken is JObject objJObject) {
                var pollutions = objJObject["pollution"]?.ToObject<Dictionary<string, double>>();
                foreach (var pollution in pollutions ?? [])
                    entity.pollution.Add(pollution.Key, pollution.Value);
            }

            //energy production
            foreach (JToken speedToken in objJToken["q_energy_production"]?.AsEnumerable() ?? [])
                if ((string?)speedToken["quality"] is string quality && (double?)speedToken["value"] is double value)
                    entity.energyProduction.Add(qualities[quality], value);

            //energy consumption
            entity.energyDrain = (double?)objJToken["drain"] ?? 0; //seconds
            foreach (JToken speedToken in objJToken["q_max_energy_usage"]?.AsEnumerable() ?? [])
                if ((string?)speedToken["quality"] is string quality && (double?)speedToken["value"] is double value)
                    entity.energyConsumption.Add(qualities[quality], value);

            //fuel processing
            switch (entity.EnergySource) {
                case EnergySource.Burner:
                    foreach (var categoryJToken in objJToken["fuel_categories"]?.Select(o => (string?)o).OfType<string>() ?? []) {
                        if (fuelCategories.ContainsKey(categoryJToken)) {
                            foreach (ItemPrototype item in fuelCategories[categoryJToken]) {
                                entity.fuels.Add(item);
                                item.fuelsEntities.Add(entity);
                            }
                        }
                    }
                    break;

                case EnergySource.FluidBurner:
                    entity.IsTemperatureFluidBurner = (bool?)objJToken["burns_fluid"] is not true;
                    entity.FluidFuelTemperatureRange = new fRange((double?)objJToken["minimum_fuel_temperature"] ?? double.NegativeInfinity, (double?)objJToken["maximum_fuel_temperature"] ?? double.PositiveInfinity);

                    if ((string?)objJToken["fuel_filter"] is string fuelFilter) {
                        ItemPrototype fuel = (ItemPrototype)items[fuelFilter];
                        if (entity.IsTemperatureFluidBurner || fuelCategories["§§fc:liquids"].Contains(fuel)) {
                            entity.fuels.Add(fuel);
                            fuel.fuelsEntities.Add(entity);
                        }
                        //else
                        //	; //there is no valid fuel for this entity. Realistically this means it cant be used. It will thus have an error when placed (no fuel selected -> due to no fuel existing)
                    } else if (!entity.IsTemperatureFluidBurner) {
                        //add in all liquid fuels
                        foreach (ItemPrototype fluid in fuelCategories["§§fc:liquids"]) {
                            entity.fuels.Add(fluid);
                            fluid.fuelsEntities.Add(entity);
                        }
                    } else //ok, this is a bit of a FK U, but this basically means this entity can burn any fluid, and burns it as a temperature range. This is how the old steam generators worked (where you could feed in hot sulfuric acid and it would just burn through it no problem). If you want to use it, fine. Here you go.
                      {
                        foreach (FluidPrototype fluid in items.Values.Where(i => i is Fluid).Cast<FluidPrototype>()) {
                            entity.fuels.Add(fluid);
                            fluid.fuelsEntities.Add(entity);
                        }
                    }
                    break;

                case EnergySource.Heat:
                    if (HeatItem is not null) {
                        entity.fuels.Add(HeatItem);
                        HeatItem.fuelsEntities.Add(entity);
                    }
                    break;

                case EnergySource.Electric:
                    break;

                case EnergySource.Void:
                default:
                    break;
            }
        }

        private bool BeaconAdditionalProcessing(JToken objJToken, BeaconPrototype bEntity) {
            bEntity.DistributionEffectivity = (double?)objJToken["distribution_effectivity"] ?? 0.5;
            bEntity.DistributionEffectivityQualityBoost = (double?)objJToken["distribution_effectivity_bonus_per_quality_level"] ?? 0;

            if (objJToken["profile"] != null) {
                int quantity = 1;
                double lastProfile = 0.5;
                foreach (var profileJToken in objJToken["profile"]?.AsEnumerable() ?? []) {
                    lastProfile = (double)profileJToken;
                    bEntity.profile[quantity] = lastProfile;

                    quantity++;
                    if (quantity >= bEntity.profile.Length)
                        break;
                }
                while (quantity < bEntity.profile.Length) {
                    bEntity.profile[quantity] = lastProfile;
                    quantity++;
                }
                bEntity.profile[0] = bEntity.profile[1]; //helps with calculating partial beacon values (ex: 0.5 beacons)
            }

            return true;
        }

        private bool AssemblerAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity, Dictionary<string, List<RecipePrototype>> craftingCategories) //recipe user
        {
            foreach (var recipe in objJToken["crafting_categories"]
                ?.Select(o => (string?)o)
                .OfType<string>()
                .SelectMany(s => craftingCategories.TryGetValue(s, out var list) ? list : [])
                .Where(recipe => TestRecipeEntityPipeFit(recipe, objJToken)) ?? []) {
                recipe.assemblers.Add(aEntity);
                aEntity.recipes.Add(recipe);
            }
            //TODO: What is the point of returning a bool?
            return true;
        }

        private bool MinerAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity, Dictionary<string, List<RecipePrototype>> resourceCategories, List<Recipe> miningWithFluidRecipes) //resource provider
        {
            foreach (var recipe in objJToken["resource_categories"]
                ?.Select(o => (string?)o)
                .OfType<string>()
                .SelectMany(s => resourceCategories.TryGetValue(s, out var list) ? list : [])
                .Where(recipe => TestRecipeEntityPipeFit(recipe, objJToken)) ?? []) {
                if (!miningWithFluidRecipes.Contains(recipe))
                    ProcessEntityRecipeTechlink(aEntity, recipe);

                recipe.assemblers.Add(aEntity);
                aEntity.recipes.Add(recipe);
            }
            //TODO: What is the point of returning a bool?
            return true;
        }

        private bool OffshorePumpAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity, List<RecipePrototype> waterPumpRecipes) {
            //check if the pump has a specified 'output' fluid preset. if yes then only that recipe is added to it; if not then all water tile resource recipes are added
            var outPipeFilters = objJToken["out_pipe_filters"]?.Select(o => (string?)o).OfType<string>().ToList() ?? [];

            if (outPipeFilters.Count != 0) {
                if (recipes.TryGetValue(GetExtractionRecipeName(outPipeFilters[0]), out var extractionRecipe)) {
                    ProcessEntityRecipeTechlink(aEntity, (RecipePrototype)extractionRecipe);
                    ((RecipePrototype)extractionRecipe).assemblers.Add(aEntity);
                    aEntity.recipes.Add((RecipePrototype)extractionRecipe);
                } else {
                    //add new recipe
                    if (!items.TryGetValue(outPipeFilters[0], out var extractionFluid) || extractionSubgroupFluids is null)
                        return false;

                    RecipePrototype recipe = new RecipePrototype(
                        this,
                        GetExtractionRecipeName(outPipeFilters[0]),
                        extractionFluid.FriendlyName + " Extraction",
                        extractionSubgroupFluids,
                        extractionFluid.Name);

                    recipe.Time = 1;

                    recipe.InternalOneWayAddProduct((ItemPrototype)extractionFluid, 60, 60);
                    ((ItemPrototype)extractionFluid).productionRecipes.Add(recipe);

                    recipe.SetIconAndColor(new IconColorPair(recipe.productList[0].Icon, recipe.productList[0].AverageColor));

                    recipes.Add(recipe.Name, recipe);
                }
            } else {
                foreach (RecipePrototype recipe in waterPumpRecipes) {
                    ProcessEntityRecipeTechlink(aEntity, recipe);
                    recipe.assemblers.Add(aEntity);
                    aEntity.recipes.Add(recipe);
                }
            }

            return true;
        }

        private bool BoilerAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity) //Uses whatever the default energy source of it is to convert water into steam of a given temperature
        {
            if ((string?)objJToken["fluid_ingredient"] is not string fluidIng || (string?)objJToken["fluid_product"] is not string fluidProduct)
                return false;
            FluidPrototype ingredient = (FluidPrototype)items[fluidIng];
            FluidPrototype product = (FluidPrototype)items[fluidProduct];

            //boiler is a ingredient to product conversion with product coming out at the  target_temperature *C at a rate based on energy efficiency & energy use to bring the INGREDIENT to the given temperature (basically ingredient goes from default temp to target temp, then shifts to product). we will add an extra recipe for this
            double temp = (double?)objJToken["target_temperature"] ?? default;

            //I will be honest here. Testing has shown that the actual 'speed' is dependent on the incoming temperature (not the default temperature), as could have likely been expected.
            //this means that if you put in 65* water instead of 15* water to boil it to 165* steam it will result in 1.5x the 'maximum' output as listed in the factorio info menu and calculated below.
            //so if some mod does some wonky things like water pre-heating, or uses boiler to heat other fluids at non-default temperatures (I havent found any such mods, but testing shows it is possible to make such a mod)
            //then the values calculated here will be wrong.
            //Still, for now I will leave it as is.
            if (ingredient.SpecificHeatCapacity == 0) {
                foreach (Quality quality in qualities.Values)
                    aEntity.speed.Add(quality, 0);
            } else {
                foreach (Quality quality in qualities.Values)
                    aEntity.speed.Add(quality, (double)(aEntity.GetEnergyConsumption(quality) / ((temp - ingredient.DefaultTemperature) * ingredient.SpecificHeatCapacity * 60))); //by placing this here we can keep the recipe as a 1 sec -> 60 production, simplifying recipe comparing for presets.
            }

            RecipePrototype recipe;
            string boilRecipeName = string.Format("§§r:b:{0}:{1}:{2}", ingredient.Name, product.Name, temp.ToString());
            if (!recipes.ContainsKey(boilRecipeName) && energySubgroupBoiling is not null) {
                recipe = new RecipePrototype(
                    this,
                    boilRecipeName,
                    ingredient == product ? string.Format("{0} boiling to {1}°c", ingredient.FriendlyName, temp.ToString()) : string.Format("{0} boiling to {1}°c {2}", ingredient.FriendlyName, temp.ToString(), product.FriendlyName),
                    energySubgroupBoiling,
                    boilRecipeName);

                recipe.SetIconAndColor(new IconColorPair(IconCache.ConbineIcons(ingredient.Icon, product.Icon, ingredient.Icon.Height), product.AverageColor));

                recipe.Time = 1;

                recipe.InternalOneWayAddIngredient(ingredient, 60);
                ingredient.consumptionRecipes.Add(recipe);

                double productQuantity = 60 * ingredient.SpecificHeatCapacity / product.SpecificHeatCapacity;
                recipe.InternalOneWayAddProduct(product, productQuantity, productQuantity, temp);
                product.productionRecipes.Add(recipe);


                foreach (ModulePrototype module in modules.Values.Cast<ModulePrototype>()) //we will let the assembler sort out which module can be used with this recipe
                {
                    module.recipes.Add(recipe);
                    recipe.assemblerModules.Add(module);
                }

                recipes.Add(recipe.Name, recipe);
            } else
                recipe = (RecipePrototype)recipes[boilRecipeName];

            ProcessEntityRecipeTechlink(aEntity, recipe);
            recipe.assemblers.Add(aEntity);
            aEntity.recipes.Add(recipe);

            return true;
        }

        private bool GeneratorAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity) //consumes steam (at the provided temperature up to the given maximum) to generate electricity
        {
            if ((string?)objJToken["fluid_ingredient"] is not string fluidIng)
                return false;
            FluidPrototype ingredient = (FluidPrototype)items[fluidIng];

            double baseSpeed = ((double?)objJToken["fluid_usage_per_sec"] ?? default) / 60; //use 60 multiplier to make recipes easier
            double baseEnergyProduction = (double?)objJToken["max_power_output"] ?? default; //in seconds

            foreach (Quality quality in qualities.Values)
                aEntity.speed.Add(quality, baseSpeed * aEntity.GetEnergyProduction(quality) / baseEnergyProduction);

            aEntity.OperationTemperature = (double?)objJToken["full_power_temperature"] ?? default;
            double minTemp = (double)(objJToken["minimum_temperature"] ?? double.NaN);
            double maxTemp = (double)(objJToken["maximum_temperature"] ?? double.NaN);
            if (!double.IsNaN(minTemp) && minTemp < ingredient.DefaultTemperature)
                minTemp = ingredient.DefaultTemperature;
            if (!double.IsNaN(maxTemp) && maxTemp > MaxTemp)
                maxTemp = double.NaN;

            //actual energy production is a bit more complicated here (as it involves actual temperatures), but we will have to handle it in the graph (after all values have been calculated and we know the amounts and temperatures getting passed here, we can calc the energy produced)

            RecipePrototype recipe;
            string generationRecipeName = string.Format("§§r:g:{0}:{1}>{2}", ingredient.Name, minTemp, maxTemp);
            if (!recipes.ContainsKey(generationRecipeName) && energySubgroupEnergy is not null) {
                recipe = new RecipePrototype(
                    this,
                    generationRecipeName,
                    string.Format("{0} to Electricity", ingredient.FriendlyName),
                    energySubgroupEnergy,
                    generationRecipeName);

                recipe.SetIconAndColor(new IconColorPair(IconCache.ConbineIcons(ingredient.Icon, ElectricityIcon ?? IconCache.UnknownIcon, ingredient.Icon.Height, false), ingredient.AverageColor));

                recipe.Time = 1;

                recipe.InternalOneWayAddIngredient(ingredient, 60, double.IsNaN(minTemp) ? double.NegativeInfinity : minTemp, double.IsNaN(maxTemp) ? double.PositiveInfinity : maxTemp);

                ingredient.consumptionRecipes.Add(recipe);

                foreach (ModulePrototype module in modules.Values.Cast<ModulePrototype>()) //we will let the assembler sort out which module can be used with this recipe
                {
                    module.recipes.Add(recipe);
                    recipe.assemblerModules.Add(module);
                }

                recipes.Add(recipe.Name, recipe);
            } else
                recipe = (RecipePrototype)recipes[generationRecipeName];

            ProcessEntityRecipeTechlink(aEntity, recipe);
            recipe.assemblers.Add(aEntity);
            aEntity.recipes.Add(recipe);

            return true;
        }

        private bool BurnerGeneratorAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity) //consumes fuel to generate electricity
        {
            if (BurnerRecipe is null)
                return false;
            aEntity.recipes.Add(BurnerRecipe);
            BurnerRecipe.assemblers.Add(aEntity);
            ProcessEntityRecipeTechlink(aEntity, BurnerRecipe);

            foreach (Quality quality in qualities.Values)
                aEntity.speed.Add(quality, 1f); //doesnt matter - recipe is empty

            return true;
        }

        private bool ReactorAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity) {
            if (HeatRecipe is null || HeatItem is null)
                return false;
            aEntity.NeighbourBonus = (double?)objJToken["neighbour_bonus"] ?? 0;
            aEntity.recipes.Add(HeatRecipe);
            HeatRecipe.assemblers.Add(aEntity);
            ProcessEntityRecipeTechlink(aEntity, HeatRecipe);

            foreach (Quality quality in qualities.Values)
                aEntity.speed.Add(quality, (aEntity.GetEnergyConsumption(quality)) / HeatItem.FuelValue); //the speed of producing 1MJ of energy as heat for this reactor based on quality

            return true;
        }

        private void ProcessEntityRecipeTechlink(EntityObjectBasePrototype entity, RecipePrototype recipe) {
            if (entity.associatedItems.Count == 0 && startingTech is not null) {
                recipe.myUnlockTechnologies.Add(startingTech);
                startingTech.unlockedRecipes.Add(recipe);
            } else {
                foreach (Item placeItem in entity.associatedItems) {
                    foreach (Recipe placeItemRecipe in placeItem.ProductionRecipes) {
                        foreach (TechnologyPrototype tech in placeItemRecipe.MyUnlockTechnologies.Cast<TechnologyPrototype>()) {
                            recipe.myUnlockTechnologies.Add(tech);
                            tech.unlockedRecipes.Add(recipe);
                        }
                    }
                }
            }
        }

        private bool TestRecipeEntityPipeFit(RecipePrototype recipe, JToken objJToken) //returns true if the fluid boxes of the entity (assembler or miner) can accept the provided recipe (with its in/out fluids)
        {
            int inPipes = (int?)objJToken["in_pipes"] ?? default;
            var inPipeFilters = objJToken["in_pipe_filters"]?.Select(o => (string?)o).OfType<string>().ToHashSet();
            int outPipes = (int?)objJToken["out_pipes"] ?? default;
            var outPipeFilters = objJToken["out_pipe_filters"]?.Select(o => (string?)o).OfType<string>().ToHashSet();
            int ioPipes = (int?)objJToken["io_pipes"] ?? default;
            var ioPipeFilters = objJToken["io_pipe_filters"]?.Select(o => (string?)o).OfType<string>().ToHashSet();

            int inCount = 0; //unfiltered
            int outCount = 0; //unfiltered
            foreach (ItemPrototype inFluid in recipe.ingredientList.Where(i => i is Fluid)) {
                if (inPipeFilters?.Contains(inFluid.Name) is true) {
                    inPipes--;
                    inPipeFilters.Remove(inFluid.Name);
                } else if (ioPipeFilters?.Contains(inFluid.Name) is true) {
                    ioPipes--;
                    ioPipeFilters.Remove(inFluid.Name);
                } else
                    inCount++;
            }
            foreach (ItemPrototype outFluid in recipe.productList.Where(i => i is Fluid)) {
                if (outPipeFilters?.Contains(outFluid.Name) is true) {
                    outPipes--;
                    outPipeFilters.Remove(outFluid.Name);
                } else if (ioPipeFilters?.Contains(outFluid.Name) is true) {
                    ioPipes--;
                    ioPipeFilters.Remove(outFluid.Name);
                } else
                    outCount++;
            }
            //remove any unused filtered pipes from the equation - they cant be used due to the filters.
            inPipes -= inPipeFilters?.Count ?? 0;
            ioPipes -= ioPipeFilters?.Count ?? 0;
            outPipes -= outPipeFilters?.Count ?? 0;

            //return true if the remaining unfiltered ingredients & products (fluids) can fit into the remaining unfiltered pipes
            return (inCount - inPipes <= ioPipes && outCount - outPipes <= ioPipes && inCount + outCount <= inPipes + outPipes + ioPipes);
        }

        private void ProcessRocketLaunch(JToken objJToken) {
            if ((string?)objJToken["name"] is not string name || rocketLaunchSubgroup is null || rocketAssembler is null)
                return;
            if (!items.ContainsKey("rocket-part") || !recipes.ContainsKey("rocket-part") || !assemblers.ContainsKey("rocket-silo")) {
                ErrorLogging.LogLine(string.Format("No Rocket silo / rocket part found! launch product for {0} will be ignored.", (string?)objJToken["name"] ?? "<NULL JSON>"));
                return;
            }

            ItemPrototype rocketPart = (ItemPrototype)items["rocket-part"];
            RecipePrototype rocketPartRecipe = (RecipePrototype)recipes["rocket-part"];
            ItemPrototype launchItem = (ItemPrototype)items[name];

            RecipePrototype recipe = new RecipePrototype(
                this,
                string.Format("§§r:rl:launch-{0}", launchItem.Name),
                string.Format("Rocket Launch: {0}", launchItem.FriendlyName),
                rocketLaunchSubgroup,
                launchItem.Name);

            recipe.Time = 1; //placeholder really...

            //process products - have to calculate what the maximum input size of the launch item is so as not to waste any products (ex: you can launch 2000 science packs, but you will only get 100 fish. so input size must be set to 100 -> 100 science packs to 100 fish)
            int inputSize = launchItem.StackSize;
            Dictionary<ItemPrototype, double> products = new Dictionary<ItemPrototype, double>();
            Dictionary<ItemPrototype, double> productTemp = new Dictionary<ItemPrototype, double>();
            foreach (var productJToken in objJToken["rocket_launch_products"]?.AsEnumerable() ?? []) {
                if ((string?)productJToken["name"] is not string prodName)
                    continue;
                ItemPrototype product = (ItemPrototype)items[prodName];
                double amount = (double?)productJToken["amount"] ?? default;
                if (amount != 0) {
                    if (inputSize * amount > product.StackSize)
                        inputSize = (int)(product.StackSize / amount);

                    amount = inputSize * amount;

                    if ((string?)productJToken["type"] == "fluid")
                        productTemp.Add(product, (double?)productJToken["temperature"] ?? ((FluidPrototype)product).DefaultTemperature);
                    products.Add(product, amount);

                    product.productionRecipes.Add(recipe);
                    recipe.SetIconAndColor(new IconColorPair(product.Icon, Color.DarkGray));
                }
            }
            foreach (ItemPrototype product in products.Keys)
                recipe.InternalOneWayAddProduct(product, inputSize * products[product], 0, productTemp.ContainsKey(product) ? productTemp[product] : double.NaN);

            recipe.InternalOneWayAddIngredient(launchItem, inputSize);
            launchItem.consumptionRecipes.Add(recipe);

            recipe.InternalOneWayAddIngredient(rocketPart, 100);
            rocketPart.consumptionRecipes.Add(recipe);

            foreach (TechnologyPrototype tech in rocketPartRecipe.myUnlockTechnologies) {
                recipe.myUnlockTechnologies.Add(tech);
                tech.unlockedRecipes.Add(recipe);
            }

            recipe.assemblers.Add(rocketAssembler);
            rocketAssembler.recipes.Add(recipe);

            recipes.Add(recipe.Name, recipe);
        }

        //------------------------------------------------------Finalization steps of LoadAllData (cleanup and cyclic checks)

        private void ProcessSciencePacks() {
            //DFS for processing the required sci packs of each technology. Basically some research only requires 1 sci pack, but to unlock it requires researching tech with many sci packs. Need to account for that
            Dictionary<TechnologyPrototype, HashSet<Item>> techRequirements = new Dictionary<TechnologyPrototype, HashSet<Item>>();
            HashSet<Item> sciPacks = new HashSet<Item>();
            HashSet<Item> TechRequiredSciPacks(TechnologyPrototype tech) {
                if (techRequirements.ContainsKey(tech))
                    return techRequirements[tech];

                HashSet<Item> requiredItems = new HashSet<Item>(tech.sciPackList);
                foreach (TechnologyPrototype prereq in tech.prerequisites)
                    foreach (Item sciPack in TechRequiredSciPacks(prereq))
                        requiredItems.Add(sciPack);

                sciPacks.UnionWith(requiredItems);
                techRequirements.Add(tech, requiredItems);

                return requiredItems;
            }

            //tech ordering - set each technology's 'tier' to be its furthest distance from the 'starting tech' node
            HashSet<TechnologyPrototype> visitedTech = new();
            if (startingTech is not null)
                visitedTech.Add(startingTech); //tier 0, everything starts from here.
            int GetTechnologyTier(TechnologyPrototype tech) {
                if (!visitedTech.Contains(tech)) {
                    int maxPrerequisiteTier = 0;
                    foreach (TechnologyPrototype prereq in tech.prerequisites)
                        maxPrerequisiteTier = Math.Max(maxPrerequisiteTier, GetTechnologyTier(prereq));
                    tech.Tier = maxPrerequisiteTier + 1;
                    visitedTech.Add(tech);
                }
                return tech.Tier;
            }

            //science pack processing - DF again where we want to calculate which science packs are required to get to the given science pack
            HashSet<Item> visitedPacks = new HashSet<Item>();
            void UpdateSciencePackPrerequisites(Item sciPack) {
                if (visitedPacks.Contains(sciPack))
                    return;

                //for simplicities sake we will only account for prerequisites of the first available production recipe (or first non-available if no available production recipes exist). This means that if (for who knows what reason) there are multiple valid production recipes only the first one will count!
                HashSet<Item> prerequisites = new HashSet<Item>(sciPack.ProductionRecipes.OrderByDescending(r => r.Available).FirstOrDefault()?.MyUnlockTechnologies.OrderByDescending(t => t.Available).FirstOrDefault()?.SciPackList ?? new Item[0]);
                foreach (Recipe r in sciPack.ProductionRecipes)
                    foreach (Technology t in r.MyUnlockTechnologies)
                        prerequisites.IntersectWith(t.SciPackList);

                //prerequisites now contains all the immediate required sci packs. we will now Update their prerequisites via this function, then add their prerequisites to our own set before finalizing it.
                foreach (Item prereq in prerequisites.ToList()) {
                    UpdateSciencePackPrerequisites(prereq);
                    prerequisites.UnionWith(sciencePackPrerequisites[prereq]);
                }
                sciencePackPrerequisites.Add(sciPack, prerequisites);
                visitedPacks.Add(sciPack);
            }

            //step 1: update tech unlock status & science packs (add a 0 cost pack to the tech if it has no such requirement but its prerequisites do), set tech tier
            foreach (TechnologyPrototype tech in technologies.Values.Cast<TechnologyPrototype>()) {
                TechRequiredSciPacks(tech);
                GetTechnologyTier(tech);
                foreach (ItemPrototype sciPack in techRequirements[tech].Cast<ItemPrototype>())
                    tech.InternalOneWayAddSciPack(sciPack, 0);
            }

            //step 2: further sci pack processing -> for every available science pack we want to build a list of science packs necessary to aquire it. In a situation with multiple (non-equal) research paths (ex: 3 can be aquired through either pack 1&2 or pack 1 alone), take the intersect (1 in this case). These will be added to the sci pack requirement lists
            foreach (Item sciPack in sciPacks)
                UpdateSciencePackPrerequisites(sciPack);


            //step 2.5: update the technology science packs to account for the science pack prerequisites
            foreach (TechnologyPrototype tech in technologies.Values.Cast<TechnologyPrototype>())
                foreach (Item sciPack in tech.SciPackList.ToList())
                    foreach (ItemPrototype reqSciPack in sciencePackPrerequisites[sciPack].Cast<ItemPrototype>())
                        tech.InternalOneWayAddSciPack(reqSciPack, 0);

            //step 3: calculate science pack tier (minimum tier of technology that unlocks the recipe for the given science pack). also make the sciencePacks list.
            Dictionary<Item, int> sciencePackTiers = new Dictionary<Item, int>();
            foreach (ItemPrototype sciPack in sciPacks.Cast<ItemPrototype>()) {
                int minTier = int.MaxValue;
                foreach (Recipe recipe in sciPack.productionRecipes)
                    foreach (Technology tech in recipe.MyUnlockTechnologies)
                        minTier = Math.Min(minTier, tech.Tier);
                if (minTier == int.MaxValue) //there are no recipes for this sci pack. EX: space science pack. We will grant it the same tier as the first tech to require this sci pack. This should sort them relatively correctly (ex - placing space sci pack last, and placing seablock starting tech first)
                    minTier = techRequirements.Where(kvp => kvp.Value.Contains(sciPack)).Select(kvp => kvp.Key).Min(t => t.Tier);
                sciencePackTiers.Add(sciPack, minTier);
                sciencePacks.Add(sciPack);
            }

            //step 4: update all science pack lists (main sciencePacks list, plus SciPackList of every technology). Sorting is done by A: if science pack B has science pack A as a prerequisite (in sciPackRequiredPacks), then B goes after A. If neither has the other as a prerequisite, then compare by sciencePack tiers
            sciencePacks.Sort((s1, s2) => sciencePackTiers[s1].CompareTo(sciencePackTiers[s2]) + (sciencePackPrerequisites[s1].Contains(s2) ? 1000 : sciencePackPrerequisites[s2].Contains(s1) ? -1000 : 0));
            foreach (TechnologyPrototype tech in technologies.Values.Cast<TechnologyPrototype>())
                tech.sciPackList.Sort((s1, s2) => sciencePackTiers[s1].CompareTo(sciencePackTiers[s2]) + (sciencePackPrerequisites[s1].Contains(s2) ? 1000 : sciencePackPrerequisites[s2].Contains(s1) ? -1000 : 0));

            //step 5: create science pack lists for each recipe (list of distinct min-pack sets -> ex: if recipe can be aquired through 4 techs with [ A+B, A+B, A+C, A+B+C ] science pack requirements, we will only include A+B and A+C
            foreach (RecipePrototype recipe in recipes.Values.Cast<RecipePrototype>()) {
                List<List<Item>> sciPackLists = new List<List<Item>>();
                foreach (TechnologyPrototype tech in recipe.myUnlockTechnologies) {
                    bool exists = false;
                    foreach (List<Item> sciPackList in sciPackLists.ToList()) {
                        if (!sciPackList.Except(tech.sciPackList).Any()) // sci pack lists already includes a list that is a subset of the technologies sci pack list (ex: already have A+B while tech's is A+B+C)
                            exists = true;
                        else if (!tech.sciPackList.Except(sciPackList).Any()) //technology sci pack list is a subset of an already included sci pack list. we will add thi to the list and delete the existing one (ex: have A+B while tech's is A -> need to remove A+B and include A)
                            sciPackLists.Remove(sciPackList);
                    }
                    if (!exists)
                        sciPackLists.Add(tech.sciPackList);
                }
                recipe.MyUnlockSciencePacks = sciPackLists;
            }
        }


        private void ProcessAvailableStatuses() {
            //quick function to depth-first search the tech tree to calculate the availability of the technology. Hashset used to keep track of visited tech and not have to re-check them.
            //NOTE: factorio ensures no cyclic, so we are guaranteed to have a directed acyclic graph (may be disconnected)
            HashSet<TechnologyPrototype> unlockableTechSet = new HashSet<TechnologyPrototype>();
            bool IsUnlockable(TechnologyPrototype tech) {
                if (!tech.Available)
                    return false;
                else if (unlockableTechSet.Contains(tech))
                    return true;
                else if (tech.prerequisites.Count == 0)
                    return true;
                else {
                    bool available = true;
                    foreach (TechnologyPrototype preTech in tech.prerequisites)
                        available = available && IsUnlockable(preTech);
                    tech.Available = available;

                    if (available)
                        unlockableTechSet.Add(tech);
                    return available;
                }
            }

            //step 0: check availability of technologies
            foreach (TechnologyPrototype tech in technologies.Values)
                IsUnlockable(tech);

            //step 1: update recipe unlock status
            foreach (RecipePrototype recipe in recipes.Values)
                recipe.Available = recipe.myUnlockTechnologies.Any(t => t.Available);

            //step 2: mark any recipe for barelling / crating as unavailable
            if (UseRecipeBWLists) {
                foreach (RecipePrototype recipe in recipes.Values) {
                    //part 1: make unavailable if recipe fits the black & doesnt fit the white recipe black lists (these should be the 'barelling' and 'unbarelling' recipes)
                    if (!recipeWhiteList.Any(white => white.IsMatch(recipe.Name)) && recipeBlackList.Any(black => black.IsMatch(recipe.Name))) //if we dont match a whitelist and match a blacklist...
                        recipe.Available = false;
                    //part 2: make unavailable if recipe fits the recyclingItemNameBlackList (should remove any of the barel recycling recipes added by 2.0 SA)
                    foreach (KeyValuePair<string, Regex> recycleBL in recyclingItemNameBlackList)
                        if (recipe.productList.Count == 1 && (Item)recipe.productList[0] == items[recycleBL.Key] && recipe.ingredientList.Count == 1 && recycleBL.Value.IsMatch(recipe.ingredientList[0].Name))
                            recipe.Available = false;
                }
            }


            //step 3: mark any recipe with no unlocks, or 0->0 recipes (industrial revolution... what are those aetheric glow recipes?) as unavailable.
            foreach (RecipePrototype recipe in recipes.Values)
                if (recipe.myUnlockTechnologies.Count == 0 || (recipe.productList.Count == 0 && recipe.ingredientList.Count == 0 && !recipe.Name.StartsWith("§§"))) //§§ denotes foreman added recipes. ignored during this pass (but not during the assembler check pass)
                    recipe.Available = false;

            //step 4 (loop) switch any recipe with no available assemblers to unavailable, switch any useless item to unavailable (no available recipe produces it, it isnt used by any available recipe / only by incineration recipes
            bool clean = false;
            while (!clean) {
                clean = true;

                //4.1: mark any recipe with no available assemblers to unavailable.
                foreach (RecipePrototype recipe in recipes.Values.Where(r => r.Available && !r.Assemblers.Any(a => a.Available || (a as AssemblerPrototype) == playerAssembler || (a as AssemblerPrototype) == rocketAssembler))) {
                    recipe.Available = false;
                    clean = false;
                }

                //4.2: mark any useless items as unavailable (nothing/unavailable recipes produce it, it isnt consumed by anything / only consumed by incineration / only consumed by unavailable recipes, only produced by a itself->itself recipe)
                //this will also update assembler availability status for those whose items become unavailable automatically.
                //note: while this gets rid of those annoying 'burn/incinerate' auto-generated recipes, if the modder decided to have a 'recycle' auto-generated recipe (item->raw ore or something), we will be forced to accept those items as 'available'
                //good example from vanilla: most of the 'garbage' items such as 'item-unknown' and 'electric-energy-interface' are removed as their only recipes are 'recycle to themselves', but 'heat interface' isnt removed as its only recipe is a 'recycle into several parts' (so nothing we can do about it)
                foreach (ItemPrototype item in items.Values.Where(i => i.Available && !i.ProductionRecipes.Any(r => r.Available && !(r.IngredientList.Count == 1 && r.IngredientList[0] == i))).Cast<ItemPrototype>()) {


                    bool useful = false;

                    foreach (RecipePrototype r in item.consumptionRecipes.Where(r => r.Available))
                        useful |= (r.ingredientList.Count > 1 || r.productList.Count > 1 || (r.productList.Count == 1 && r.productList[0] != item)); //recipe with multiple items coming in or some ingredients coming out (that arent itself) -> not an incineration type

                    if (!useful && !item.Name.StartsWith("§§")) {
                        item.Available = false;
                        clean = false;
                        foreach (RecipePrototype r in item.consumptionRecipes) //from above these recipes are all item->nothing
                            r.Available = false;
                    }
                }
                //4.3: go over the item list one more time and ensure that if an item that is available has any growth or spoil results then they are also available (edge case: item grows or spoils into something that has no recipes aka: unavailable, but it should be available even though its only 'use' is as a spoil or grow result)
                foreach (ItemPrototype item in items.Values.Where(i => !i.Available).Cast<ItemPrototype>()) {
                    bool useful = false;
                    useful |= item.spoilOrigins.Count(i => i.Available) > 0;
                    useful |= item.plantOrigins.Count(i => i.Available) > 0;
                    item.Available = useful;
                }

            }

            //step 5: set the 'default' enabled statuses of recipes,assemblers,modules & beacons to their available status.
            foreach (RecipePrototype recipe in recipes.Values)
                recipe.Enabled = recipe.Available;
            foreach (AssemblerPrototype assembler in assemblers.Values)
                assembler.Enabled = assembler.Available;
            foreach (ModulePrototype module in modules.Values)
                module.Enabled = module.Available;
            foreach (BeaconPrototype beacon in beacons.Values)
                beacon.Enabled = beacon.Available;
            playerAssembler?.Enabled = true; //its enabled, so it can theoretically be used, but it is set as 'unavailable' so a warning will be issued if you use it.

            rocketAssembler?.Enabled = assemblers["rocket-silo"]?.Enabled ?? false; //rocket assembler is set to enabled if rocket silo is enabled
            rocketAssembler?.Available = assemblers["rocket-silo"] is not null; //override
        }

        private void CleanupGroups() {
            //step 6: clean up groups and subgroups (delete any subgroups that have no items/recipes, then delete any groups that have no subgroups)
            foreach (SubgroupPrototype subgroup in subgroups.Values.ToList()) {
                if (subgroup.items.Count == 0 && subgroup.recipes.Count == 0 && subgroup.MyGroup is GroupPrototype gp) {
                    gp.subgroups.Remove(subgroup);
                    subgroups.Remove(subgroup.Name);
                }
            }
            foreach (GroupPrototype group in groups.Values.ToList())
                if (group.subgroups.Count == 0)
                    groups.Remove(group.Name);

            //step 7: update subgroups and groups to set them to unavailable if they only contain unavailable items/recipes
            foreach (SubgroupPrototype subgroup in subgroups.Values)
                if (!subgroup.items.Any(i => i.Available) && !subgroup.recipes.Any(r => r.Available))
                    subgroup.Available = false;
            foreach (GroupPrototype group in groups.Values)
                if (!group.subgroups.Any(sg => sg.Available))
                    group.Available = false;

            //step 8: sort groups/subgroups
            foreach (GroupPrototype group in groups.Values)
                group.SortSubgroups();
            foreach (SubgroupPrototype sgroup in subgroups.Values)
                sgroup.SortIRs();

        }

        private void UpdateFluidTemperatureDependencies() {
            //step 9: update the temperature dependent status of items (fluids)
            foreach (FluidPrototype fluid in items.Values.Where(i => i is Fluid)) {
                fRange productionRange = new fRange(double.MaxValue, double.MinValue);
                fRange consumptionRange = new fRange(double.MinValue, double.MaxValue); //a bit different -> the min value is the LARGEST minimum of each consumption recipe, and the max value is the SMALLEST max of each consumption recipe

                foreach (Recipe recipe in fluid.productionRecipes) {
                    productionRange.Min = Math.Min(productionRange.Min, recipe.ProductTemperatureMap[fluid]);
                    productionRange.Max = Math.Max(productionRange.Max, recipe.ProductTemperatureMap[fluid]);
                }
                foreach (Recipe recipe in fluid.consumptionRecipes) {
                    consumptionRange.Min = Math.Max(consumptionRange.Min, recipe.IngredientTemperatureMap[fluid].Min);
                    consumptionRange.Max = Math.Min(consumptionRange.Max, recipe.IngredientTemperatureMap[fluid].Max);
                }
                fluid.IsTemperatureDependent = !(consumptionRange.Contains(productionRange));
            }
        }

        //--------------------------------------------------------------------DEBUG PRINTING FUNCTIONS

        private void PrintDataCache() {
            Console.WriteLine("AVAILABLE: ----------------------------------------------------------------");
            Console.WriteLine("Technologies:");
            foreach (TechnologyPrototype tech in technologies.Values)
                if (tech.Available)
                    Console.WriteLine("    " + tech);
            Console.WriteLine("Groups:");
            foreach (GroupPrototype group in groups.Values)
                if (group.Available)
                    Console.WriteLine("    " + group);
            Console.WriteLine("Subgroups:");
            foreach (SubgroupPrototype sgroup in subgroups.Values)
                if (sgroup.Available)
                    Console.WriteLine("    " + sgroup);
            Console.WriteLine("Items:");
            foreach (ItemPrototype item in items.Values)
                if (item.Available)
                    Console.WriteLine("    " + item);
            Console.WriteLine("Assemblers:");
            foreach (AssemblerPrototype assembler in assemblers.Values)
                if (assembler.Available)
                    Console.WriteLine("    " + assembler);
            Console.WriteLine("Modules:");
            foreach (ModulePrototype module in modules.Values)
                if (module.Available)
                    Console.WriteLine("    " + module);
            Console.WriteLine("Recipes:");
            foreach (RecipePrototype recipe in recipes.Values)
                if (recipe.Available)
                    Console.WriteLine("    " + recipe);
            Console.WriteLine("Beacons:");
            foreach (BeaconPrototype beacon in beacons.Values)
                if (beacon.Available)
                    Console.WriteLine("    " + beacon);

            Console.WriteLine("UNAVAILABLE: ----------------------------------------------------------------");
            Console.WriteLine("Technologies:");
            foreach (TechnologyPrototype tech in technologies.Values)
                if (!tech.Available)
                    Console.WriteLine("    " + tech);
            Console.WriteLine("Groups:");
            foreach (GroupPrototype group in groups.Values)
                if (!group.Available)
                    Console.WriteLine("    " + group);
            Console.WriteLine("Subgroups:");
            foreach (SubgroupPrototype sgroup in subgroups.Values)
                if (!sgroup.Available)
                    Console.WriteLine("    " + sgroup);
            Console.WriteLine("Items:");
            foreach (ItemPrototype item in items.Values)
                if (!item.Available)
                    Console.WriteLine("    " + item);
            Console.WriteLine("Assemblers:");
            foreach (AssemblerPrototype assembler in assemblers.Values)
                if (!assembler.Available)
                    Console.WriteLine("    " + assembler);
            Console.WriteLine("Modules:");
            foreach (ModulePrototype module in modules.Values)
                if (!module.Available)
                    Console.WriteLine("    " + module);
            Console.WriteLine("Recipes:");
            foreach (RecipePrototype recipe in recipes.Values)
                if (!recipe.Available)
                    Console.WriteLine("    " + recipe);
            Console.WriteLine("Beacons:");
            foreach (BeaconPrototype beacon in beacons.Values)
                if (!beacon.Available)
                    Console.WriteLine("    " + beacon);

            Console.WriteLine("TECHNOLOGIES: ----------------------------------------------------------------");
            Console.WriteLine("Technology tiers:");
            foreach (TechnologyPrototype tech in technologies.Values.OrderBy(t => t.Tier)) {
                Console.WriteLine("   T:" + tech.Tier.ToString("000") + " : " + tech.Name);
                foreach (TechnologyPrototype prereq in tech.prerequisites)
                    Console.WriteLine("      > T:" + prereq.Tier.ToString("000" + " : " + prereq.Name));
            }
            Console.WriteLine("Science Pack order:");
            foreach (Item sciPack in sciencePacks)
                Console.WriteLine("   >" + sciPack.FriendlyName);
            Console.WriteLine("Science Pack prerequisites:");
            foreach (Item sciPack in sciencePacks) {
                Console.WriteLine("   >" + sciPack);
                foreach (Item i in sciencePackPrerequisites[sciPack])
                    Console.WriteLine("      >" + i);
            }

            Console.WriteLine("RECIPES: ----------------------------------------------------------------");
            foreach (RecipePrototype recipe in recipes.Values) {
                Console.WriteLine("R: " + recipe.Name);
                foreach (TechnologyPrototype tech in recipe.myUnlockTechnologies)
                    Console.WriteLine("  >" + tech.Tier.ToString("000") + ":" + tech.Name);
                foreach (IReadOnlyList<Item> sciPackList in recipe.MyUnlockSciencePacks) {
                    Console.Write("    >Science Packs Option: ");
                    foreach (Item sciPack in sciPackList)
                        Console.Write(sciPack.Name + ", ");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("TEMPERATURE DEPENDENT FLUIDS: ----------------------------------------------------------------");
            foreach (ItemPrototype fluid in items.Values.Where(i => i is Fluid f && f.IsTemperatureDependent)) {
                Console.WriteLine(fluid.Name);
                HashSet<double> productionTemps = new HashSet<double>();
                foreach (Recipe recipe in fluid.productionRecipes)
                    productionTemps.Add(recipe.ProductTemperatureMap[fluid]);
                Console.Write("   Production ranges:          >");
                foreach (double temp in productionTemps.ToList().OrderBy(t => t))
                    Console.Write(temp + ", ");
                Console.WriteLine();
                Console.Write("   Failed Consumption ranges:  >");
                foreach (Recipe recipe in fluid.consumptionRecipes.Where(r => productionTemps.Any(t => !r.IngredientTemperatureMap[fluid].Contains(t))))
                    Console.Write("(" + recipe.IngredientTemperatureMap[fluid].Min + ">" + recipe.IngredientTemperatureMap[fluid].Max + ": " + recipe.Name + "), ");
                Console.WriteLine();
            }
        }
    }
}