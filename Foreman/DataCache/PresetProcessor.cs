using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public static class PresetProcessor {
        public static PresetInfo ReadPresetInfo(Preset preset) {
            var mods = new Dictionary<string, string>();
            var presetPath = Path.Combine([Application.StartupPath, "Presets", preset.Name + ".pjson"]);
            if (!File.Exists(presetPath))
                return new PresetInfo(null, false, false);

            try {
                var jsonData = JObject.Parse(File.ReadAllText(presetPath));
                foreach (var objJToken in jsonData["mods"].ToList())
                    mods.Add((string) objJToken["name"], (string) objJToken["version"]);
                return new PresetInfo(mods, (int) jsonData["difficulty"][0] == 1, (int) jsonData["difficulty"][1] == 1);
            } catch {
                mods.Clear();
                mods.Add("ERROR READING PRESET!", "");
                return new PresetInfo(mods, false, false);
            }
        }

        public static JObject PrepPreset(Preset preset) {
            var presetPath = Path.Combine([Application.StartupPath, "Presets", preset.Name + ".pjson"]);
            var presetCustomPath = Path.Combine([Application.StartupPath, "Presets", preset.Name + ".json"]);

            var jsonData = JObject.Parse(File.ReadAllText(presetPath));
            if (!File.Exists(presetCustomPath))
                return
                    jsonData;

            var cjsonData = JObject.Parse(File.ReadAllText(presetCustomPath));
            foreach (var groupToken in cjsonData) {
                foreach (var itemToken in groupToken.Value.Cast<JObject>()) {
                    var presetItemToken = (JObject) jsonData[groupToken.Key].FirstOrDefault(t => (string) t["name"] == (string) itemToken["name"]);
                    if (presetItemToken != null)
                        foreach (var parameter in itemToken)
                            presetItemToken[parameter.Key] = parameter.Value;
                    else
                        ((JArray) jsonData[groupToken.Key]).Add(itemToken);
                }
            }

            return jsonData;
        }

        public static async Task<PresetErrorPackage> TestPreset(Preset preset, Dictionary<string, string> modList, List<string> itemList,
            List<string> entityList, List<string> qualityList, List<RecipeShort> recipeShorts, List<PlantShort> plantShorts) {
            return await TestPresetStreamlined(preset, modList, itemList, entityList, qualityList, recipeShorts, plantShorts);
        }

        // full load of data cache and comparison. This is naturally slower than the streamlined version,
        // since we load all the extras that aren't necessary for comparison (like energy types, technologies, availability calculations, etc.)
        // but on the +ve side any changes to preset json format is incorporated into data cache and requires no update to this function.
        private static async Task<PresetErrorPackage> TestPresetThroughDataCache(Preset preset, Dictionary<string, string> modList, List<string> itemList,
            List<string> entityList, List<string> qualityList, List<RecipeShort> recipeShorts, List<PlantShort> plantShorts) {
            var presetPath = Path.Combine([Application.StartupPath, "Presets", preset.Name + ".pjson"]);
            if (!File.Exists(presetPath))
                return null;

            var presetCache = new DataCache(Properties.Settings.Default.UseRecipeBWfilters);
            await presetCache.LoadAllData(preset, null, false);

            // compare to provided mod/item/recipe sets (recipes have a chance of existing in multitudes - aka: missing recipes)

            var errors = new PresetErrorPackage(preset);
            foreach (var mod in modList) {
                errors.RequiredMods.Add(mod.Key + "|" + mod.Value);

                if (!presetCache.IncludedMods.ContainsKey(mod.Key))
                    errors.MissingMods.Add(mod.Key + "|" + mod.Value);
                else if (presetCache.IncludedMods[mod.Key] != mod.Value)
                    errors.WrongVersionMods.Add(mod.Key + "|" + mod.Value + "|" + presetCache.IncludedMods[mod.Key]);
            }

            foreach (var mod in presetCache.IncludedMods)
                if (!modList.ContainsKey(mod.Key))
                    errors.AddedMods.Add(mod.Key + "|" + mod.Value);

            foreach (var itemName in itemList) {
                errors.RequiredItems.Add(itemName);

                if (!presetCache.Items.ContainsKey(itemName))
                    errors.MissingItems.Add(itemName);
            }

            foreach (var recipeS in recipeShorts) {
                errors.RequiredRecipes.Add(recipeS.Name);
                if (recipeS.isMissing) {
                    if (presetCache.Recipes.ContainsKey(recipeS.Name) && recipeS.Equals(presetCache.Recipes[recipeS.Name]))
                        errors.ValidMissingRecipes.Add(recipeS.Name);
                    else
                        errors.IncorrectRecipes.Add(recipeS.Name);
                } else {
                    if (!presetCache.Recipes.ContainsKey(recipeS.Name))
                        errors.MissingRecipes.Add(recipeS.Name);
                    else if (!recipeS.Equals(presetCache.Recipes[recipeS.Name]))
                        errors.IncorrectRecipes.Add(recipeS.Name);
                }
            }

            foreach (var plantS in plantShorts) {
                errors.RequiredPlanting.Add(plantS.Name);
                if (plantS.isMissing) {
                    if (presetCache.PlantProcesses.ContainsKey(plantS.Name) && plantS.Equals(presetCache.PlantProcesses[plantS.Name]))
                        errors.ValidMissingPlanting.Add(plantS.Name);
                    else
                        errors.IncorrectPlanting.Add(plantS.Name);
                } else {
                    if (!presetCache.PlantProcesses.ContainsKey(plantS.Name))
                        errors.MissingPlanting.Add(plantS.Name);
                    else if (!plantS.Equals(presetCache.PlantProcesses[plantS.Name]))
                        errors.IncorrectPlanting.Add(plantS.Name);
                }
            }

            foreach (var qualityName in qualityList) {
                errors.RequiredQualities.Add(qualityName);

                if (!presetCache.Qualities.ContainsKey(qualityName))
                    errors.MissingQualities.Add(qualityName);
            }

            return errors;
        }

        // this preset comparer loads a 'light' version of the preset - basically loading the items and entities as strings only (no data),
        // and only the minimal info for recipes (name, ingredients + amounts, products + amounts)
        // this speeds things up such that the comparison takes around 150ms for a large preset like seablock (10x vanilla),
        // instead of 250ms as for a full datacache load.
        // still, this is only really helpful if you are using 10 presets (1.5 sec load instead of 2.5 sec) or more,
        // but hey; I will keep it.
        // any changes to preset json style have to be reflected here though
        // (unlike for a full data cache loader above, which just incorporates any changes to data cache as long as they don't impact the outputs)
        private static async Task<PresetErrorPackage> TestPresetStreamlined(Preset preset, Dictionary<string, string> modList, List<string> itemList,
            List<string> entityList, List<string> qualityList, List<RecipeShort> recipeShorts, List<PlantShort> plantShorts) {
            var jsonData = PrepPreset(preset);

            // parse preset (note: this is preset data, so we are guaranteed to only have one name per item/recipe/mod/etc.)

            var presetItems = new HashSet<string>();
            var presetEntities = new HashSet<string>();
            var presetRecipes = new Dictionary<string, RecipeShort>();
            var presetPlantProcesses = new Dictionary<string, PlantShort>();
            var presetQualities = new HashSet<string>();

            // built in items

            presetItems.Add("§§i:heat");

            // built in recipes:

            var heatRecipe = new RecipeShort("§§r:h:heat-generation");
            heatRecipe.Products.Add("§§i:heat", 1);
            presetRecipes.Add(heatRecipe.Name, heatRecipe);
            var burnerRecipe = new RecipeShort("§§r:h:burner-electricity");
            presetRecipes.Add(burnerRecipe.Name, burnerRecipe);

            // built in assemblers:

            presetEntities.Add("§§a:player-assembler");
            presetEntities.Add("§§a:rocket-assembler");

            // read in mods

            var presetMods = jsonData["mods"].ToList()
                .ToDictionary(objJToken => (string) objJToken["name"], objJToken => (string) objJToken["version"]);

            // read in items (and their plant results)

            foreach (var objJToken in jsonData["items"].ToList()) {
                presetItems.Add((string) objJToken["name"]);
                if (objJToken["plant_results"] == null)
                    continue;

                var plantProcess = new PlantShort((string) objJToken["name"]);
                foreach (var productJToken in objJToken["plant_results"]) {
                    var amount = (double) productJToken["amount"];
                    if (amount <= 0)
                        continue;

                    var productName = (string) productJToken["name"];
                    if (plantProcess.Products.ContainsKey(productName))
                        plantProcess.Products[productName] += amount;
                    else
                        plantProcess.Products.Add(productName, amount);
                }

                presetPlantProcesses.Add(plantProcess.Name, plantProcess);
            }

            // read in fluids

            foreach (var objJToken in jsonData["fluids"].ToList())
                presetItems.Add((string) objJToken["name"]);

            // read in entities

            foreach (var objJToken in jsonData["entities"].ToList())
                presetEntities.Add((string) objJToken["name"]);

            // read in quality data

            foreach (var objJToken in jsonData["qualities"].ToList())
                presetQualities.Add((string) objJToken["name"]);

            // read in recipes

            foreach (var objJToken in jsonData["recipes"].ToList()) {
                var recipe = new RecipeShort((string) objJToken["name"]);
                foreach (var ingredientJToken in objJToken["ingredients"].ToList()) {
                    var amount = (double) ingredientJToken["amount"];
                    if (amount <= 0)
                        continue;

                    var ingredientName = (string) ingredientJToken["name"];
                    if (recipe.Ingredients.ContainsKey(ingredientName))
                        recipe.Ingredients[ingredientName] += amount;
                    else
                        recipe.Ingredients.Add(ingredientName, amount);
                }

                foreach (var productJToken in objJToken["products"].ToList()) {
                    var amount = (double) productJToken["amount"];
                    if (amount <= 0)
                        continue;

                    var productName = (string) productJToken["name"];
                    if (recipe.Products.ContainsKey(productName))
                        recipe.Products[productName] += amount;
                    else
                        recipe.Products.Add(productName, amount);
                }

                presetRecipes.Add(recipe.Name, recipe);
            }

            // have to process mining, generators and boilers (since we convert them to recipes as well)

            foreach (var objJToken in jsonData["resources"]) {
                if (!objJToken["products"].Any())
                    continue;

                var recipe = new RecipeShort("§§r:e:" + (string) objJToken["name"]);

                foreach (var productJToken in objJToken["products"]) {
                    var amount = (double) productJToken["amount"];
                    if (amount <= 0)
                        continue;

                    var productName = (string) productJToken["name"];
                    if (recipe.Products.ContainsKey(productName))
                        recipe.Products[productName] += amount;
                    else
                        recipe.Products.Add(productName, amount);
                }

                if (recipe.Products.Count == 0)
                    continue;

                if (objJToken["required_fluid"] != null && (double) objJToken["fluid_amount"] != 0)
                    recipe.Ingredients.Add((string) objJToken["required_fluid"], (double) objJToken["fluid_amount"]);

                presetRecipes.Add(recipe.Name, recipe);
            }

            foreach (var objJToken in jsonData["entities"]) {
                var type = (string) objJToken["type"];
                switch (type) {
                    case "boiler" when objJToken["fluid_ingredient"] == null || objJToken["fluid_product"] == null:
                        continue;

                    case "boiler": {
                        var temp = (double) objJToken["target_temperature"];
                        var ingredient = (string) objJToken["fluid_ingredient"];
                        var product = (string) objJToken["fluid_product"];

                        var recipe = new RecipeShort($"§§r:b:{ingredient}:{product}:{temp}");
                        recipe.Ingredients.Add(ingredient, 60);
                        recipe.Products.Add(product, 60);

                        if (!presetRecipes.ContainsKey(recipe.Name))
                            presetRecipes.Add(recipe.Name, recipe);
                        break;
                    }

                    case "generator" when objJToken["fluid_ingredient"] == null:
                        continue;

                    case "generator": {
                        var ingredient = (string) objJToken["fluid_ingredient"];
                        var minTemp = (double) (objJToken["minimum_temperature"] ?? double.NaN);
                        var maxTemp = (double) (objJToken["maximum_temperature"] ?? double.NaN);
                        var recipe = new RecipeShort($"§§r:g:{ingredient}:{minTemp}>{maxTemp}");
                        recipe.Ingredients.Add(ingredient, 60);

                        if (!presetRecipes.ContainsKey(recipe.Name))
                            presetRecipes.Add(recipe.Name, recipe);
                        break;
                    }
                }
            }

            // process launch product recipes

            if (presetItems.Contains("rocket-part") && presetRecipes.ContainsKey("rocket-part") && presetEntities.Contains("rocket-silo")) {
                foreach (var objJToken in jsonData["items"].Concat(jsonData["fluids"]).Where(t => t["launch_products"] != null)) {
                    var recipe = new RecipeShort($"§§r:rl:launch-{(string) objJToken["name"]}");

                    var inputSize = (int) objJToken["stack"];
                    foreach (var productJToken in objJToken["launch_products"]) {
                        var amount = (double) productJToken["amount"];
                        var productStack = (int) (jsonData["items"].First(t => (string) t["name"] == (string) productJToken["name"])["stack"] ?? 1);
                        if (amount != 0 && inputSize * amount > productStack)
                            inputSize = (int) (productStack / amount);
                    }

                    foreach (var productJToken in objJToken["launch_products"]) {
                        var amount = (double) productJToken["amount"];
                        if (amount != 0)
                            recipe.Products.Add((string) productJToken["name"], amount * inputSize);
                    }

                    recipe.Ingredients.Add((string) objJToken["name"], inputSize);
                    recipe.Ingredients.Add("rocket-part", 100);

                    presetRecipes.Add(recipe.Name, recipe);
                }
            }

            // compare to provided mod/item/recipe sets (recipes have a chance of existing in multitudes - aka: missing recipes)

            var errors = new PresetErrorPackage(preset);
            foreach (var mod in modList) {
                errors.RequiredMods.Add(mod.Key + "|" + mod.Value);

                if (!presetMods.ContainsKey(mod.Key))
                    errors.MissingMods.Add(mod.Key + "|" + mod.Value);
                else if (presetMods[mod.Key] != mod.Value)
                    errors.WrongVersionMods.Add(mod.Key + "|" + mod.Value + "|" + presetMods[mod.Key]);
            }

            foreach (var mod in presetMods) {
                if (!modList.ContainsKey(mod.Key))
                    errors.AddedMods.Add(mod.Key + "|" + mod.Value);
            }

            foreach (var itemName in itemList) {
                errors.RequiredItems.Add(itemName);

                if (!presetItems.Contains(itemName))
                    errors.MissingItems.Add(itemName);
            }

            foreach (var recipeS in recipeShorts) {
                errors.RequiredRecipes.Add(recipeS.Name);
                if (recipeS.isMissing) {
                    if (presetRecipes.ContainsKey(recipeS.Name) && recipeS.Equals(presetRecipes[recipeS.Name]))
                        errors.ValidMissingRecipes.Add(recipeS.Name);
                    else
                        errors.IncorrectRecipes.Add(recipeS.Name);
                } else {
                    if (!presetRecipes.TryGetValue(recipeS.Name, out var recipe))
                        errors.MissingRecipes.Add(recipeS.Name);
                    else if (!recipeS.Equals(recipe))
                        errors.IncorrectRecipes.Add(recipeS.Name);
                }
            }

            foreach (var plantS in plantShorts) {
                errors.RequiredPlanting.Add(plantS.Name);
                if (plantS.isMissing) {
                    if (presetPlantProcesses.ContainsKey(plantS.Name) && plantS.Equals(presetPlantProcesses[plantS.Name]))
                        errors.ValidMissingPlanting.Add(plantS.Name);
                    else
                        errors.IncorrectPlanting.Add(plantS.Name);
                } else {
                    if (!presetPlantProcesses.TryGetValue(plantS.Name, out var process))
                        errors.MissingPlanting.Add(plantS.Name);
                    else if (!plantS.Equals(process))
                        errors.IncorrectPlanting.Add(plantS.Name);
                }
            }

            foreach (var qualityName in qualityList) {
                errors.RequiredQualities.Add(qualityName);

                if (!presetQualities.Contains(qualityName))
                    errors.MissingQualities.Add(qualityName);
            }

            return errors;
        }
    }
}