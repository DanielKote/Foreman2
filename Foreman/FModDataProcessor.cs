using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Foreman.Properties;
using System.ComponentModel;
using System.Threading;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Foreman
{
    class FModDataProcessor : DataProcessor
    {
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

        private List<Mod> Mods = DataCache.Mods;
        private static string DataPath { get { return Path.Combine(Settings.Default.FactorioPath, "data"); } }
        private static string ModPath { get { return Path.Combine(Settings.Default.FactorioUserDataPath, "mods"); } }
        private static string ScriptOutPath { get { return Path.Combine(Settings.Default.FactorioUserDataPath, "script-output"); } }

        private static readonly int ProgressPrevious = DataReloadForm.ProcessBreakpoints[1];
        private static readonly int ProgressThis = DataReloadForm.ProcessBreakpoints[3] - DataReloadForm.ProcessBreakpoints[1];

        private const float defaultRecipeTime = 0.5f;

        public Dictionary<string, Item> GetItems() { return Items; }
        public Dictionary<string, Recipe> GetRecipes() { return Recipes; }
        public Dictionary<string, Assembler> GetAssemblers() { return Assemblers; }
        public Dictionary<string, Miner> GetMiners() { return Miners; }
        public Dictionary<string, Resource> GetResources() { return Resources; }
        public Dictionary<string, Module> GetModules() { return Modules; }

        public Dictionary<string, Exception> GetFileExceptions() { return FailedFiles; }
        public Dictionary<string, Exception> GetPathExceptions() { return FailedPaths; }

        public FModDataProcessor()
        {
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

        public void LoadData(IProgress<int> progress, CancellationToken ctoken)
        {
            progress.Report(ProgressPrevious);

            //read in the data from the foreman mod (in the script output folder)
            string setupFile = Path.Combine(ScriptOutPath, "ForemanFactorioSetup.txt");
            if (!File.Exists(setupFile))
            {
                MessageBox.Show("The setup file could not be found!\n\nEnsure you have the \"z-z-foremanexport\" mod installed, enabled, and started a new game with it. It should freeze the game for a bit (1sec for vanilla installation, 30sec+ for Angel/Bob with extras) before displaying \"Foreman export complete.\" ");
                progress.Report(100);
                return;
            }
            JObject jsonData = JObject.Parse(File.ReadAllText(setupFile));

            //process mods
            List<String> EnabledMods = jsonData["mods"].Select(t => (String)t).ToList();
            foreach (Mod mod in DataCache.Mods)
            {
                mod.Enabled = EnabledMods.Contains(mod.Name);
            }
            List<String> enabledMods = DataCache.Mods.Where(m => m.Enabled).Select(m => m.Name).ToList();

            int totalCount =
                jsonData["items"].Count() +
                jsonData["fluids"].Count() +
                jsonData["recipes"].Count() +
                jsonData["crafting_machines"].Count() +
                jsonData["miners"].Count() +
                jsonData["resources"].Count() +
                jsonData["modules"].Count();
            int counter = 0;

            foreach (var objJToken in jsonData["items"].ToList())
            {
                progress.Report(ProgressPrevious + ProgressThis * counter++ / totalCount);
                ProcessItem(objJToken);
            }
            foreach (var objJToken in jsonData["fluids"].ToList())
            {
                progress.Report(ProgressPrevious + ProgressThis * counter++ / totalCount);
                ProcessFluid(objJToken);
            }
            foreach (var objJToken in jsonData["recipes"].ToList())
            {
                progress.Report(ProgressPrevious + ProgressThis * counter++ / totalCount);
                ProcessRecipe(objJToken);
            }
            foreach (var objJToken in jsonData["recipes"].ToList())
                ProcessRecipeProducts(objJToken);                   //this is done to add extra 'items' representing the temperature ranges of the liquids
            foreach (var objJToken in jsonData["recipes"].ToList())
                ProcessRecipeIngredients(objJToken);                //with the extra 'items' from above, we can now process ingredients with linking to the liquid of appropriate temp

            foreach (var objJToken in jsonData["crafting_machines"].ToList())
            {
                progress.Report(ProgressPrevious + ProgressThis * counter++ / totalCount);
                ProcessAssembler(objJToken);
            }
            foreach (var objJToken in jsonData["miners"].ToList())
            {
                progress.Report(ProgressPrevious + ProgressThis * counter++ / totalCount);
                ProcessMiner(objJToken);
            }
            foreach (var objJToken in jsonData["resources"].ToList())
            {
                progress.Report(ProgressPrevious + ProgressThis * counter++ / totalCount);
                ProcessResource(objJToken);
            }
            foreach (var objJToken in jsonData["modules"].ToList())
            {
                progress.Report(ProgressPrevious + ProgressThis * counter++ / totalCount);
                ProcessModule(objJToken);
            }

            foreach (String s in Settings.Default.EnabledAssemblers)
            {
                string[] split = s.Split('|');
                if (Assemblers.ContainsKey(split[0]))
                    Assemblers[split[0]].Enabled = (split[1] == "True");
            }
            foreach (String s in Settings.Default.EnabledMiners)
            {
                string[] split = s.Split('|');
                if (Miners.ContainsKey(split[0]))
                    Miners[split[0]].Enabled = (split[1] == "True");
            }
            foreach (String s in Settings.Default.EnabledModules)
            {
                string[] split = s.Split('|');
                if (Modules.ContainsKey(split[0]))
                    Modules[split[0]].Enabled = (split[1] == "True");
            }

            progress.Report(ProgressPrevious + ProgressThis);
        }

        private Bitmap ProcessIcon(JToken iconInfoJToken)
        {
            string mainIconPath = (string)iconInfoJToken["icon"];
            int defaultIconSize = (int)iconInfoJToken["icon_size"];

            IconInfo iicon = new IconInfo(mainIconPath, defaultIconSize);

            List<IconInfo> iicons = new List<IconInfo>();
            List<JToken> iconJTokens = iconInfoJToken["icons"].ToList();
            foreach (var iconJToken in iconJTokens)
            {
                IconInfo picon = new IconInfo((string)iconJToken["icon"], (int)iconJToken["icon_size"]);
                picon.iconScale = (double)iconJToken["scale"];

                picon.iconOffset = new Point((int)iconJToken["shift"][0], (int)iconJToken["shift"][0]);
                picon.SetIconTint((double)iconJToken["tint"][3], (double)iconJToken["tint"][0], (double)iconJToken["tint"][1], (double)iconJToken["tint"][2]);
                iicons.Add(picon);
            }
            return IconProcessor.GetIcon(iicon, iicons);
        }

        private void ProcessItem(JToken objJToken)
        {
            Item item = new Item((string)objJToken["name"]);
            item.LName = (string)objJToken["localised_name"];
            item.Icon = ProcessIcon(objJToken["icon_info"]);
            Items.Add(item.Name, item);
        }

        private void ProcessFluid(JToken objJToken)
        {
            Item item = new Item((string)objJToken["name"]);
            item.LName = (string)objJToken["localised_name"];
            item.Temperature = (double)objJToken["default_temperature"];
            item.Icon = ProcessIcon(objJToken["icon_info"]);
            Items.Add(item.Name, item);
        }

        private void ProcessRecipe(JToken objJToken)
        {
            Recipe recipe = new Recipe((string)objJToken["name"]);
            recipe.LName = (string)objJToken["localised_name"];
            recipe.Time = (float)objJToken["energy"] > 0 ? (float)objJToken["energy"] : defaultRecipeTime;
            recipe.Category = (string)objJToken["category"];
            recipe.Icon = ProcessIcon(objJToken["icon_info"]);
            Recipes.Add(recipe.Name, recipe);
        }
        private void ProcessRecipeProducts(JToken objJToken)
        {
            Recipe recipe = Recipes[(string)objJToken["name"]];

            foreach (var productJToken in objJToken["products"].ToList())
            {
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
                        if (recipe.Results.ContainsKey(Items[fluidName]))
                            recipe.Results[Items[fluidName]] += (float)productJToken["amount"];
                        else
                        {
                            recipe.Results.Add(Items[fluidName], (float)productJToken["amount"]);
                            Items[fluidName].ProductionRecipes.Add(recipe);
                        }
                    }
                    else
                    {
                        if (!FluidDuplicants.ContainsKey(name))
                            FluidDuplicants[name] = new List<string>();
                        FluidDuplicants[name].Add(fluidName);

                        Item defaultFluid = Items[name];
                        Item newFluid = new Item(fluidName);
                        newFluid.LName = defaultFluid.LName + " (" + temp + "*)";
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
                    if (!RecipeDuplicants.ContainsKey(name))
                        RecipeDuplicants[name] = new List<string>();

                    for (int j = 0; j < baseRecipeCount; j++)
                    {
                        Recipe newRecipe = new Recipe(recipes[0].Name + String.Format("\n{0}", i * baseRecipeCount + j + 1));
                        newRecipe.Category = recipes[j].Category;
                        newRecipe.Enabled = recipes[j].Enabled;
                        newRecipe.LName = recipes[j].LName;
                        newRecipe.Time = recipes[j].Time;
                        newRecipe.Icon = recipes[j].Icon;

                        foreach (KeyValuePair<Item, float> kvp in recipes[j].Ingredients)
                            newRecipe.Ingredients.Add(kvp.Key, kvp.Value);
                        foreach (KeyValuePair<Item, float> kvp in recipes[j].Results)
                            newRecipe.Results.Add(kvp.Key, kvp.Value);

                        recipes.Add(newRecipe);
                        Recipes.Add(newRecipe.Name, newRecipe);
                        RecipeDuplicants[name].Add(newRecipe.Name);
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
            Assembler assember = new Assembler((string)objJToken["name"]);
            assember.LName = (string)objJToken["localised_name"];
            assember.Speed = (float)objJToken["crafting_speed"];
            assember.MaxIngredients = (int)objJToken["ingredient_count"];
            assember.ModuleSlots = (int)objJToken["module_inventory_size"];
            assember.Icon = ProcessIcon(objJToken["icon_info"]);

            foreach (var categoryJToken in objJToken["crafting_categories"])
                assember.Categories.Add((string)categoryJToken);
            foreach (var effectJToken in objJToken["allowed_effects"])
                assember.AllowedEffects.Add((string)effectJToken);

            Assemblers.Add(assember.Name, assember);
        }

        private void ProcessMiner(JToken objJToken)
        {
            Miner miner = new Miner((string)objJToken["name"]);
            miner.LName = (string)objJToken["localised_name"];
            miner.MiningPower = (float)objJToken["mining_speed"];
            miner.ModuleSlots = (int)objJToken["module_inventory_size"];
            miner.Icon = ProcessIcon(objJToken["icon_info"]);

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
            List<string> limitations = new List<string>();
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
    }
}