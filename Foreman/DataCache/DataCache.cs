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
        public List<string> MissingRecipes;
        public List<string> IncorrectRecipes;
        public List<string> MissingItems;
        public List<string> MissingMods;
        public List<string> AddedMods;
        public List<string> WrongVersionMods;

        public int ErrorCount { get { return MissingRecipes.Count + IncorrectRecipes.Count + MissingItems.Count + MissingMods.Count + AddedMods.Count + WrongVersionMods.Count; } }
        public int MICount { get { return MissingRecipes.Count + IncorrectRecipes.Count + MissingItems.Count; } }

        public PresetErrorPackage()
        {
            MissingRecipes = new List<string>();
            IncorrectRecipes = new List<string>();
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
        public IReadOnlyDictionary<string, Miner> Miners { get { return miners; } }
        public IReadOnlyDictionary<string, Resource> Resources { get { return resources; } }
        public IReadOnlyDictionary<string, Module> Modules { get { return modules; } }

        public IReadOnlyCollection<Recipe> MissingRecipes { get { return missingRecipes; } }
        public IReadOnlyDictionary<string, Item> MissingItems { get { return missingItems; } }

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
        private Dictionary<string, Miner> miners;
        private Dictionary<string, Resource> resources;
        private Dictionary<string, Module> modules;

        private HashSet<Recipe> missingRecipes;
        private Dictionary<string, Item> missingItems;

        private SubgroupPrototype missingSubgroup;
        private TechnologyPrototype startingTech;

        private Dictionary<string, List<RecipePrototype>> craftingCategories;
        private Dictionary<string, List<ModulePrototype>> moduleCategories;
        private Dictionary<string, List<ResourcePrototype>> resourceCategories;
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
            miners = new Dictionary<string, Miner>();
            resources = new Dictionary<string, Resource>();
            modules = new Dictionary<string, Module>();

            missingItems = new Dictionary<string, Item>();
            missingRecipes = new HashSet<Recipe>(new MissingRecipeComparer()); //deep compare that checks name, ingredients, and products
            missingSubgroup = new SubgroupPrototype(this, "", "");
            missingSubgroup.myGroup = new GroupPrototype(this, "", "", "");

            startingTech = new TechnologyPrototype(this, "", "");

            craftingCategories = new Dictionary<string, List<RecipePrototype>>();
            moduleCategories = new Dictionary<string, List<ModulePrototype>>();
            resourceCategories = new Dictionary<string, List<ResourcePrototype>>();
        }

        public async Task LoadAllData(Preset preset, IProgress<KeyValuePair<int, string>> progress, CancellationToken ctoken)
        {
            Clear();

            JObject jsonData = JObject.Parse(File.ReadAllText(Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".json" })));
            Dictionary<string, IconColorPair> iconCache = await IconCache.LoadIconCache(Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".dat" }), progress, ctoken, 0, 90);
            PresetName = preset.Name;

            await Task.Run(() =>
            {

                progress.Report(new KeyValuePair<int, string>(90, "Processing Data...")); //this is SUPER quick, so we dont need to worry about timing stuff here

                //process each section
                foreach (var objJToken in jsonData["mods"].ToList())
                    ProcessMod(objJToken);
                foreach (var objJToken in jsonData["subgroups"].ToList())
                    ProcessSubgroup(objJToken);
                foreach (var objJToken in jsonData["groups"].ToList())
                    ProcessGroup(objJToken, iconCache);
                foreach (var objJToken in jsonData["items"].ToList())
                    ProcessItem(objJToken, iconCache);
                foreach (var objJToken in jsonData["fluids"].ToList())
                    ProcessFluid(objJToken, iconCache);
                foreach (var objJToken in jsonData["resources"].ToList())
                    ProcessResource(objJToken);
                foreach (var objJToken in jsonData["miners"].ToList())
                    ProcessMiner(objJToken, iconCache);
                foreach (var objJToken in jsonData["offshorepumps"].ToList())
                    ProcessOffshorePump(objJToken, iconCache);
                foreach (var objJToken in jsonData["recipes"].ToList())
                    ProcessRecipe(objJToken, iconCache);
                foreach (var objJToken in jsonData["modules"].ToList())
                    ProcessModule(objJToken);
                foreach (var objJToken in jsonData["technologies"].ToList())
                    ProcessTechnology(objJToken, iconCache);
                foreach (var objJToken in jsonData["technologies"].ToList())
                    ProcessTechnologyP2(objJToken); //required to properly link technology prerequisites
                foreach (var objJToken in jsonData["assemblers"].ToList())
                    ProcessAssembler(objJToken, iconCache);

                //remove these temporary dictionaries (no longer necessary)
                craftingCategories.Clear();
                moduleCategories.Clear();
                resourceCategories.Clear();

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
            PresetErrorPackage errors = new PresetErrorPackage();
            foreach(var mod in modList)
            {
                if (!presetMods.ContainsKey(mod.Key))
                    errors.MissingMods.Add(mod.Key + "|" + mod.Value);
                else if (presetMods[mod.Key] != mod.Value)
                    errors.WrongVersionMods.Add(mod.Key + "|" + mod.Value + "|" + presetMods[mod.Key]);
            }
            foreach (var mod in presetMods)
                if (!modList.ContainsKey(mod.Key))
                    errors.AddedMods.Add(mod.Key + "|" + mod.Value);

            foreach (string itemName in itemList)
                if (!presetItems.Contains(itemName))
                    errors.MissingItems.Add(itemName);

            foreach (RecipeShort recipeS in recipeShorts)
            {
                if (recipeS.isMissing)
                    continue;

                if (!presetRecipes.ContainsKey(recipeS.Name))
                    errors.MissingRecipes.Add(recipeS.Name);
                else if (!recipeS.Equals(presetRecipes[recipeS.Name]))
                    errors.IncorrectRecipes.Add(recipeS.Name);
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
            miners.Clear();
            resources.Clear();
            modules.Clear();

            missingItems.Clear();
            missingRecipes.Clear();

            craftingCategories.Clear();
            moduleCategories.Clear();
            resourceCategories.Clear();
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

        private void ProcessItem(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
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
            items.Add(item.Name, item);
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

            item.DefaultTemperature = (double)objJToken["default_temperature"];
            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                item.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
            items.Add(item.Name, item);
        }

        private void ProcessResource(JToken objJToken)
        {
            ResourcePrototype resource = new ResourcePrototype(
                this,
                (string)objJToken["name"]);

            resource.Time = (float)objJToken["mining_time"];
            foreach (var productJToken in objJToken["products"])
            {
                if (items.ContainsKey((string)productJToken["name"]))
                {
                    resource.resultingItems.Add((ItemPrototype)items[(string)productJToken["name"]]);
                    ((ItemPrototype)items[(string)productJToken["name"]]).miningResources.Add(resource);
                }
            }
            if (resource.ResultingItems.Count == 0)
                return; //If the resource doesnt actually produce any products, just ignore it.

            string category = (string)objJToken["resource_category"];
            if (!resourceCategories.ContainsKey(category))
                resourceCategories.Add(category, new List<ResourcePrototype>());
            resourceCategories[category].Add(resource);

            resources.Add(resource.Name, resource);
        }

        private void ProcessMiner(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            MinerPrototype miner = new MinerPrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"]);

            miner.MiningSpeed = (float)objJToken["mining_speed"];
            miner.ModuleSlots = (int)objJToken["module_inventory_size"];
            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                miner.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

            foreach (var categoryJToken in objJToken["resource_categories"])
            {
                if (resourceCategories.ContainsKey((string)categoryJToken))
                {
                    foreach (ResourcePrototype resource in resourceCategories[(string)categoryJToken])
                    {
                        resource.validMiners.Add(miner);
                        miner.mineableResources.Add(resource);
                    }
                }
            }

            miners.Add(miner.Name, miner);
        }

        private void ProcessOffshorePump(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            MinerPrototype miner = new MinerPrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"]);

            miner.MiningSpeed = (float)objJToken["pumping_speed"];
            miner.ModuleSlots = 0;
            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                miner.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

            string fluidName = (string)objJToken["fluid"];
            if (!items.ContainsKey(fluidName))
                return;
            ItemPrototype fluid = (ItemPrototype)items[fluidName];

            //now to add an extra 'resource' if there isnt one with the type of fluid being pumped out here. Once we ensure there is one, we add this miner to it
            string rcName = "fer." + fluid.Name;
            if(!resources.ContainsKey(rcName))
            {
                ResourcePrototype extraResource = new ResourcePrototype(this, rcName);
                extraResource.Time = (1f/60); //'mining-speed' of 20, and time of 1/60s (every tick) leads to 1200 water per second. mods can set the mining/pumping-speed.
                extraResource.resultingItems.Add(fluid);
                fluid.miningResources.Add(extraResource);

                resources.Add(extraResource.Name, extraResource);
            }

            ((ResourcePrototype)resources[rcName]).validMiners.Add(miner);
            miner.mineableResources.Add((ResourcePrototype)resources[rcName]);
            miners.Add(miner.Name, miner);
        }

        private void ProcessRecipe(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
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

        private void ProcessModule(JToken objJToken)
        {
            ModulePrototype module = new ModulePrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"],
                (float)objJToken["module_effects_speed"],
                (float)objJToken["module_effects_productivity"]);

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

            string category = (string)objJToken["category"];
            if (!moduleCategories.ContainsKey(category))
                moduleCategories.Add(category, new List<ModulePrototype>());
            moduleCategories[category].Add(module);

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

        private void ProcessAssembler(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            AssemblerPrototype assembler = new AssemblerPrototype(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"]);

            assembler.Speed = (float)objJToken["crafting_speed"];
            assembler.ModuleSlots = (int)objJToken["module_inventory_size"];

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

            foreach (var effectJToken in objJToken["allowed_effects"])
            {
                if (moduleCategories.ContainsKey((string)effectJToken))
                {
                    foreach (ModulePrototype module in moduleCategories[(string)effectJToken])
                    {
                        module.validAssemblers.Add(assembler);
                        assembler.validModules.Add(module);
                    }
                }
            }

            assemblers.Add(assembler.Name, assembler);
        }

        //This is used to clean up the items & recipes to those that can actually appear given the settings.
        //A very similar process is done in the 'control.lua' of the foreman mod, but this also processes the enabled assembly machines
        //to further clean up items
        //NOTE: if the FactorioLuaProcessor is used (as opposed to the foreman mod export), then this does the entire job of control.lua in
        //checking researchable tech, removing un-unlockable recipes, removing any items that dont appear in the clean recipe list, etc.
        private void RemoveUnusableItems()
        {
            HashSet<TechnologyPrototype> temp_unlockableTechSet = new HashSet<TechnologyPrototype>();
            bool IsUnlockable(TechnologyPrototype tech) //sets the locked parameter based on unlockability of it & its prerequisites. returns true if unlocked
            {
                if (tech.Locked)
                    return false;
                else if (temp_unlockableTechSet.Contains(tech))
                    return true;
                else if (tech.prerequisites.Count == 0)
                    return true;
                else
                {
                    bool available = true;
                    foreach (TechnologyPrototype preTech in tech.prerequisites)
                        available = available && IsUnlockable(preTech);
                    tech.Locked = !available;

                    if (available)
                        temp_unlockableTechSet.Add(tech);
                    return available;
                }
            }

            //step 1: calculate unlockable technologies (either already researched or recursive search to confirm all prerequisites are unlockable / researched)
            List<string> unavailableTech = new List<string>();
            foreach (TechnologyPrototype tech in technologies.Values)
                if (!IsUnlockable(tech))
                    unavailableTech.Add(tech.Name);

            //step 1.5: remove blocked tech (this will also delete some recipes that are no longer able to be unlocked)
            foreach (string techName in unavailableTech)
                DeleteTechnology((TechnologyPrototype)technologies[techName]);

            //step 2: delete any recipes with no unlocks (those that had no unlocks in the beginning - those that were part of the removed tech were also removed)
            //step 2.1: also delete any recipes with no viable assembler
            foreach (RecipePrototype recipe in recipes.Values.ToList())
                if (recipe.myUnlockTechnologies.Count == 0 || recipe.validAssemblers.Count == 0)
                    DeleteRecipe(recipe);

            //step 3: try and delete all items (can only delete if item isnt produced/consumed by any recipe, and isnt part of assemblers, miners, modules (that exist in this cache)
            foreach (ItemPrototype item in items.Values.ToList())
                TryDeleteItem(item); //soft delete

            //step 3.5: clean up those items which are kind of useless (those that are not produced anywhere, and are used in a single recipe that uses only them)
            //this is necessary to take care of those mods that add item destruction and allow for any item (read - all of them) to be destroyed.
            foreach(ItemPrototype item in items.Values.ToList())
            {
                if(item.productionRecipes.Count == 0)
                {
                    bool useful = false;
                    foreach (RecipePrototype recipe in item.consumptionRecipes)
                        useful |= (recipe.ingredientList.Count > 1 || recipe.productList.Count != 0); //recipe with multiple items coming in or some ingredients coming out -> not an incineration type
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
            ProductionGraph testGraph = new ProductionGraph(this);
            foreach (Recipe recipe in Recipes.Values)
                RecipeNode.Create(recipe, testGraph);
            testGraph.CreateAllPossibleInputLinks();
            foreach (var scc in testGraph.GetStronglyConnectedComponents(true))
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
                if (miners.ContainsKey(item.Name))
                    DeleteMiner((MinerPrototype)miners[item.Name]);
                if (modules.ContainsKey(item.Name))
                    DeleteModule((ModulePrototype)modules[item.Name]);
            }

            //can only delete an item if it has no production recipes, consumption recipes, module, assembler, or miner associated with it.
            if( forceDelete || (
                item.productionRecipes.Count == 0 && 
                item.consumptionRecipes.Count == 0 && 
                !assemblers.ContainsKey(item.Name) &&
                !miners.ContainsKey(item.Name) &&
                !modules.ContainsKey(item.Name)))
            {
                foreach (ResourcePrototype resource in item.miningResources)
                {
                    resource.resultingItems.Remove(item);
                    if (resource.resultingItems.Count == 0)
                        DeleteResource(resource);
                }

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
            assemblers.Remove(assembler.Name);
            TryDeleteItem(assembler.AssociatedItem as ItemPrototype);
            Console.WriteLine("Deleting assembler: " + assembler);
        }
        private void DeleteMiner(MinerPrototype miner)
        {
            foreach (ResourcePrototype resource in miner.mineableResources)
            {
                resource.validMiners.Remove(miner);
                if (resource.validMiners.Count == 0)
                    DeleteResource(resource);
            }
            miners.Remove(miner.Name);
            TryDeleteItem(miner.AssociatedItem as ItemPrototype);
            Console.WriteLine("Deleting miner: " + miner);
        }
        private void DeleteResource(ResourcePrototype resource)
        {
            foreach(ItemPrototype item in resource.resultingItems)
                item.miningResources.Remove(resource);
            foreach (MinerPrototype miner in resource.validMiners)
                miner.mineableResources.Remove(resource);
            resources.Remove(resource.Name);
            Console.WriteLine("Deleting resource: " + resource);
        }
        private void DeleteModule(ModulePrototype module)
        {
            foreach (RecipePrototype recipe in module.validRecipes)
                recipe.validModules.Remove(module);
            foreach (AssemblerPrototype assembler in module.validAssemblers)
                assembler.validModules.Remove(module);
            modules.Remove(module.Name);
            TryDeleteItem(module.AssociatedItem as ItemPrototype);
            Console.WriteLine("Deleting module: " + module.Name);
        }
    }
}
