using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;
using System.IO;
using System.Windows.Forms;

namespace Foreman
{
	static class DataCache
	{
		private static String factorioPath = Path.Combine(Path.GetPathRoot(Application.StartupPath), "Program Files", "Factorio", "data");
		public static Dictionary<String, Item> Items = new Dictionary<String, Item>();
		private const float defaultRecipeTime = 0.5f;

		public static void LoadRecipes()
		{
			factorioPath = Path.Combine(Path.GetPathRoot(Application.StartupPath), "Program Files", "Factorio", "data");
			Lua lua = new Lua();
			List<String> luaFiles = Directory.GetFiles(factorioPath, "*.lua", SearchOption.AllDirectories).ToList();

			//Add all these files to the Lua path variable
			foreach (String f in luaFiles)
			{
				lua.DoString(String.Format("package.path = package.path .. '{0};'", f.Replace("\\", "\\\\")));
			}

			String dataloaderFile = luaFiles.Find(f => f.EndsWith("dataloader.lua"));

			List<String> recipeFiles = luaFiles.ToList();
			recipeFiles.RemoveAll(f => !f.Contains("prototypes"));
			recipeFiles.RemoveAll(f => !f.Contains("recipe"));

			lua.DoFile(dataloaderFile);
			recipeFiles.ForEach(f => lua.DoFile(f));

			LuaTable recipeTable = lua.GetTable("data.raw")["recipe"] as LuaTable;

			var enumerator = recipeTable.GetEnumerator();
			while (enumerator.MoveNext())
			{
				InterpretLuaRecipe(enumerator.Key as String, enumerator.Value as LuaTable);
			}
		}

		//Adds the item name to the cache and returns the Item object
		private static Item LoadItem(String itemName)
		{
			Item newItem;
			if (!Items.ContainsKey(itemName))
			{
				Items.Add(itemName, newItem = new Item(itemName));
			}
			else
			{
				newItem = Items[itemName];
			}
			return newItem;
		}

		private static void InterpretLuaRecipe(String name, LuaTable values)
		{
			float time = Convert.ToSingle(values["energy_required"]);
			Dictionary<Item, float> ingredients = extractIngredientsFromLuaRecipe(values);
			Dictionary<Item, float> results = extractResultsFromLuaRecipe(values);

			foreach (Item result in results.Keys)
			{
				result.Recipes.Add(new Recipe(name, time == 0.0f ? defaultRecipeTime : time, ingredients, results));
			}
		}

		private static Dictionary<Item, float> extractResultsFromLuaRecipe(LuaTable values)
		{
			Dictionary<Item, float> results = new Dictionary<Item, float>();
			if (values["result"] is String)
			{
				float resultCount = Convert.ToSingle(values["result_count"]);
				if (resultCount == 0f)
				{
					resultCount = 1f;
				}
				results.Add(LoadItem(values["result"] as String), resultCount);
			}
			else
			{
				var resultEnumerator = (values["results"] as LuaTable).GetEnumerator();
				while (resultEnumerator.MoveNext())
				{
					LuaTable resultTable = resultEnumerator.Value as LuaTable;
					results.Add(LoadItem(resultTable["name"] as String), Convert.ToSingle(resultTable["amount"]));
				}
			}

			return results;
		}

		private static Dictionary<Item, float> extractIngredientsFromLuaRecipe(LuaTable values)
		{
			Dictionary<Item, float> ingredients = new Dictionary<Item, float>();
			var ingredientEnumerator = (values["ingredients"] as LuaTable).GetEnumerator();
			while (ingredientEnumerator.MoveNext())
			{
				LuaTable ingredientTable = ingredientEnumerator.Value as LuaTable;
				String name;
				float amount;
				if (ingredientTable["name"] != null)
				{
					name = ingredientTable["name"] as String;
				}
				else
				{
					name = ingredientTable[1] as String;
				}
				if (ingredientTable["amount"] != null)
				{
					amount = Convert.ToSingle(ingredientTable["amount"]);
				}
				else
				{
					amount = Convert.ToSingle(ingredientTable[2]);
				}
				ingredients.Add(LoadItem(name), amount);
			}

			return ingredients;
		}

	}
}