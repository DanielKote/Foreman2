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

        public string PresetName { get; private set; }

        public HashSet<Recipe> MissingRecipes { get; private set; }
        public Dictionary<string, Item> MissingItems { get; private set; }
        public Subgroup MissingSubgroup { get; private set; }
        public Technology StartingTech { get; private set; }
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

        private Dictionary<string, List<Recipe>> craftingCategories;
        private Dictionary<string, List<Module>> moduleCategories;
        private Dictionary<string, List<Resource>> resourceCategories;
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

            MissingItems = new Dictionary<string, Item>();
            MissingRecipes = new HashSet<Recipe>();
            MissingSubgroup = new Subgroup(this, "", "");
            MissingSubgroup.SetGroup(new Group(this, "", "", ""));

            StartingTech = new Technology(this, "", "");

            craftingCategories = new Dictionary<string, List<Recipe>>();
            moduleCategories = new Dictionary<string, List<Module>>();
            resourceCategories = new Dictionary<string, List<Resource>>();
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
                foreach (Group g in groups.Values)
                    g.SortSubgroups();
                foreach (Subgroup sg in subgroups.Values)
                    sg.SortIRs();

                RemoveUnusableItems();

                progress.Report(new KeyValuePair<int, string>(96, "Checking for cyclic recipes...")); //a bit longer
                MarkCyclicRecipes();
                progress.Report(new KeyValuePair<int, string>(98, "Finalizing..."));

                progress.Report(new KeyValuePair<int, string>(100, "Done!"));
            });
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

            MissingItems.Clear();
            MissingRecipes.Clear();

            craftingCategories.Clear();
            moduleCategories.Clear();
            resourceCategories.Clear();
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

        public static PresetErrorPackage TestPreset(Preset preset, Dictionary<string,string> modList,  List<string> itemList, Dictionary<string, List<RecipeShort>> recipeShortSet)
        {

            string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".json" });
            if (!File.Exists(presetPath))
                return null;

            //parse preset
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

            //compare to provided mod/item/recipe sets
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
            foreach (List<RecipeShort> recipeShortList in recipeShortSet.Values)
            {
                foreach (RecipeShort recipeS in recipeShortList)
                {
                    if (recipeS.isMissing)
                        continue;

                    if (!presetRecipes.ContainsKey(recipeS.Name))
                        errors.MissingRecipes.Add(recipeS.Name);
                    else if (!recipeS.Equals(presetRecipes[recipeS.Name]))
                        errors.IncorrectRecipes.Add(recipeS.Name);
                }
            }
            return errors;
        }

        private void ProcessMod(JToken objJToken)
        {
            includedMods.Add((string)objJToken["name"], (string)objJToken["version"]);
        }


        private void ProcessSubgroup(JToken objJToken)
        {
            Subgroup subgroup = new Subgroup(
                this,
                (string)objJToken["name"],
                (string)objJToken["order"]);

            subgroups.Add(subgroup.Name, subgroup);
        }

        private void ProcessGroup(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            Group group = new Group(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"],
                (string)objJToken["order"]);

            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                group.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
            foreach (var subgroupJToken in objJToken["subgroups"])
                subgroups[(string)subgroupJToken].SetGroup(group);
            groups.Add(group.Name, group);
        }

        private void ProcessItem(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            Item item = new Item(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"],
                false, //item (not a fluid)
                Subgroups[(string)objJToken["subgroup"]],
                (string)objJToken["order"]);

            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                item.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
            items.Add(item.Name, item);
        }

        private void ProcessFluid(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            Item item = new Item(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"],
                true, //fluid
                Subgroups[(string)objJToken["subgroup"]],
                (string)objJToken["order"]);

            item.Temperature = (double)objJToken["default_temperature"];
            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                item.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);
            items.Add(item.Name, item);
        }

        private void ProcessResource(JToken objJToken)
        {
            Resource resource = new Resource(
                this,
                (string)objJToken["name"]);

            resource.Time = (float)objJToken["mining_time"];
            foreach (var productJToken in objJToken["products"])
                if (items.ContainsKey((string)productJToken["name"]))
                    resource.AddResult(items[(string)productJToken["name"]]);
            if (resource.ResultingItems.Count == 0)
                return; //If the resource doesnt actually produce any products, just ignore it.

            string category = (string)objJToken["resource_category"];
            if (!resourceCategories.ContainsKey(category))
                resourceCategories.Add(category, new List<Resource>());
            resourceCategories[category].Add(resource);

            resources.Add(resource.Name, resource);
        }

        private void ProcessMiner(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            Miner miner = new Miner(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"]);

            miner.MiningSpeed = (float)objJToken["mining_speed"];
            miner.ModuleSlots = (int)objJToken["module_inventory_size"];
            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                miner.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

            foreach (var categoryJToken in objJToken["resource_categories"])
                if(resourceCategories.ContainsKey((string)categoryJToken))
                    foreach (Resource resource in resourceCategories[(string)categoryJToken])
                        resource.AddMiner(miner);

            miners.Add(miner.Name, miner);
        }

        private void ProcessOffshorePump(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            Miner miner = new Miner(
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
            Item fluid = items[fluidName];

            //now to add an extra 'resource' if there isnt one with the type of fluid being pumped out here. Once we ensure there is one, we add this miner to it
            string rcName = "fer." + fluid.Name;
            if(!resources.ContainsKey(rcName))
            {
                Resource extraResource = new Resource(this, rcName);
                extraResource.Time = (1f/60); //'mining-speed' of 20, and time of 1/60s (every tick) leads to 1200 water per second. mods can set the mining/pumping-speed.
                extraResource.AddResult(fluid);
                resources.Add(extraResource.Name, extraResource);
            }

            resources[rcName].AddMiner(miner);
            miners.Add(miner.Name, miner);
        }

        private void ProcessRecipe(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            Recipe recipe = new Recipe(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"],
                Subgroups[(string)objJToken["subgroup"]],
                (string)objJToken["order"]);

            recipe.Time = (float)objJToken["energy"] > 0 ? (float)objJToken["energy"] : defaultRecipeTime;
            if ((bool)objJToken["enabled"])
                recipe.AddUnlockTechnology(StartingTech);

            string category = (string)objJToken["category"];
            if (!craftingCategories.ContainsKey(category))
                craftingCategories.Add(category, new List<Recipe>());
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
                    if (items[name].Temperature != temperature)
                        items[name].IsTemperatureDependent = true;
                }

                if (amount != 0)
                    recipe.AddProduct(Items[name], amount, temperature);
            }

            foreach (var ingredientJToken in objJToken["ingredients"].ToList())
            {
                string name = (string)ingredientJToken["name"];
                float amount = (float)ingredientJToken["amount"];

                float minTemp = ((string)ingredientJToken["type"] == "fluid" && ingredientJToken["minimum_temperature"].Type != JTokenType.Null) ? (float)ingredientJToken["minimum_temperature"] : float.NegativeInfinity;
                float maxTemp = ((string)ingredientJToken["type"] == "fluid" && ingredientJToken["maximum_temperature"].Type != JTokenType.Null) ? (float)ingredientJToken["maximum_temperature"] : float.PositiveInfinity;

                if (amount != 0)
                    recipe.AddIngredient(Items[name], amount, minTemp, maxTemp);
            }

            recipes.Add(recipe.Name, recipe);
        }

        private void ProcessModule(JToken objJToken)
        {
            Module module = new Module(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"],
                (float)objJToken["module_effects_speed"],
                (float)objJToken["module_effects_productivity"]);

            foreach (var recipe in objJToken["limitations"])
                if(recipes.ContainsKey((string)recipe)) //only add if the recipe is in the list of recipes (if it isnt, means it was deleted either in data phase of LUA or during foreman export cleanup)
                    recipes[(string)recipe].AddValidModule(module);

            if (objJToken["limitations"].Count() == 0) //means all recipes work
                foreach (Recipe recipe in recipes.Values)
                    recipe.AddValidModule(module);

            string category = (string)objJToken["category"];
            if (!moduleCategories.ContainsKey(category))
                moduleCategories.Add(category, new List<Module>());
            moduleCategories[category].Add(module);

            modules.Add(module.Name, module);
        }

        private void ProcessTechnology(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            Technology technology = new Technology(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"]);

            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                technology.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

            foreach (var recipe in objJToken["recipes"])
                if (recipes.ContainsKey((string)recipe))
                    recipes[(string)recipe].AddUnlockTechnology(technology);

            technologies.Add(technology.Name, technology);
        }

        private void ProcessTechnologyP2(JToken objJToken)
        {
            Technology technology = technologies[(string)objJToken["name"]];
            foreach (var prerequisite in objJToken["prerequisites"])
                if (technologies.ContainsKey((string)prerequisite))
                    technology.AddPrerequisite(technologies[(string)prerequisite]); //if it doesnt, it gets ignored in Factorio
        }

        private void ProcessAssembler(JToken objJToken, Dictionary<string, IconColorPair> iconCache)
        {
            Assembler assembler = new Assembler(
                this,
                (string)objJToken["name"],
                (string)objJToken["localised_name"]);

            assembler.Speed = (float)objJToken["crafting_speed"];
            assembler.ModuleSlots = (int)objJToken["module_inventory_size"];

            if (iconCache.ContainsKey((string)objJToken["icon_name"]))
                assembler.SetIconAndColor(iconCache[(string)objJToken["icon_name"]]);

            foreach (var categoryJToken in objJToken["crafting_categories"])
                if(craftingCategories.ContainsKey((string)categoryJToken))
                    foreach (Recipe recipe in craftingCategories[(string)categoryJToken])
                        recipe.AddValidAssembler(assembler);

            foreach (var effectJToken in objJToken["allowed_effects"])
                if(moduleCategories.ContainsKey((string)effectJToken))
                    foreach (Module module in moduleCategories[(string)effectJToken])
                        assembler.AddValidModule(module);

            assemblers.Add(assembler.Name, assembler);
        }

        //This is used to clean up the items & recipes to those that can actually appear given the settings.
        //A very similar process is done in the 'control.lua' of the foreman mod, but this also processes the enabled assembly machines
        //to further clean up items
        //NOTE: if the FactorioLuaProcessor is used (as opposed to the foreman mod export), then this does the entire job of control.lua in
        //checking researchable tech, removing un-unlockable recipes, removing any items that dont appear in the clean recipe list, etc.
        private void RemoveUnusableItems()
        {
            HashSet<Technology> temp_unlockableTechSet = new HashSet<Technology>();
            bool IsUnlockable(Technology tech) //sets the locked parameter based on unlockability of it & its prerequisites. returns true if unlocked
            {
                if (tech.Locked)
                    return false;
                else if (temp_unlockableTechSet.Contains(tech))
                    return true;
                else if (tech.Prerequisites.Count == 0)
                    return true;
                else
                {
                    bool available = true;
                    foreach (Technology preTech in tech.Prerequisites)
                        available = available && IsUnlockable(preTech);
                    tech.Locked = !available;

                    if (available)
                        temp_unlockableTechSet.Add(tech);
                    return available;
                }
            }

            //step 1: calculate unlockable technologies (either already researched or recursive search to confirm all prerequisites are unlockable / researched)
            List<string> unavailableTech = new List<string>();
            foreach (Technology tech in technologies.Values)
                if (!IsUnlockable(tech))
                    unavailableTech.Add(tech.Name);

            //step 1.5: remove blocked tech (this will also delete some recipes that are no longer able to be unlocked)
            foreach (string techName in unavailableTech)
                DeleteTechnology(technologies[techName]);

            //step 2: delete any recipes with no unlocks (those that had no unlocks in the beginning - those that were part of the removed tech were also removed)
            //step 2.1: also delete any recipes with no viable assembler
            foreach (Recipe recipe in recipes.Values.ToList())
                if (recipe.MyUnlockTechnologies.Count == 0 || recipe.ValidAssemblers.Count == 0)
                    DeleteRecipe(recipe);

            //step 3: try and delete all items (can only delete if item isnt produced/consumed by any recipe, and isnt part of assemblers, miners, modules (that exist in this cache)
            foreach (Item item in items.Values.ToList())
                TryDeleteItem(item); //soft delete

            //step 3.5: clean up those items which are kind of useless (those that are not produced anywhere, and are used in a single recipe that uses only them)
            //this is necessary to take care of those mods that add item destruction and allow for any item (read - all of them) to be destroyed.
            foreach(Item item in items.Values.ToList())
            {
                if(item.ProductionRecipes.Count == 0)
                {
                    bool useful = false;
                    foreach (Recipe recipe in item.ConsumptionRecipes)
                        useful |= (recipe.IngredientList.Count > 1 || recipe.ProductList.Count != 0); //recipe with multiple items coming in or some ingredients coming out -> not an incineration type
                    if (!useful)
                        TryDeleteItem(item, true); //hard delete.
                }
            }

            //step 4: clean up groups and subgroups (basically, clear the entire dictionary and for each recipe & item 'add' their subgroup & group into the dictionary.
            groups.Clear();
            subgroups.Clear();
            foreach (Item item in Items.Values)
                if (!subgroups.ContainsKey(item.MySubgroup.Name))
                    subgroups.Add(item.MySubgroup.Name, item.MySubgroup);
            foreach (Recipe recipe in recipes.Values)
                if (!subgroups.ContainsKey(recipe.MySubgroup.Name))
                    subgroups.Add(recipe.MySubgroup.Name, recipe.MySubgroup);
            foreach (Subgroup subgroup in subgroups.Values)
                if (!groups.ContainsKey(subgroup.MyGroup.Name))
                    groups.Add(subgroup.MyGroup.Name, subgroup.MyGroup);
        }

        private void MarkCyclicRecipes()
        {
            ProductionGraph testGraph = new ProductionGraph(this);
            foreach (Recipe recipe in Recipes.Values)
                RecipeNode.Create(recipe, testGraph);
            testGraph.CreateAllPossibleInputLinks();
            foreach (var scc in testGraph.GetStronglyConnectedComponents(true))
                foreach (var node in scc)
                    ((RecipeNode)node).BaseRecipe.IsCyclic = true;
        }

        private void DeleteTechnology(Technology technology)
        {
            foreach (Technology tech in technology.Prerequisites)
                tech.InternalOneWayRemovePostTech(technology);
            foreach (Technology tech in technology.PostTechs)
                tech.InternalOneWayRemovePrerequisite(technology);
            foreach (Recipe recipe in technology.UnlockedRecipes)
            {
                recipe.InternalOneWayRemoveUnlockTechnology(technology);
                if (recipe.MyUnlockTechnologies.Count == 0)
                    DeleteRecipe(recipe);
            }
            technologies.Remove(technology.Name);
            Console.WriteLine("Deleting technology: " + technology);
        }
        private bool TryDeleteItem(Item item, bool forceDelete = false)
        {
            if(forceDelete) //remove it from every production & consumption recipe, remove recipe if it no longer has any ingredients & products, then proceed.
            {
                Console.WriteLine("FORCEFUL DELETE! Deleting item: " + item);
                foreach (Recipe r in item.ConsumptionRecipes)
                {
                    r.InternalOneWayDeleteIngredient(item);
                    if (r.IngredientList.Count == 0 && r.ProductList.Count == 0)
                        DeleteRecipe(r);
                }
                foreach (Recipe r in item.ProductionRecipes)
                {
                    r.InternalOneWayDeleteProduct(item);
                    if (r.IngredientList.Count == 0 && r.ProductList.Count == 0)
                        DeleteRecipe(r);
                }

                if (assemblers.ContainsKey(item.Name))
                    DeleteAssembler(assemblers[item.Name]);
                if (miners.ContainsKey(item.Name))
                    DeleteMiner(miners[item.Name]);
                if (modules.ContainsKey(item.Name))
                    DeleteModule(modules[item.Name]);
            }

            //can only delete an item if it has no production recipes, consumption recipes, module, assembler, or miner associated with it.
            if( forceDelete || (
                item.ProductionRecipes.Count == 0 && 
                item.ConsumptionRecipes.Count == 0 && 
                !assemblers.ContainsKey(item.Name) &&
                !miners.ContainsKey(item.Name) &&
                !modules.ContainsKey(item.Name)))
            {
                foreach (Resource resource in item.MiningResources)
                {
                    resource.InternalOneWayRemoveResult(item);
                    if (resource.ResultingItems.Count == 0)
                        DeleteResource(resource);
                }

                item.MySubgroup.InternalOneWayRemoveItem(item);
                items.Remove(item.Name);
                if(!forceDelete)
                    Console.WriteLine("Deleting item: " + item);
                return true;
            }
            return false;
        }
        private void DeleteRecipe(Recipe recipe)
        {
            foreach (Item item in recipe.IngredientList)
            {
                item.InternalOneWayRemoveConsumptionRecipe(recipe);
                TryDeleteItem(item);
            }
            foreach (Item item in recipe.ProductList)
            {
                item.InternalOneWayRemoveProductionRecipe(recipe);
                TryDeleteItem(item);
            }
            foreach (Assembler assembler in recipe.ValidAssemblers)
                assembler.InternalOneWayRemoveRecipe(recipe);
            foreach (Module module in recipe.ValidModules)
                module.InternalOneWayRemoveRecipe(recipe);
            foreach (Technology tech in recipe.MyUnlockTechnologies)
                tech.InternalOneWayRemoveRecipe(recipe);

            recipe.MySubgroup.InternalOneWayRemoveRecipe(recipe);
            recipes.Remove(recipe.Name);
            Console.WriteLine("Deleting recipe: " + recipe);
        }
        private void DeleteAssembler(Assembler assembler)
        {
            foreach (Recipe recipe in assembler.ValidRecipes)
            {
                recipe.InternalOneWayRemoveValidAssembler(assembler);
                if (recipe.ValidAssemblers.Count == 0)
                    DeleteRecipe(recipe);
            }
            foreach (Module module in assembler.ValidModules)
                module.InternalOneWayRemoveAssembler(assembler);
            assemblers.Remove(assembler.Name);
            TryDeleteItem(assembler.AssociatedItem);
            Console.WriteLine("Deleting assembler: " + assembler);
        }
        private void DeleteMiner(Miner miner)
        {
            foreach (Resource resource in miner.MineableResources)
            {
                resource.InternalOneWayRemoveMiner(miner);
                if (resource.ValidMiners.Count == 0)
                    DeleteResource(resource);
            }
            miners.Remove(miner.Name);
            TryDeleteItem(miner.AssociatedItem);
            Console.WriteLine("Deleting miner: " + miner);
        }
        private void DeleteResource(Resource resource)
        {
            foreach(Item item in resource.ResultingItems)
                item.InternalOneWayRemoveMiningResource(resource);
            foreach (Miner miner in resource.ValidMiners)
                miner.InternalOneWayRemoveResource(resource);
            resources.Remove(resource.Name);
            Console.WriteLine("Deleting resource: " + resource);
        }
        private void DeleteModule(Module module)
        {
            foreach (Recipe recipe in module.ValidRecipes)
                recipe.InternalOneWayRemoveValidModule(module);
            foreach (Assembler assembler in module.ValidAssemblers)
                assembler.InternalOneWayRemoveValidModule(module);
            modules.Remove(module.Name);
            TryDeleteItem(module.AssociatedItem);
            Console.WriteLine("Deleting module: " + module.Name);
        }
    }
}
