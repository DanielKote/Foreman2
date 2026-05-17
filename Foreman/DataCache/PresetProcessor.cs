using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public static class PresetProcessor {
        internal static string GetPresetPath(string presetName, string extension) =>
            Path.Combine(Application.StartupPath, "Presets", presetName + extension);

        public static PresetInfo ReadPresetInfo(Preset preset) {
            Dictionary<string, string> mods = new Dictionary<string, string>();
            string presetPath = GetPresetPath(preset.Name, ".pjson");
            if (!File.Exists(presetPath))
                return new PresetInfo(null, false, false);

            try {
                JsonObject jsonData = PresetJson.ParseObject(Utf8File.ReadAllText(presetPath));
                foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "mods"))
                    if (PresetJson.GetString(objJsonNode, "name") is string name && PresetJson.GetString(objJsonNode, "version") is string version)
                        mods.Add(name, version);
                return new PresetInfo(mods, PresetJson.GetInt32At(jsonData, "difficulty", 0) == 1, PresetJson.GetInt32At(jsonData, "difficulty", 1) == 1);
            } catch (Exception ex) {
                ErrorLogging.LogException(ex, string.Format("Failed to read preset info from {0}", presetPath));
                mods.Clear();
                mods.Add("ERROR READING PRESET!", "");
                return new PresetInfo(mods, false, false);
            }

        }

        public static JsonObject PrepPreset(Preset preset) {
            string presetPath = GetPresetPath(preset.Name, ".pjson");
            string presetCustomPath = GetPresetPath(preset.Name, ".json");

            JsonObject jsonData = PresetJson.ParseObject(Utf8File.ReadAllText(presetPath));
            if (File.Exists(presetCustomPath))
                PresetJson.MergePresetOverlay(jsonData, PresetJson.ParseObject(Utf8File.ReadAllText(presetCustomPath)));
            return jsonData;
        }

        //this preset comparer loads a 'light' version of the preset - basically loading the items and entities as strings only (no data), and only the minimal info for recipes (name, ingredients + amounts, products + amounts)
        //this speeds things up such that the comparison takes around 150ms for a large preset like seablock (10x vanilla), instead of 250ms as for a full datacache load.
        //still, this is only really helpful if you are using 10 presets (1.5 sec load inatead of 2.5 sec) or more, but hey; i will keep it.
        //any changes to preset json style have to be reflected here though (unlike for a full data cache loader above, which just incorporates any changes to data cache as long as they dont impact the outputs)
        public static async Task<PresetErrorPackage> TestPreset(Preset preset, Dictionary<string, string> modList, List<string> itemList, List<string> entityList, List<string> qualityList, List<RecipeShort> recipeShorts, List<PlantShort> plantShorts) {
            JsonObject jsonData = PrepPreset(preset);

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
            foreach (var objJsonNode in PresetJson.EnumerateArray(jsonData, "mods"))
                if (PresetJson.GetString(objJsonNode, "name") is string name && PresetJson.GetString(objJsonNode, "version") is string version)
                    presetMods.Add(name, version);
            //read in items (and their plant results)
            foreach (var objJsonNode in PresetJson.EnumerateArray(jsonData, "items")) {
                if (PresetJson.GetString(objJsonNode, "name") is not string name)
                    continue;
                presetItems.Add(name);
                if (objJsonNode["plant_results"] != null) {
                    PlantShort plantProcess = new PlantShort(name);
                    foreach (JsonNode productJsonNode in PresetJson.EnumerateArray(objJsonNode, "plant_results")) {
                        double amount = PresetJson.GetDouble(productJsonNode, "amount") ?? default;
                        if (amount > 0 && PresetJson.GetString(productJsonNode, "name") is string productName) {
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
            foreach (var objJsonNode in PresetJson.EnumerateArray(jsonData, "fluids"))
                if (PresetJson.GetString(objJsonNode, "name") is string name)
                    presetItems.Add(name);
            //read in entities
            foreach (var objJsonNode in PresetJson.EnumerateArray(jsonData, "entities"))
                if (PresetJson.GetString(objJsonNode, "name") is string name)
                    presetEntities.Add(name);
            //read in quality data
            foreach (var objJsonNode in PresetJson.EnumerateArray(jsonData, "qualities"))
                if (PresetJson.GetString(objJsonNode, "name") is string name)
                    presetQualities.Add(name);

            //read in recipes
            foreach (var objJsonNode in PresetJson.EnumerateArray(jsonData, "recipes")) {
                if (PresetJson.GetString(objJsonNode, "name") is not string name)
                    continue;
                RecipeShort recipe = new RecipeShort(name);
                foreach (JsonNode ingredientJsonNode in PresetJson.EnumerateArray(objJsonNode, "ingredients")) {
                    double amount = PresetJson.GetDouble(ingredientJsonNode, "amount") ?? default;
                    if (amount > 0 && PresetJson.GetString(ingredientJsonNode, "name") is string ingredientName) {
                        if (recipe.Ingredients.ContainsKey(ingredientName))
                            recipe.Ingredients[ingredientName] += amount;
                        else
                            recipe.Ingredients.Add(ingredientName, amount);
                    }
                }
                foreach (JsonNode productJsonNode in PresetJson.EnumerateArray(objJsonNode, "products")) {
                    double amount = PresetJson.GetDouble(productJsonNode, "amount") ?? default;
                    if (amount > 0 && PresetJson.GetString(productJsonNode, "name") is string productName) {
                        if (recipe.Products.ContainsKey(productName))
                            recipe.Products[productName] += amount;
                        else
                            recipe.Products.Add(productName, amount);
                    }
                }
                presetRecipes.Add(recipe.Name, recipe);
            }

            //have to process mining, generators and boilers (since we convert them to recipes as well)
            foreach (var objJsonNode in PresetJson.EnumerateArray(jsonData, "resources"))
                AddResourceExtractionRecipe(objJsonNode, presetRecipes);
            //offshore-pump / water-tile fluids (same pseudo-recipes as DataCache; not listed under "resources")
            foreach (var objJsonNode in PresetJson.EnumerateArray(jsonData, "water_resources"))
                AddResourceExtractionRecipe(objJsonNode, presetRecipes);

            foreach (var objJsonNode in PresetJson.EnumerateArray(jsonData, "entities")) {
                var type = PresetJson.GetString(objJsonNode, "type");
                if (type == "boiler") {
                    if (PresetJson.GetString(objJsonNode, "fluid_ingredient") is not string ingredient || PresetJson.GetString(objJsonNode, "fluid_product") is not string product)
                        continue;

                    double temp = PresetJson.GetDouble(objJsonNode, "target_temperature") ?? default;

                    RecipeShort recipe = new RecipeShort(string.Format("§§r:b:{0}:{1}:{2}", ingredient, product, temp.ToString()));
                    recipe.Ingredients.Add(ingredient, 60);
                    double ingredientHeatCapacity = GetFluidHeatCapacity(jsonData, ingredient);
                    double productHeatCapacity = GetFluidHeatCapacity(jsonData, product);
                    double productQuantity = productHeatCapacity > 0
                        ? 60 * ingredientHeatCapacity / productHeatCapacity
                        : 60;
                    recipe.Products.Add(product, productQuantity);

                    if (!presetRecipes.ContainsKey(recipe.Name))
                        presetRecipes.Add(recipe.Name, recipe);
                } else if (type == "generator") {
                    if (PresetJson.GetString(objJsonNode, "fluid_ingredient") is not string ingredient)
                        continue;

                    double minTemp = PresetJson.GetDouble(objJsonNode, "minimum_temperature") ?? double.NaN;
                    double maxTemp = PresetJson.GetDouble(objJsonNode, "maximum_temperature") ?? double.NaN;
                    RecipeShort recipe = new RecipeShort(string.Format("§§r:g:{0}:{1}>{2}", ingredient, minTemp, maxTemp));
                    recipe.Ingredients.Add(ingredient, 60);

                    if (!presetRecipes.ContainsKey(recipe.Name))
                        presetRecipes.Add(recipe.Name, recipe);
                }
            }

            //process launch product recipes
            if (presetItems.Contains("rocket-part") && presetRecipes.ContainsKey("rocket-part") && presetEntities.Contains("rocket-silo")) {
                foreach (JsonNode objJsonNode in PresetJson.EnumerateArray(jsonData, "items").Concat(PresetJson.EnumerateArray(jsonData, "fluids")).Where(t => PresetJson.GetNode(t, "rocket_launch_products") is not null)) {
                    if (PresetJson.GetString(objJsonNode, "name") is not string name)
                        continue;
                    RecipeShort recipe = new RecipeShort(string.Format("§§r:rl:launch-{0}", name));

                    double inputSize = PresetJson.GetInt32(objJsonNode, "stack_size") ?? default;
                    foreach (JsonNode productJsonNode in PresetJson.EnumerateArray(objJsonNode, "rocket_launch_products")) {
                        double amount = PresetJson.GetDouble(productJsonNode, "amount") ?? default;
                        if (amount == 0 || PresetJson.GetString(productJsonNode, "name") is not string prodName)
                            continue;
                        JsonNode? productItemNode = PresetJson.EnumerateArray(jsonData, "items").FirstOrDefault(t => PresetJson.GetString(t, "name") == prodName);
                        double productStack = PresetJson.GetInt32(productItemNode, "stack_size") ?? 1;
                        if (inputSize * amount > productStack)
                            inputSize = Math.Floor(productStack / amount);
                    }
                    foreach (JsonNode productJsonNode in PresetJson.EnumerateArray(objJsonNode, "rocket_launch_products")) {
                        double amount = PresetJson.GetDouble(productJsonNode, "amount") ?? default;
                        if (amount != 0 && PresetJson.GetString(productJsonNode, "name") is string prodName)
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

        private static double GetFluidHeatCapacity(JsonObject jsonData, string fluidName) {
            foreach (JsonNode fluidNode in PresetJson.EnumerateArray(jsonData, "fluids")) {
                if (PresetJson.GetString(fluidNode, "name") == fluidName)
                    return PresetJson.GetDouble(fluidNode, "heat_capacity") ?? 0;
            }
            return 0;
        }

        private static void AddResourceExtractionRecipe(JsonNode objJsonNode, Dictionary<string, RecipeShort> presetRecipes) {
            if (!PresetJson.EnumerateArray(objJsonNode, "products").Any())
                return;
            if (PresetJson.GetString(objJsonNode, "name") is not string name)
                return;

            RecipeShort recipe = new RecipeShort("§§r:e:" + name);

            foreach (JsonNode productJsonNode in PresetJson.EnumerateArray(objJsonNode, "products")) {
                double amount = PresetJson.GetDouble(productJsonNode, "amount") ?? default;
                if (amount > 0 && PresetJson.GetString(productJsonNode, "name") is string productName) {
                    if (recipe.Products.ContainsKey(productName))
                        recipe.Products[productName] += amount;
                    else
                        recipe.Products.Add(productName, amount);
                }
            }
            if (recipe.Products.Count == 0)
                return;

            if (PresetJson.GetString(objJsonNode, "required_fluid") is string reqFluid && PresetJson.GetDouble(objJsonNode, "fluid_amount") is double fluidAmnt && fluidAmnt != 0)
                recipe.Ingredients.Add(reqFluid, fluidAmnt);

            presetRecipes.Add(recipe.Name, recipe);
        }
    }
}