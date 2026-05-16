using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public static class PresetProcessor {
        public static PresetInfo ReadPresetInfo(Preset preset) {
            Dictionary<string, string> mods = new Dictionary<string, string>();
            string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".pjson" });
            if (!File.Exists(presetPath))
                return new PresetInfo(null, false, false);

            try {
                JObject jsonData = JObject.Parse(File.ReadAllText(presetPath));
                foreach (var objJToken in jsonData["mods"]?.AsEnumerable() ?? [])
                    if ((string?)objJToken["name"] is string name && (string?)objJToken["version"] is string version)
                        mods.Add(name, version);
                return new PresetInfo(mods, (int?)jsonData["difficulty"]?[0] == 1, (int?)jsonData["difficulty"]?[1] == 1);
            } catch {
                mods.Clear();
                mods.Add("ERROR READING PRESET!", "");
                return new PresetInfo(mods, false, false);
            }

        }

        public static JObject PrepPreset(Preset preset) {
            string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".pjson" });
            string presetCustomPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".json" });

            JObject jsonData = JObject.Parse(File.ReadAllText(presetPath));
            if (File.Exists(presetCustomPath)) {
                JObject cjsonData = JObject.Parse(File.ReadAllText(presetCustomPath));
                foreach (var groupToken in cjsonData) {
                    foreach (JObject itemToken in groupToken.Value?.Cast<JObject>() ?? []) {
                        var presetItemToken = (JObject?)jsonData[groupToken.Key]?.FirstOrDefault(t => (string?)t["name"] == (string?)itemToken["name"]);
                        if (presetItemToken is not null)
                            foreach (var parameter in itemToken)
                                presetItemToken[parameter.Key] = parameter.Value;
                        else
                            (jsonData[groupToken.Key] as JArray)?.Add(itemToken);
                    }
                }
            }
            return jsonData;
        }

        public static async Task<PresetErrorPackage> TestPreset(Preset preset, Dictionary<string, string> modList, List<string> itemList, List<string> entityList, List<string> qualityList, List<RecipeShort> recipeShorts, List<PlantShort> plantShorts) {
            return await TestPresetStreamlined(preset, modList, itemList, entityList, qualityList, recipeShorts, plantShorts);
        }

        //this preset comparer loads a 'light' version of the preset - basically loading the items and entities as strings only (no data), and only the minimal info for recipes (name, ingredients + amounts, products + amounts)
        //this speeds things up such that the comparison takes around 150ms for a large preset like seablock (10x vanilla), instead of 250ms as for a full datacache load.
        //still, this is only really helpful if you are using 10 presets (1.5 sec load inatead of 2.5 sec) or more, but hey; i will keep it.
        //any changes to preset json style have to be reflected here though (unlike for a full data cache loader above, which just incorporates any changes to data cache as long as they dont impact the outputs)
        private static async Task<PresetErrorPackage> TestPresetStreamlined(Preset preset, Dictionary<string, string> modList, List<string> itemList, List<string> entityList, List<string> qualityList, List<RecipeShort> recipeShorts, List<PlantShort> plantShorts) {
            JObject jsonData = PrepPreset(preset);

            //parse preset (note: this is preset data, so we are guaranteed to only have one name per item/recipe/mod/etc.)
            HashSet<string> presetItems = new HashSet<string>();
            HashSet<string> presetEntities = new HashSet<string>();
            Dictionary<string, RecipeShort> presetRecipes = new Dictionary<string, RecipeShort>();
            Dictionary<string, PlantShort> presetPlantProcesses = new Dictionary<string, PlantShort>();
            Dictionary<string, string> presetMods = new Dictionary<string, string>();
            HashSet<string> presetQualities = new HashSet<string>();

            //built in items
            presetItems.Add("§§i:heat");
            //built in recipes:
            RecipeShort heatRecipe = new RecipeShort("§§r:h:heat-generation");
            heatRecipe.Products.Add("§§i:heat", 1);
            presetRecipes.Add(heatRecipe.Name, heatRecipe);
            RecipeShort burnerRecipe = new RecipeShort("§§r:h:burner-electicity");
            presetRecipes.Add(burnerRecipe.Name, burnerRecipe);
            //built in assemblers:
            presetEntities.Add("§§a:player-assembler");
            presetEntities.Add("§§a:rocket-assembler");

            //read in mods
            foreach (var objJToken in jsonData["mods"]?.AsEnumerable() ?? [])
                if ((string?)objJToken["name"] is string name && (string?)objJToken["version"] is string version)
                    presetMods.Add(name, version);
            //read in items (and their plant results)
            foreach (var objJToken in jsonData["items"]?.AsEnumerable() ?? []) {
                if ((string?)objJToken["name"] is not string name)
                    continue;
                presetItems.Add(name);
                if (objJToken["plant_results"] != null) {
                    PlantShort plantProcess = new PlantShort(name);
                    foreach (var productJToken in objJToken["plant_results"]?.AsEnumerable() ?? []) {
                        double amount = (double?)productJToken["amount"] ?? default;
                        if (amount > 0 && (string?)productJToken["name"] is string productName) {
                            if (plantProcess.Products.ContainsKey(productName))
                                plantProcess.Products[productName] += amount;
                            else
                                plantProcess.Products.Add(productName, amount);
                        }
                    }
                    presetPlantProcesses.Add(plantProcess.Name, plantProcess);
                }
            }
            //read in fluids
            foreach (var objJToken in jsonData["fluids"]?.AsEnumerable() ?? [])
                if ((string?)objJToken["name"] is string name)
                    presetItems.Add(name);
            //read in entities
            foreach (var objJToken in jsonData["entities"]?.AsEnumerable() ?? [])
                if ((string?)objJToken["name"] is string name)
                    presetEntities.Add(name);
            //read in quality data
            foreach (var objJToken in jsonData["qualities"]?.AsEnumerable() ?? [])
                if ((string?)objJToken["name"] is string name)
                    presetQualities.Add(name);

            //read in recipes
            foreach (var objJToken in jsonData["recipes"]?.AsEnumerable() ?? []) {
                if ((string?)objJToken["name"] is not string name)
                    continue;
                RecipeShort recipe = new RecipeShort(name);
                foreach (var ingredientJToken in objJToken["ingredients"]?.AsEnumerable() ?? []) {
                    double amount = (double?)ingredientJToken["amount"] ?? default;
                    if (amount > 0 && (string?)ingredientJToken["name"] is string ingredientName) {
                        if (recipe.Ingredients.ContainsKey(ingredientName))
                            recipe.Ingredients[ingredientName] += amount;
                        else
                            recipe.Ingredients.Add(ingredientName, amount);
                    }
                }
                foreach (var productJToken in objJToken["products"]?.AsEnumerable() ?? []) {
                    double amount = (double?)productJToken["amount"] ?? default;
                    if (amount > 0 && (string?)productJToken["name"] is string productName) {
                        if (recipe.Products.ContainsKey(productName))
                            recipe.Products[productName] += amount;
                        else
                            recipe.Products.Add(productName, amount);
                    }
                }
                presetRecipes.Add(recipe.Name, recipe);
            }

            //have to process mining, generators and boilers (since we convert them to recipes as well)
            foreach (var objJToken in jsonData["resources"]?.AsEnumerable() ?? [])
                AddResourceExtractionRecipe(objJToken, presetRecipes);
            //offshore-pump / water-tile fluids (same pseudo-recipes as DataCache; not listed under "resources")
            foreach (var objJToken in jsonData["water_resources"]?.AsEnumerable() ?? [])
                AddResourceExtractionRecipe(objJToken, presetRecipes);

            foreach (var objJToken in jsonData["entities"]?.AsEnumerable() ?? []) {
                var type = (string?)objJToken["type"];
                if (type == "boiler") {
                    if ((string?)objJToken["fluid_ingredient"] is not string ingredient || (string?)objJToken["fluid_product"] is not string product)
                        continue;

                    double temp = (double?)objJToken["target_temperature"] ?? default;

                    RecipeShort recipe = new RecipeShort(string.Format("§§r:b:{0}:{1}:{2}", ingredient, product, temp.ToString()));
                    recipe.Ingredients.Add(ingredient, 60);
                    recipe.Products.Add(product, 60);

                    if (!presetRecipes.ContainsKey(recipe.Name))
                        presetRecipes.Add(recipe.Name, recipe);
                } else if (type == "generator") {
                    if ((string?)objJToken["fluid_ingredient"] is not string ingredient)
                        continue;

                    double minTemp = (double?)objJToken["minimum_temperature"] ?? double.NaN;
                    double maxTemp = (double?)objJToken["maximum_temperature"] ?? double.NaN;
                    RecipeShort recipe = new RecipeShort(string.Format("§§r:g:{0}:{1}>{2}", ingredient, minTemp, maxTemp));
                    recipe.Ingredients.Add(ingredient, 60);

                    if (!presetRecipes.ContainsKey(recipe.Name))
                        presetRecipes.Add(recipe.Name, recipe);
                }
            }

            //process launch product recipes
            if (presetItems.Contains("rocket-part") && presetRecipes.ContainsKey("rocket-part") && presetEntities.Contains("rocket-silo")) {
                foreach (var objJToken in jsonData["items"]?.Concat(jsonData["fluids"]?.AsEnumerable() ?? []).Where(t => t["launch_products"] is not null) ?? []) {
                    if ((string?)objJToken["name"] is not string name)
                        continue;
                    RecipeShort recipe = new RecipeShort(string.Format("§§r:rl:launch-{0}", name));

                    int inputSize = (int?)objJToken["stack"] ?? default;
                    foreach (var productJToken in objJToken["launch_products"]?.AsEnumerable() ?? []) {
                        double amount = (double?)productJToken["amount"] ?? default;
                        int productStack = (int?)jsonData["items"]?.First(t => (string?)t["name"] == (string?)productJToken["name"])?["stack"] ?? 1;
                        if (amount != 0 && inputSize * amount > productStack)
                            inputSize = (int)(productStack / amount);
                    }
                    foreach (var productJToken in objJToken["launch_products"]?.AsEnumerable() ?? []) {
                        double amount = (double?)productJToken["amount"] ?? default;
                        if (amount != 0 && (string?)productJToken["name"] is string prodName)
                            recipe.Products.Add(prodName, amount * inputSize);
                    }

                    recipe.Ingredients.Add(name, inputSize);
                    recipe.Ingredients.Add("rocket-part", 100);

                    presetRecipes.Add(recipe.Name, recipe);
                }
            }

            //compare to provided mod/item/recipe sets (recipes have a chance of existing in multitudes - aka: missing recipes)
            PresetErrorPackage errors = new PresetErrorPackage(preset);
            foreach (var mod in modList) {
                errors.RequiredMods.Add(mod.Key + "|" + mod.Value);

                if (!presetMods.ContainsKey(mod.Key))
                    errors.MissingMods.Add(mod.Key + "|" + mod.Value);
                else if (presetMods[mod.Key] != mod.Value)
                    errors.WrongVersionMods.Add(mod.Key + "|" + mod.Value + "|" + presetMods[mod.Key]);
            }
            foreach (var mod in presetMods)
                if (!modList.ContainsKey(mod.Key))
                    errors.AddedMods.Add(mod.Key + "|" + mod.Value);

            foreach (string itemName in itemList) {
                errors.RequiredItems.Add(itemName);

                if (!presetItems.Contains(itemName))
                    errors.MissingItems.Add(itemName);
            }

            foreach (RecipeShort recipeS in recipeShorts) {
                errors.RequiredRecipes.Add(recipeS.Name);
                if (recipeS.isMissing) {
                    if (presetRecipes.ContainsKey(recipeS.Name) && recipeS.Equals(presetRecipes[recipeS.Name]))
                        errors.ValidMissingRecipes.Add(recipeS.Name);
                    else
                        errors.IncorrectRecipes.Add(recipeS.Name);
                } else {
                    if (!presetRecipes.ContainsKey(recipeS.Name))
                        errors.MissingRecipes.Add(recipeS.Name);
                    else if (!recipeS.Equals(presetRecipes[recipeS.Name]))
                        errors.IncorrectRecipes.Add(recipeS.Name);
                }
            }

            foreach (PlantShort plantS in plantShorts) {
                errors.RequiredPlanting.Add(plantS.Name);
                if (plantS.isMissing) {
                    if (presetPlantProcesses.ContainsKey(plantS.Name) && plantS.Equals(presetPlantProcesses[plantS.Name]))
                        errors.ValidMissingPlanting.Add(plantS.Name);
                    else
                        errors.IncorrectPlanting.Add(plantS.Name);
                } else {
                    if (!presetPlantProcesses.ContainsKey(plantS.Name))
                        errors.MissingPlanting.Add(plantS.Name);
                    else if (!plantS.Equals(presetPlantProcesses[plantS.Name]))
                        errors.IncorrectPlanting.Add(plantS.Name);
                }
            }

            foreach (string qualityName in qualityList) {
                errors.RequiredQualities.Add(qualityName);

                if (!presetQualities.Contains(qualityName))
                    errors.MissingQualities.Add(qualityName);
            }
            return errors;
        }

        private static void AddResourceExtractionRecipe(JToken objJToken, Dictionary<string, RecipeShort> presetRecipes) {
            if (objJToken["products"] is not JArray products || products.Count == 0)
                return;
            if ((string?)objJToken["name"] is not string name)
                return;

            RecipeShort recipe = new RecipeShort("§§r:e:" + name);

            foreach (var productJToken in objJToken["products"]?.AsEnumerable() ?? []) {
                double amount = (double?)productJToken["amount"] ?? default;
                if (amount > 0 && (string?)productJToken["name"] is string productName) {
                    if (recipe.Products.ContainsKey(productName))
                        recipe.Products[productName] += amount;
                    else
                        recipe.Products.Add(productName, amount);
                }
            }
            if (recipe.Products.Count == 0)
                return;

            if ((string?)objJToken["required_fluid"] is string reqFluid && (double?)objJToken["fluid_amount"] is double fluidAmnt && fluidAmnt != 0)
                recipe.Ingredients.Add(reqFluid, fluidAmnt);

            presetRecipes.Add(recipe.Name, recipe);
        }
    }
}