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
        public string PresetName { get; private set; }

        public IEnumerable<Group> AvailableGroups {
            get { return groups.Values.Where(g => g.Available); }
        }

        public IEnumerable<Subgroup> AvailableSubgroups {
            get { return subgroups.Values.Where(g => g.Available); }
        }

        public IEnumerable<Quality> AvailableQualities {
            get { return qualities.Values.Where(g => g.Available); }
        }

        public IEnumerable<Item> AvailableItems {
            get { return items.Values.Where(g => g.Available); }
        }

        public IEnumerable<Recipe> AvailableRecipes {
            get { return recipes.Values.Where(g => g.Available); }
        }

        public IEnumerable<PlantProcess> AvailablePlantProcesses {
            get { return plantProcesses.Values.Where(g => g.Available); }
        }

        // mods: <name, version>
        // others: <name, object>

        public IReadOnlyDictionary<string, string> IncludedMods => includedMods;

        public IReadOnlyDictionary<string, Technology> Technologies => technologies;

        public IReadOnlyDictionary<string, Group> Groups => groups;

        public IReadOnlyDictionary<string, Subgroup> Subgroups => subgroups;

        public IReadOnlyDictionary<string, Quality> Qualities => qualities;

        public IReadOnlyDictionary<string, Item> Items => items;

        public IReadOnlyDictionary<string, Recipe> Recipes => recipes;

        public IReadOnlyDictionary<string, PlantProcess> PlantProcesses => plantProcesses;

        public IReadOnlyDictionary<string, Assembler> Assemblers => assemblers;

        public IReadOnlyDictionary<string, Module> Modules => modules;

        public IReadOnlyDictionary<string, Beacon> Beacons => beacons;

        public IReadOnlyList<Item> SciencePacks => sciencePacks;

        public IReadOnlyDictionary<Item, ICollection<Item>> SciencePackPrerequisites => sciencePackPrerequisites;

        public Assembler PlayerAssembler => playerAssembler;

        public Assembler RocketAssembler => rocketAssembler;

        public Technology StartingTech => startingTech;

        // missing objects are not linked properly and just have the minimal values necessary to function.
        // They are just placeholders, and cant actually be added to graph except while importing. They are also not solved for.
        public Subgroup MissingSubgroup => missingSubgroup;

        public IReadOnlyDictionary<string, Quality> MissingQualities => missingQualities;

        public IReadOnlyDictionary<string, Item> MissingItems => missingItems;

        public IReadOnlyDictionary<string, Assembler> MissingAssemblers => missingAssemblers;

        public IReadOnlyDictionary<string, Module> MissingModules => missingModules;

        public IReadOnlyDictionary<string, Beacon> MissingBeacons => missingBeacons;

        public IReadOnlyDictionary<RecipeShort, Recipe> MissingRecipes => missingRecipes;

        public IReadOnlyDictionary<PlantShort, PlantProcess> MissingPlantProcesses => missingPlantProcesses;

        public Quality DefaultQuality { get; private set; }
        public uint QualityMaxChainLength { get; private set; }
        private Quality ErrorQuality;

        public static Bitmap UnknownIcon => IconCache.GetUnknownIcon();

        private static Bitmap noBeaconIcon;

        public static Bitmap NoBeaconIcon {
            get {
                noBeaconIcon ??= IconCache.GetIcon(Path.Combine("Graphics", "NoBeacon.png"), 64);
                return noBeaconIcon;
            }
        }

        // name : version
        private Dictionary<string, string> includedMods;
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

        private GroupPrototype extraFormanGroup;
        private SubgroupPrototype extractionSubgroupItems;
        private SubgroupPrototype extractionSubgroupFluids;
        // offshore pumps
        private SubgroupPrototype extractionSubgroupFluidsOp;
        // water to steam (boilers)
        private SubgroupPrototype energySubgroupBoiling;
        // heat production (heat consumption is processed as 'fuel'), steam consumption, burning to energy
        private SubgroupPrototype energySubgroupEnergy;
        // any rocket launch recipes will go here
        private SubgroupPrototype rocketLaunchSubgroup;

        private ItemPrototype HeatItem;
        private RecipePrototype HeatRecipe;
        // for burner-generators
        private RecipePrototype BurnerRecipe;

        private Bitmap ElectricityIcon;

        // for handcrafting. Because Fk automation, that's why.
        private AssemblerPrototype playerAssembler;
        // for those rocket recipes
        private AssemblerPrototype rocketAssembler;

        private SubgroupPrototype missingSubgroup;
        private TechnologyPrototype startingTech;
        // missing recipes will have this set as their one and only assembler.
        private AssemblerPrototype missingAssembler;

        private readonly bool UseRecipeBWLists;
        // whitelist takes priority over blacklist
        private static readonly Regex[] recipeWhiteList = [new("^empty-barrel$")];

        private static readonly Regex[] recipeBlackList =
            [new("-barrel$"), new("^deadlock-packrecipe-"), new("^deadlock-unpackrecipe-"), new("^deadlock-plastic-packaging$")];

        private static readonly KeyValuePair<string, Regex>[] recyclingItemNameBlackList = [new("barrel", new Regex("-barrel$"))];

        private Dictionary<string, IconColorPair> iconCache;

        // some mods set the temperature ranges as 'way too high' and expect factorio to handle it (it does).
        // Since we prefer to show temperature ranges we will define any temp beyond these as no limit
        private const double MaxTemp = 10000000;
        private const double MinTemp = -MaxTemp;

        // if true then the read recipes will be filtered by the white and black lists above.
        // In most cases this is desirable (why bother with barreling, etc.???),
        // but if the user want to use them, then so be it.
        public DataCache(bool filterRecipes) {
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
            sciencePacks = [];
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
            startingTech = new TechnologyPrototype(this, "§§t:starting_tech", "Starting Technology") {
                Tier = 0
            };

            extraFormanGroup = new GroupPrototype(this, "§§g:extra_group", "Resource Extraction\nPower Generation\nRocket Launches", "~~~z1");
            extraFormanGroup.SetIconAndColor(new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "ExtraGroupIcon.png"), 64), Color.Gray));

            extractionSubgroupItems = new SubgroupPrototype(this, "§§sg:extraction_items", "1") {
                myGroup = extraFormanGroup
            };
            extraFormanGroup.subgroups.Add(extractionSubgroupItems);

            extractionSubgroupFluids = new SubgroupPrototype(this, "§§sg:extraction_fluids", "2") {
                myGroup = extraFormanGroup
            };
            extraFormanGroup.subgroups.Add(extractionSubgroupFluids);

            extractionSubgroupFluidsOp = new SubgroupPrototype(this, "§§sg:extraction_fluids_2", "3") {
                myGroup = extraFormanGroup
            };
            extraFormanGroup.subgroups.Add(extractionSubgroupFluidsOp);

            energySubgroupBoiling = new SubgroupPrototype(this, "§§sg:energy_boiling", "4") {
                myGroup = extraFormanGroup
            };
            extraFormanGroup.subgroups.Add(energySubgroupBoiling);

            energySubgroupEnergy = new SubgroupPrototype(this, "§§sg:energy_heat", "5") {
                myGroup = extraFormanGroup
            };
            extraFormanGroup.subgroups.Add(energySubgroupEnergy);

            rocketLaunchSubgroup = new SubgroupPrototype(this, "§§sg:rocket_launches", "6") {
                myGroup = extraFormanGroup
            };
            extraFormanGroup.subgroups.Add(rocketLaunchSubgroup);

            ErrorQuality = new QualityPrototype(this, "§§error_quality", "ERROR", "-");

            var heatIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "HeatIcon.png"), 64), Color.DarkRed);
            var burnerGeneratorIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "BurnerGeneratorIcon.png"), 64), Color.DarkRed);
            var playerAssemblerIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "PlayerAssembler.png"), 64), Color.Gray);
            var rocketAssemblerIcon = new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "RocketAssembler.png"), 64), Color.Gray);
            // we don't want heat to appear as an item in the lists, so just give it a blank subgroup.
            HeatItem = new ItemPrototype(this, "§§i:heat", "Heat (1MJ)", new SubgroupPrototype(this, "-", "-"), "-");
            HeatItem.SetIconAndColor(heatIcon);
            HeatItem.FuelValue = 1000000; //1MJ - nice amount

            HeatRecipe = new RecipePrototype(this, "§§r:h:heat-generation", "Heat Generation", energySubgroupEnergy, "1");
            HeatRecipe.SetIconAndColor(heatIcon);
            HeatRecipe.InternalOneWayAddProduct(HeatItem, 1, 0);
            HeatItem.productionRecipes.Add(HeatRecipe);
            HeatRecipe.Time = 1;

            BurnerRecipe = new RecipePrototype(this, "§§r:h:burner-electricity", "Burner Generator", energySubgroupEnergy, "2");
            BurnerRecipe.SetIconAndColor(burnerGeneratorIcon);
            BurnerRecipe.Time = 1;

            playerAssembler = new AssemblerPrototype(this, "§§a:player-assembler", "Player", EntityType.Assembler, EnergySource.Void) {
                energyDrain = 0
            };
            playerAssembler.SetIconAndColor(playerAssemblerIcon);

            rocketAssembler = new AssemblerPrototype(this, "§§a:rocket-assembler", "Rocket", EntityType.Rocket, EnergySource.Void) {
                energyDrain = 0
            };
            rocketAssembler.SetIconAndColor(rocketAssemblerIcon);

            ElectricityIcon = IconCache.GetIcon(Path.Combine("Graphics", "ElectricityIcon.png"), 64);

            missingSubgroup = new SubgroupPrototype(this, "§§MISSING-SG", "") {
                myGroup = new GroupPrototype(this, "§§MISSING-G", "MISSING", "")
            };

            missingAssembler = new AssemblerPrototype(this, "§§a:MISSING-A", "missing assembler", EntityType.Assembler, EnergySource.Void, true);
        }

        public async Task LoadAllData(Preset preset, IProgress<KeyValuePair<int, string>> progress, bool loadIcons = true) {
            Clear();
            //return;

            var craftingCategories = new Dictionary<string, List<RecipePrototype>>();
            var moduleCategories = new Dictionary<string, List<ModulePrototype>>();
            var resourceCategories = new Dictionary<string, List<RecipePrototype>> {
                { "<<foreman_resource_category_water_tile>>", [] } //the water resources
            };
            var fuelCategories = new Dictionary<string, List<ItemPrototype>> {
                { "§§fc:liquids", [] } //the liquid fuels category
            };
            var burnResults = new Dictionary<Item, string>();
            var spoilResults = new Dictionary<Item, string>();
            var nextQualities = new Dictionary<Quality, string>();
            var miningWithFluidRecipes = new List<Recipe>();

            PresetName = preset.Name;
            var jsonData = PresetProcessor.PrepPreset(preset);
            if (jsonData == null)
                return;

            iconCache = loadIcons
                ? await IconCache.LoadIconCache(Path.Combine([Application.StartupPath, "Presets", preset.Name + ".dat"]), progress, 0, 90)
                : new Dictionary<string, IconColorPair>();

            await Task.Run(() => {
                // this is SUPER quick, so we don't need to worry about timing stuff here
                progress.Report(new KeyValuePair<int, string>(90, "Processing Data..."));

                // process each section (order is rather important here)
                foreach (var objJToken in jsonData["mods"].ToList())
                    ProcessMod(objJToken);

                foreach (var objJToken in jsonData["subgroups"].ToList())
                    ProcessSubgroup(objJToken);

                foreach (var objJToken in jsonData["groups"].ToList())
                    ProcessGroup(objJToken, iconCache);

                foreach (var objToken in jsonData["qualities"].ToList())
                    ProcessQuality(objToken, iconCache, nextQualities);
                foreach (var quality in qualities.Values.Cast<QualityPrototype>())
                    ProcessQualityLink(quality, nextQualities);
                PostProcessQuality();

                foreach (var objJToken in jsonData["fluids"].ToList())
                    ProcessFluid(objJToken, iconCache, fuelCategories);

                // items after fluids to take care of duplicates (if name exists in fluid and in item set, then only the fluid is counted)
                foreach (var objJToken in jsonData["items"].ToList())
                    ProcessItem(objJToken, iconCache, fuelCategories, burnResults,
                        spoilResults);
                // link up any items with burn remains
                foreach (var item in items.Values.Cast<ItemPrototype>())
                    ProcessBurnItem(item, burnResults);
                // process items json specifically for plant processes (items should all be populated by now)
                foreach (var objJToken in jsonData["items"].ToList())
                    ProcessPlantProcess(objJToken);
                // link up any items with spoil remains
                foreach (var item in items.Values.Cast<ItemPrototype>())
                    ProcessSpoilItem(item, spoilResults);

                foreach (var objJToken in jsonData["modules"].ToList())
                    ProcessModule(objJToken, iconCache, moduleCategories);

                foreach (var objJToken in jsonData["recipes"].ToList())
                    ProcessRecipe(objJToken, iconCache, craftingCategories, moduleCategories);

                foreach (var objJToken in jsonData["resources"].ToList())
                    ProcessResource(objJToken, resourceCategories, miningWithFluidRecipes);
                foreach (var objToken in jsonData["water_resources"].ToList())
                    ProcessResource(objToken, resourceCategories, miningWithFluidRecipes);

                foreach (var objJToken in jsonData["technologies"].ToList())
                    ProcessTechnology(objJToken, iconCache, miningWithFluidRecipes);
                // required to properly link technology prerequisites
                foreach (var objJToken in jsonData["technologies"].ToList())
                    ProcessTechnologyP2(objJToken);

                foreach (var objJToken in jsonData["entities"].ToList())
                    ProcessEntity(objJToken, iconCache, craftingCategories, resourceCategories, fuelCategories, miningWithFluidRecipes, moduleCategories);

                // process launch products (empty now - deprecated)
                foreach (var objJToken in jsonData["items"].Where(t => t["rocket_launch_products"] != null).ToList())
                    ProcessRocketLaunch(objJToken);
                foreach (var objJToken in jsonData["fluids"].Where(t => t["rocket_launch_products"] != null).ToList())
                    ProcessRocketLaunch(objJToken);

                // process character
                ProcessCharacter(jsonData["entities"].First(a => (string) a["name"] == "character"), craftingCategories);

                // add rocket assembler
                assemblers.Add(rocketAssembler.Name, rocketAssembler);

                // remove these temporary dictionaries (no longer necessary)
                craftingCategories.Clear();
                resourceCategories.Clear();
                fuelCategories.Clear();
                burnResults.Clear();
                spoilResults.Clear();


                // sort

                foreach (var g in groups.Values.Cast<GroupPrototype>())
                    g.SortSubgroups();
                foreach (var sg in subgroups.Values.Cast<SubgroupPrototype>())
                    sg.SortIRs();

                //The data read by the dataCache (json preset) includes everything.
                //We need to now process it such that any items/recipes that cant be used dont appear.
                //thus any object that has Unavailable set to true should be ignored.
                //We will leave the option to use them to the user, but in most cases it's better without them


                // delete any recipe that has no assembler.
                // This is the only type of deletion that we will do, as we MUST enforce the 'at least 1 assembler' per recipe.
                // The only recipes with no assemblers linked are those added to 'missing' category, and those are handled separately.
                //note that even handcrafting has been handled: there is a player assembler that has been added.
                //So the only recipes removed here are those that literally can not be crafted.

                foreach (var recipe in recipes.Values.Where(r => r.Assemblers.Count == 0).ToList().Cast<RecipePrototype>()) {
                    foreach (var ingredient in recipe.ingredientList)
                        ingredient.consumptionRecipes.Remove(recipe);
                    foreach (var product in recipe.productList)
                        product.productionRecipes.Remove(recipe);
                    foreach (var tech in recipe.myUnlockTechnologies)
                        tech.unlockedRecipes.Remove(recipe);
                    foreach (var module in recipe.assemblerModules)
                        module.recipes.Remove(recipe);
                    recipe.mySubgroup.recipes.Remove(recipe);

                    recipes.Remove(recipe.Name);
                    ErrorLogging.LogLine($"Removal of {recipe} due to having no assemblers associated with it.");
                    Console.WriteLine($"Removal of {recipe} due to having no assemblers associated with it.");
                }

                // calculate the availability of various recipes and entities (based on their unlock technologies + entity place objects' unlock technologies)
                ProcessAvailableStatuses();

                // calculate the science packs for each technology (based on both their listed science packs, the science packs of their prerequisites,
                // and the science packs required to research the science packs)
                ProcessSciencePacks();

                // delete any groups/subgroups without any items/recipes within them, and sort by order
                CleanupGroups();

                // check each fluid to see if all production recipe temperatures can fit within all consumption recipe ranges.
                // if not, then the item / fluid is set to be 'temperature dependent' and requires further processing when checking link validity.
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
                foreach (var iconSet in iconCache.Values)
                    iconSet.Icon.Dispose();
                iconCache.Clear();
            }

            groups.Add(extraFormanGroup.Name, extraFormanGroup);
            subgroups.Add(extractionSubgroupItems.Name, extractionSubgroupItems);
            subgroups.Add(extractionSubgroupFluids.Name, extractionSubgroupFluids);
            subgroups.Add(extractionSubgroupFluidsOp.Name, extractionSubgroupFluidsOp);
            items.Add(HeatItem.Name, HeatItem);
            recipes.Add(HeatRecipe.Name, HeatRecipe);
            recipes.Add(BurnerRecipe.Name, BurnerRecipe);
            technologies.Add(StartingTech.Name, startingTech);
        }

        //------------------------------------------------------Import processing

        // will ensure that all items are now part of the data cache -> existing ones (regular and missing) are skipped, new ones are added to MissingItems
        public void ProcessImportedItemsSet(IEnumerable<string> itemNames) {
            foreach (var iItem in itemNames) {
                // want to check for missing items too - in this case don't want duplicates
                if (items.ContainsKey(iItem) || missingItems.ContainsKey(iItem))
                    continue;

                // just assume it isn't a fluid. we don't honestly care (no temperatures)
                var missingItem = new ItemPrototype(this, iItem, iItem, missingSubgroup, "", true);
                missingItems.Add(missingItem.Name, missingItem);
            }
        }

        public Dictionary<string, Quality> ProcessImportedQualitiesSet(IEnumerable<KeyValuePair<string, int>> qualityPairs) {
            // check that a quality exists in the set of qualities (missing or otherwise) that has the correct level; if not, make a new one
            var qualityMap = new Dictionary<string, Quality>();

            foreach (var quality in qualityPairs) {
                // check quality sets for any direct matches (name & level)

                if (qualities.Values.Any(q => q.Name == quality.Key && q.Level == quality.Value)) {
                    qualityMap.Add(quality.Key, qualities[quality.Key]);
                    continue;
                }

                if (missingQualities.Values.Any(q => q.Name == quality.Key && q.Level == quality.Value)) {
                    qualityMap.Add(quality.Key, missingQualities[quality.Key]);
                    continue;
                }

                // check for any matching level quality in the base chain (starting from 'normal' and going until null)

                var curQuality = DefaultQuality;
                while (curQuality != null) {
                    if (curQuality.Level == quality.Value)
                        break;
                    curQuality = curQuality.NextQuality;
                }

                if (curQuality != null) {
                    qualityMap.Add(quality.Key, curQuality);
                    continue;
                }

                // step 3:
                // check if there is a quality of the same level

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

                // step 4:
                // no other option, make a new quality and add it to missing qualities

                var missingQualityName = quality.Key;
                while (qualities.ContainsKey(missingQualityName) || missingQualities.ContainsKey(missingQualityName))
                    missingQualityName += "_";

                var missingQuality = new QualityPrototype(this, missingQualityName, quality.Key, "-", true);
                missingQualities.Add(missingQuality.Name, missingQuality);
                qualityMap.Add(quality.Key, null);
            }

            return qualityMap;
        }

        public void ProcessImportedAssemblersSet(IEnumerable<string> assemblerNames) {
            foreach (var iAssembler in assemblerNames) {
                if (assemblers.ContainsKey(iAssembler) || missingAssemblers.ContainsKey(iAssembler))
                    continue;

                // don't know, don't care about entity type we will just treat it as a void-assembler (and let fuel io + recipe figure it out)
                var missingAssembler = new AssemblerPrototype(this, iAssembler, iAssembler, EntityType.Assembler, EnergySource.Void, true);
                missingAssemblers.Add(missingAssembler.Name, missingAssembler);
            }
        }

        public void ProcessImportedModulesSet(IEnumerable<string> moduleNames) {
            foreach (var iModule in moduleNames) {
                if (modules.ContainsKey(iModule) || missingModules.ContainsKey(iModule))
                    continue;

                var missingModule = new ModulePrototype(this, iModule, iModule, true);
                missingModules.Add(missingModule.Name, missingModule);
            }
        }

        public void ProcessImportedBeaconsSet(IEnumerable<string> beaconNames) {
            foreach (var iBeacon in beaconNames) {
                if (beacons.ContainsKey(iBeacon) || missingBeacons.ContainsKey(iBeacon))
                    continue;

                var missingBeacon = new BeaconPrototype(this, iBeacon, iBeacon, EnergySource.Void, true);
                missingBeacons.Add(missingBeacon.Name, missingBeacon);
            }
        }

        // will ensure all recipes are now part of the data cache -> each one is checked against existing recipes (regular & missing),
        // and if it doesn't exist are added to MissingRecipes. Returns a set of links of original recipeID (NOT! the new recipeIDs) to the recipe
        public Dictionary<long, Recipe> ProcessImportedRecipesSet(IEnumerable<RecipeShort> recipeShorts) {
            var recipeLinks = new Dictionary<long, Recipe>();
            foreach (var recipeShort in recipeShorts) {
                Recipe recipe = null;

                // recipe check #1: does its name exist in database
                // (note: we don't quite care about extra missing recipes here - so what if we have a couple identical ones? they will combine during save/load anyway)

                var recipeExists = recipes.ContainsKey(recipeShort.Name);
                if (recipeExists) {
                    // recipe check #2: do the number of ingredients & products match?

                    recipe = recipes[recipeShort.Name];
                    recipeExists &= recipeShort.Ingredients.Count == recipe.IngredientList.Count;
                    recipeExists &= recipeShort.Products.Count == recipe.ProductList.Count;
                }

                // recipe check #3: do the ingredients & products from the loaded data match the actual recipe?
                // (names, not quantities -> this is to allow some recipes to pass; ex: normal->expensive might change the values,
                // but importing such a recipe should just use the 'correct' quantities and soft-pass the different recipe)

                if (recipeExists) {
                    foreach (var ingredient in recipeShort.Ingredients.Keys)
                        recipeExists &= items.ContainsKey(ingredient) && recipe.IngredientSet.ContainsKey(items[ingredient]);
                    foreach (var product in recipeShort.Products.Keys)
                        recipeExists &= items.ContainsKey(product) && recipe.ProductSet.ContainsKey(items[product]);
                }

                if (!recipeExists) {
                    var missingRecipeExists = missingRecipes.ContainsKey(recipeShort);

                    if (missingRecipeExists) {
                        recipe = missingRecipes[recipeShort];
                    } else {
                        var missingRecipe = new RecipePrototype(this, recipeShort.Name, recipeShort.Name, missingSubgroup, "", true);
                        foreach (var ingredient in recipeShort.Ingredients) {
                            if (items.TryGetValue(ingredient.Key, out var item))
                                missingRecipe.InternalOneWayAddIngredient((ItemPrototype) item, ingredient.Value);
                            else
                                missingRecipe.InternalOneWayAddIngredient((ItemPrototype) missingItems[ingredient.Key], ingredient.Value);
                        }

                        foreach (var product in recipeShort.Products) {
                            if (items.TryGetValue(product.Key, out var item))
                                missingRecipe.InternalOneWayAddProduct((ItemPrototype) item, product.Value, 0);
                            else
                                missingRecipe.InternalOneWayAddProduct((ItemPrototype) missingItems[product.Key], product.Value, 0);
                        }

                        missingRecipe.assemblers.Add(missingAssembler);
                        missingAssembler.Recipes.Add(missingRecipe);

                        missingRecipes.Add(recipeShort, missingRecipe);
                        recipe = missingRecipe;
                    }
                }

                if (!recipeLinks.ContainsKey(recipeShort.RecipeID))
                    recipeLinks.Add(recipeShort.RecipeID, recipe);
            }

            return recipeLinks;
        }

        // pretty much a copy of the above, just for plant processes (so no ingredient list, and using different data sets)
        public Dictionary<long, PlantProcess> ProcessImportedPlantProcessesSet(IEnumerable<PlantShort> plantShorts) {
            var plantLinks = new Dictionary<long, PlantProcess>();
            foreach (var plantShort in plantShorts) {
                PlantProcess plantProcess = null;

                // recipe check #1: does its name exist in database
                // (note: we don't quite care about extra missing recipes here - so what if we have a couple identical ones?
                // they will combine during save/load anyway)

                var plantProcessExist = plantProcesses.ContainsKey(plantShort.Name);
                if (plantProcessExist) {
                    // recipe check #2: do the number of ingredients & products match?

                    plantProcess = plantProcesses[plantShort.Name];
                    plantProcessExist &= plantShort.Products.Count == plantProcess.ProductList.Count;
                }

                // recipe check #3: do the ingredients & products from the loaded data match the actual recipe?
                // (names, not quantities -> this is to allow some recipes to pass; ex: normal->expensive might change the values,
                // but importing such a recipe should just use the 'correct' quantities and soft-pass the different recipe)

                if (plantProcessExist) {
                    foreach (var product in plantShort.Products.Keys)
                        plantProcessExist &= items.ContainsKey(product) && plantProcess.ProductSet.ContainsKey(items[product]);
                }

                if (!plantProcessExist) {
                    var missingPProcessExists = missingPlantProcesses.ContainsKey(plantShort);

                    if (missingPProcessExists) {
                        plantProcess = missingPlantProcesses[plantShort];
                    } else {
                        var missingPProcess = new PlantProcessPrototype(this, plantShort.Name, true);
                        foreach (var product in plantShort.Products) {
                            if (items.TryGetValue(product.Key, out var item))
                                missingPProcess.InternalOneWayAddProduct((ItemPrototype) item, product.Value);
                            else
                                missingPProcess.InternalOneWayAddProduct((ItemPrototype) missingItems[product.Key], product.Value);
                        }

                        missingPlantProcesses.Add(plantShort, missingPProcess);
                        plantProcess = missingPProcess;
                    }
                }

                if (!plantLinks.ContainsKey(plantShort.PlantID))
                    plantLinks.Add(plantShort.PlantID, plantProcess);
            }

            return plantLinks;
        }

        //------------------------------------------------------Data cache load helper functions (all the process functions from LoadAllData)

        private void ProcessMod(JToken objJToken) {
            includedMods.Add((string) objJToken["name"], (string) objJToken["version"]);
        }

        private void ProcessSubgroup(JToken objJToken) {
            var subgroup = new SubgroupPrototype(
                this,
                (string) objJToken["name"],
                (string) objJToken["order"]);

            subgroups.Add(subgroup.Name, subgroup);
        }

        private void ProcessGroup(JToken objJToken, Dictionary<string, IconColorPair> iconCache) {
            var group = new GroupPrototype(
                this,
                (string) objJToken["name"],
                (string) objJToken["localised_name"],
                (string) objJToken["order"]);

            if (iconCache.ContainsKey((string) objJToken["icon_name"]))
                group.SetIconAndColor(iconCache[(string) objJToken["icon_name"]]);

            foreach (var subgroupJToken in objJToken["subgroups"]) {
                ((SubgroupPrototype) subgroups[(string) subgroupJToken]).myGroup = group;
                group.subgroups.Add((SubgroupPrototype) subgroups[(string) subgroupJToken]);
            }

            groups.Add(group.Name, group);
        }

        private void ProcessQuality(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<Quality, string> nextQualities) {
            var quality = new QualityPrototype(
                this,
                (string) objJToken["name"],
                (string) objJToken["localised_name"],
                (string) objJToken["order"]);

            if (iconCache.ContainsKey((string) objJToken["icon_name"]))
                quality.SetIconAndColor(iconCache[(string) objJToken["icon_name"]]);

            quality.Available = !(bool) objJToken["hidden"];
            // can be set via science packs, but this requires modifying datacache... so later
            quality.Enabled = quality.Available;

            quality.Level = (int) objJToken["level"];
            quality.BeaconPowerMultiplier = (double) objJToken["beacon_power_multiplier"];
            quality.MiningDrillResourceDrainMultiplier = (double) objJToken["mining_drill_resource_drain_multiplier"];
            quality.NextProbability = objJToken["next_probability"] != null ? (double) objJToken["next_probability"] : 0;

            if (quality.NextProbability != 0)
                nextQualities.Add(quality, (string) objJToken["next"]);

            qualities.Add(quality.Name, quality);
        }

        private void ProcessQualityLink(QualityPrototype quality, Dictionary<Quality, string> nextQualities) {
            if (!nextQualities.ContainsKey(quality) || !qualities.ContainsKey(nextQualities[quality]))
                return;

            quality.NextQuality = qualities[nextQualities[quality]];
            ((QualityPrototype) qualities[nextQualities[quality]]).PrevQuality = quality;
        }

        private void PostProcessQuality() {
            // make sure that the default quality is always enabled & available

            DefaultQuality = qualities.TryGetValue("normal", out var quality1) ? quality1 : ErrorQuality;
            DefaultQuality.Enabled = true;
            ((QualityPrototype) DefaultQuality).Available = true;

            //make available all qualities that are within the default quality chain

            var cQuality = DefaultQuality;
            while (cQuality != null) {
                ((QualityPrototype) cQuality).Available = cQuality.Enabled;
                cQuality = cQuality.NextQuality;
            }

            foreach (var quality in qualities.Values) {
                uint currentChain = 1;
                var currentQuality = quality;
                while (currentQuality.NextQuality != null && currentQuality.NextProbability != 0) {
                    currentChain++;
                    currentQuality = currentQuality.NextQuality;
                }

                QualityMaxChainLength = Math.Max(QualityMaxChainLength, currentChain);
            }
        }

        private void ProcessFluid(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<ItemPrototype>> fuelCategories) {
            var item = new FluidPrototype(
                this,
                (string) objJToken["name"],
                (string) objJToken["localised_name"],
                (SubgroupPrototype) subgroups[(string) objJToken["subgroup"]],
                (string) objJToken["order"]);

            if (iconCache.ContainsKey((string) objJToken["icon_name"]))
                item.SetIconAndColor(iconCache[(string) objJToken["icon_name"]]);

            item.DefaultTemperature = (double) objJToken["default_temperature"];
            item.SpecificHeatCapacity = (double) objJToken["heat_capacity"];
            item.GasTemperature = (double) objJToken["gas_temperature"];
            item.MaxTemperature = (double) objJToken["max_temperature"];

            if (objJToken["fuel_value"] != null && (double) objJToken["fuel_value"] > 0) {
                item.FuelValue = (double) objJToken["fuel_value"];
                item.PollutionMultiplier = (double) objJToken["emissions_multiplier"];
                fuelCategories["§§fc:liquids"].Add(item);
            }

            items.Add(item.Name, item);
        }

        private void ProcessItem(
            JToken objJToken,
            Dictionary<string, IconColorPair> iconCache,
            Dictionary<string, List<ItemPrototype>> fuelCategories,
            Dictionary<Item, string> burnResults,
            Dictionary<Item, string> spoilResults
        ) {
            // special handling for fluids which appear in both items & fluid lists (ex: fluid-unknown)
            if (items.ContainsKey((string) objJToken["name"]))
                return;

            var item = new ItemPrototype(
                this,
                (string) objJToken["name"],
                (string) objJToken["localised_name"],
                (SubgroupPrototype) subgroups[(string) objJToken["subgroup"]],
                (string) objJToken["order"]);

            if (iconCache.ContainsKey((string) objJToken["icon_name"]))
                item.SetIconAndColor(iconCache[(string) objJToken["icon_name"]]);

            item.StackSize = (int) objJToken["stack_size"];
            item.Weight = (double) objJToken["weight"];
            item.IngredientToWeightCoefficient = (double) objJToken["ingredient_to_weight_coefficient"];

            // factorio eliminates any 0fuel value fuel from the list (checked)
            if (objJToken["fuel_category"] != null && (double) objJToken["fuel_value"] > 0) {
                item.FuelValue = (double) objJToken["fuel_value"];
                item.PollutionMultiplier = (double) objJToken["fuel_emissions_multiplier"];

                if (!fuelCategories.ContainsKey((string) objJToken["fuel_category"]))
                    fuelCategories.Add((string) objJToken["fuel_category"], []);
                fuelCategories[(string) objJToken["fuel_category"]].Add(item);
            }

            if (objJToken["burnt_result"] != null)
                burnResults.Add(item, (string) objJToken["burnt_result"]);
            if (objJToken["spoil_result"] != null) {
                spoilResults.Add(item, (string) objJToken["spoil_result"]);
                foreach (var spoilToken in objJToken["q_spoil_time"])
                    item.spoilageTimes.Add(qualities[(string) spoilToken["quality"]], (double) spoilToken["value"]);
            }

            items.Add(item.Name, item);
        }

        private void ProcessBurnItem(ItemPrototype item, Dictionary<Item, string> burnResults) {
            if (!burnResults.TryGetValue(item, out var burnResult))
                return;

            item.BurnResult = items[burnResult];
            ((ItemPrototype) items[burnResults[item]]).FuelOrigin = item;
        }

        private void ProcessPlantProcess(JToken objJToken) {
            if (objJToken["plant_results"] == null)
                return;

            var seed = (ItemPrototype) items[(string) objJToken["name"]];
            var plantProcess = new PlantProcessPrototype(
                this,
                seed.Name) {
                Seed = seed,
                GrowTime = (double) objJToken["plant_growth_time"]
            };

            foreach (var productJToken in objJToken["plant_results"].ToList()) {
                var product = (ItemPrototype) items[(string) productJToken["name"]];
                var amount = (double) productJToken["amount"];
                if (amount == 0)
                    continue;

                plantProcess.InternalOneWayAddProduct(product, amount);
                product.plantOrigins.Add(seed);
                seed.PlantResult = plantProcess;
            }

            // seed.Name = plantProcess.name, but for clarity: any searches will be done via seed's name
            plantProcesses.Add(seed.Name, plantProcess);
        }

        private void ProcessSpoilItem(ItemPrototype item, Dictionary<Item, string> spoilResults) {
            if (!spoilResults.TryGetValue(item, out var spoilResult))
                return;

            item.SpoilResult = items[spoilResult];
            ((ItemPrototype) items[spoilResults[item]]).spoilOrigins.Add(item);
        }

        private void ProcessModule(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<ModulePrototype>> moduleCategories) {
            var module = new ModulePrototype(
                this,
                (string) objJToken["name"],
                (string) objJToken["localised_name"]);

            if (iconCache.ContainsKey((string) objJToken["icon_name"]))
                module.SetIconAndColor(iconCache[(string) objJToken["icon_name"]]);
            else if (iconCache.ContainsKey((string) objJToken["icon_alt_name"]))
                module.SetIconAndColor(iconCache[(string) objJToken["icon_alt_name"]]);

            module.SpeedBonus = Math.Round((double) objJToken["module_effects"]["speed"] * 1000, 0, MidpointRounding.AwayFromZero) / 1000;
            module.ProductivityBonus = Math.Round((double) objJToken["module_effects"]["productivity"] * 1000, 0, MidpointRounding.AwayFromZero) / 1000;
            module.ConsumptionBonus = Math.Round((double) objJToken["module_effects"]["consumption"] * 1000, 0, MidpointRounding.AwayFromZero) / 1000;
            module.PollutionBonus = Math.Round((double) objJToken["module_effects"]["pollution"] * 1000, 0, MidpointRounding.AwayFromZero) / 1000;
            module.QualityBonus = Math.Round((double) objJToken["module_effects"]["quality"] * 1000, 0, MidpointRounding.AwayFromZero) / 1000;

            module.Tier = (int) objJToken["tier"];

            module.Category = (string) objJToken["category"];
            if (!moduleCategories.ContainsKey(module.Category))
                moduleCategories.Add(module.Category, []);
            moduleCategories[module.Category].Add(module);

            modules.Add(module.Name, module);
        }

        private void ProcessRecipe(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<RecipePrototype>> craftingCategories,
            Dictionary<string, List<ModulePrototype>> moduleCategories) {
            var recipe = new RecipePrototype(
                this,
                (string) objJToken["name"],
                (string) objJToken["localised_name"],
                (SubgroupPrototype) subgroups[(string) objJToken["subgroup"]],
                (string) objJToken["order"]) {
                Time = (double) objJToken["energy"]
            };

            // due to the way the import of presets happens,
            // enabled at this stage means the recipe is available without any research necessary
            // (aka: available at start)

            if ((bool) objJToken["enabled"]) {
                recipe.myUnlockTechnologies.Add(startingTech);
                startingTech.unlockedRecipes.Add(recipe);
            }

            var category = (string) objJToken["category"];
            if (!craftingCategories.ContainsKey(category))
                craftingCategories.Add(category, []);
            craftingCategories[category].Add(recipe);

            if (iconCache.ContainsKey((string) objJToken["icon_name"]))
                recipe.SetIconAndColor(iconCache[(string) objJToken["icon_name"]]);
            else if (iconCache.ContainsKey((string) objJToken["icon_alt_name"]))
                recipe.SetIconAndColor(iconCache[(string) objJToken["icon_alt_name"]]);

            recipe.HasProductivityResearch = objJToken["prod_research"] != null && (bool) objJToken["prod_research"];
            recipe.MaxProductivityBonus = objJToken["maximum_productivity"] == null ? 1000 : (double) objJToken["maximum_productivity"];

            foreach (var productJToken in objJToken["products"].ToList()) {
                var product = (ItemPrototype) items[(string) productJToken["name"]];
                var amount = (double) productJToken["amount"];
                if (amount == 0)
                    continue;

                if ((string) productJToken["type"] == "fluid")
                    recipe.InternalOneWayAddProduct(product, amount, (double) productJToken["p_amount"],
                        productJToken["temperature"] == null ? ((FluidPrototype) product).DefaultTemperature : (double) productJToken["temperature"]);
                else
                    recipe.InternalOneWayAddProduct(product, amount, (double) productJToken["p_amount"]);

                product.productionRecipes.Add(recipe);
            }

            foreach (var ingredientJToken in objJToken["ingredients"].ToList()) {
                var ingredient = (ItemPrototype) items[(string) ingredientJToken["name"]];
                var amount = (double) ingredientJToken["amount"];
                if (amount == 0)
                    continue;

                var minTemp = (string) ingredientJToken["type"] == "fluid" && ingredientJToken["minimum_temperature"] != null
                    ? (double) ingredientJToken["minimum_temperature"]
                    : double.NegativeInfinity;
                var maxTemp = (string) ingredientJToken["type"] == "fluid" && ingredientJToken["maximum_temperature"] != null
                    ? (double) ingredientJToken["maximum_temperature"]
                    : double.PositiveInfinity;

                if (minTemp < MinTemp)
                    minTemp = double.NegativeInfinity;
                if (maxTemp > MaxTemp)
                    maxTemp = double.PositiveInfinity;

                recipe.InternalOneWayAddIngredient(ingredient, amount, minTemp, maxTemp);
                ingredient.consumptionRecipes.Add(recipe);
            }

            if (objJToken["allowed_effects"] != null) {
                recipe.AllowConsumptionBonus = (bool) objJToken["allowed_effects"]["consumption"];
                recipe.AllowSpeedBonus = (bool) objJToken["allowed_effects"]["speed"];
                recipe.AllowProductivityBonus = (bool) objJToken["allowed_effects"]["productivity"];
                recipe.AllowPollutionBonus = (bool) objJToken["allowed_effects"]["pollution"];
                recipe.AllowQualityBonus = (bool) objJToken["allowed_effects"]["quality"];

                foreach (var module in modules.Values.Cast<ModulePrototype>()) {
                    var validModule = (recipe.AllowConsumptionBonus || module.ConsumptionBonus >= 0) &&
                        (recipe.AllowSpeedBonus || module.SpeedBonus <= 0) &&
                        (recipe.AllowProductivityBonus || module.ProductivityBonus <= 0) &&
                        (recipe.AllowPollutionBonus || module.PollutionBonus >= 0) &&
                        (recipe.AllowQualityBonus || module.QualityBonus <= 0);
                    if (validModule) {
                        recipe.beaconModules.Add(module);
                    }
                }

                if (objJToken["allowed_module_categories"] == null || !objJToken["allowed_module_categories"].Any()) {
                    foreach (var module in modules.Values.Cast<ModulePrototype>()) {
                        var validModule = (recipe.AllowConsumptionBonus || module.ConsumptionBonus >= 0) &&
                            (recipe.AllowSpeedBonus || module.SpeedBonus <= 0) &&
                            (recipe.AllowProductivityBonus || module.ProductivityBonus <= 0) &&
                            (recipe.AllowPollutionBonus || module.PollutionBonus >= 0) &&
                            (recipe.AllowQualityBonus || module.QualityBonus <= 0);
                        if (!validModule)
                            continue;

                        recipe.assemblerModules.Add(module);
                        module.recipes.Add(recipe);
                    }
                } else {
                    foreach (var moduleCategory in objJToken["allowed_module_categories"].Select(a => ((JProperty) a).Name)) {
                        if (!moduleCategories.TryGetValue(moduleCategory, out var tempCategory))
                            continue;

                        foreach (var module in tempCategory) {
                            var validModule = (recipe.AllowConsumptionBonus || module.ConsumptionBonus >= 0) &&
                                (recipe.AllowSpeedBonus || module.SpeedBonus <= 0) &&
                                (recipe.AllowProductivityBonus || module.ProductivityBonus <= 0) &&
                                (recipe.AllowPollutionBonus || module.PollutionBonus >= 0) &&
                                (recipe.AllowQualityBonus || module.QualityBonus <= 0);
                            if (!validModule)
                                continue;

                            recipe.assemblerModules.Add(module);
                            module.recipes.Add(recipe);
                        }
                    }
                }
            }

            recipes.Add(recipe.Name, recipe);
        }

        private string GetExtractionRecipeName(string itemName) {
            return "§§r:e:" + itemName;
        }

        private void ProcessResource(JToken objJToken, Dictionary<string, List<RecipePrototype>> resourceCategories, List<Recipe> miningWithFluidRecipes) {
            if (objJToken["products"].Count() == 0)
                return;

            var recipe = new RecipePrototype(
                this,
                GetExtractionRecipeName((string) objJToken["name"]),
                (string) objJToken["localised_name"] + " Extraction",
                (string) objJToken["products"][0]["type"] == "fluid" ? extractionSubgroupFluids : extractionSubgroupItems,
                (string) objJToken["name"]) {
                Time = (double) objJToken["mining_time"]
            };

            foreach (var productJToken in objJToken["products"]) {
                if (!items.ContainsKey((string) productJToken["name"]) || (double) productJToken["amount"] <= 0)
                    continue;
                var product = (ItemPrototype) items[(string) productJToken["name"]];
                recipe.InternalOneWayAddProduct(product, (double) productJToken["amount"], (double) productJToken["amount"]);
                product.productionRecipes.Add(recipe);
            }

            if (recipe.productList.Count == 0) {
                recipe.mySubgroup.recipes.Remove(recipe);
                return;
            }

            if (objJToken["required_fluid"] != null && (double) objJToken["fluid_amount"] != 0) {
                var reqLiquid = (ItemPrototype) items[(string) objJToken["required_fluid"]];
                recipe.InternalOneWayAddIngredient(reqLiquid, (double) objJToken["fluid_amount"]);
                reqLiquid.consumptionRecipes.Add(recipe);
                miningWithFluidRecipes.Add(recipe);
            }

            // we will let the assembler sort out which module can be used with this recipe

            foreach (var module in modules.Values.Cast<ModulePrototype>()) {
                module.recipes.Add(recipe);
                recipe.assemblerModules.Add(module);
            }

            recipe.SetIconAndColor(new IconColorPair(recipe.productList[0].Icon, recipe.productList[0].AverageColor));

            var category = (string) objJToken["resource_category"];
            if (!resourceCategories.ContainsKey(category))
                resourceCategories.Add(category, []);
            resourceCategories[category].Add(recipe);

            // resource recipe will be processed when adding to miners
            // (each miner that can use this recipe will have its recipe's techs added to unlock tech of the resource recipe)
            // this is for any non-fluid based resource!
            // (fluid based item mining is locked behind research and processed in research function)

            //recipe.myUnlockTechnologies.Add(startingTech);
            //startingTech.unlockedRecipes.Add(recipe);

            recipes.Add(recipe.Name, recipe);
        }

        private void ProcessTechnology(JToken objJToken, Dictionary<string, IconColorPair> iconCache, List<Recipe> miningWithFluidRecipes) {
            var technology = new TechnologyPrototype(
                this,
                (string) objJToken["name"],
                (string) objJToken["localised_name"]);

            if (iconCache.ContainsKey((string) objJToken["icon_name"]))
                technology.SetIconAndColor(iconCache[(string) objJToken["icon_name"]]);

            // not sure - factorio documentation states 'enabled' means 'available at start',
            // but in this case 'enabled' being false seems to represent the technology not appearing on screen (same as hidden)???
            // I will just work with what tests show -> tech is available if it is enabled & not hidden.
            technology.Available = !(bool) objJToken["hidden"] && (bool) objJToken["enabled"];

            foreach (var recipe in objJToken["recipes"]) {
                if (!recipes.ContainsKey((string) recipe))
                    continue;

                ((RecipePrototype) recipes[(string) recipe]).myUnlockTechnologies.Add(technology);
                technology.unlockedRecipes.Add((RecipePrototype) recipes[(string) recipe]);
            }

            foreach (var qualityName in objJToken["qualities"]) {
                if (!qualities.TryGetValue((string) qualityName, out var quality))
                    continue;

                ((QualityPrototype) quality).myUnlockTechnologies.Add(technology);
                technology.unlockedQualities.Add((QualityPrototype) quality);
            }

            if (objJToken["unlocks-mining-with-fluid"] != null) {
                foreach (var recipe in miningWithFluidRecipes.Cast<RecipePrototype>()) {
                    recipe.myUnlockTechnologies.Add(technology);
                    technology.unlockedRecipes.Add(recipe);
                }
            }

            foreach (var ingredientJToken in objJToken["research_unit_ingredients"].ToList()) {
                var name = (string) ingredientJToken["name"];
                var amount = (double) ingredientJToken["amount"];

                if (amount == 0)
                    continue;

                technology.InternalOneWayAddSciPack((ItemPrototype) items[name], amount);
                ((ItemPrototype) items[name]).consumptionTechnologies.Add(technology);
            }

            technologies.Add(technology.Name, technology);
        }

        private void ProcessTechnologyP2(JToken objJToken) {
            var technology = (TechnologyPrototype) technologies[(string) objJToken["name"]];
            foreach (var prerequisite in objJToken["prerequisites"]) {
                if (!technologies.ContainsKey((string) prerequisite))
                    continue;

                technology.prerequisites.Add((TechnologyPrototype) technologies[(string) prerequisite]);
                ((TechnologyPrototype) technologies[(string) prerequisite]).postTechs.Add(technology);
            }

            // entire tech tree will stem from teh 'startingTech' node.
            if (technology.prerequisites.Count == 0) {
                technology.prerequisites.Add(startingTech);
                startingTech.postTechs.Add(technology);
            }
        }

        private void ProcessCharacter(JToken objJtoken, Dictionary<string, List<RecipePrototype>> craftingCategories) {
            AssemblerAdditionalProcessing(objJtoken, playerAssembler, craftingCategories);
            assemblers.Add(playerAssembler.Name, playerAssembler);
        }

        private void ProcessEntity(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<RecipePrototype>> craftingCategories,
            Dictionary<string, List<RecipePrototype>> resourceCategories, Dictionary<string, List<ItemPrototype>> fuelCategories,
            List<Recipe> miningWithFluidRecipes, Dictionary<string, List<ModulePrototype>> moduleCategories) {
            var type = (string) objJToken["type"];

            // character is processed later
            if (type == "character")
                return;

            EntityObjectBasePrototype entity;

            var energySource = (string) objJToken["fuel_type"] switch {
                "item" => EnergySource.Burner,
                "fluid" => EnergySource.FluidBurner,
                "electricity" => EnergySource.Electric,
                "heat" => EnergySource.Heat,
                _ => EnergySource.Void
            };

            var energyType = type switch {
                "beacon" => EntityType.Beacon,
                "mining-drill" => EntityType.Miner,
                "offshore-pump" => EntityType.OffshorePump,
                "furnace" or "assembling-machine" or "rocket-silo" => EntityType.Assembler,
                "boiler" => EntityType.Boiler,
                "generator" => EntityType.Generator,
                "burner-generator" => EntityType.BurnerGenerator,
                "reactor" => EntityType.Reactor,
                _ => EntityType.Error
            };

            if (energyType == EntityType.Error)
                Trace.Fail($"Unexpected type of entity ({type} in json data!");


            if (energyType == EntityType.Beacon) {
                entity = new BeaconPrototype(this,
                    (string) objJToken["name"],
                    (string) objJToken["localised_name"],
                    energySource);
            } else {
                entity = new AssemblerPrototype(this,
                    (string) objJToken["name"],
                    (string) objJToken["localised_name"],
                    energyType,
                    energySource);
            }

            // icons

            if (iconCache.ContainsKey((string) objJToken["icon_name"]))
                entity.SetIconAndColor(iconCache[(string) objJToken["icon_name"]]);
            else if (iconCache.ContainsKey((string) objJToken["icon_alt_name"]))
                entity.SetIconAndColor(iconCache[(string) objJToken["icon_alt_name"]]);

            // associated items

            if (objJToken["items_to_place_this"] != null) {
                foreach (var item in objJToken["items_to_place_this"].Select(i => (string) i)) {
                    if (items.TryGetValue(item, out var item1))
                        entity.associatedItems.Add((ItemPrototype) item1);
                }
            }

            // base parameters

            if (objJToken["q_speed"] != null) {
                foreach (var speedToken in objJToken["q_speed"])
                    entity.speed.Add(qualities[(string) speedToken["quality"]], (double) speedToken["value"]);
            } else if (objJToken["speed"] != null) {
                foreach (var quality in qualities.Values)
                    entity.speed.Add(quality, (double) objJToken["speed"]);
            }

            entity.ModuleSlots = objJToken["module_inventory_size"] == null ? 0 : (int) objJToken["module_inventory_size"];

            // modules

            if (entity.EntityType is EntityType.Assembler or EntityType.Miner or EntityType.Rocket or EntityType.Beacon) {
                if (entity is AssemblerPrototype prototype) {
                    prototype.BaseConsumptionBonus = (double) objJToken["base_module_effects"]["consumption"];
                    prototype.BaseSpeedBonus = (double) objJToken["base_module_effects"]["speed"];
                    prototype.BaseProductivityBonus = (double) objJToken["base_module_effects"]["productivity"];
                    prototype.BasePollutionBonus = (double) objJToken["base_module_effects"]["pollution"];
                    prototype.BaseQualityBonus = (double) objJToken["base_module_effects"]["quality"];
                    prototype.AllowModules = (bool) objJToken["uses_module_effects"];
                    prototype.AllowBeacons = (bool) objJToken["uses_beacon_effects"];
                }

                if (objJToken["allowed_effects"] != null) {
                    var allowConsumption = (bool) objJToken["allowed_effects"]["consumption"];
                    var allowSpeed = (bool) objJToken["allowed_effects"]["speed"];
                    var allowProductivity = (bool) objJToken["allowed_effects"]["productivity"];
                    var allowPollution = (bool) objJToken["allowed_effects"]["pollution"];
                    var allowQuality = (bool) objJToken["allowed_effects"]["quality"];

                    if (objJToken["allowed_module_categories"] == null || !objJToken["allowed_module_categories"].Any()) {
                        foreach (var module in modules.Values.Cast<ModulePrototype>()) {
                            var validModule = (allowConsumption || module.ConsumptionBonus >= 0) &&
                                (allowSpeed || module.SpeedBonus <= 0) &&
                                (allowProductivity || module.ProductivityBonus <= 0) &&
                                (allowPollution || module.PollutionBonus >= 0) &&
                                (allowQuality || module.QualityBonus <= 0);
                            if (!validModule)
                                continue;

                            entity.modules.Add(module);
                            if (entity is AssemblerPrototype aEntity)
                                module.assemblers.Add(aEntity);
                            else if (entity is BeaconPrototype bEntity)
                                module.beacons.Add(bEntity);
                        }
                    } else {
                        foreach (var moduleCategory in objJToken["allowed_module_categories"].Select(a => ((JProperty) a).Name)) {
                            if (!moduleCategories.TryGetValue(moduleCategory, out var category))
                                continue;

                            foreach (var module in category) {
                                var validModule = (allowConsumption || module.ConsumptionBonus >= 0) &&
                                    (allowSpeed || module.SpeedBonus <= 0) &&
                                    (allowProductivity || module.ProductivityBonus <= 0) &&
                                    (allowPollution || module.PollutionBonus >= 0) &&
                                    (allowQuality || module.QualityBonus <= 0);
                                if (!validModule)
                                    continue;

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

            // energy types

            EntityEnergyFurtherProcessing(objJToken, entity, fuelCategories);

            // assembler / beacon specific parameters

            if (energyType == EntityType.Beacon) {
                var bEntity = (BeaconPrototype) entity;

                if (BeaconAdditionalProcessing(objJToken, bEntity))
                    beacons.Add(bEntity.Name, bEntity);
            } else {
                var aEntity = (AssemblerPrototype) entity;

                var success = energyType switch {
                    EntityType.Assembler => AssemblerAdditionalProcessing(objJToken, aEntity, craftingCategories),
                    EntityType.Boiler => BoilerAdditionalProcessing(objJToken, aEntity),
                    EntityType.BurnerGenerator => BurnerGeneratorAdditionalProcessing(objJToken, aEntity),
                    EntityType.Generator => GeneratorAdditionalProcessing(objJToken, aEntity),
                    EntityType.Miner => MinerAdditionalProcessing(objJToken, aEntity, resourceCategories, miningWithFluidRecipes),
                    EntityType.OffshorePump => OffshorePumpAdditionalProcessing(
                        objJToken,
                        aEntity,
                        resourceCategories["<<foreman_resource_category_water_tile>>"]
                    ),
                    EntityType.Reactor => ReactorAdditionalProcessing(objJToken, aEntity),
                    _ => false
                };

                if (success)
                    assemblers.Add(aEntity.Name, aEntity);
            }
        }

        private void EntityEnergyFurtherProcessing(JToken objJToken, EntityObjectBasePrototype entity, Dictionary<string, List<ItemPrototype>> fuelCategories) {
            entity.ConsumptionEffectivity = (double) objJToken["fuel_effectivity"];

            // pollution

            if (objJToken is JObject objJObject) {
                var pollutions = objJObject["pollution"].ToObject<Dictionary<string, double>>();
                foreach (var pollution in pollutions)
                    entity.pollution.Add(pollution.Key, pollution.Value);
            }

            // energy production

            foreach (var speedToken in objJToken["q_energy_production"])
                entity.energyProduction.Add(qualities[(string) speedToken["quality"]], (double) speedToken["value"]);

            // energy consumption

            entity.energyDrain = objJToken["drain"] != null ? (double) objJToken["drain"] : 0; // seconds
            foreach (var speedToken in objJToken["q_max_energy_usage"])
                entity.energyConsumption.Add(qualities[(string) speedToken["quality"]], (double) speedToken["value"]);

            // fuel processing

            switch (entity.EnergySource) {
                case EnergySource.Burner:
                    foreach (var categoryJToken in objJToken["fuel_categories"]) {
                        if (!fuelCategories.ContainsKey((string) categoryJToken))
                            continue;

                        foreach (var item in fuelCategories[(string) categoryJToken]) {
                            entity.fuels.Add(item);
                            item.fuelsEntities.Add(entity);
                        }
                    }

                    break;

                case EnergySource.FluidBurner:
                    entity.IsTemperatureFluidBurner = !(bool) objJToken["burns_fluid"];
                    entity.FluidFuelTemperatureRange =
                        new FRange(objJToken["minimum_fuel_temperature"] == null ? double.NegativeInfinity : (double) objJToken["minimum_fuel_temperature"],
                            objJToken["maximum_fuel_temperature"] == null ? double.PositiveInfinity : (double) objJToken["maximum_fuel_temperature"]);
                    var fuelFilter = objJToken["fuel_filter"] == null ? null : (string) objJToken["fuel_filter"];

                    if (objJToken["fuel_filter"] != null) {
                        var fuel = (ItemPrototype) items[(string) objJToken["fuel_filter"]];
                        if (entity.IsTemperatureFluidBurner || fuelCategories["§§fc:liquids"].Contains(fuel)) {
                            entity.fuels.Add(fuel);
                            fuel.fuelsEntities.Add(entity);
                        }

                        // there is no valid fuel for this entity. Realistically this means it cant be used.
                        // It will thus have an error when placed (no fuel selected -> due to no fuel existing)
                    } else if (!entity.IsTemperatureFluidBurner) {
                        // add in all liquid fuels
                        foreach (var fluid in fuelCategories["§§fc:liquids"]) {
                            entity.fuels.Add(fluid);
                            fluid.fuelsEntities.Add(entity);
                        }
                    } else {
                        //ok, this is a bit of a FK U, but this basically means this entity can burn any fluid, and burns it as a temperature range.
                        //This is how the old steam generators worked (where you could feed in hot sulfuric acid, and it would just burn through it no problem).
                        //If you want to use it, fine. Here you go.
                        foreach (var fluid in items.Values.Where(i => i is Fluid).Cast<FluidPrototype>()) {
                            entity.fuels.Add(fluid);
                            fluid.fuelsEntities.Add(entity);
                        }
                    }

                    break;

                case EnergySource.Heat:
                    entity.fuels.Add(HeatItem);
                    HeatItem.fuelsEntities.Add(entity);
                    break;

                case EnergySource.Electric:
                case EnergySource.Void:
                default:
                    break;
            }
        }

        private bool BeaconAdditionalProcessing(JToken objJToken, BeaconPrototype bEntity) {
            bEntity.DistributionEffectivity = objJToken["distribution_effectivity"] != null
                ? (double) objJToken["distribution_effectivity"]
                : 0.5f;
            bEntity.DistributionEffectivityQualityBoost = objJToken["distribution_effectivity_bonus_per_quality_level"] != null
                ? (double) objJToken["distribution_effectivity_bonus_per_quality_level"]
                : 0f;

            if (objJToken["profile"] == null)
                return true;

            var quantity = 1;
            double lastProfile = 0.5f;
            foreach (var profileJToken in objJToken["profile"]) {
                lastProfile = (double) profileJToken;
                bEntity.profile[quantity] = lastProfile;

                quantity++;
                if (quantity >= bEntity.profile.Length)
                    break;
            }

            while (quantity < bEntity.profile.Length) {
                bEntity.profile[quantity] = lastProfile;
                quantity++;
            }

            // helps with calculating partial beacon values (ex: 0.5 beacons)
            bEntity.profile[0] = bEntity.profile[1];

            return true;
        }

        // recipe user
        private bool AssemblerAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity, Dictionary<string, List<RecipePrototype>> craftingCategories) {
            foreach (var categoryJToken in objJToken["crafting_categories"]) {
                if (!craftingCategories.ContainsKey((string) categoryJToken))
                    continue;

                foreach (var recipe in craftingCategories[(string) categoryJToken]) {
                    if (!TestRecipeEntityPipeFit(recipe, objJToken))
                        continue;

                    recipe.assemblers.Add(aEntity);
                    aEntity.Recipes.Add(recipe);
                }
            }

            return true;
        }

        // resource provider
        private bool MinerAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity, Dictionary<string, List<RecipePrototype>> resourceCategories,
            List<Recipe> miningWithFluidRecipes) {
            foreach (var categoryJToken in objJToken["resource_categories"]) {
                if (!resourceCategories.ContainsKey((string) categoryJToken))
                    continue;

                foreach (var recipe in resourceCategories[(string) categoryJToken]) {
                    if (!TestRecipeEntityPipeFit(recipe, objJToken))
                        continue;

                    if (!miningWithFluidRecipes.Contains(recipe))
                        ProcessEntityRecipeTechlink(aEntity, recipe);

                    recipe.assemblers.Add(aEntity);
                    aEntity.Recipes.Add(recipe);
                }
            }

            return true;
        }

        // check if the pump has a specified 'output' fluid preset.
        // if yes then only that recipe is added to it; if not then all water tile resource recipes are added
        private bool OffshorePumpAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity, List<RecipePrototype> waterPumpRecipes) {
            var outPipeFilters = objJToken["out_pipe_filters"].Select(o => (string) o).ToList();

            if (outPipeFilters.Count != 0) {
                if (recipes.TryGetValue(GetExtractionRecipeName(outPipeFilters[0]), out var extractionRecipe)) {
                    ProcessEntityRecipeTechlink(aEntity, (RecipePrototype) extractionRecipe);
                    ((RecipePrototype) extractionRecipe).assemblers.Add(aEntity);
                    aEntity.Recipes.Add((RecipePrototype) extractionRecipe);
                } else { // add new recipe
                    if (!items.TryGetValue(outPipeFilters[0], out var extractionFluid))
                        return false;

                    var recipe = new RecipePrototype(
                        this,
                        GetExtractionRecipeName(outPipeFilters[0]),
                        extractionFluid.FriendlyName + " Extraction",
                        extractionSubgroupFluids,
                        extractionFluid.Name
                    ) {
                        Time = 1
                    };

                    recipe.InternalOneWayAddProduct((ItemPrototype) extractionFluid, 60, 60);
                    ((ItemPrototype) extractionFluid).productionRecipes.Add(recipe);

                    recipe.SetIconAndColor(new IconColorPair(recipe.productList[0].Icon, recipe.productList[0].AverageColor));

                    recipes.Add(recipe.Name, recipe);
                }
            } else {
                foreach (var recipe in waterPumpRecipes) {
                    ProcessEntityRecipeTechlink(aEntity, recipe);
                    recipe.assemblers.Add(aEntity);
                    aEntity.Recipes.Add(recipe);
                }
            }

            return true;
        }

        // Uses whatever the default energy source of it is to convert water into steam of a given temperature
        private bool BoilerAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity) {
            if (objJToken["fluid_ingredient"] == null || objJToken["fluid_product"] == null)
                return false;
            var ingredient = (FluidPrototype) items[(string) objJToken["fluid_ingredient"]];
            var product = (FluidPrototype) items[(string) objJToken["fluid_product"]];

            // boiler is an ingredient to product conversion with product coming out at the target_temperature *C at a rate
            // based on energy efficiency & energy use to bring the INGREDIENT to the given temperature
            // (basically ingredient goes from default temp to target temp, then shifts to product).
            // we will add an extra recipe for this
            var temp = (double) objJToken["target_temperature"];

            // I will be honest here. Testing has shown that the actual 'speed' is dependent on the incoming temperature
            // (not the default temperature), as could have likely been expected.
            // this means that if you put in 65* water instead of 15* water to boil it to 165* steam
            // it will result in 1.5x the 'maximum' output as listed in the factorio info menu and calculated below.
            // so if some mod does some wonky things like water pre-heating, or uses boiler to heat other fluids at non-default temperatures
            // (I haven't found any such mods, but testing shows it is possible to make such a mod)
            // then the values calculated here will be wrong.
            // Still, for now I will leave it as is.

            if (ingredient.SpecificHeatCapacity == 0) {
                foreach (var quality in qualities.Values)
                    aEntity.speed.Add(quality, 0);
            } else {
                // by placing this here we can keep the recipe as a 1 sec -> 60 production, simplifying recipe comparing for presets.
                foreach (var quality in qualities.Values)
                    aEntity.speed.Add(quality, aEntity.GetEnergyConsumption(quality)
                        / ((temp - ingredient.DefaultTemperature) * ingredient.SpecificHeatCapacity * 60));
            }

            RecipePrototype recipe;
            var boilRecipeName = $"§§r:b:{ingredient.Name}:{product.Name}:{temp}";
            if (!recipes.TryGetValue(boilRecipeName, out var recipe1)) {
                recipe = new RecipePrototype(
                    this,
                    boilRecipeName,
                    ingredient == product
                        ? $"{ingredient.FriendlyName} boiling to {temp}°c"
                        : $"{ingredient.FriendlyName} boiling to {temp}°c {product.FriendlyName}",
                    energySubgroupBoiling,
                    boilRecipeName
                );

                recipe.SetIconAndColor(new IconColorPair(IconCache.CombineIcons(ingredient.Icon, product.Icon, ingredient.Icon.Height), product.AverageColor));

                recipe.Time = 1;

                recipe.InternalOneWayAddIngredient(ingredient, 60);
                ingredient.consumptionRecipes.Add(recipe);

                var productQuantity = 60 * ingredient.SpecificHeatCapacity / product.SpecificHeatCapacity;
                recipe.InternalOneWayAddProduct(product, productQuantity, productQuantity, temp);
                product.productionRecipes.Add(recipe);

                // we will let the assembler sort out which module can be used with this recipe
                foreach (var module in modules.Values.Cast<ModulePrototype>()) {
                    module.recipes.Add(recipe);
                    recipe.assemblerModules.Add(module);
                }

                recipes.Add(recipe.Name, recipe);
            } else
                recipe = (RecipePrototype) recipe1;

            ProcessEntityRecipeTechlink(aEntity, recipe);
            recipe.assemblers.Add(aEntity);
            aEntity.Recipes.Add(recipe);

            return true;
        }

        // consumes steam (at the provided temperature up to the given maximum) to generate electricity
        private bool GeneratorAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity) {
            if (objJToken["fluid_ingredient"] == null)
                return false;
            var ingredient = (FluidPrototype) items[(string) objJToken["fluid_ingredient"]];

            // use 60 multiplier to make recipes easier
            var baseSpeed = (double) objJToken["fluid_usage_per_sec"] / 60;
            // in seconds
            var baseEnergyProduction = (double) objJToken["max_power_output"];

            foreach (var quality in qualities.Values)
                aEntity.speed.Add(quality, baseSpeed * aEntity.GetEnergyProduction(quality) / baseEnergyProduction);

            aEntity.OperationTemperature = (double) objJToken["full_power_temperature"];
            var minTemp = (double) (objJToken["minimum_temperature"] ?? double.NaN);
            var maxTemp = (double) (objJToken["maximum_temperature"] ?? double.NaN);
            if (!double.IsNaN(minTemp) && minTemp < ingredient.DefaultTemperature) minTemp = ingredient.DefaultTemperature;
            if (!double.IsNaN(maxTemp) && maxTemp > MaxTemp) maxTemp = double.NaN;

            // actual energy production is a bit more complicated here (as it involves actual temperatures),
            // but we will have to handle it in the graph (after all values have been calculated,
            // and we know the amounts and temperatures getting passed here, we can calc the energy produced)

            RecipePrototype recipe;
            var generationRecipeName = $"§§r:g:{ingredient.Name}:{minTemp}>{maxTemp}";
            if (!recipes.TryGetValue(generationRecipeName, out var recipe1)) {
                recipe = new RecipePrototype(
                    this,
                    generationRecipeName,
                    $"{ingredient.FriendlyName} to Electricity",
                    energySubgroupEnergy,
                    generationRecipeName);

                recipe.SetIconAndColor(new IconColorPair(
                    IconCache.CombineIcons(ingredient.Icon, ElectricityIcon, ingredient.Icon.Height, false),
                    ingredient.AverageColor)
                );

                recipe.Time = 1;

                recipe.InternalOneWayAddIngredient(
                    ingredient,
                    60,
                    double.IsNaN(minTemp) ? double.NegativeInfinity : minTemp,
                    double.IsNaN(maxTemp) ? double.PositiveInfinity : maxTemp
                );

                ingredient.consumptionRecipes.Add(recipe);

                // we will let the assembler sort out which module can be used with this recipe
                foreach (var module in modules.Values.Cast<ModulePrototype>()) {
                    module.recipes.Add(recipe);
                    recipe.assemblerModules.Add(module);
                }

                recipes.Add(recipe.Name, recipe);
            } else
                recipe = (RecipePrototype) recipe1;

            ProcessEntityRecipeTechlink(aEntity, recipe);
            recipe.assemblers.Add(aEntity);
            aEntity.Recipes.Add(recipe);

            return true;
        }

        // consumes fuel to generate electricity
        private bool BurnerGeneratorAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity) {
            aEntity.Recipes.Add(BurnerRecipe);
            BurnerRecipe.assemblers.Add(aEntity);
            ProcessEntityRecipeTechlink(aEntity, BurnerRecipe);

            // doesn't matter - recipe is empty
            foreach (var quality in qualities.Values)
                aEntity.speed.Add(quality, 1f);

            return true;
        }

        private bool ReactorAdditionalProcessing(JToken objJToken, AssemblerPrototype aEntity) {
            aEntity.NeighbourBonus = objJToken["neighbour_bonus"] == null ? 0 : (double) objJToken["neighbour_bonus"];
            aEntity.Recipes.Add(HeatRecipe);
            HeatRecipe.assemblers.Add(aEntity);
            ProcessEntityRecipeTechlink(aEntity, HeatRecipe);

            // the speed of producing 1MJ of energy as heat for this reactor based on quality
            foreach (var quality in qualities.Values)
                aEntity.speed.Add(quality, aEntity.GetEnergyConsumption(quality) / HeatItem.FuelValue);

            return true;
        }

        private void ProcessEntityRecipeTechlink(EntityObjectBasePrototype entity, RecipePrototype recipe) {
            if (entity.associatedItems.Count == 0) {
                recipe.myUnlockTechnologies.Add(startingTech);
                startingTech.unlockedRecipes.Add(recipe);
            } else {
                foreach (Item placeItem in entity.associatedItems) {
                    foreach (var placeItemRecipe in placeItem.ProductionRecipes) {
                        foreach (var tech in placeItemRecipe.MyUnlockTechnologies.Cast<TechnologyPrototype>()) {
                            recipe.myUnlockTechnologies.Add(tech);
                            tech.unlockedRecipes.Add(recipe);
                        }
                    }
                }
            }
        }

        // returns true if the fluid boxes of the entity (assembler or miner) can accept the provided recipe (with its in/out fluids)
        private bool TestRecipeEntityPipeFit(RecipePrototype recipe, JToken objJToken) {
            var inPipes = (int) objJToken["in_pipes"];
            var inPipeFilters = objJToken["in_pipe_filters"].Select(o => (string) o).ToList();
            var outPipes = (int) objJToken["out_pipes"];
            var outPipeFilters = objJToken["out_pipe_filters"].Select(o => (string) o).ToList();
            var ioPipes = (int) objJToken["io_pipes"];
            var ioPipeFilters = objJToken["io_pipe_filters"].Select(o => (string) o).ToList();

            // unfiltered
            var inCount = 0;
            // unfiltered
            var outCount = 0;
            foreach (var inFluid in recipe.ingredientList.Where(i => i is Fluid)) {
                if (inPipeFilters.Contains(inFluid.Name)) {
                    inPipes--;
                    inPipeFilters.Remove(inFluid.Name);
                } else if (ioPipeFilters.Contains(inFluid.Name)) {
                    ioPipes--;
                    ioPipeFilters.Remove(inFluid.Name);
                } else
                    inCount++;
            }

            foreach (var outFluid in recipe.productList.Where(i => i is Fluid)) {
                if (outPipeFilters.Contains(outFluid.Name)) {
                    outPipes--;
                    outPipeFilters.Remove(outFluid.Name);
                } else if (ioPipeFilters.Contains(outFluid.Name)) {
                    ioPipes--;
                    ioPipeFilters.Remove(outFluid.Name);
                } else
                    outCount++;
            }

            // remove any unused filtered pipes from the equation - they cant be used due to the filters.

            inPipes -= inPipeFilters.Count;
            ioPipes -= ioPipeFilters.Count;
            outPipes -= outPipeFilters.Count;

            // return true if the remaining unfiltered ingredients & products (fluids) can fit into the remaining unfiltered pipes

            return inCount - inPipes <= ioPipes && outCount - outPipes <= ioPipes && inCount + outCount <= inPipes + outPipes + ioPipes;
        }

        private void ProcessRocketLaunch(JToken objJToken) {
            if (!items.ContainsKey("rocket-part") || !recipes.ContainsKey("rocket-part") || !assemblers.ContainsKey("rocket-silo")) {
                ErrorLogging.LogLine($"No Rocket silo / rocket part found! launch product for {(string) objJToken["name"]} will be ignored.");
                return;
            }

            var rocketPart = (ItemPrototype) items["rocket-part"];
            var rocketPartRecipe = (RecipePrototype) recipes["rocket-part"];
            var launchItem = (ItemPrototype) items[(string) objJToken["name"]];

            var recipe = new RecipePrototype(
                this,
                $"§§r:rl:launch-{launchItem.Name}",
                $"Rocket Launch: {launchItem.FriendlyName}",
                rocketLaunchSubgroup,
                launchItem.Name
            ) {
                Time = 1 //placeholder really...
            };

            // process products - have to calculate what the maximum input size of the launch item is so as not to waste any products
            // (ex: you can launch 2000 science packs, but you will only get 100 fish. so input size must be set to 100 -> 100 science packs to 100 fish)

            var inputSize = launchItem.StackSize;
            var products = new Dictionary<ItemPrototype, double>();
            var productTemp = new Dictionary<ItemPrototype, double>();

            foreach (var productJToken in objJToken["rocket_launch_products"].ToList()) {
                var product = (ItemPrototype) items[(string) productJToken["name"]];
                var amount = (double) productJToken["amount"];
                if (amount == 0)
                    continue;

                if (inputSize * amount > product.StackSize)
                    inputSize = (int) (product.StackSize / amount);

                amount = inputSize * amount;

                if ((string) productJToken["type"] == "fluid") {
                    productTemp.Add(product, productJToken["temperature"] != null
                        ? (double) productJToken["temperature"]
                        : ((FluidPrototype) product).DefaultTemperature
                    );
                }

                products.Add(product, amount);

                product.productionRecipes.Add(recipe);
                recipe.SetIconAndColor(new IconColorPair(product.Icon, Color.DarkGray));
            }

            foreach (var product in products.Keys)
                recipe.InternalOneWayAddProduct(product, inputSize * products[product], 0,
                    productTemp.TryGetValue(product, out var value) ? value : double.NaN);

            recipe.InternalOneWayAddIngredient(launchItem, inputSize);
            launchItem.consumptionRecipes.Add(recipe);

            recipe.InternalOneWayAddIngredient(rocketPart, 100);
            rocketPart.consumptionRecipes.Add(recipe);

            foreach (var tech in rocketPartRecipe.myUnlockTechnologies) {
                recipe.myUnlockTechnologies.Add(tech);
                tech.unlockedRecipes.Add(recipe);
            }

            recipe.assemblers.Add(rocketAssembler);
            rocketAssembler.Recipes.Add(recipe);

            recipes.Add(recipe.Name, recipe);
        }

        //------------------------------------------------------Finalization steps of LoadAllData (cleanup and cyclic checks)

        // DFS for processing the required sci packs of each technology.
        // Basically some research only requires 1 sci pack, but to unlock it requires researching tech with many sci packs.
        // Need to account for that
        private void ProcessSciencePacks() {
            var techRequirements = new Dictionary<TechnologyPrototype, HashSet<Item>>();
            var sciPacks = new HashSet<Item>();

            // tech ordering - set each technology's 'tier' to be its furthest distance from the 'starting tech' node
            var visitedTech = new HashSet<TechnologyPrototype> {
                startingTech // tier 0, everything starts from here.
            };

            // science pack processing - DF again where we want to calculate which science packs are required to get to the given science pack
            var visitedPacks = new HashSet<Item>();

            // step 1:
            // update tech unlock status & science packs
            // (add a 0 cost pack to the tech if it has no such requirement but its prerequisites do),
            // set tech tier

            foreach (var tech in technologies.Values.Cast<TechnologyPrototype>()) {
                TechRequiredSciPacks(tech);
                GetTechnologyTier(tech);
                foreach (var sciPack in techRequirements[tech].Cast<ItemPrototype>())
                    tech.InternalOneWayAddSciPack(sciPack, 0);
            }

            // step 2:
            // further sci pack processing -> for every available science pack we want to build a list of science packs necessary to acquire it.
            // In a situation with multiple (non-equal) research paths
            // (ex: 3 can be acquired through either pack 1&2 or pack 1 alone), take the intersection (1 in this case).
            // These will be added to the sci pack requirement lists

            foreach (var sciPack in sciPacks)
                UpdateSciencePackPrerequisites(sciPack);


            // step 2.5:
            // update the technology science packs to account for the science pack prerequisites

            foreach (var tech in technologies.Values.Cast<TechnologyPrototype>())
            foreach (var sciPack in tech.SciPackList.ToList())
            foreach (var reqSciPack in sciencePackPrerequisites[sciPack].Cast<ItemPrototype>())
                tech.InternalOneWayAddSciPack(reqSciPack, 0);

            // step 3:
            // calculate science pack tier
            // (minimum tier of technology that unlocks the recipe for the given science pack).
            // also make the sciencePacks list.

            var sciencePackTiers = new Dictionary<Item, int>();
            foreach (var sciPack in sciPacks.Cast<ItemPrototype>()) {
                var minTier = int.MaxValue;
                foreach (Recipe recipe in sciPack.productionRecipes)
                foreach (var tech in recipe.MyUnlockTechnologies)
                    minTier = Math.Min(minTier, tech.Tier);

                // there are no recipes for this sci pack.
                // EX: space science pack.
                // We will grant it the same tier as the first tech to require this sci pack.
                // This should sort them relatively correctly (ex - placing space sci pack last, and placing seablock starting tech first)

                if (minTier == int.MaxValue)
                    minTier = techRequirements.Where(kvp => kvp.Value.Contains(sciPack)).Select(kvp => kvp.Key).Min(t => t.Tier);
                sciencePackTiers.Add(sciPack, minTier);
                sciencePacks.Add(sciPack);
            }

            // step 4:
            // update all science pack lists
            // (main sciencePacks list, plus SciPackList of every technology).
            // Sorting is done by A: if science pack B has science pack A as a prerequisite (in sciPackRequiredPacks), then B goes after A.
            // If neither has the other as a prerequisite, then compare by sciencePack tiers

            sciencePacks.Sort((s1, s2) =>
                sciencePackTiers[s1].CompareTo(sciencePackTiers[s2]) +
                (sciencePackPrerequisites[s1].Contains(s2) ? 1000 : sciencePackPrerequisites[s2].Contains(s1) ? -1000 : 0));
            foreach (var tech in technologies.Values.Cast<TechnologyPrototype>())
                tech.sciPackList.Sort((s1, s2) =>
                    sciencePackTiers[s1].CompareTo(sciencePackTiers[s2]) +
                    (sciencePackPrerequisites[s1].Contains(s2) ? 1000 : sciencePackPrerequisites[s2].Contains(s1) ? -1000 : 0));

            // step 5:
            // create science pack lists for each recipe
            // (list of distinct min-pack sets -> ex: if recipe can be acquired through 4 techs with [A + B, A + B, A + C, A + B + C] science pack requirements,
            // we will only include A + B and A + C)

            foreach (var recipe in recipes.Values.Cast<RecipePrototype>()) {
                var sciPackLists = new List<List<Item>>();
                foreach (var tech in recipe.myUnlockTechnologies) {
                    var exists = false;
                    foreach (var sciPackList in sciPackLists.ToList()) {
                        if (!sciPackList.Except(tech.sciPackList).Any()) {
                            // sci pack lists already includes a list that is a subset of the technologies sci pack list
                            // (ex: already have A+B while tech's is A+B+C)
                            exists = true;
                        } else if (!tech.sciPackList.Except(sciPackList).Any()) {
                            // technology sci pack list is a subset of an already included sci pack list.
                            // we will add thi to the list and delete the existing one
                            // (ex: have A+B while tech's is A -> need to remove A+B and include A)
                            sciPackLists.Remove(sciPackList);
                        }
                    }

                    if (!exists)
                        sciPackLists.Add(tech.sciPackList);
                }

                recipe.MyUnlockSciencePacks = sciPackLists;
            }

            return;

            void UpdateSciencePackPrerequisites(Item sciPack) {
                if (visitedPacks.Contains(sciPack))
                    return;

                // for simplicity’s sake we will only account for prerequisites of the first available production recipe
                // (or first non-available if no available production recipes exist).
                // This means that if (for who knows what reason) there are multiple valid production recipes only the first one will count!

                var prerequisites = new HashSet<Item>(sciPack.ProductionRecipes.OrderByDescending(r => r.Available).FirstOrDefault()
                    ?.MyUnlockTechnologies.OrderByDescending(t => t.Available).FirstOrDefault()?.SciPackList ?? []);
                foreach (var r in sciPack.ProductionRecipes)
                foreach (var t in r.MyUnlockTechnologies)
                    prerequisites.IntersectWith(t.SciPackList);

                // prerequisites now contains all the immediate required sci packs.
                // we will now Update their prerequisites via this function,
                // then add their prerequisites to our own set before finalizing it.

                foreach (var prereq in prerequisites.ToList()) {
                    UpdateSciencePackPrerequisites(prereq);
                    prerequisites.UnionWith(sciencePackPrerequisites[prereq]);
                }

                sciencePackPrerequisites.Add(sciPack, prerequisites);
                visitedPacks.Add(sciPack);
            }

            int GetTechnologyTier(TechnologyPrototype tech) {
                if (visitedTech.Contains(tech))
                    return tech.Tier;

                var maxPrerequisiteTier = tech.prerequisites.Select(GetTechnologyTier).Prepend(0).Max();
                tech.Tier = maxPrerequisiteTier + 1;
                visitedTech.Add(tech);

                return tech.Tier;
            }

            HashSet<Item> TechRequiredSciPacks(TechnologyPrototype tech) {
                if (techRequirements.TryGetValue(tech, out var packs))
                    return packs;

                var requiredItems = new HashSet<Item>(tech.sciPackList);
                foreach (var sciPack in tech.prerequisites.SelectMany(TechRequiredSciPacks))
                    requiredItems.Add(sciPack);

                sciPacks.UnionWith(requiredItems);
                techRequirements.Add(tech, requiredItems);

                return requiredItems;
            }
        }


        private void ProcessAvailableStatuses() {
            // quick function to depth-first search the tech tree to calculate the availability of the technology.
            // Hashset used to keep track of visited tech and not have to re-check them.
            // NOTE: factorio ensures no cyclic, so we are guaranteed to have a directed acyclic graph (maybe disconnected)
            var unlockableTechSet = new HashSet<TechnologyPrototype>();

            // step 0:
            // check availability of technologies

            foreach (TechnologyPrototype tech in technologies.Values)
                IsUnlockable(tech);

            // step 1:
            // update recipe unlock status

            foreach (RecipePrototype recipe in recipes.Values)
                recipe.Available = recipe.myUnlockTechnologies.Any(t => t.Available);

            // step 2:
            // mark any recipe for barreling / crating as unavailable

            if (UseRecipeBWLists) {
                foreach (RecipePrototype recipe in recipes.Values) {
                    // part 1:
                    // make unavailable if recipe fits the black & doesn't fit the white recipe black lists
                    // (these should be the 'barreling' and 'unbarrelling' recipes)

                    // if we don't match a whitelist and match a blacklist...
                    if (!recipeWhiteList.Any(white => white.IsMatch(recipe.Name)) && recipeBlackList.Any(black => black.IsMatch(recipe.Name)))
                        recipe.Available = false;

                    // part 2:
                    // make unavailable if recipe fits the recyclingItemNameBlackList
                    // (should remove any of the barrel recycling recipes added by 2.0 SA)

                    foreach (var recycleBlacklist in recyclingItemNameBlackList) {
                        if (recipe.productList.Count == 1 && recipe.productList[0] == items[recycleBlacklist.Key]
                            && recipe.ingredientList.Count == 1
                            && recycleBlacklist.Value.IsMatch(recipe.ingredientList[0].Name)) {
                            recipe.Available = false;
                        }
                    }
                }
            }


            // step 3:
            // mark any recipe with no unlocks, or 0->0 recipes
            // (industrial revolution... what are those aetheric glow recipes?) as unavailable.

            foreach (RecipePrototype recipe in recipes.Values) {
                // §§ denotes foreman added recipes. ignored during this pass (but not during the assembler check pass)
                if (recipe.myUnlockTechnologies.Count == 0
                    || (recipe.productList.Count == 0 && recipe.ingredientList.Count == 0 && !recipe.Name.StartsWith("§§"))) {
                    recipe.Available = false;
                }
            }

            // step 4 (loop):
            // switch any recipe with no available assemblers to unavailable,
            // switch any useless item to unavailable (no available recipe produces it,
            // it isn't used by any available recipe / only by incineration recipes

            var clean = false;
            while (!clean) {
                clean = true;

                // 4.1:
                // mark any recipe with no available assemblers to unavailable.

                foreach (RecipePrototype recipe in recipes.Values.Where(r =>
                    r.Available && !r.Assemblers.Any(a =>
                        a.Available || a as AssemblerPrototype == playerAssembler || a as AssemblerPrototype == rocketAssembler))) {
                    recipe.Available = false;
                    clean = false;
                }

                // 4.2:
                // mark any useless items as unavailable (nothing/unavailable recipes produce it,
                // it isn't consumed by anything / only consumed by incineration / only consumed by unavailable recipes, only produced by an itself->itself recipe)
                // this will also update assembler availability status for those whose items become unavailable automatically.
                // note: while this gets rid of those annoying 'burn/incinerate' auto-generated recipes,
                // if the modder decided to have a 'recycle' auto-generated recipe (item->raw ore or something), we will be forced to accept those items as 'available'
                // good example from vanilla: most of the 'garbage' items such as 'item-unknown'
                // and 'electric-energy-interface' are removed as their only recipes are 'recycle to themselves',
                // but 'heat interface' isn't removed as its only recipe is a 'recycle into several parts' (so nothing we can do about it)

                foreach (var item in items.Values.Where(i => i.Available
                        && !i.ProductionRecipes.Any(r => r.Available && !(r.IngredientList.Count == 1 && r.IngredientList[0] == i)))
                    .Cast<ItemPrototype>()
                ) {
                    var useful = false;

                    // recipe with multiple items coming in or some ingredients coming out (that aren't itself) -> not an incineration type
                    foreach (var r in item.consumptionRecipes.Where(r => r.Available))
                        useful |= r.ingredientList.Count > 1
                            || r.productList.Count > 1
                            || (r.productList.Count == 1 && r.productList[0] != item);

                    if (useful || item.Name.StartsWith("§§"))
                        continue;

                    item.Available = false;
                    clean = false;

                    // from above these recipes are all item->nothing
                    foreach (var r in item.consumptionRecipes)
                        r.Available = false;
                }

                // 4.3:
                // go over the item list one more time and ensure that
                // if an item that is available has any growth or spoil results then they are also available
                // (edge case: item grows or spoils into something that has no recipes aka: unavailable,
                // but it should be available even though its only 'use' is as a spoil or grow result)

                foreach (var item in items.Values.Where(i => !i.Available).Cast<ItemPrototype>()) {
                    var useful = false;
                    useful |= item.spoilOrigins.Count(i => i.Available) > 0;
                    useful |= item.plantOrigins.Count(i => i.Available) > 0;
                    item.Available = useful;
                }
            }

            //step 5:
            //set the 'default' enabled statuses of recipes, assemblers, modules & beacons to their available status.

            foreach (RecipePrototype recipe in recipes.Values)
                recipe.Enabled = recipe.Available;
            foreach (AssemblerPrototype assembler in assemblers.Values)
                assembler.Enabled = assembler.Available;
            foreach (ModulePrototype module in modules.Values)
                module.Enabled = module.Available;
            foreach (BeaconPrototype beacon in beacons.Values)
                beacon.Enabled = beacon.Available;

            // its enabled, so it can theoretically be used, but it is set as 'unavailable' so a warning will be issued if you use it.
            playerAssembler.Enabled = true;

            // rocket assembler is set to enabled if rocket silo is enabled
            rocketAssembler.Enabled = assemblers["rocket-silo"]?.Enabled ?? false;
            // override
            rocketAssembler.Available = assemblers["rocket-silo"] != null;
            return;

            bool IsUnlockable(TechnologyPrototype tech) {
                if (!tech.Available)
                    return false;

                if (unlockableTechSet.Contains(tech))
                    return true;

                if (tech.prerequisites.Count == 0)
                    return true;

                var available = true;
                foreach (var preTech in tech.prerequisites)
                    available = available && IsUnlockable(preTech);
                tech.Available = available;

                if (available)
                    unlockableTechSet.Add(tech);

                return available;
            }
        }

        private void CleanupGroups() {
            // step 6:
            // clean up groups and subgroups
            // (delete any subgroups that have no items/recipes, then delete any groups that have no subgroups)

            foreach (SubgroupPrototype subgroup in subgroups.Values.ToList()) {
                if (subgroup.items.Count != 0 || subgroup.recipes.Count != 0)
                    continue;

                ((GroupPrototype) subgroup.MyGroup).subgroups.Remove(subgroup);
                subgroups.Remove(subgroup.Name);
            }

            foreach (GroupPrototype group in groups.Values.ToList()) {
                if (group.subgroups.Count == 0)
                    groups.Remove(group.Name);
            }

            // step 7:
            // update subgroups and groups to set them to unavailable
            // if they only contain unavailable items/recipes

            foreach (SubgroupPrototype subgroup in subgroups.Values) {
                if (!subgroup.items.Any(i => i.Available) && !subgroup.recipes.Any(r => r.Available))
                    subgroup.Available = false;
            }

            foreach (GroupPrototype group in groups.Values) {
                if (!group.subgroups.Any(sg => sg.Available))
                    group.Available = false;
            }

            // step 8:
            // sort groups/subgroups

            foreach (GroupPrototype group in groups.Values)
                group.SortSubgroups();
            foreach (SubgroupPrototype sgroup in subgroups.Values)
                sgroup.SortIRs();
        }

        private void UpdateFluidTemperatureDependencies() {
            // step 9:
            // update the temperature dependent status of items (fluids)

            foreach (FluidPrototype fluid in items.Values.Where(i => i is Fluid)) {
                var productionRange = new FRange(double.MaxValue, double.MinValue);
                // a bit different -> the min value is the LARGEST minimum of each consumption recipe,
                // and the max value is the SMALLEST max of each consumption recipe
                var consumptionRange = new FRange(double.MinValue, double.MaxValue);

                foreach (Recipe recipe in fluid.productionRecipes) {
                    productionRange.Min = Math.Min(productionRange.Min, recipe.ProductTemperatureMap[fluid]);
                    productionRange.Max = Math.Max(productionRange.Max, recipe.ProductTemperatureMap[fluid]);
                }

                foreach (Recipe recipe in fluid.consumptionRecipes) {
                    consumptionRange.Min = Math.Max(consumptionRange.Min, recipe.IngredientTemperatureMap[fluid].Min);
                    consumptionRange.Max = Math.Min(consumptionRange.Max, recipe.IngredientTemperatureMap[fluid].Max);
                }

                fluid.IsTemperatureDependent = !consumptionRange.Contains(productionRange);
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
            Console.WriteLine("RecipesView:");
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
            Console.WriteLine("RecipesView:");
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
                foreach (var prereq in tech.prerequisites)
                    Console.WriteLine("      > T:" + prereq.Tier.ToString("000" + " : " + prereq.Name));
            }

            Console.WriteLine("Science Pack order:");
            foreach (var sciPack in sciencePacks)
                Console.WriteLine("   >" + sciPack.FriendlyName);
            Console.WriteLine("Science Pack prerequisites:");
            foreach (var sciPack in sciencePacks) {
                Console.WriteLine("   >" + sciPack);
                foreach (var i in sciencePackPrerequisites[sciPack])
                    Console.WriteLine("      >" + i);
            }

            Console.WriteLine("RECIPES: ----------------------------------------------------------------");
            foreach (RecipePrototype recipe in recipes.Values) {
                Console.WriteLine("R: " + recipe.Name);
                foreach (var tech in recipe.myUnlockTechnologies)
                    Console.WriteLine("  >" + tech.Tier.ToString("000") + ":" + tech.Name);
                foreach (var sciPackList in recipe.MyUnlockSciencePacks) {
                    Console.Write("    >Science Packs Option: ");
                    foreach (var sciPack in sciPackList)
                        Console.Write(sciPack.Name + ", ");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("TEMPERATURE DEPENDENT FLUIDS: ----------------------------------------------------------------");
            foreach (ItemPrototype fluid in items.Values.Where(i => i is Fluid { IsTemperatureDependent: true })) {
                Console.WriteLine(fluid.Name);
                var productionTemps = new HashSet<double>();
                foreach (Recipe recipe in fluid.productionRecipes)
                    productionTemps.Add(recipe.ProductTemperatureMap[fluid]);
                Console.Write("   Production ranges:          >");
                foreach (var temp in productionTemps.ToList().OrderBy(t => t))
                    Console.Write(temp + ", ");
                Console.WriteLine();
                Console.Write("   Failed Consumption ranges:  >");
                foreach (Recipe recipe in fluid.consumptionRecipes.Where(r => productionTemps.Any(t => !r.IngredientTemperatureMap[fluid].Contains(t))))
                    Console.Write("(" + recipe.IngredientTemperatureMap[fluid].Min + ">" + recipe.IngredientTemperatureMap[fluid].Max + ": " + recipe.Name +
                        "), ");
                Console.WriteLine();
            }
        }
    }
}