using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Newtonsoft.Json.Linq;
using Foreman.Properties;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Foreman
{
    class JsonDataProcessor
    {
        public List<string> IncludedMods;
        public Dictionary<string, Technology> Technologies;
        public Dictionary<string, Group> Groups;
        public Dictionary<string, Subgroup> Subgroups;
        public Dictionary<string, Item> Items;
        public Dictionary<string, Recipe> Recipes;
        public Dictionary<string, Assembler> Assemblers;
        public Dictionary<string, Miner> Miners;
        public Dictionary<string, Resource> Resources;
        public Dictionary<string, Module> Modules;

        private Dictionary<string, Group> SubgroupToGroupLinks; //used internally to link up subgroup to the correct group
        private const float defaultRecipeTime = 0.5f;

        private Dictionary<int, IconColorPair> IconCache; //icons are generated first and loaded into this cache

        public JsonDataProcessor()
        {
            IncludedMods = new List<string>();

            Technologies = new Dictionary<string, Technology>();

            Groups = new Dictionary<string, Group>();
            Subgroups = new Dictionary<string, Subgroup>();
            SubgroupToGroupLinks = new Dictionary<string, Group>();

            Items = new Dictionary<string, Item>();
            Recipes = new Dictionary<string, Recipe>();
            Assemblers = new Dictionary<string, Assembler>();
            Miners = new Dictionary<string, Miner>();
            Resources = new Dictionary<string, Resource>();
            Modules = new Dictionary<string, Module>();

            IconCache = new Dictionary<int, IconColorPair>();
        }

        public void LoadData(JObject jsonData, IProgress<KeyValuePair<int, string>> progress, CancellationToken ctoken, int startingPercent, int endingPercent)
        {
            //process mods (just add the names of enabled mods to the list - this is just the set of mods enabled during this particular snapshot of factorio data to json)
            Settings.Default.EnabledMods.Clear();
            foreach (string mod in jsonData["mods"].Select(t => (string)t))
            {
                Settings.Default.EnabledMods.Add(mod);
                IncludedMods.Add(mod);
            }

            //if icon cache is empty, we need to process icons. if it isnt, then we assume it has been loaded (and if it hasnt / is wrong, well - thats what 'reload' is there for)
            if (IconCache.Count == 0)
            {
                progress.Report(new KeyValuePair<int, string>(startingPercent, "Preparing Icons")); //lets be honest - thats what most of the time is used for here
                int totalCount = jsonData["icons"].Count();
                int counter = 0;
                foreach (var objJToken in jsonData["icons"].ToList())
                {
                    progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                    ProcessIcon(objJToken);
                }
            }

            progress.Report(new KeyValuePair<int, string>(endingPercent, "Creating Data...")); //yea, I gave it all of 0% to complete this entire part. its under 0.1s even for A&B...
            foreach (var objJToken in jsonData["technologies"].ToList())
                ProcessTechnology(objJToken);
            foreach (var objJToken in jsonData["groups"].ToList())
                ProcessGroup(objJToken);
            foreach (var objJToken in jsonData["subgroups"].ToList())
                ProcessSubgroup(objJToken);
            foreach (var objJToken in jsonData["items"].ToList())
                ProcessItem(objJToken);
            foreach (var objJToken in jsonData["fluids"].ToList())
                ProcessFluid(objJToken);
            foreach (var objJToken in jsonData["recipes"].ToList())
                ProcessRecipe(objJToken);
            foreach (var objJToken in jsonData["technologies"].ToList())
                ProcessTechnologyPassTwo(objJToken);                //adds in the recipes (now that we have added all of them) that are unlocked along with the prerequisite tech (now that all tech has been added)
            foreach (var objJToken in jsonData["crafting_machines"].ToList())
                ProcessAssembler(objJToken);
            foreach (var objJToken in jsonData["miners"].ToList())
                ProcessMiner(objJToken);
            foreach (var objJToken in jsonData["resources"].ToList())
                ProcessResource(objJToken);
            foreach (var objJToken in jsonData["modules"].ToList())
                ProcessModule(objJToken);

            //sort
            foreach (Group g in Groups.Values)
                g.SortSubgroups();
            foreach (Subgroup sg in Subgroups.Values)
                sg.SortIRs();

            //enable/disable based on settings
            foreach (string s in Settings.Default.EnabledAssemblers)
                if (Assemblers.ContainsKey(s))
                    Assemblers[s].Enabled = true;
            foreach (string s in Settings.Default.EnabledMiners)
                if (Miners.ContainsKey(s))
                    Miners[s].Enabled = true;
            foreach (string s in Settings.Default.EnabledModules)
                if (Modules.ContainsKey(s))
                    Modules[s].Enabled = true;

            RemoveUnusableItems();

            progress.Report(new KeyValuePair<int, string>(endingPercent, ""));
        }

        private void ProcessIcon(JToken objJToken)
        {
            int iconIndex = (int)objJToken["icon_id"];
            IconColorPair iconData = new IconColorPair(null, Color.Black);

            if (objJToken["icon_info"].Type != JTokenType.Null)
            {
                JToken iconInfoJToken = objJToken["icon_info"];

                string mainIconPath = (string)iconInfoJToken["icon"];
                int baseIconSize = (int)iconInfoJToken["icon_size"];
                int defaultIconSize = (int)iconInfoJToken["icon_dsize"];

                IconInfo iicon = new IconInfo(mainIconPath, baseIconSize);

                List<IconInfo> iicons = new List<IconInfo>();
                List<JToken> iconJTokens = iconInfoJToken["icons"].ToList();
                foreach (var iconJToken in iconJTokens)
                {
                    IconInfo picon = new IconInfo((string)iconJToken["icon"], (int)iconJToken["icon_size"]);
                    picon.iconScale = (double)iconJToken["scale"];

                    picon.iconOffset = new Point((int)iconJToken["shift"][0], (int)iconJToken["shift"][1]);
                    picon.SetIconTint((double)iconJToken["tint"][3], (double)iconJToken["tint"][0], (double)iconJToken["tint"][1], (double)iconJToken["tint"][2]);
                    iicons.Add(picon);
                }
                iconData = IconProcessor.GetIconAndColor(iicon, iicons, defaultIconSize);
            }
            IconCache.Add(iconIndex, iconData);
        }

        private void ProcessTechnology(JToken objJToken)
        {
            Technology technology = new Technology((string)objJToken["name"]);
            technology.LName = (string)objJToken["localised_name"];
            if (IconCache.ContainsKey((int)objJToken["icon_id"]))
                technology.Icon = IconCache[(int)objJToken["icon_id"]].Icon;
            Technologies.Add(technology.Name, technology);
        }

        private void ProcessTechnologyPassTwo(JToken objJToken)
        {
            Technology technology = Technologies[(string)objJToken["name"]];

            foreach (var recipe in objJToken["recipes"])
                if (Recipes.ContainsKey((string)recipe))
                    technology.Recipes.Add(Recipes[(string)recipe]);

            foreach (var prerequisite in objJToken["prerequisites"])
            {
                if (Technologies.ContainsKey((string)prerequisite))
                    technology.Prerequisites.Add(Technologies[(string)prerequisite]);
                else
                    technology.Locked = true; //if the required prerequisite isnt in the list, then this tech cant be researched
            }
        }

        private void ProcessGroup(JToken objJToken)
        {
            Group group = new Group((string)objJToken["name"], (string)objJToken["localised_name"], (string)objJToken["order"]);
            if (IconCache.ContainsKey((int)objJToken["icon_id"]))
                group.SetIconAndColor(IconCache[(int)objJToken["icon_id"]]);
            foreach (var subgroupJToken in objJToken["subgroups"])
                SubgroupToGroupLinks.Add((string)subgroupJToken, group);
            Groups.Add(group.Name, group);
        }

        private void ProcessSubgroup(JToken objJToken)
        {
            Subgroup subgroup = new Subgroup((string)objJToken["name"], SubgroupToGroupLinks[(string)objJToken["name"]], (string)objJToken["order"]);
            Subgroups.Add(subgroup.Name, subgroup);
        }

        private void ProcessItem(JToken objJToken)
        {
            Item item = new Item((string)objJToken["name"], (string)objJToken["localised_name"], false, Subgroups[(string)objJToken["subgroup"]], (string)objJToken["order"]);
            if (IconCache.ContainsKey((int)objJToken["icon_id"]))
                item.SetIconAndColor(IconCache[(int)objJToken["icon_id"]]);
            Items.Add(item.Name, item);
        }

        private void ProcessFluid(JToken objJToken)
        {
            Item item = new Item((string)objJToken["name"], (string)objJToken["localised_name"], true, Subgroups[(string)objJToken["subgroup"]], (string)objJToken["order"]);
            item.Temperature = (double)objJToken["default_temperature"];
            if (IconCache.ContainsKey((int)objJToken["icon_id"]))
                item.SetIconAndColor(IconCache[(int)objJToken["icon_id"]]);
            Items.Add(item.Name, item);
        }

        private void ProcessRecipe(JToken objJToken)
        {
            Recipe recipe = new Recipe((string)objJToken["name"], (string)objJToken["localised_name"], Subgroups[(string)objJToken["subgroup"]], (string)objJToken["order"]);
            recipe.Time = (float)objJToken["energy"] > 0 ? (float)objJToken["energy"] : defaultRecipeTime;
            recipe.Category = (string)objJToken["category"];
            recipe.IsAvailableAtStart = (bool)objJToken["enabled"];
            if (IconCache.ContainsKey((int)objJToken["icon_id"]))
                recipe.SetIconAndColor(IconCache[(int)objJToken["icon_id"]]);

            foreach (var productJToken in objJToken["products"].ToList())
            {
                string name = (string)productJToken["name"];
                float amount = (float)productJToken["amount"];
                float temperature = 0; // ((string)productJToken["type"] == "fluid" && productJToken["temperature"].Type != JTokenType.Null) ? (float)productJToken["temperature"] : 0;
                if((string)productJToken["type"] == "fluid" && productJToken["temperature"].Type != JTokenType.Null)
                {
                    temperature = (float)productJToken["temperature"];
                    if (Items[name].Temperature != temperature)
                        Items[name].IsTemperatureDependent = true;
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

            Recipes.Add(recipe.Name, recipe);
        }

        private void ProcessAssembler(JToken objJToken)
        {
            Assembler assembler = new Assembler((string)objJToken["name"]);
            assembler.LName = (string)objJToken["localised_name"];
            assembler.Speed = (float)objJToken["crafting_speed"];
            assembler.ModuleSlots = (int)objJToken["module_inventory_size"];
            if (IconCache.ContainsKey((int)objJToken["icon_id"]))
                assembler.Icon = IconCache[(int)objJToken["icon_id"]].Icon;

            foreach (var categoryJToken in objJToken["crafting_categories"])
                assembler.Categories.Add((string)categoryJToken);
            foreach (var effectJToken in objJToken["allowed_effects"])
                assembler.AllowedEffects.Add((string)effectJToken);

            Assemblers.Add(assembler.Name, assembler);
        }

        private void ProcessMiner(JToken objJToken)
        {
            Miner miner = new Miner((string)objJToken["name"]);
            miner.LName = (string)objJToken["localised_name"];
            miner.MiningPower = (float)objJToken["mining_speed"];
            miner.ModuleSlots = (int)objJToken["module_inventory_size"];
            if (IconCache.ContainsKey((int)objJToken["icon_id"]))
                miner.Icon = IconCache[(int)objJToken["icon_id"]].Icon;

            foreach (var categoryJToken in objJToken["resource_categories"])
                miner.ResourceCategories.Add((string)categoryJToken);

            Miners.Add(miner.Name, miner);
        }

        private void ProcessResource(JToken objJToken)
        {
            Resource resource = new Resource((string)objJToken["name"]);
            resource.Category = (string)objJToken["resource_category"];
            resource.Time = (float)objJToken["mining_time"];
            resource.Hardness = 0.5f;
            resource.result = (string)objJToken["products"][0]["name"];

            Resources.Add(resource.Name, resource);
        }

        private void ProcessModule(JToken objJToken)
        {
            HashSet<string> limitations = new HashSet<string>();
            Module module = new Module(
                (string)objJToken["name"],
                (float)objJToken["module_effects_speed"],
                (float)objJToken["module_effects_productivity"],
                limitations);
            module.LName = (string)objJToken["localised_name"];

            foreach (var recipe in objJToken["limitations"])
                limitations.Add((string)recipe);

            if (limitations.Count == 0) //means all recipes work
            {
                foreach (Recipe recipe in Recipes.Values)
                    limitations.Add(recipe.Name);
            }

            Modules.Add(module.Name, module);
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
            foreach (Technology tech in Technologies.Values)
                if (!IsUnlockable(tech))
                    unavailableTech.Add(tech.Name);

            //step 1.5: remove blocked tech
            foreach (string techName in unavailableTech)
                Technologies.Remove(techName);

            //step 2: calculate unlockable recipes (those unlocked at start, or unlocked via unlockable tech)
            HashSet<Recipe> unusableRecipes = new HashSet<Recipe>(Recipes.Values);
            foreach (Technology tech in Technologies.Values)
                foreach (Recipe recipe in tech.Recipes)
                    unusableRecipes.Remove(recipe);
            foreach (Recipe recipe in Recipes.Values)
                if (recipe.IsAvailableAtStart)
                    unusableRecipes.Remove(recipe);
            //step 2.1: also remove any recipes that dont have ANY assembler that fits
            foreach (Recipe recipe in Recipes.Values)
            {
                bool usable = false;
                foreach (Assembler assembler in Assemblers.Values)
                    usable |= assembler.Categories.Contains(recipe.Category);
                if (!usable)
                    unusableRecipes.Add(recipe);
            }
            //step 2.5: remove blocked recipe (those not part of a tech or unlocked at start), including any alt-recipes of it (due to fluid temps)
            foreach (Recipe recipe in unusableRecipes)
            {
                recipe.MySubgroup.Recipes.Remove(recipe);
                Recipes.Remove(recipe.Name);
                foreach (Item ingredient in recipe.IngredientList.ToList())
                    recipe.DeleteIngredient(ingredient);
                foreach (Item product in recipe.ProductList.ToList())
                    recipe.DeleteProduct(product);
            }

            //step 3: remove any items that arent a part of a recipe (either ingredient or product) -> we dont care about any item that is alone
            foreach (Item item in Items.Values.Where(i => i.ConsumptionRecipes.Count == 0 && i.ProductionRecipes.Count == 0).ToList())
            {
                item.MySubgroup.Items.Remove(item);
                Items.Remove(item.Name);
            }

            //step 4: clean up tech tree (removing any recipes from the unlocks that are no longer present in recipe set)
            foreach(Technology tech in Technologies.Values)
                tech.Recipes.IntersectWith(Recipes.Values);

            //step 5: clean up groups and subgroups (basically, clear the entire dictionary and for each recipe & item 'add' their subgroup & group into the dictionary.
            Groups.Clear();
            Subgroups.Clear();
            foreach (Item item in Items.Values)
            {
                if (!Subgroups.ContainsKey(item.MySubgroup.Name))
                    Subgroups.Add(item.MySubgroup.Name, item.MySubgroup);
            }
            foreach (Recipe recipe in Recipes.Values)
                if (!Subgroups.ContainsKey(recipe.MySubgroup.Name))
                    Subgroups.Add(recipe.MySubgroup.Name, recipe.MySubgroup);
            foreach (Subgroup subgroup in Subgroups.Values)
                if (!Groups.ContainsKey(subgroup.MyGroup.Name))
                    Groups.Add(subgroup.MyGroup.Name, subgroup.MyGroup);
        }
        /*
        private void SaveIconData()
        {
            IconBitmapCollection iCollection = new IconBitmapCollection();

            foreach(Technology tech in Technologies.Values)
            {
                iCollection.IconCollection.Add*
            }
        }*/


        public void LoadIconCache(JObject jsonData, IProgress<KeyValuePair<int, string>> progress, CancellationToken ctoken, int startingPercent, int endingPercent)
        {

        }

        public void SaveIconCache(string path)
        {
            IconBitmapCollection iCollection = new IconBitmapCollection();

            foreach (KeyValuePair<int, IconColorPair> iconKVP in IconCache)
                iCollection.Icons.Add(iconKVP.Key, iconKVP.Value);

            if (File.Exists(path))
                File.Delete(path);
            using (Stream stream = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, iCollection);
            }
        }

        public bool LoadIconCache(string path)
        { 
            IconCache.Clear();
            try
            {
                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    var binaryFormatter = new BinaryFormatter();
                    IconBitmapCollection iCollection = (IconBitmapCollection)binaryFormatter.Deserialize(stream);

                    foreach (KeyValuePair<int, IconColorPair> iconKVP in iCollection.Icons)
                        IconCache.Add(iconKVP.Key, iconKVP.Value);
                }
                return true;
            }
            catch //there was an error reading the cache. Just ignore it and continue (we will have to load the icons from the files directly)
            {
                if (File.Exists(path))
                    File.Delete(path);

                IconCache.Clear();
                return false;
            }
        }
    }
}