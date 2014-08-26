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
		public static Dictionary<String, Recipe> Recipes = new Dictionary<String, Recipe>();
		public static Dictionary<String, Assembler> Assemblers = new Dictionary<string, Assembler>();
		private const float defaultRecipeTime = 0.5f;
		private static Dictionary<Bitmap, Color> colourCache = new Dictionary<Bitmap, Color>();
		public static Bitmap UnknownIcon;
		public static Dictionary<String, String> KnownRecipeNames = new Dictionary<string, string>();

		public static void LoadRecipes()
		{
			using (Lua lua = new Lua())
			{
				List<String> luaFiles = Directory.GetFiles(factorioPath, "*.lua", SearchOption.AllDirectories).ToList();
				List<String> luaDirs = Directory.GetDirectories(factorioPath, "*", SearchOption.AllDirectories).ToList();

				//Add all these files to the Lua path variable
				foreach (String d in luaDirs)
				{
					lua.DoString(String.Format("package.path = package.path .. ';{0}\\\\?.lua'", d.Replace("\\", "\\\\")));
				}

				String dataloaderFile = luaFiles.Find(f => f.EndsWith("dataloader.lua"));
				String autoplaceFile = luaFiles.Find(f => f.EndsWith("autoplace_utils.lua"));

				List<String> itemFiles = luaFiles.Where(f => f.Contains("prototypes" + Path.DirectorySeparatorChar + "item")).ToList();
				itemFiles.AddRange(luaFiles.Where(f => f.Contains("prototypes" + Path.DirectorySeparatorChar + "fluid")).ToList());
				itemFiles.AddRange(luaFiles.Where(f => f.Contains("prototypes" + Path.DirectorySeparatorChar + "equipment")).ToList());
				List<String> recipeFiles = luaFiles.Where(f => f.Contains("prototypes" + Path.DirectorySeparatorChar + "recipe")).ToList();
				List<String> entityFiles = luaFiles.Where(f => f.Contains("prototypes" + Path.DirectorySeparatorChar + "entity")).ToList();

				lua.DoFile(dataloaderFile);
				lua.DoFile(autoplaceFile);
				foreach (String f in itemFiles.Union(recipeFiles).Union(entityFiles))
				{
					try
					{
						lua.DoFile(f);
					}
					catch (NLua.Exceptions.LuaScriptException e)
					{

					}
				}

				foreach (String type in new List<String> { "item", "fluid", "capsule", "module", "ammo", "gun", "armor", "blueprint", "deconstruction-item" })
				{
					InterpretItems(lua, type);
				}

				LuaTable recipeTable = lua.GetTable("data.raw")["recipe"] as LuaTable;
				var recipeEnumerator = recipeTable.GetEnumerator();
				while (recipeEnumerator.MoveNext())
				{
					InterpretLuaRecipe(recipeEnumerator.Key as String, recipeEnumerator.Value as LuaTable);
				}

				LuaTable assemblerTable = lua.GetTable("data.raw")["assembling-machine"] as LuaTable;
				var assemblerEnumerator = assemblerTable.GetEnumerator();
				while (assemblerEnumerator.MoveNext())
				{
					InterpretAssemblingMachine(assemblerEnumerator.Key as String, assemblerEnumerator.Value as LuaTable);
				}
				LuaTable furnaceTable = lua.GetTable("data.raw")["furnace"] as LuaTable;
				var furnaceEnumerator = furnaceTable.GetEnumerator();
				while (furnaceEnumerator.MoveNext())
				{
					InterpretFurnace(furnaceEnumerator.Key as String, furnaceEnumerator.Value as LuaTable);
				}

				UnknownIcon = LoadImage("UnknownIcon.png");

				LoadItemNames("item-names.cfg", "[item-name]");
				LoadItemNames("fluids.cfg", "[fluid-name]");
				LoadItemNames("entity-names.cfg", "[entity-name]");
				LoadItemNames("equipment-names.cfg", "[equipment-name]");

				LoadRecipeNames();

				LoadAssemblerNames();
			}
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

		private static void LoadItemNames(String fileName, String iniSectionName, String locale = "en")
		{
			foreach (String dir in Directory.GetDirectories(factorioPath))
			{
				String fullFilePath = Path.Combine(dir, "locale", locale, fileName);
				if (File.Exists(fullFilePath))
				{
					using (StreamReader fStream = new StreamReader(fullFilePath))
					{
						bool inItemNamesSection = false;

						while (!fStream.EndOfStream)
						{
							String line = fStream.ReadLine();
							if (line.StartsWith("[") && line.EndsWith("]"))
							{
								if (line == iniSectionName)
								{
									inItemNamesSection = true;
								}
								else
								{
									inItemNamesSection = false;
								}
							}
							else if (inItemNamesSection)
							{
								String[] split = line.Split('=');
								if (split.Count() == 2)
								{
									if (Items.ContainsKey(split[0]))
									{
										Items[split[0]].FriendlyName = split[1];
									}
								}
							}
						}
					}
				}
			}
		}

		private static void LoadRecipeNames(String locale = "en")
		{
			foreach (String dir in Directory.GetDirectories(factorioPath))
			{
				String fullFilePath = Path.Combine(dir, "locale", locale, "recipe-names.cfg");
				if (File.Exists(fullFilePath))
				{
					using (StreamReader fStream = new StreamReader(fullFilePath))
					{
						bool inRecipeNamesSection = false;
						while (!fStream.EndOfStream)
						{
							String line = fStream.ReadLine();
							if (line.StartsWith("[") && line.EndsWith("]"))
							{
								if (line == "[recipe-name]")
								{
									inRecipeNamesSection = true;
								}
								else
								{
									inRecipeNamesSection = false;
								}
							}
							else
							{
								if (inRecipeNamesSection)
								{
									String[] split = line.Split('=');
									if (split.Count() == 2)
									{
										if (KnownRecipeNames.ContainsKey(split[0]))
										{
											KnownRecipeNames[split[0]] = split[1];
										}
										else
										{
											KnownRecipeNames.Add(split[0], split[1]);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		private static void LoadAssemblerNames(String locale = "en")
		{
			foreach (String dir in Directory.GetDirectories(factorioPath))
			{
				String fullFilePath = Path.Combine(dir, "locale", locale, "entity-names.cfg");
				if (File.Exists(fullFilePath))
				{
					using (StreamReader fStream = new StreamReader(fullFilePath))
					{
						bool inEntityNamesSection = false;
						while (!fStream.EndOfStream)
						{
							String line = fStream.ReadLine();
							if (line.StartsWith("[") && line.EndsWith("]"))
							{
								if (line == "[entity-name]")
								{
									inEntityNamesSection = true;
								}
								else
								{
									inEntityNamesSection = false;
								}
							}
							else
							{
								if (inEntityNamesSection)
								{
									String[] split = line.Split('=');
									if (split.Count() == 2)
									{
										if (Assemblers.ContainsKey(split[0]))
										{
											Assemblers[split[0]].FriendlyName = split[1];
										}
									}
								}
							}
						}
					}
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
				using (Bitmap pixel = new Bitmap(1, 1))
				using (Graphics g = Graphics.FromImage(pixel))
				{
					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
					g.DrawImage(icon, new Rectangle(0, 0, 1, 1)); //Scale the icon down to a 1-pixel image, which does the averaging for us
					result = pixel.GetPixel(0, 0);
				}
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

			Recipe newRecipe = new Recipe(name, time == 0.0f ? defaultRecipeTime : time, ingredients, results);

			newRecipe.Category = values["category"] as String ?? "crafting";

			String iconFile = values["icon"] as String;
			if (iconFile != null)
			{
				Bitmap icon = LoadImage(iconFile);
				newRecipe.Icon = icon;
			}

			foreach (Item result in results.Keys)
			{
				result.Recipes.Add(newRecipe);
			}
			Recipes.Add(newRecipe.Name, newRecipe);
		}

		private static void InterpretAssemblingMachine(String name, LuaTable values)
		{
			Assembler newAssembler = new Assembler(name);

			newAssembler.Icon = LoadImage(values["icon"] as String);
			newAssembler.MaxIngredients = Convert.ToInt32(values["ingredient_count"]);
			newAssembler.ModuleSlots = Convert.ToInt32(values["module_slots"]);
			if (newAssembler.ModuleSlots == 0) newAssembler.ModuleSlots = 2;
			newAssembler.Speed = Convert.ToSingle(values["crafting_speed"]);

			LuaTable effects = values["allowed_effects"] as LuaTable;
			if (effects != null)
			{
				foreach (String effect in effects.Values)
				{
					newAssembler.AllowedEffects.Add(effect);
				}
			}
			foreach (String category in (values["crafting_categories"] as LuaTable).Values)
			{
				newAssembler.Categories.Add(category);
			}

			Assemblers.Add(newAssembler.Name, newAssembler);
		}

		private static void InterpretFurnace(String name, LuaTable values)
		{
			Assembler newFurnace = new Assembler(name);

			newFurnace.Icon = LoadImage(values["icon"] as String);
			newFurnace.MaxIngredients = 1;
			newFurnace.ModuleSlots = Convert.ToInt32(values["module_slots"]);
			newFurnace.Speed = Convert.ToSingle(values["smelting_speed"]) - 1f;

			foreach (String category in (values["smelting_categories"] as LuaTable).Values)
			{
				newFurnace.Categories.Add(category);
			}

			Assemblers.Add(newFurnace.Name, newFurnace);
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