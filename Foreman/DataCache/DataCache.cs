using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
    public class PresetErrorPackage : IComparable<PresetErrorPackage>
    {
        public Preset Preset;

        public List<string> RequiredMods;
        public List<string> RequiredItems;
        public List<string> RequiredRecipes;

        public List<string> MissingRecipes;
        public List<string> IncorrectRecipes;
        public List<string> ValidMissingRecipes; //any recipes that were missing previously but have been found to fit in this current preset
        public List<string> MissingItems;
        public List<string> MissingMods;
        public List<string> AddedMods;
        public List<string> WrongVersionMods;

        public int ErrorCount { get { return MissingRecipes.Count + IncorrectRecipes.Count + MissingItems.Count + MissingMods.Count + AddedMods.Count + WrongVersionMods.Count; } }
        public int MICount { get { return MissingRecipes.Count + IncorrectRecipes.Count + MissingItems.Count; } }

        public PresetErrorPackage(Preset preset)
        {
            Preset = preset;
            RequiredMods = new List<string>();
            RequiredItems = new List<string>();
            RequiredRecipes = new List<string>();

            MissingRecipes = new List<string>();
            IncorrectRecipes = new List<string>();
            ValidMissingRecipes = new List<string>();
            MissingItems = new List<string>();
            MissingMods = new List<string>(); // in mod-name|version format
            AddedMods = new List<string>(); //in mod-name|version format
            WrongVersionMods = new List<string>(); //in mod-name|expected-version|preset-version format
        }

        public int CompareTo(PresetErrorPackage other) //usefull for sorting the Presets by increasing severity (mods, items/recipes)
        {
            int modErrorComparison = this.MissingMods.Count.CompareTo(other.MissingMods.Count);
            if (modErrorComparison != 0)
                return modErrorComparison;
            modErrorComparison = this.AddedMods.Count.CompareTo(other.AddedMods.Count);
            if (modErrorComparison != 0)
                return modErrorComparison;
            return this.MICount.CompareTo(other.MICount);
        }
    }

    public class DataCache
    {
        public string PresetName { get; private set; }

        public IReadOnlyDictionary<string, string> IncludedMods { get { return includedMods; } }
        public IReadOnlyDictionary<string, Technology> Technologies { get { return technologies; } }
        public IReadOnlyDictionary<string, Group> Groups { get { return groups; } }
        public IReadOnlyDictionary<string, Subgroup> Subgroups { get { return subgroups; } }
        public IReadOnlyDictionary<string, Item> Items { get { return items; } }
        public IReadOnlyDictionary<string, Recipe> Recipes { get { return recipes; } }
        public IReadOnlyDictionary<string, Assembler> Assemblers { get { return assemblers; } }
        public IReadOnlyDictionary<string, Module> Modules { get { return modules; } }
        public IReadOnlyDictionary<string, Beacon> Beacons { get { return beacons; } }

        public IReadOnlyDictionary<string, Item> MissingItems { get { return missingItems; } }
        //missing recipes are not given a list. they are just processed directly and set to the given nodes. we honestly dont care about them. (plus, you can have multiple 'wrong' recipes with the same name key - not a good thing)
        //items are kind of the same thing, but since we cant have an item-name-collision its just easier to store them here than pass a dictionary of them around.

        public Group ExtractionGroup { get { return extractionGroup; } }
        public Subgroup MissingSubgroup { get { return missingSubgroup; } }
        public Technology StartingTech { get { return startingTech; } }
        public static Bitmap UnknownIcon { get { return IconCache.GetUnknownIcon(); } }

        private Dictionary<string, string> includedMods; //name : version
        private Dictionary<string, Technology> technologies;
        private Dictionary<string, Group> groups;
        private Dictionary<string, Subgroup> subgroups;
        private Dictionary<string, Item> items;
        private Dictionary<string, Recipe> recipes;
        private Dictionary<string, Assembler> assemblers;
        private Dictionary<string, Module> modules;
        private Dictionary<string, Beacon> beacons;

        private Dictionary<string, Item> missingItems;

        private GroupPrototype extractionGroup;
        private SubgroupPrototype extractionSubgroupItems;
        private SubgroupPrototype extractionSubgroupFluids;
        private SubgroupPrototype extractionSubgroupFluidsOP; //offshore pumps
        private SubgroupPrototype missingSubgroup;
        private TechnologyPrototype startingTech;

        private const float defaultRecipeTime = 0.5f;

        public DataCache()
        {
            includedMods = new Dictionary<string, string>();
            technologies = new Dictionary<string, Technology>();
            groups = new Dictionary<string, Group>();
            subgroups = new Dictionary<string, Subgroup>();
            items = new Dictionary<string, Item>();
            recipes = new Dictionary<string, Recipe>();
            assemblers = new Dictionary<string, Assembler>();
            modules = new Dictionary<string, Module>();
            beacons = new Dictionary<string, Beacon>();

            extractionGroup = new GroupPrototype(this, "$g:extraction_group", "Resource Extraction", "zzzzzzz");
            extractionGroup.SetIconAndColor(new IconColorPair(IconCache.GetIcon(Path.Combine("Graphics", "MiningIcon.png"), 64), Color.Gray));
            extractionSubgroupItems = new SubgroupPrototype(this, "$sg:extraction_items", "1");
            extractionSubgroupItems.myGroup = extractionGroup;
            extractionGroup.subgroups.Add(extractionSubgroupItems);
            extractionSubgroupFluids = new SubgroupPrototype(this, "$sg:extraction_fluids", "2");
            extractionSubgroupFluids.myGroup = extractionGroup;
            extractionGroup.subgroups.Add(extractionSubgroupFluids);
            extractionSubgroupFluidsOP = new SubgroupPrototype(this, "$sg:extraction_fluids_2", "3");
            extractionSubgroupFluidsOP.myGroup = extractionGroup;
            extractionGroup.subgroups.Add(extractionSubgroupFluidsOP);

            missingItems = new Dictionary<string, Item>();
            missingSubgroup = new SubgroupPrototype(this, "$MISSING-SG", "");
            missingSubgroup.myGroup = new GroupPrototype(this, "$MISSING-G", "MISSING", "");

            startingTech = new TechnologyPrototype(this, "", "");
        }

        public async Task LoadAllData(Preset preset, IProgress<KeyValuePair<int, string>> progress)
        {
            Clear();
            Dictionary<string, List<RecipePrototype>> craftingCategories = new Dictionary<string, List<RecipePrototype>>();
            Dictionary<string, List<RecipePrototype>> resourceCategories = new Dictionary<string, List<RecipePrototype>>();
            Dictionary<string, List<ItemPrototype>> fuelCategories = new Dictionary<string, List<ItemPrototype>>();
            Dictionary<Item, string> burnResults = new Dictionary<Item, string>();

            JObject jsonData = JObject.Parse(File.ReadAllText(Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".json" })));
            Dictionary<string, IconColorPair> iconCache = await IconCache.LoadIconCache(Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".dat" }), progress, 0, 90);
            PresetName = preset.Name;

            await Task.Run(() =>
            {
                progress.Report(new KeyValuePair<int, string>(90, "Processing Data...")); //this is SUPER quick, so we dont need to worry about timing stuff here

                groups.Add(extractionGroup.Name, extractionGroup);
                subgroups.Add(extractionSubgroupItems.Name, extractionSubgroupItems);
                subgroups.Add(extractionSubgroupFluids.Name, extractionSubgroupFluids);
                subgroups.Add(extractionSubgroupFluidsOP.Name, extractionSubgroupFluidsOP);

                //process each section
                foreach (var objJToken in jsonData["mods"].ToList())
                    ProcessMod(objJToken);
                foreach (var objJToken in jsonData["subgroups"].ToList())
                    ProcessSubgroup(objJToken);
                foreach (var objJToken in jsonData["groups"].ToList())
                    ProcessGroup(objJToken, iconCache);
                foreach (var objJToken in jsonData["items"].ToList())
                    ProcessItem(objJToken, iconCache, fuelCategories, burnResults);
                foreach (var objJToken in jsonData["fluids"].ToList())
                    ProcessFluid(objJToken, iconCache);
                foreach (ItemPrototype item in items.Values)
                    ProcessBurnItem(item, fuelCategories, burnResults); //link up any items with burn remains
                foreach (var objJToken in jsonData["recipes"].ToList())
                    ProcessRecipe(objJToken, iconCache, craftingCategories);
                foreach (var objJToken in jsonData["resources"].ToList())
                    ProcessResource(objJToken, resourceCategories);
                foreach (var objJToken in jsonData["modules"].ToList())
                    ProcessModule(objJToken, iconCache);
                foreach (var objJToken in jsonData["technologies"].ToList())
                    ProcessTechnology(objJToken, iconCache);
                foreach (var objJToken in jsonData["technologies"].ToList())
                    ProcessTechnologyP2(objJToken); //required to properly link technology prerequisites
                foreach (var objJToken in jsonData["assemblers"].ToList())
                    ProcessAssembler(objJToken, iconCache, craftingCategories);
                foreach (var objJToken in jsonData["miners"].ToList())
                    ProcessMiner(objJToken, iconCache, resourceCategories);
                foreach (var objJToken in jsonData["offshorepumps"].ToList())
                    ProcessOffshorePump(objJToken, iconCache);
                foreach (var objJtoken in jsonData["beacons"].ToList())
                    ProcessBeacon(objJtoken, iconCache);

                //process assemblers, miners, and offshore pumps one more time to read the item-burner / fluid-burner information
                foreach (var objJtoken in jsonData["assemblers"].ToList())
                    ProcessBurnerInfo(objJtoken, fuelCategories);
                foreach (var objJtoken in jsonData["miners"].ToList())
                    ProcessBurnerInfo(objJtoken, fuelCategories);
                foreach (var objJtoken in jsonData["offshorepumps"].ToList())
                    ProcessBurnerInfo(objJtoken, fuelCategories);


                //remove these temporary dictionaries (no longer necessary)
                craftingCategories.Clear();
                resourceCategories.Clear();
                fuelCategories.Clear();
                burnResults.Clear();

                //sort
                foreach (GroupPrototype g in groups.Values)
                    g.SortSubgroups();
                foreach (SubgroupPrototype sg in subgroups.Values)
                    sg.SortIRs();

                RemoveUnusableItems();

                progress.Report(new KeyValuePair<int, string>(96, "Checking for cyclic recipes...")); //a bit longer
                MarkCyclicRecipes();
                progress.Report(new KeyValuePair<int, string>(98, "Finalizing..."));

                progress.Report(new KeyValuePair<int, string>(100, "Done!"));
            });
        }

        public void ProcessNewItemSet(ICollection<string> itemNames) //will ensure that all items are now part of the data cache -> existing ones (regular and missing) are skipped, new ones are added to MissingItems
        {
            foreach (string iItem in itemNames)
            {
                if (!items.ContainsKey(iItem) && !missingItems.ContainsKey(iItem)) //want to check for missing items too - in this case dont want duplicates
                {
                    ItemPrototype missingItem = new ItemPrototype(this, iItem, iItem, false, missingSubgroup, "", true); //just assume it isnt a fluid. we dont honestly care (no temperatures)
                    missingItems.Add(missingItem.Name, missingItem);
                }
            }
        }

        public Dictionary<long, Recipe> ProcessNewRecipeShorts(ICollection<RecipeShort> recipeShorts) //will ensure all recipes are now part of the data cache -> each one is checked against existing recipes (regular & missing), and if it doesnt exist are added to MissingRecipes. Returns a set of links of original recipeID (NOT! the noew recipeIDs) to the recipe
        {
            Dictionary<long, Recipe> recipeLinks = new Dictionary<long, Recipe>();
            foreach (RecipeShort recipeShort in recipeShorts)
            {
                Recipe recipe = null;

                //recipe check #1 : does its name exist in database (note: we dont quite care about extra missing recipes here - so what if we have a couple identical ones? they will combine during save/load anyway)
                bool recipeExists = recipes.ContainsKey(recipeShort.Name);
                //recipe check #2 : do the number of ingredients & products match?
                recipeExists &= recipeShort.Ingredients.Count == recipes[recipeShort.Name].IngredientList.Count;
                recipeExists &= recipeShort.Products.Count == recipes[recipeShort.Name].ProductList.Count;
                //recipe check #3 : do the ingredients & products from the loaded data exist within the actual recipe?
                if (recipeExists)
                {
                    recipe = recipes[recipeShort.Name];
                    //check #2 (from above) - dodnt care about amounts of each
                    foreach (string ingredient in recipeShort.Ingredients.Keys)
                        recipeExists &= items.ContainsKey(ingredient) && recipe.IngredientSet.ContainsKey(items[ingredient]);
                    foreach (string product in recipeShort.Products.Keys)
                        recipeExists &= items.ContainsKey(product) && recipe.ProductSet.ContainsKey(items[product]);
                }

                if (!recipeExists)
                {
                    RecipePrototype missingRecipe = new RecipePrototype(this, recipeShort.Name, recipeShort.Name, missingSubgroup, "", true);
                    foreach (var ingredient in recipeShort.Ingredients)
                    {
                        if (items.ContainsKey(ingredient.Key))
                            missingRecipe.InternalOneWayAddIngredient((ItemPrototype)items[ingredient.Key], ingredient.Value);
                        else
                            missingRecipe.InternalOneWayAddIngredient((ItemPrototype)missingItems[ingredient.Key], ingredient.Value);
                    }
                    foreach (var product in recipeShort.Products)
                    {
                        if (items.ContainsKey(product.Key))
                            missingRecipe.InternalOneWayAddProduct((ItemPrototype)items[product.Key], product.Value);
                        else
                            missingRecipe.InternalOneWayAddProduct((ItemPrototype)missingItems[product.Key], product.Value);
                    }
                    recipe = missingRecipe;
                }
                recipeLinks.Add(recipeShort.RecipeID, recipe);
            }
            return recipeLinks;
        }

        public static Dictionary<string,string> ReadModList(Preset preset)
        {
            Dictionary<string, string> mods = new Dictionary<string, string>();
            string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".json" });
            if (!File.Exists(presetPath))
                return mods;

            JObject jsonData = JObject.Parse(File.ReadAllText(presetPath));
            foreach (var objJToken in jsonData["mods"].ToList())
                mods.Add((string)objJToken["name"], (string)objJToken["version"]);

            return mods;
        }

        public static PresetErrorPackage TestPreset(Preset preset, Dictionary<string,string> modList,  List<string> itemList, List<RecipeShort> recipeShorts)
        {

            string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".json" });
            if (!File.Exists(presetPath))
                return null;

            //parse preset (note: this is preset data, so we are guaranteed to only have one name per item/recipe/mod/etc.)
            JObject jsonData = JObject.Parse(File.ReadAllText(presetPath));
            HashSet<string> presetItems = new HashSet<string>();
            Dictionary<string, RecipeShort> presetRecipes = new Dictionary<string, RecipeShort>();
            Dictionary<string, string> presetMods = new Dictionary<string, string>();

            foreach (var objJToken in jsonData["mods"].ToList())
                presetMods.Add((string)objJToken["name"], (string)objJToken["version"]);
            foreach (var objJToken in jsonData["items"].ToList())
                presetItems.Add((string)objJToken["name"]);
            foreach (var objJToken in jsonData["fluids"].ToList())
                presetItems.Add((string)objJToken["name"]);

            foreach (var objJToken in jsonData["recipes"].ToList())
            {
                RecipeShort recipe = new RecipeShort((string)objJToken["name"]);
                foreach (var ingredientJToken in objJToken["ingredients"].ToList())
                {
                    string ingredientName = (string)ingredientJToken["name"];
                    if (recipe.Ingredients.ContainsKey(ingredientName))
                        recipe.Ingredients[ingredientName] += (float)ingredientJToken["amount"];
                    else
                        recipe.Ingredients.Add(ingredientName, (float)ingredientJToken["amount"]);
                }
                foreach (var productJToken in objJToken["products"].ToList())
                {
                    string productName = (string)productJToken["name"];
                    if (recipe.Products.ContainsKey(productName))
                        recipe.Products[productName] += (float)productJToken["amount"];
                    else
                        recipe.Products.Add(productName, (float)productJToken["amount"]);
                }
                presetRecipes.Add(recipe.Name, recipe);
            }

            //compare to provided mod/item/recipe sets (recipes have a chance of existing in multitudes - aka: missing recipes)
            PresetErrorPackage errors = new PresetErrorPackage(preset);
            foreach(var mod in modList)
            {
                errors.RequiredMods.Add(mod.Key + "|" + mod.Value);

                if (!presetMods.ContainsKey(mod.Key))
                    errors.MissingMods.Add(mod.Key + "|" + mod.Value);
                else if (presetMods[mod.Key] != mod.Value)
                    errors.WrongVersionMods.Add(mod.Key + "|" + mod.Value + "|" + presetMods[mod.Key]);
            }
            foreach (var mod in presetMods)
                if (!modList.ContainsKey(mod.Key))
                    errors.AddedMods.Add(mod.Key + "|" + mod.Value);

            foreach (string itemName in itemList)
            {
                errors.RequiredItems.Add(itemName);

                if (!presetItems.Contains(itemName))
                    errors.MissingItems.Add(itemName);
            }

            foreach (RecipeShort recipeS in recipeShorts)
            {
                errors.RequiredRecipes.Add(recipeS.Name);
                if (recipeS.isMissing)
                {
                    if (presetRecipes.ContainsKey(recipeS.Name) && recipeS.Equals(presetRecipes[recipeS.Name]))
                        errors.ValidMissingRecipes.Add(recipeS.Name);
                    else
                        errors.IncorrectRecipes.Add(recipeS.Name);
                }
                else
                {
                    if (!presetRecipes.ContainsKey(recipeS.Name))
                        errors.MissingRecipes.Add(recipeS.Name);
                    else if (!recipeS.Equals(presetRecipes[recipeS.Name]))
                        errors.IncorrectRecipes.Add(recipeS.Name);
                }
            }

            return errors;
        }

        public void Clear()
        {
            includedMods.Clear();
            technologies.Clear();
            groups.Clear();
            subgroups.Clear();
            items.Clear();
            recipes.Clear();
            assemblers.Clear();
            modules.Clear();
            beacons.Clear();

            missingItems.Clear();
        }

        private void ProcessMod(JToken objJToken)
        {
            includedMods.Add((string)objJToken["name"], (string)objJToken["version"]);
        }


        private void ProcessSubgroup(JToken objJToken)
        {
            SubgroupPrototype subgroup = new SubgroupPrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["order"]);

            subgroups.Add(subgroup.Name, subgroup);
        }

        private void ProcessGroup(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            GroupPrototype group = new GroupPrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"],
                (string)objJToken["order"]);

            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                group.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
            foreach (var subgroupJToken in objJToken["subgroups"])
            {
                ((SubgroupPrototype)subgroups[(string)subgroupJToken]).myGroup = group;
                group.subgroups.Add((SubgroupPrototype)subgroups[(string)subgroupJToken]);
            }
            groups.Add(group.Name, group);
        }

        private void ProcessItem(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<ItemPrototype>> fuelCategories, Dictionary<Item, string> burnResults)
        {
            ItemPrototype item = new ItemPrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"],
                false, //item (not a fluid)
                (SubgroupPrototype)subgroups[(string)objJToken["subgroup"]],
                (string)objJToken["order"]);

            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                item.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

            if(objJToken["fuel_category"] != null)
            {
                item.FuelValue = (float)objJToken["fuel_value"];
                if (!fuelCategories.ContainsKey((string)objJToken["fuel_category"]))
                    fuelCategories.Add((string)objJToken["fuel_category"], new List<ItemPrototype>());
                fuelCategories[(string)objJToken["fuel_category"]].Add(item);
            }
            if (objJToken["burnt_result"] != null)
                burnResults.Add(item, (string)objJToken["burnt_result"]);

            items.Add(item.Name, item);
        }

        private void ProcessBurnItem(ItemPrototype item, Dictionary<string, List<ItemPrototype>> fuelCategories, Dictionary<Item, string> burnResults)
        {
            if (burnResults.ContainsKey(item))
                item.BurnResult = items[burnResults[item]];
        }

        private void ProcessFluid(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            ItemPrototype item = new ItemPrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"],
                true, //fluid
                (SubgroupPrototype)subgroups[(string)objJToken["subgroup"]],
                (string)objJToken["order"]);

            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                item.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

            item.DefaultTemperature = (double)objJToken["default_temperature"];
            if (objJToken["fuel_value"] != null)
                item.FuelValue = (float)objJToken["fuel_value"];

                items.Add(item.Name, item);
        }

        private void ProcessRecipe(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<RecipePrototype>> craftingCategories)
        {
            RecipePrototype recipe = new RecipePrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"],
                (SubgroupPrototype)subgroups[(string)objJToken["subgroup"]],
                (string)objJToken["order"]);

            recipe.Time = (float)objJToken["energy"] > 0 ? (float)objJToken["energy"] : defaultRecipeTime;
            if ((bool)objJToken["enabled"]) //due to the way the import of presets happens, enabled at this stage means the recipe is available without any research necessary (aka: available at start)
            {
                recipe.myUnlockTechnologies.Add(startingTech);
                startingTech.unlockedRecipes.Add(recipe);
            }

            string category = (string)objJToken["category"];
            if (!craftingCategories.ContainsKey(category))
                craftingCategories.Add(category, new List<RecipePrototype>());
            craftingCategories[category].Add(recipe);

            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                recipe.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
            else if (iconCache.ContainsKey((string)objJToken["icon_alt_name"]))
                recipe.SetIconAndColor(iconCache[(string)objJToken["icon_alt_name"]]);

            foreach (var productJToken in objJToken["products"].ToList())
            {
                string name = (string)productJToken["name"];
                float amount = (float)productJToken["amount"];
                float temperature = 0; // ((string)productJToken["type"] == "fluid" && productJToken["temperature"].Type != JTokenType.Null) ? (float)productJToken["temperature"] : 0;
                if ((string)productJToken["type"] == "fluid" && productJToken["temperature"].Type != JTokenType.Null)
                {
                    temperature = (float)productJToken["temperature"];
                    if (((ItemPrototype)items[name]).DefaultTemperature != temperature)
                        ((ItemPrototype)items[name]).IsTemperatureDependent = true;
                }

                if (amount != 0)
                {
                    recipe.InternalOneWayAddProduct((ItemPrototype)items[name], amount, temperature);
                    ((ItemPrototype)items[name]).productionRecipes.Add(recipe);
                }
            }

            foreach (var ingredientJToken in objJToken["ingredients"].ToList())
            {
                string name = (string)ingredientJToken["name"];
                float amount = (float)ingredientJToken["amount"];

                float minTemp = ((string)ingredientJToken["type"] == "fluid" && ingredientJToken["minimum_temperature"].Type != JTokenType.Null) ? (float)ingredientJToken["minimum_temperature"] : float.NegativeInfinity;
                float maxTemp = ((string)ingredientJToken["type"] == "fluid" && ingredientJToken["maximum_temperature"].Type != JTokenType.Null) ? (float)ingredientJToken["maximum_temperature"] : float.PositiveInfinity;

                if (amount != 0)
                {
                    recipe.InternalOneWayAddIngredient((ItemPrototype)items[name], amount, minTemp, maxTemp);
                    ((ItemPrototype)items[name]).consumptionRecipes.Add(recipe);
                }
            }

            recipes.Add(recipe.Name, recipe);
        }

        private void ProcessResource(JToken objJToken, Dictionary<string, List<RecipePrototype>> resourceCategories)
        {
            if (objJToken["products"].Count() == 0)
                return;

            RecipePrototype recipe = new RecipePrototype(
                this,
                "$r:" + (string)objJToken["name"],
                (string)objJToken["localised_name"] + " Extraction",
                (string)objJToken["products"][0]["type"] == "fluid" ? extractionSubgroupFluids : extractionSubgroupItems,
                (string)objJToken["name"]);

            recipe.Time = (float)objJToken["mining_time"];

            foreach (var productJToken in objJToken["products"])
            {
                if (!items.ContainsKey((string)productJToken["name"]))
                    continue;
                ItemPrototype product = (ItemPrototype)items[(string)productJToken["name"]];
                recipe.InternalOneWayAddProduct(product, (float)productJToken["amount"]);
                product.productionRecipes.Add(recipe);
            }
            if (recipe.productList.Count == 0)
                return;

            if (objJToken["required_fluid"] != null)
            {
                ItemPrototype reqLiquid = (ItemPrototype)items[(string)objJToken["required_fluid"]];
                recipe.InternalOneWayAddIngredient(reqLiquid, (float)objJToken["fluid_amount"]);
                reqLiquid.consumptionRecipes.Add(recipe);
            }

            recipe.SetIconAndColor(new IconColorPair(recipe.productList[0].Icon, recipe.productList[0].AverageColor));

            string category = (string)objJToken["resource_category"];
            if (!resourceCategories.ContainsKey(category))
                resourceCategories.Add(category, new List<RecipePrototype>());
            resourceCategories[category].Add(recipe);

            recipe.myUnlockTechnologies.Add(startingTech);
            startingTech.unlockedRecipes.Add(recipe);

            recipes.Add(recipe.Name, recipe);
        }

        private void ProcessModule(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            ModulePrototype module = new ModulePrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"]);

            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                module.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

            module.SpeedBonus = (float)objJToken["module_effects_speed"];
            module.ProductivityBonus = (float)objJToken["module_effects_productivity"];
            module.ConsumptionBonus = (float)objJToken["module_effects_consumption"];
            module.PollutionBonus = (float)objJToken["module_effects_pollution"];

            foreach (var recipe in objJToken["limitations"])
            {
                if (recipes.ContainsKey((string)recipe)) //only add if the recipe is in the list of recipes (if it isnt, means it was deleted either in data phase of LUA or during foreman export cleanup)
                {
                    ((RecipePrototype)recipes[(string)recipe]).validModules.Add(module);
                    module.validRecipes.Add((RecipePrototype)recipes[(string)recipe]);
                }
            }

            if (objJToken["limitations"].Count() == 0) //means all recipes work
            {
                foreach (RecipePrototype recipe in recipes.Values)
                {
                    recipe.validModules.Add(module);
                    module.validRecipes.Add(recipe);
                }
            }

            //string category = (string)objJToken["category"]; //apparently, this is USELESS! the allowance of modules into entites is based off of their bonus% values rather than categories (now? maybe it was different before?)

            modules.Add(module.Name, module);
        }

        private void ProcessTechnology(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            TechnologyPrototype technology = new TechnologyPrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"]);

            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                technology.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

            foreach (var recipe in objJToken["recipes"])
            {
                if (recipes.ContainsKey((string)recipe))
                {
                    ((RecipePrototype)recipes[(string)recipe]).myUnlockTechnologies.Add(technology);
                    technology.unlockedRecipes.Add((RecipePrototype)recipes[(string)recipe]);
                }
            }

            technologies.Add(technology.Name, technology);
        }

        private void ProcessTechnologyP2(JToken objJToken)
        {
            TechnologyPrototype technology = (TechnologyPrototype)technologies[(string)objJToken["name"]];
            foreach (var prerequisite in objJToken["prerequisites"])
            {
                if (technologies.ContainsKey((string)prerequisite))
                {
                    technology.prerequisites.Add((TechnologyPrototype)technologies[(string)prerequisite]);
                    ((TechnologyPrototype)technologies[(string)prerequisite]).postTechs.Add(technology);
                }
            }
        }

        private void ProcessAssembler(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<RecipePrototype>> craftingCategories)
        {
            AssemblerPrototype assembler = new AssemblerPrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"],
                false);

            assembler.Speed = (float)objJToken["crafting_speed"];
            assembler.ModuleSlots = (int)objJToken["module_inventory_size"];
            assembler.BaseProductivityBonus = (float)objJToken["base_productivity"];

            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                assembler.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

            foreach (var categoryJToken in objJToken["crafting_categories"])
            {
                if (craftingCategories.ContainsKey((string)categoryJToken))
                {
                    foreach (RecipePrototype recipe in craftingCategories[(string)categoryJToken])
                    {
                        recipe.validAssemblers.Add(assembler);
                        assembler.validRecipes.Add(recipe);
                    }
                }
            }

            List<string> allowedEffectsList = objJToken["allowed_effects"].Select(token => (string)token).ToList();
            bool[] allowedEffects = new bool[] { 
                allowedEffectsList.Contains("consumption"), 
                allowedEffectsList.Contains("speed"), 
                allowedEffectsList.Contains("productivity"), 
                allowedEffectsList.Contains("pollution") };

            foreach(ModulePrototype module in modules.Values.Where(module => 
                (allowedEffects[0] || module.ConsumptionBonus == 0) &&
                (allowedEffects[1] || module.SpeedBonus == 0) &&
                (allowedEffects[2] || module.ProductivityBonus == 0) &&
                (allowedEffects[3] || module.PollutionBonus == 0)))
            {
                module.validAssemblers.Add(assembler);
                assembler.validModules.Add(module);
            }

            assemblers.Add(assembler.Name, assembler);
        }

        private void ProcessMiner(JToken objJToken, Dictionary<string, IconColorPair> iconCache, Dictionary<string, List<RecipePrototype>> resourceCategories)
        {
            AssemblerPrototype assembler = new AssemblerPrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"],
                true);

            assembler.Speed = (float)objJToken["mining_speed"];
            assembler.ModuleSlots = (int)objJToken["module_inventory_size"];
            assembler.BaseProductivityBonus = (float)objJToken["base_productivity"];

            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                assembler.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

            foreach (var categoryJToken in objJToken["resource_categories"])
            {
                if (resourceCategories.ContainsKey((string)categoryJToken))
                {
                    foreach (RecipePrototype recipe in resourceCategories[(string)categoryJToken])
                    {
                        recipe.validAssemblers.Add(assembler);
                        assembler.validRecipes.Add(recipe);
                    }
                }
            }

            List<string> allowedEffectsList = objJToken["allowed_effects"].Select(token => (string)token).ToList();
            bool[] allowedEffects = new bool[] {
                allowedEffectsList.Contains("consumption"),
                allowedEffectsList.Contains("speed"),
                allowedEffectsList.Contains("productivity"),
                allowedEffectsList.Contains("pollution") };

            foreach (ModulePrototype module in modules.Values.Where(module =>
                 (allowedEffects[0] || module.ConsumptionBonus == 0) &&
                 (allowedEffects[1] || module.SpeedBonus == 0) &&
                 (allowedEffects[2] || module.ProductivityBonus == 0) &&
                 (allowedEffects[3] || module.PollutionBonus == 0)))
            {
                module.validAssemblers.Add(assembler);
                assembler.validModules.Add(module);
            }
            assemblers.Add(assembler.Name, assembler);
        }

        private void ProcessOffshorePump(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            AssemblerPrototype assembler = new AssemblerPrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"],
                true);

            assembler.Speed = (float)objJToken["pumping_speed"];
            assembler.ModuleSlots = 0;
            assembler.BaseProductivityBonus = 0;

            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                assembler.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

            string fluidName = (string)objJToken["fluid"];
            if (!items.ContainsKey(fluidName))
                return;
            ItemPrototype fluid = (ItemPrototype)items[fluidName];

            //now to add an extra recipe that will be used to 'mine' this fluid
            RecipePrototype recipe;
            if (!recipes.ContainsKey("$p:" + fluid.Name))
            {
                recipe = new RecipePrototype(
                    this,
                    "$p:" + fluid.Name,
                    fluid.FriendlyName + " Extraction",
                    extractionSubgroupFluidsOP,
                    fluid.Name);

                recipe.SetIconAndColor(new IconColorPair(fluid.Icon, fluid.AverageColor));
                recipe.Time = 1;
                recipe.InternalOneWayAddProduct(fluid, 60);

                recipe.myUnlockTechnologies.Add(startingTech);
                startingTech.unlockedRecipes.Add(recipe);

                recipes.Add(recipe.Name, recipe);
            }
            else
                recipe = (RecipePrototype)recipes["$p:" + fluid.Name];

            recipe.validAssemblers.Add(assembler);
            assembler.validRecipes.Add(recipe);

            assemblers.Add(assembler.Name, assembler);
        }

        private void ProcessBeacon(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            BeaconPrototype beacon = new BeaconPrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"]);

            beacon.Effectivity = (float)objJToken["distribution_effectivity"];
            beacon.ModuleSlots = (int)objJToken["module_inventory_size"];

            List<string> allowedEffectsList = objJToken["allowed_effects"].Select(token => (string)token).ToList();
            bool[] allowedEffects = new bool[] {
                allowedEffectsList.Contains("consumption"),
                allowedEffectsList.Contains("speed"),
                allowedEffectsList.Contains("productivity"),
                allowedEffectsList.Contains("pollution") };

            foreach (ModulePrototype module in modules.Values.Where(module =>
                 (allowedEffects[0] || module.ConsumptionBonus == 0) &&
                 (allowedEffects[1] || module.SpeedBonus == 0) &&
                 (allowedEffects[2] || module.ProductivityBonus == 0) &&
                 (allowedEffects[3] || module.PollutionBonus == 0)))
            {
                module.validBeacons.Add(beacon);
                beacon.validModules.Add(module);
            }

            beacons.Add(beacon.Name, beacon);
        }

        private void ProcessBurnerInfo(JToken objJToken, Dictionary<string, List<ItemPrototype>> fuelCategories)
        {
            AssemblerPrototype assembler = (AssemblerPrototype)assemblers[(string)objJToken["name"]];
            assembler.EnergyConsumption = (float)objJToken["max_energy_usage"];
            if(objJToken["fuel_type"] != null)
            {
                assembler.IsBurner = true;
                assembler.EnergyEffectivity = (float)objJToken["fuel_effectivity"];

                if((string)objJToken["fuel_type"] == "item")
                {
                    foreach (var categoryJToken in objJToken["fuel_categories"])
                    {
                        if (fuelCategories.ContainsKey((string)categoryJToken))
                        {
                            foreach (ItemPrototype item in fuelCategories[(string)categoryJToken])
                            {
                                assembler.validFuels.Add(item);
                                item.fuelsAssemblers.Add(assembler);
                            }
                        }
                    }

                }
                else if((string)objJToken["fuel_type"] == "fluid")
                {
                    if((bool)objJToken["burns_fluid"] == false) //this entity burns the fluid and calculates power based on fluid temperature. So.... I will just say it isnt a burner. FK THAT! (is this a leftover from old factorio with steam turbines burning any fluid?)
                    {
                        assembler.IsBurner = false;
                        assembler.EnergyEffectivity = 0;
                        return;
                    }

                    foreach(ItemPrototype fluid in items.Values.Where(i => i.IsFluid && i.FuelValue > 0))
                    {
                        assembler.validFuels.Add(fluid);
                        fluid.fuelsAssemblers.Add(assembler);
                    }
                }
            }
        }

        //The data read by the dataCache (json preset) has already had its technology filtered such that only those tech that are 'unlockable' (aka: not hidden, disabled or broken)
        //will be added. The list of recipes and items have also been culled to include only recipes that are unlocked at start or unlocked by unlockable tech,
        //with items being only those that are part of the ingredients or products of the filtered recipes.
        //also - barreling recipes (and crating from deadlock crating) have been removed in mod.
        //NOTE: this is the reason why cheat assemblers (for creative mode) dont appear here.

        //so in this last filter we just need to clear out any recipes with no valid assemblers, recipes with 0/0 ingredients/products, items that no longer have any recipes (as ingredient or product) or are an assembler/beacon,
        //and items that arent created anywhere and are only consumed by a single recipe with 1 ingredient and 0 products (burn/incinerate type recipes)
        //Additionally we process groups and subgroups to include only those that have items/recipes (removing for example the 'signals' group)
        private void RemoveUnusableItems()
        {
            //step 1: delete any recipes with no unlocks (those that had no unlocks in the beginning - those that were part of the removed tech were also removed)
            //step 1.1: also delete any recipes with no viable assembler
            //step 1.2: and the recipes with no ingredients and products (... industrial revolution, what are those aetheric glow recipes????)
            foreach (RecipePrototype recipe in recipes.Values.ToList())
                if (recipe.myUnlockTechnologies.Count == 0 || recipe.validAssemblers.Count == 0 || (recipe.productList.Count == 0 && recipe.ingredientList.Count == 0))
                    DeleteRecipe(recipe);

            //step 2: try and delete all items (can only delete if item isnt produced/consumed by any recipe, and isnt part of assemblers (that exist in this cache)
            foreach (ItemPrototype item in items.Values.ToList())
                TryDeleteItem(item); //soft delete

            //step 3: clean up those items which are kind of useless (those that are not produced anywhere, arent an assembler, and are used in a single recipe that uses only them)
            //this is necessary to take care of those mods that add item destruction and allow for any item (read - all of them) to be destroyed.
            foreach (ItemPrototype item in items.Values.ToList())
            {
                if (item.productionRecipes.Count == 0)
                {
                    bool useful = false;
                    foreach (RecipePrototype r in item.consumptionRecipes)
                        useful |= (r.ingredientList.Count > 1 || r.productList.Count != 0); //recipe with multiple items coming in or some ingredients coming out -> not an incineration type
                    if(assemblers.Values.FirstOrDefault(a => (ItemPrototype)a.AssociatedItem == item) != null)
                        useful = true;
                    if (beacons.Values.FirstOrDefault(b => (ItemPrototype)b.AssociatedItem == item) != null)
                        useful = true;
                    if (!useful)
                        TryDeleteItem(item, true); //hard delete.
                }
            }

            //step 4: clean up groups and subgroups (basically, clear the entire dictionary and for each recipe & item 'add' their subgroup & group into the dictionary.
            groups.Clear();
            subgroups.Clear();
            foreach (ItemPrototype item in Items.Values)
                if (!subgroups.ContainsKey(item.mySubgroup.Name))
                    subgroups.Add(item.mySubgroup.Name, item.mySubgroup);
            foreach (RecipePrototype recipe in recipes.Values)
                if (!subgroups.ContainsKey(recipe.mySubgroup.Name))
                    subgroups.Add(recipe.mySubgroup.Name, recipe.mySubgroup);
            foreach (SubgroupPrototype subgroup in subgroups.Values)
                if (!groups.ContainsKey(subgroup.myGroup.Name))
                    groups.Add(subgroup.myGroup.Name, subgroup.myGroup);
        }

        private void MarkCyclicRecipes()
        {
            foreach (var scc in CyclicNodeTester.GetStronglyConnectedComponents(this))
                foreach (var node in scc)
                    ((RecipePrototype)((RecipeNode)node).BaseRecipe).IsCyclic = true;
        }

        private void DeleteTechnology(TechnologyPrototype technology)
        {
            foreach (TechnologyPrototype tech in technology.prerequisites)
                tech.postTechs.Remove(technology);
            foreach (TechnologyPrototype tech in technology.postTechs)
                tech.prerequisites.Remove(technology);
            foreach (RecipePrototype recipe in technology.unlockedRecipes)
            {
                recipe.myUnlockTechnologies.Remove(technology);
                if (recipe.myUnlockTechnologies.Count == 0)
                    DeleteRecipe(recipe);
            }
            technologies.Remove(technology.Name);
            Console.WriteLine("Deleting technology: " + technology);
        }
        private bool TryDeleteItem(ItemPrototype item, bool forceDelete = false)
        {
            if(forceDelete) //remove it from every production & consumption recipe, remove recipe if it no longer has any ingredients & products, then proceed.
            {
                foreach (RecipePrototype r in item.consumptionRecipes)
                {
                    r.InternalOneWayDeleteIngredient(item);
                    if (r.ingredientList.Count == 0 && r.productList.Count == 0)
                        DeleteRecipe(r);
                }
                foreach (RecipePrototype r in item.productionRecipes)
                {
                    r.InternalOneWayDeleteProduct(item);
                    if (r.ingredientList.Count == 0 && r.productList.Count == 0)
                        DeleteRecipe(r);
                }

                if (assemblers.ContainsKey(item.Name))
                    DeleteAssembler((AssemblerPrototype)assemblers[item.Name]);
                if (beacons.ContainsKey(item.Name))
                    DeleteBeacon((BeaconPrototype)beacons[item.Name]);
                if (modules.ContainsKey(item.Name))
                    DeleteModule((ModulePrototype)modules[item.Name]);
            }

            //can only delete an item if it has no production recipes, consumption recipes, module, assembler, or miner associated with it.
            if( forceDelete || (
                item.productionRecipes.Count == 0 && 
                item.consumptionRecipes.Count == 0 && 
                !assemblers.ContainsKey(item.Name) &&
                !beacons.ContainsKey(item.Name) &&
                !modules.ContainsKey(item.Name)))
            {
                foreach (AssemblerPrototype p in item.fuelsAssemblers)
                    p.validFuels.Remove(item);

                item.mySubgroup.items.Remove(item);
                items.Remove(item.Name);
                if(forceDelete)
                    Console.WriteLine("FORCEFUL DELETE! Deleting item: " + item);
                else
                    Console.WriteLine("Deleting item: " + item);
                return true;
            }
            return false;
        }
        private void DeleteRecipe(RecipePrototype recipe)
        {
            foreach (ItemPrototype item in recipe.ingredientList)
            {
                item.consumptionRecipes.Remove(recipe);
                TryDeleteItem(item);
            }
            foreach (ItemPrototype item in recipe.productList)
            {
                item.productionRecipes.Remove(recipe);
                TryDeleteItem(item);
            }
            foreach (AssemblerPrototype assembler in recipe.validAssemblers)
                assembler.validRecipes.Remove(recipe);
            foreach (ModulePrototype module in recipe.validModules)
                module.validRecipes.Remove(recipe);
            foreach (TechnologyPrototype tech in recipe.myUnlockTechnologies)
                tech.unlockedRecipes.Remove(recipe);

            recipe.mySubgroup.recipes.Remove(recipe);
            recipes.Remove(recipe.Name);
            Console.WriteLine("Deleting recipe: " + recipe);
        }
        private void DeleteAssembler(AssemblerPrototype assembler)
        {
            foreach (RecipePrototype recipe in assembler.validRecipes)
            {
                recipe.validAssemblers.Remove(assembler);
                if (recipe.validAssemblers.Count == 0)
                    DeleteRecipe(recipe);
            }
            foreach (ModulePrototype module in assembler.validModules)
                module.validAssemblers.Remove(assembler);
            foreach (ItemPrototype item in assembler.validFuels)
                item.fuelsAssemblers.Remove(assembler);

            assemblers.Remove(assembler.Name);
            TryDeleteItem(assembler.AssociatedItem as ItemPrototype);
            Console.WriteLine("Deleting assembler: " + assembler);
        }
        private void DeleteModule(ModulePrototype module)
        {
            foreach (RecipePrototype recipe in module.validRecipes)
                recipe.validModules.Remove(module);
            foreach (AssemblerPrototype assembler in module.validAssemblers)
                assembler.validModules.Remove(module);
            foreach (BeaconPrototype beacon in module.validBeacons)
                beacon.validModules.Remove(module);

            modules.Remove(module.Name);
            TryDeleteItem(module.AssociatedItem as ItemPrototype);
            Console.WriteLine("Deleting module: " + module.Name);
        }
        private void DeleteBeacon(BeaconPrototype beacon)
        {
            foreach (ModulePrototype module in beacon.validModules)
                module.validBeacons.Remove(beacon);

            beacons.Remove(beacon.Name);
            TryDeleteItem(beacon.AssociatedItem as ItemPrototype);
            Console.WriteLine("Deleting beacon: " + beacon.Name);
        }
    }
}
