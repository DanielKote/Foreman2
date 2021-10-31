using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Foreman.Properties;
using System.Threading;

namespace Foreman
{
    class JsonDataProcessor
    {
        private Dictionary<string, Technology> Technologies;
        private Dictionary<string, Group> Groups;
        private Dictionary<string, Subgroup> Subgroups;
        private Dictionary<string, Item> Items;
        private Dictionary<string, List<string>> FluidDuplicants;
        private Dictionary<string, Recipe> Recipes;
        private Dictionary<string, List<string>> RecipeDuplicants;
        private Dictionary<string, Assembler> Assemblers;
        private Dictionary<string, Miner> Miners;
        private Dictionary<string, Resource> Resources;
        private Dictionary<string, Module> Modules;

        private Dictionary<string, Exception> FailedFiles;
        private Dictionary<string, Exception> FailedPaths;

        private Dictionary<string, Group> SubgroupToGroupLinks; //used internally to link up subgroup to the correct group
        private const float defaultRecipeTime = 0.5f;

        public IReadOnlyDictionary<string, Technology> GetTechnologies() { return Technologies; }
        public IReadOnlyDictionary<string, Group> GetGroups() { return Groups; }
        public IReadOnlyDictionary<string, Subgroup> GetSubgroups() { return Subgroups; }
        public IReadOnlyDictionary<string, Item> GetItems() { return Items; }
        public IReadOnlyDictionary<string, Recipe> GetRecipes() { return Recipes; }
        public IReadOnlyDictionary<string, Assembler> GetAssemblers() { return Assemblers; }
        public IReadOnlyDictionary<string, Miner> GetMiners() { return Miners; }
        public IReadOnlyDictionary<string, Resource> GetResources() { return Resources; }
        public IReadOnlyDictionary<string, Module> GetModules() { return Modules; }

        public IReadOnlyDictionary<string, Exception> GetFileExceptions() { return FailedFiles; }
        public IReadOnlyDictionary<string, Exception> GetPathExceptions() { return FailedPaths; }

        public JsonDataProcessor()
        {
            Technologies = new Dictionary<string, Technology>();

            Groups = new Dictionary<string, Group>();
            Subgroups = new Dictionary<string, Subgroup>();
            SubgroupToGroupLinks = new Dictionary<string, Group>();

            Items = new Dictionary<string, Item>();
            FluidDuplicants = new Dictionary<string, List<string>>(); //used to store references to the names in items for any extra fluids (ex: coolant *300, coolant *200, coolant *100)

            Recipes = new Dictionary<string, Recipe>();
            RecipeDuplicants = new Dictionary<string, List<string>>(); //same as above - duplicate recipes due to fluid temps

            Assemblers = new Dictionary<string, Assembler>();
            Miners = new Dictionary<string, Miner>();
            Resources = new Dictionary<string, Resource>();
            Modules = new Dictionary<string, Module>();

            FailedFiles = new Dictionary<string, Exception>();
            FailedPaths = new Dictionary<string, Exception>();
        }

        public void LoadData(JObject jsonData, IProgress<KeyValuePair<int, string>> progress, CancellationToken ctoken, int startingPercent, int endingPercent)
        {
            progress.Report(new KeyValuePair<int, string>(startingPercent, "Preparing Icons")); //lets be honest - thats what most of the time is used for here

            //process mods
            List<String> EnabledMods = jsonData["mods"].Select(t => (String)t).ToList();
            foreach (Mod mod in DataCache.Mods)
            {
                mod.Enabled = EnabledMods.Contains(mod.Name);
            }
            List<String> enabledMods = DataCache.Mods.Where(m => m.Enabled).Select(m => m.Name).ToList();

            int totalCount =
                jsonData["technologies"].Count() +
                jsonData["groups"].Count() + 
                //jsonData["subgroups"].Count() + //they dont have any icons, so are extremely fast (and thus can be ignored for the progress bar)
                jsonData["items"].Count() +
                jsonData["fluids"].Count() +
                jsonData["recipes"].Count() +
                jsonData["crafting_machines"].Count() +
                jsonData["miners"].Count() +
                jsonData["resources"].Count() +
                jsonData["modules"].Count();
            int counter = 0;

            foreach (var objJToken in jsonData["technologies"].ToList())
            {
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessTechnology(objJToken);
            }
            foreach (var objJToken in jsonData["groups"].ToList())
            {
                ProcessGroup(objJToken);
            }
            foreach (var objJToken in jsonData["subgroups"].ToList())
            {
                ProcessSubgroup(objJToken);
            }
            foreach (var objJToken in jsonData["items"].ToList())
            {
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount,""));
                ProcessItem(objJToken);
            }
            foreach (var objJToken in jsonData["fluids"].ToList())
            {
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessFluid(objJToken);
            }
            foreach (var objJToken in jsonData["recipes"].ToList())
            {
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessRecipe(objJToken);
            }
            foreach (var objJToken in jsonData["recipes"].ToList())
                ProcessRecipeProducts(objJToken);                   //this is done to add extra 'items' representing the temperature ranges of the liquids
            foreach (var objJToken in jsonData["recipes"].ToList())
                ProcessRecipeIngredients(objJToken);                //with the extra 'items' from above, we can now process ingredients with linking to the liquid of appropriate temp
            foreach (var objJToken in jsonData["technologies"].ToList())
                ProcessTechnologyPassTwo(objJToken);                //adds in the recipes (now that we have added all of them) that are unlocked along with the prerequisite tech (now that all tech has been added)

            foreach (var objJToken in jsonData["crafting_machines"].ToList())
            {
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessAssembler(objJToken);
            }
            foreach (var objJToken in jsonData["miners"].ToList())
            {
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessMiner(objJToken);
            }
            foreach (var objJToken in jsonData["resources"].ToList())
            {
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessResource(objJToken);
            }
            foreach (var objJToken in jsonData["modules"].ToList())
            {
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessModule(objJToken);
            }

            ProcessDuplicates(); //checks if the 'default' item should be removed (ex: nothing creates it and everything that consumes it also consumes a variation of it)

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

            progress.Report(new KeyValuePair<int, string>(endingPercent, "Removing unreachable items"));
            RemoveUnusableItems();
            progress.Report(new KeyValuePair<int, string>(endingPercent, ""));
        }

        private IconColorPair ProcessIcon(JToken iconInfoJToken, int defaultIconSize)
        {
            string mainIconPath = (string)iconInfoJToken["icon"];
            int baseIconSize = (int)iconInfoJToken["icon_size"];

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
            return IconProcessor.GetIconAndColor(iicon, iicons, defaultIconSize);
        }

        private void ProcessTechnology(JToken objJToken)
        {
            Technology technology = new Technology((string)objJToken["name"]);
            technology.LName = (string)objJToken["localised_name"];
            technology.Icon = ProcessIcon(objJToken["icon_info"], 256).Icon;
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
            group.SetIconAndColor(ProcessIcon(objJToken["icon_info"], 64));

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
            Item item = new Item((string)objJToken["name"], (string)objJToken["localised_name"], Subgroups[(string)objJToken["subgroup"]], (string)objJToken["order"]);
            item.SetIconAndColor(ProcessIcon(objJToken["icon_info"], 32));
            Items.Add(item.Name, item);
        }

        private void ProcessFluid(JToken objJToken)
        {
            Item item = new Item((string)objJToken["name"], (string)objJToken["localised_name"], Subgroups[(string)objJToken["subgroup"]], (string)objJToken["order"]);
            item.Temperature = (double)objJToken["default_temperature"];
            item.SetIconAndColor(ProcessIcon(objJToken["icon_info"], 32));
            Items.Add(item.Name, item);
        }

        private void ProcessRecipe(JToken objJToken)
        {
            Recipe recipe = new Recipe((string)objJToken["name"], (string)objJToken["localised_name"], Subgroups[(string)objJToken["subgroup"]], (string)objJToken["order"]);
            recipe.Time = (float)objJToken["energy"] > 0 ? (float)objJToken["energy"] : defaultRecipeTime;
            recipe.Category = (string)objJToken["category"];
            recipe.IsAvailableAtStart = (bool)objJToken["enabled"];
            recipe.SetIconAndColor(ProcessIcon(objJToken["icon_info"], 32));
            Recipes.Add(recipe.Name, recipe);
        }
        private void ProcessRecipeProducts(JToken objJToken)
        {
            Recipe recipe = Recipes[(string)objJToken["name"]];

            foreach (var productJToken in objJToken["products"].ToList())
            {
                if ((float)productJToken["amount"] == 0)
                    continue;

                string name = (string)productJToken["name"];

                if((string)productJToken["type"] == "item" || productJToken["temperature"].Type == JTokenType.Null || Items[name].Temperature == (double)productJToken["temperature"])
                {
                    if (recipe.Results.ContainsKey(Items[name])) //base name double check
                        recipe.Results[Items[name]] += (float)productJToken["amount"];
                    else
                    {
                        recipe.Results.Add(Items[name], (float)productJToken["amount"]);
                        Items[name].ProductionRecipes.Add(recipe);
                    }
                }
                else //this is a fluid with a specified temperature different from the default fluid temperature
                {
                    double temp = (double)productJToken["temperature"];
                    string fluidName = name + String.Format("\n{0:N2}", temp);
                    if (Items.ContainsKey(fluidName))
                    {
                        //the new fluid has already been 'created' by a previous recipe
                        if (recipe.Results.ContainsKey(Items[fluidName]))
                            recipe.Results[Items[fluidName]] += (float)productJToken["amount"];
                        else if ((float)productJToken["amount"] != 0)
                        {
                            recipe.Results.Add(Items[fluidName], (float)productJToken["amount"]);
                            Items[fluidName].ProductionRecipes.Add(recipe);
                        }
                    }
                    else
                    {
                        //we have to add the new fluid
                        if (!FluidDuplicants.ContainsKey(name))
                            FluidDuplicants[name] = new List<string>();
                        FluidDuplicants[name].Add(fluidName);

                        Item defaultFluid = Items[name];
                        Item newFluid = new Item(fluidName, defaultFluid.LName + " (" + temp + "*)", defaultFluid.MySubgroup, defaultFluid.Order+temp);
                        newFluid.Temperature = temp;
                        newFluid.Icon = defaultFluid.Icon;

                        Items.Add(newFluid.Name, newFluid);
                        recipe.Results.Add(Items[newFluid.Name], (float)productJToken["amount"]);
                        newFluid.ProductionRecipes.Add(recipe);
                    }
                }
            }
        }
        private void ProcessRecipeIngredients(JToken objJToken)
        {
            List<Recipe> recipes = new List<Recipe>(); //any situation where multiple fluids fit as an ingredient will require addition of new recipes
            recipes.Add(Recipes[(string)objJToken["name"]]);
            foreach (var ingredientJToken in objJToken["ingredients"].ToList())
            {
                if ((float)ingredientJToken["amount"] == 0)
                    continue;

                string name = (string)ingredientJToken["name"];
                double minTemp = (ingredientJToken["minimum_temperature"] == null || ingredientJToken["minimum_temperature"].Type == JTokenType.Null) ? double.MinValue : (double)ingredientJToken["minimum_temperature"];
                double maxTemp = (ingredientJToken["maximum_temperature"] == null || ingredientJToken["maximum_temperature"].Type == JTokenType.Null) ? double.MaxValue : (double)ingredientJToken["maximum_temperature"];

                Item defaultIngredient = Items[name];
                List<Item> validIngredients = new List<Item>();
                if ((string)ingredientJToken["type"] == "item" || defaultIngredient.Temperature >= minTemp && defaultIngredient.Temperature <= maxTemp)
                    validIngredients.Add(defaultIngredient);
                if (FluidDuplicants.ContainsKey(name)) //have to check for any duplicants and possibly create alternate recipes
                {
                    foreach (string altName in FluidDuplicants[name])
                    {
                        Item altIngredient = Items[altName];
                        if (altIngredient.Temperature >= minTemp && altIngredient.Temperature <= maxTemp)
                            validIngredients.Add(altIngredient);
                    }
                }

                //increase the number of recipes if necessary (each one beyond the first valid ingredient)
                int baseRecipeCount = recipes.Count();
                for (int i = 1; i < validIngredients.Count; i++)
                {
                    if (!RecipeDuplicants.ContainsKey(recipes[0].Name))
                        RecipeDuplicants[recipes[0].Name] = new List<string>();

                    for (int j = 0; j < baseRecipeCount; j++)
                    {
                        Recipe newRecipe = new Recipe(recipes[0].Name + String.Format("\n{0}", i * baseRecipeCount + j + 1), recipes[j].LName, recipes[0].MySubgroup, recipes[0].Order+(i * baseRecipeCount + j + 1));
                        newRecipe.Category = recipes[j].Category;
                        newRecipe.Hidden = recipes[j].Hidden;
                        newRecipe.Time = recipes[j].Time;
                        newRecipe.Icon = recipes[j].Icon;

                        foreach (KeyValuePair<Item, float> kvp in recipes[j].Ingredients)
                            newRecipe.Ingredients.Add(kvp.Key, kvp.Value);
                        foreach (KeyValuePair<Item, float> kvp in recipes[j].Results)
                            newRecipe.Results.Add(kvp.Key, kvp.Value);

                        recipes.Add(newRecipe);
                        Recipes.Add(newRecipe.Name, newRecipe);
                        RecipeDuplicants[recipes[0].Name].Add(newRecipe.Name);
                    }
                }

                //process each ingredient
                for (int i = 0; i < validIngredients.Count; i++)
                {
                    for (int j = 0; j < baseRecipeCount; j++)
                    {
                        Recipe crecipe = recipes[i * baseRecipeCount + j];
                        if (crecipe.Ingredients.ContainsKey(validIngredients[i]))
                            crecipe.Ingredients[validIngredients[i]] += (float)ingredientJToken["amount"];
                        else
                        {
                            crecipe.Ingredients.Add(validIngredients[i], (float)ingredientJToken["amount"]);
                            validIngredients[i].ConsumptionRecipes.Add(crecipe);
                        }
                    }
                }
            }
        }

        private void ProcessAssembler(JToken objJToken)
        {
            Assembler assembler = new Assembler((string)objJToken["name"]);
            assembler.LName = (string)objJToken["localised_name"];
            assembler.Speed = (float)objJToken["crafting_speed"];
            assembler.ModuleSlots = (int)objJToken["module_inventory_size"];
            assembler.Icon = ProcessIcon(objJToken["icon_info"], 32).Icon;

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
            miner.Icon = ProcessIcon(objJToken["icon_info"], 32).Icon;

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
            else //only the specified recipes work. Have to check for duplicate recipes (due to fluid temps)
            {
                List<string> newLimitations = new List<string>();
                foreach(string rname in limitations)
                    if (RecipeDuplicants.ContainsKey(rname))
                        newLimitations.AddRange(RecipeDuplicants[rname]);
            }

            Modules.Add(module.Name, module);
        }

        private void ProcessDuplicates()
        {
            foreach(KeyValuePair<string, List<string>> kvp in FluidDuplicants)
            {
                Item defaultFluid = Items[kvp.Key];
                defaultFluid.LName += " (" + defaultFluid.Temperature + "*)";
                List<Item> altFluids = new List<Item>();
                foreach (string altNames in kvp.Value)
                    altFluids.Add(Items[altNames]);

                bool defaultIsProduced = false;
                foreach(Recipe recipe in Recipes.Values)
                    foreach (Item fluid in recipe.Results.Keys)
                        defaultIsProduced |= (fluid == defaultFluid);

                if(!defaultIsProduced) //nothing produces it, have to check if every consumer has an alternative
                {
                    List<Recipe> defaultUseRecipes = new List<Recipe>();
                    foreach (Recipe recipe in Recipes.Values)
                        foreach (Item item in recipe.Ingredients.Keys)
                            if (item == defaultFluid)
                                defaultUseRecipes.Add(recipe);

                    bool defaultIsUnnecessary = true;
                    foreach (Recipe recipe in defaultUseRecipes)
                    {
                        bool canUseAltFluid = false;
                        if (RecipeDuplicants.ContainsKey(recipe.Name))
                        {
                            foreach(string altRecipeName in RecipeDuplicants[recipe.Name])
                                foreach (Item altFluid in altFluids)
                                    canUseAltFluid |= Recipes[altRecipeName].Ingredients.ContainsKey(altFluid);
                        }
                        if (!canUseAltFluid)
                            defaultIsUnnecessary = false;
                    }

                    if(defaultIsUnnecessary)
                        foreach (Recipe recipe in defaultUseRecipes)
                            Recipes.Remove(recipe.Name);
                }
            }
        }

        //This is used to clean up the items & recipes to those that can actually appear given the settings.
        //A very similar process is done in the 'control.lua' of the foreman mod, but this also processes the enabled assembly machines
        //to further clean up items
        //NOTE: if the FactorioLuaProcessor is used (as opposed to the foreman mod export), then this does the entire job of control.lua in
        //checking researchable tech, removing un-unlockable recipes, removing any items that dont appear in the clean recipe list, etc.
        private HashSet<Technology> temp_unlockableTechSet; //used within IsUnlockable to not have to go through the entire tree for every single item (once an item is set to be 'unlockable' here, it will not require a recursive search again)
        private void RemoveUnusableItems()
        {
            //step 1: calculate unlockable technologies (either already researched or recursive search to confirm all prerequisites are unlockable / researched)
            temp_unlockableTechSet = new HashSet<Technology>();
            List<string> unavailableTech = new List<string>();
            foreach (Technology tech in Technologies.Values)
            {
                if (!IsUnlockable(tech))
                    unavailableTech.Add(tech.Name);
            }
            temp_unlockableTechSet.Clear();
            temp_unlockableTechSet = null;

            //step 1.5: remove blocked tech
            foreach (string techName in unavailableTech)
                Technologies.Remove(techName);

            //step 2: calculate unlockable recipes (those unlocked at start, or unlocked via unlockable tech)
            HashSet<Recipe> unusableRecipes = new HashSet<Recipe>(Recipes.Values);
            foreach (Technology tech in Technologies.Values)
                foreach (Recipe recipe in tech.Recipes)
                    if(!recipe.Name.Contains("\n"))// "\n" in name represents that this is a duplicate recipe made specifically for fluid temperatures. Ignore
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
                if (RecipeDuplicants.ContainsKey(recipe.Name))
                {
                    foreach (string altRecipe in RecipeDuplicants[recipe.Name])
                    {
                        Recipes[altRecipe].MySubgroup.Recipes.Remove(Recipes[altRecipe]);
                        Recipes.Remove(altRecipe);
                    }
                }
            }

            //step 3: calculate unlockable items (ingredient/product of unlockable recipes -> dont care about anything that isnt part of a recipe)
            HashSet<Item> unusableItems = new HashSet<Item>(Items.Values);
            foreach (Recipe recipe in Recipes.Values)
            {
                foreach (Item item in recipe.Ingredients.Keys)
                    unusableItems.Remove(item);
                foreach (Item item in recipe.Results.Keys)
                    unusableItems.Remove(item);
            }
            //step 3.5: remove blocked items
            foreach (Item item in unusableItems)
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

        private bool IsUnlockable(Technology tech) //sets the locked parameter based on unlockability of it & its prerequisites. returns true if unlocked
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
    }
}