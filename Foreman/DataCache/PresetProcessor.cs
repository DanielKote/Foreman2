using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Foreman
{
	public static class PresetProcessor
	{
		public static PresetInfo ReadPresetInfo(Preset preset)
		{
			Dictionary<string, string> mods = new Dictionary<string, string>();
			string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".json" });
			if (!File.Exists(presetPath))
				return new PresetInfo(null, false, false);

			JObject jsonData = JObject.Parse(File.ReadAllText(presetPath));
			foreach (var objJToken in jsonData["mods"].ToList())
				mods.Add((string)objJToken["name"], (string)objJToken["version"]);

			return new PresetInfo(mods, (int)jsonData["difficulty"][0] == 1, (int)jsonData["difficulty"][1] == 1);
		}

		public static PresetErrorPackage TestPreset(Preset preset, Dictionary<string, string> modList, List<string> itemList, List<string> entityList, List<RecipeShort> recipeShorts)
		{

			string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", preset.Name + ".json" });
			if (!File.Exists(presetPath))
				return null;

			//parse preset (note: this is preset data, so we are guaranteed to only have one name per item/recipe/mod/etc.)
			JObject jsonData = JObject.Parse(File.ReadAllText(presetPath));
			HashSet<string> presetItems = new HashSet<string>();
			HashSet<string> presetEntities = new HashSet<string>();
			Dictionary<string, RecipeShort> presetRecipes = new Dictionary<string, RecipeShort>();
			Dictionary<string, string> presetMods = new Dictionary<string, string>();

			//built in items
			presetItems.Add("§§i:heat:");
			//built in recipes:
			RecipeShort heatRecipe = new RecipeShort("§§r:h:heat-generation");
			heatRecipe.Products.Add("§§i:heat", 1);
			presetRecipes.Add(heatRecipe.Name, heatRecipe);
			RecipeShort burnerRecipe = new RecipeShort("§§r:h:burner-electicity");
			presetRecipes.Add(burnerRecipe.Name, burnerRecipe);
			RecipeShort steamBurnerRecipe = new RecipeShort("§§r:h:steam-electricity");
			steamBurnerRecipe.Ingredients.Add("steam", 60);
			presetRecipes.Add(steamBurnerRecipe.Name, steamBurnerRecipe);
			//built in assemblers:
			presetEntities.Add("§§a:player-assembler");

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
					string ingredientName = (string)ingredientJToken["name"];
					if (recipe.Ingredients.ContainsKey(ingredientName))
						recipe.Ingredients[ingredientName] += (double)ingredientJToken["amount"];
					else
						recipe.Ingredients.Add(ingredientName, (double)ingredientJToken["amount"]);
				}
				foreach (var productJToken in objJToken["products"].ToList())
				{
					string productName = (string)productJToken["name"];
					if (recipe.Products.ContainsKey(productName))
						recipe.Products[productName] += (double)productJToken["amount"];
					else
						recipe.Products.Add(productName, (double)productJToken["amount"]);
				}
				presetRecipes.Add(recipe.Name, recipe);
			}

			//have to process mining, offshore pumps, and boilers (since we convert them to recipes as well)
			foreach (var objJToken in jsonData["resources"])
			{
				if (objJToken["products"].Count() == 0)
					continue;

				RecipeShort recipe = new RecipeShort("§§r:" + (string)objJToken["name"]);

				foreach (var productJToken in objJToken["products"])
				{
					string productName = (string)productJToken["name"];
					if (recipe.Products.ContainsKey(productName))
						recipe.Products[productName] += (double)productJToken["amount"];
					else
						recipe.Products.Add(productName, (double)productJToken["amount"]);
				}
				if (recipe.Products.Count == 0)
					continue;

				if (objJToken["required_fluid"] != null && (double)objJToken["fluid_amount"] != 0)
					recipe.Ingredients.Add((string)objJToken["required_fluid"], (double)objJToken["fluid_amount"]);

				presetRecipes.Add(recipe.Name, recipe);
			}

			foreach (var objJToken in jsonData["entities"])
			{
				if ((string)objJToken["type"] == "offshore-pump")
				{
					string fluidName = (string)objJToken["fluid_result"];
					RecipeShort recipe = new RecipeShort("§§r:" + fluidName);
					recipe.Products.Add(fluidName, 60);

					if (!presetRecipes.ContainsKey(recipe.Name))
						presetRecipes.Add(recipe.Name, recipe);
				}
				else if ((string)objJToken["type"] == "boiler")
				{
					double temp = (double)objJToken["target_temperature"];
					RecipeShort recipe = new RecipeShort("§§r:boil" + temp.ToString());
					recipe.Ingredients.Add("water", 60);
					recipe.Products.Add("steam", 60);

					if (!presetRecipes.ContainsKey(recipe.Name))
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
