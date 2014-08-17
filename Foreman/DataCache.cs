using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace Foreman
{
	static class DataCache
	{
		//Still hardcoded. Needs to ask the user if it can't be found.
		private static String factorioPath = Path.Combine(Path.GetPathRoot(Application.StartupPath), "Program Files", "Factorio", "data");
		public static Dictionary<String, Item> Items = new Dictionary<String, Item>();
		private const float defaultRecipeTime = 0.5f;
		private static Dictionary<Bitmap, Color> colourCache = new Dictionary<Bitmap, Color>();
		public static Bitmap UnknownIcon;
		
		public static void LoadRecipes()
		{
			Lua lua = new Lua();
			List<String> luaFiles = Directory.GetFiles(factorioPath, "*.lua", SearchOption.AllDirectories).ToList();

			//Add all these files to the Lua path variable
			foreach (String f in luaFiles)
			{
				lua.DoString(String.Format("package.path = package.path .. '{0};'", f.Replace("\\", "\\\\")));
			}

			String dataloaderFile = luaFiles.Find(f => f.EndsWith("dataloader.lua"));

			List<String> itemFiles = luaFiles.Where(f => f.Contains("prototypes" + Path.DirectorySeparatorChar + "item")).ToList();
			itemFiles.AddRange(luaFiles.Where(f => f.Contains("prototypes" + Path.DirectorySeparatorChar + "fluid")).ToList());
			itemFiles.AddRange(luaFiles.Where(f => f.Contains("prototypes" + Path.DirectorySeparatorChar + "equipment")).ToList());
			List<String> recipeFiles = luaFiles.Where(f => f.Contains("prototypes" + Path.DirectorySeparatorChar + "recipe")).ToList();

			lua.DoFile(dataloaderFile);
			itemFiles.ForEach(f => lua.DoFile(f));
			recipeFiles.ForEach(f => lua.DoFile(f));

			InterpretItems(lua, "item");
			InterpretItems(lua, "fluid");
			InterpretItems(lua, "capsule");
			InterpretItems(lua, "module");
			InterpretItems(lua, "ammo");
			
			LuaTable recipeTable = lua.GetTable("data.raw")["recipe"] as LuaTable;

			var enumerator = recipeTable.GetEnumerator();
			while (enumerator.MoveNext())
			{
				InterpretLuaRecipe(enumerator.Key as String, enumerator.Value as LuaTable);
			}

			UnknownIcon = LoadImage("UnknownIcon.png");
		}

		private static void InterpretItems(Lua lua, String typeName)
		{
			LuaTable itemTable = lua.GetTable("data.raw")[typeName] as LuaTable;

			if (itemTable != null)
			{
				var enumerator = itemTable.GetEnumerator();
				while (enumerator.MoveNext())
				{
					InterpretLuaItem(enumerator.Key as String, enumerator.Value as LuaTable);
				}
			}
		}

		private static Bitmap LoadImage(String fileName)
		{
			string fullPath;
			if (File.Exists(fileName))
			{
				fullPath = fileName;
			}
			else
			{

				string[] splitPath = fileName.Split('/');
				splitPath[0] = splitPath[0].Trim('_');
				fullPath = factorioPath;
				foreach (String pathPart in splitPath)
				{
					fullPath = Path.Combine(fullPath, pathPart);
				}
			}

			try
			{
				Bitmap image = new Bitmap(fullPath);
				return image;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		public static Color IconAverageColour(Bitmap icon)
		{
			if (icon == null)
			{
				return Color.LightGray;
			}

			Color result;
			if (colourCache.ContainsKey(icon))
			{
				result = colourCache[icon];
			}
			else
			{
				Bitmap pixel = new Bitmap(1, 1);

				using (Graphics g = Graphics.FromImage(pixel))
				{
					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
					g.DrawImage(icon, new Rectangle(0, 0, 1, 1)); //Scale the icon down to a 1-pixel image, which does the averaging for us
				}
				result = pixel.GetPixel(0, 0);
				result = Color.FromArgb(255, result.R + (255 - result.R) / 2, result.G + (255 - result.G) / 2, result.B + (255 - result.B) / 2);
				colourCache.Add(icon, result);
			}

			return result;
		}

		private static void InterpretLuaItem(String name, LuaTable values)
		{
			Item newItem = new Item(name);
			newItem.Icon = LoadImage(values["icon"] as String);

			if (!Items.ContainsKey(name))
			{
				Items.Add(name, newItem);
			}
		}

		//This is only if a recipe references an item that isn't in the item prototypes (which shouldn't really happen)
		private static Item LoadItemFromRecipe(String itemName)
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
				results.Add(LoadItemFromRecipe(values["result"] as String), resultCount);
			}
			else
			{
				var resultEnumerator = (values["results"] as LuaTable).GetEnumerator();
				while (resultEnumerator.MoveNext())
				{
					LuaTable resultTable = resultEnumerator.Value as LuaTable;
					results.Add(LoadItemFromRecipe(resultTable["name"] as String), Convert.ToSingle(resultTable["amount"]));
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
				ingredients.Add(LoadItemFromRecipe(name), amount);
			}

			return ingredients;
		}

	}
}