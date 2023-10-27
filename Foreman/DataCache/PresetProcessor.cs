using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public static class PresetProcessor
	{
		public static PresetInfo ReadPresetInfo(Preset preset)
		{
			Dictionary<string, string> mods = new Dictionary<string, string>();
			string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".pjson" });
			if (!File.Exists(presetPath))
				return new PresetInfo(null, false, false);

			try
			{
				JObject jsonData = JObject.Parse(File.ReadAllText(presetPath));
				foreach (var objJToken in jsonData["mods"].ToList())
					mods.Add((string)objJToken["name"], (string)objJToken["version"]);
				return new PresetInfo(mods, (int)jsonData["difficulty"][0] == 1, (int)jsonData["difficulty"][1] == 1);
			}
			catch
			{
				mods.Clear();
				mods.Add("ERROR READING PRESET!", "");
				return new PresetInfo(mods, false, false);
			}

		}

		public static JObject PrepPreset(Preset preset)
		{
			string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".pjson" });
			string presetCustomPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".json" });

			JObject jsonData = JObject.Parse(File.ReadAllText(presetPath));
			if (File.Exists(presetCustomPath))
			{
				JObject cjsonData = JObject.Parse(File.ReadAllText(presetCustomPath));
				foreach (var groupToken in cjsonData)
				{
					foreach (JObject itemToken in groupToken.Value)
					{
						JObject presetItemToken = (JObject)jsonData[groupToken.Key].FirstOrDefault(t => (string)t["name"] == (string)itemToken["name"]);
						if (presetItemToken != null)
							foreach (var parameter in itemToken)
								presetItemToken[parameter.Key] = parameter.Value;
						else
							((JArray)jsonData[groupToken.Key]).Add(itemToken);
					}
				}
			}
			return jsonData;
		}

		public static async Task<PresetErrorPackage> TestPreset(Preset preset, Dictionary<string, string> modList, List<string> itemList, List<string> entityList, List<RecipeShort> recipeShorts)
		{
			try
			{
				//return await TestPresetThroughDataCache(preset, modList, itemList, entityList, recipeShorts);
				return await TestPresetStreamlined(preset, modList, itemList, entityList, recipeShorts);
			}
			catch
			{
				return null;
			}
		}

		//full load of data cache and comparison. This is naturally slower than the streamlined version, since we load all the extras that arent necessary for comparison (like energy types, technologies, availability calculations, etc)
		//but on the +ve side any changes to preset json format is incorporated into data cache and requires no update to this function.
		private static async Task<PresetErrorPackage> TestPresetThroughDataCache(Preset preset, Dictionary<string, string> modList, List<string> itemList, List<string> entityList, List<RecipeShort> recipeShorts)
		{
			string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".pjson" });
			if (!File.Exists(presetPath))
				return null;

			DataCache presetCache = new DataCache(Properties.Settings.Default.UseRecipeBWfilters);
			await presetCache.LoadAllData(preset, null, false);

			//compare to provided mod/item/recipe sets (recipes have a chance of existing in multitudes - aka: missing recipes)
			PresetErrorPackage errors = new PresetErrorPackage(preset);
			foreach (var mod in modList)
			{
				errors.RequiredMods.Add(mod.Key + "|" + mod.Value);

				if (!presetCache.IncludedMods.ContainsKey(mod.Key))
					errors.MissingMods.Add(mod.Key + "|" + mod.Value);
				else if (presetCache.IncludedMods[mod.Key] != mod.Value)
					errors.WrongVersionMods.Add(mod.Key + "|" + mod.Value + "|" + presetCache.IncludedMods[mod.Key]);
			}
			foreach (var mod in presetCache.IncludedMods)
				if (!modList.ContainsKey(mod.Key))
					errors.AddedMods.Add(mod.Key + "|" + mod.Value);

			foreach (string itemName in itemList)
			{
				errors.RequiredItems.Add(itemName);

				if (!presetCache.Items.ContainsKey(itemName))
					errors.MissingItems.Add(itemName);
			}

			foreach (RecipeShort recipeS in recipeShorts)
			{
				errors.RequiredRecipes.Add(recipeS.Name);
				if (recipeS.isMissing)
				{
					if (presetCache.Recipes.ContainsKey(recipeS.Name) && recipeS.Equals(presetCache.Recipes[recipeS.Name]))
						errors.ValidMissingRecipes.Add(recipeS.Name);
					else
						errors.IncorrectRecipes.Add(recipeS.Name);
				}
				else
				{
					if (!presetCache.Recipes.ContainsKey(recipeS.Name))
						errors.MissingRecipes.Add(recipeS.Name);
					else if (!recipeS.Equals(presetCache.Recipes[recipeS.Name]))
						errors.IncorrectRecipes.Add(recipeS.Name);
				}
			}

			return errors;
		}

		//this preset comparer loads a 'light' version of the preset - basically loading the items and entities as strings only (no data), and only the minimal info for recipes (name, ingredients + amounts, products + amounts)
		//this speeds things up such that the comparison takes around 150ms for a large preset like seablock (10x vanilla), instead of 250ms as for a full datacache load.
		//still, this is only really helpful if you are using 10 presets (1.5 sec load inatead of 2.5 sec) or more, but hey; i will keep it.
		//any changes to preset json style have to be reflected here though (unlike for a full data cache loader above, which just incorporates any changes to data cache as long as they dont impact the outputs)
		private static async Task<PresetErrorPackage> TestPresetStreamlined(Preset preset, Dictionary<string, string> modList, List<string> itemList, List<string> entityList, List<RecipeShort> recipeShorts)
		{
			JObject jsonData = PrepPreset(preset);

			//parse preset (note: this is preset data, so we are guaranteed to only have one name per item/recipe/mod/etc.)
			HashSet<string> presetItems = new HashSet<string>();
			HashSet<string> presetEntities = new HashSet<string>();
			Dictionary<string, RecipeShort> presetRecipes = new Dictionary<string, RecipeShort>();
			Dictionary<string, string> presetMods = new Dictionary<string, string>();

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

			//read in mods, items and entities
			foreach (var objJToken in jsonData["mods"].ToList())
				presetMods.Add((string)objJToken["name"], (string)objJToken["version"]);
			foreach (var objJToken in jsonData["items"].ToList())
				presetItems.Add((string)objJToken["name"]);
			foreach (var objJToken in jsonData["fluids"].ToList())
				presetItems.Add((string)objJToken["name"]);
			foreach (var objJToken in jsonData["entities"].ToList())
				presetEntities.Add((string)objJToken["name"]);

			//read in recipes
			foreach (var objJToken in jsonData["recipes"].ToList())
			{

				RecipeShort recipe = new RecipeShort((string)objJToken["name"]);
				foreach (var ingredientJToken in objJToken["ingredients"].ToList())
				{
					double amount = (double)ingredientJToken["amount"];
					if (amount > 0)
					{
						string ingredientName = (string)ingredientJToken["name"];
						if (recipe.Ingredients.ContainsKey(ingredientName))
							recipe.Ingredients[ingredientName] +=amount;
						else
							recipe.Ingredients.Add(ingredientName, amount);
					}
				}
				foreach (var productJToken in objJToken["products"].ToList())
				{
					double amount = (double)productJToken["amount"];
					if (amount > 0)
					{

						string productName = (string)productJToken["name"];
						if (recipe.Products.ContainsKey(productName))
							recipe.Products[productName] += amount;
						else
							recipe.Products.Add(productName, amount);
					}
				}
				presetRecipes.Add(recipe.Name, recipe);
			}

			//have to process mining, offshore pumps, and boilers (since we convert them to recipes as well)
			foreach (var objJToken in jsonData["resources"])
			{
				if (objJToken["products"].Count() == 0)
					continue;

				RecipeShort recipe = new RecipeShort("§§r:e:" + (string)objJToken["name"]);

				foreach (var productJToken in objJToken["products"])
				{
					double amount = (double)productJToken["amount"];
					if (amount > 0)
					{
						string productName = (string)productJToken["name"];
						if (recipe.Products.ContainsKey(productName))
							recipe.Products[productName] += amount;
						else
							recipe.Products.Add(productName, amount);
					}
				}
				if (recipe.Products.Count == 0)
					continue;

				if (objJToken["required_fluid"] != null && (double)objJToken["fluid_amount"] != 0)
					recipe.Ingredients.Add((string)objJToken["required_fluid"], (double)objJToken["fluid_amount"]);

				presetRecipes.Add(recipe.Name, recipe);
			}

			foreach (var objJToken in jsonData["entities"])
			{
				string type = (string)objJToken["type"];
				if (type == "offshore-pump")
				{
					string fluidName = (string)objJToken["fluid_product"];
					RecipeShort recipe = new RecipeShort("§§r:e:" + fluidName);
					recipe.Products.Add(fluidName, 60);

					if (!presetRecipes.ContainsKey(recipe.Name))
						presetRecipes.Add(recipe.Name, recipe);
				}
				else if (type == "boiler")
				{
					if (objJToken["fluid_ingredient"] == null || objJToken["fluid_product"] == null)
						continue;

					double temp = (double)objJToken["target_temperature"];
					string ingredient = (string)objJToken["fluid_ingredient"];
					string product = (string)objJToken["fluid_product"];

					RecipeShort recipe = new RecipeShort(string.Format("§§r:b:{0}:{1}:{2}", ingredient, product, temp.ToString()));
					recipe.Ingredients.Add(ingredient, 60);
					recipe.Products.Add(product, 60);

					if (!presetRecipes.ContainsKey(recipe.Name))
						presetRecipes.Add(recipe.Name, recipe);
				}
				else if (type == "generator")
				{
					if (objJToken["fluid_ingredient"] == null)
						continue;

					string ingredient = (string)objJToken["fluid_ingredient"];
					double minTemp = (double)(objJToken["minimum_temperature"] ?? double.NaN);
					double maxTemp = (double)(objJToken["maximum_temperature"] ?? double.NaN);
					RecipeShort recipe = new RecipeShort(string.Format("§§r:g:{0}:{1}>{2}", ingredient, minTemp, maxTemp));
					recipe.Ingredients.Add(ingredient, 60);

					if (!presetRecipes.ContainsKey(recipe.Name))
						presetRecipes.Add(recipe.Name, recipe);
				}
			}

			//process launch product recipes
			if (presetItems.Contains("rocket-part") && presetRecipes.ContainsKey("rocket-part") && presetEntities.Contains("rocket-silo"))
			{
				foreach (var objJToken in jsonData["items"].Concat(jsonData["fluids"]).Where(t => t["launch_products"] != null))
				{
					RecipeShort recipe = new RecipeShort(string.Format("§§r:rl:launch-{0}", (string)objJToken["name"]));

					int inputSize = (int)objJToken["stack"];
					foreach (var productJToken in objJToken["launch_products"])
					{
						double amount = (double)productJToken["amount"];
						int productStack = (int)(jsonData["items"].First(t => (string)t["name"] == (string)productJToken["name"])["stack"]?? 1);
						if (amount != 0 && inputSize * amount > productStack)
							inputSize = (int)Math.Ceiling(productStack / amount);
					}
					foreach (var productJToken in objJToken["launch_products"])
					{
						double amount = (double)productJToken["amount"];
						if (amount != 0)
							recipe.Products.Add((string)productJToken["name"], amount * inputSize);
					}

					recipe.Ingredients.Add((string)objJToken["name"], inputSize);
					recipe.Ingredients.Add("rocket-part", 100);

					presetRecipes.Add(recipe.Name, recipe);
				}
			}

			//compare to provided mod/item/recipe sets (recipes have a chance of existing in multitudes - aka: missing recipes)
			PresetErrorPackage errors = new PresetErrorPackage(preset);
			foreach (var mod in modList)
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
	}
}
