using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;

namespace Foreman
{
	static class DataCache
	{
		private class MissingPrototypeValueException : Exception
		{
			public LuaTable Table;
			public String Key;

			public MissingPrototypeValueException(LuaTable table, String key, String message = "")
				: base(message)
			{
				Table = table;
				Key = key;
			}
		}

		public class Mod
		{
			public String name = "";
			public String title = "";
			public String version = "";
			public String dir = "";
			public String description = "";
			public String author = "";
		}

		public static List<Mod> Mods = new List<Mod>();

		public static String FactorioDataPath = "";
		public static String AppDataModPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Factorio", "mods");

		public static Dictionary<String, Item> Items = new Dictionary<String, Item>();
		public static Dictionary<String, Recipe> Recipes = new Dictionary<String, Recipe>();
		public static Dictionary<String, Assembler> Assemblers = new Dictionary<string, Assembler>();
		public static Dictionary<String, Miner> Miners = new Dictionary<string, Miner>();
		public static Dictionary<String, Resource> Resources = new Dictionary<string, Resource>();
		public static Dictionary<String, Module> Modules = new Dictionary<string, Module>();

		private const float defaultRecipeTime = 0.5f;
		private static Dictionary<Bitmap, Color> colourCache = new Dictionary<Bitmap, Color>();
		public static Bitmap UnknownIcon;
		public static Dictionary<String, String> KnownRecipeNames = new Dictionary<string, string>();
		public static Dictionary<String, Dictionary<String, String>> LocaleFiles = new Dictionary<string, Dictionary<string, string>>();

		public static Dictionary<String, Exception> failedFiles = new Dictionary<string, Exception>();
		public static Dictionary<String, Exception> failedPathDirectories = new Dictionary<string, Exception>();

		public static void LoadAllData()
		{
			using (Lua lua = new Lua())
			{
				FindAllMods();

				foreach (Mod mod in Mods)
				{
					AddLuaPackagePath(lua, mod.dir); //Prototype folder matches package hierarchy so this is enough.
				}
				AddLuaPackagePath(lua, Path.Combine(FactorioDataPath, "core", "lualib")); //Core lua functions

				String dataloaderFile = Path.Combine(FactorioDataPath, "core", "lualib", "dataloader.lua");
				try
				{
					lua.DoFile(dataloaderFile);
				}
				catch (Exception e)
				{
					failedFiles[dataloaderFile] = e;
					ErrorLogging.LogLine(String.Format("Error loading dataloader.lua. This file is required to load any values from the prototypes. Message: '{0}'", e.Message));
					return;
				}

				lua.DoString(@"
	function module(modname,...)
	end
	
	require ""util""
	util = {}
	util.table = {}
	util.table.deepcopy = table.deepcopy
	util.multiplystripes = multiplystripes");

				foreach (String filename in new String[] { "data.lua", "data-updates.lua", "data-final-fixes.lua" })
				{
					foreach (Mod mod in Mods)
					{
						String dataFile = Path.Combine(mod.dir, filename);
						if (File.Exists(dataFile))
						{
							try
							{
								lua.DoFile(dataFile);
							}
							catch (Exception e)
							{
								failedFiles[dataFile] = e;
							}
						}
					}
				}

				//------------------------------------------------------------------------------------------
				// Lua files have all been executed, now it's time to extract their data from the lua engine
				//------------------------------------------------------------------------------------------

				foreach (String type in new List<String> { "item", "fluid", "capsule", "module", "ammo", "gun", "armor", "blueprint", "deconstruction-item", "mining-tool", "repair-tool" })
				{
					InterpretItems(lua, type);
				}

				LuaTable recipeTable = lua.GetTable("data.raw")["recipe"] as LuaTable;
				if (recipeTable != null)
				{
					var recipeEnumerator = recipeTable.GetEnumerator();
					while (recipeEnumerator.MoveNext())
					{
						InterpretLuaRecipe(recipeEnumerator.Key as String, recipeEnumerator.Value as LuaTable);
					}
				}

				LuaTable assemblerTable = lua.GetTable("data.raw")["assembling-machine"] as LuaTable;
				if (assemblerTable != null)
				{
					var assemblerEnumerator = assemblerTable.GetEnumerator();
					while (assemblerEnumerator.MoveNext())
					{
						InterpretAssemblingMachine(assemblerEnumerator.Key as String, assemblerEnumerator.Value as LuaTable);
					}
				}

				LuaTable furnaceTable = lua.GetTable("data.raw")["furnace"] as LuaTable;
				if (furnaceTable != null)
				{
					var furnaceEnumerator = furnaceTable.GetEnumerator();
					while (furnaceEnumerator.MoveNext())
					{
						InterpretFurnace(furnaceEnumerator.Key as String, furnaceEnumerator.Value as LuaTable);
					}
				}

				LuaTable minerTable = lua.GetTable("data.raw")["mining-drill"] as LuaTable;
				if (minerTable != null)
				{
					var minerEnumerator = minerTable.GetEnumerator();
					while (minerEnumerator.MoveNext())
					{
						InterpretMiner(minerEnumerator.Key as String, minerEnumerator.Value as LuaTable);
					}
				}

				LuaTable resourceTable = lua.GetTable("data.raw")["resource"] as LuaTable;
				if (resourceTable != null)
				{
					var resourceEnumerator = resourceTable.GetEnumerator();
					while (resourceEnumerator.MoveNext())
					{
						InterpretResource(resourceEnumerator.Key as String, resourceEnumerator.Value as LuaTable);
					}
				}

				LuaTable moduleTable = lua.GetTable("data.raw")["module"] as LuaTable;
				if (moduleTable != null)
				{
					foreach (String moduleName in moduleTable.Keys)
					{
						InterpretModule(moduleName, moduleTable[moduleName] as LuaTable);
					}
				}

				UnknownIcon = LoadImage("UnknownIcon.png");

				LoadLocaleFiles();

				LoadItemNames("item-name");
				LoadItemNames("fluid-name");
				LoadItemNames("entity-name");
				LoadItemNames("equipment-name");
				LoadRecipeNames();
				LoadEntityNames();
				LoadModuleNames();
			}

			ReportErrors();
		}

		private static float ReadLuaFloat(LuaTable table, String key, Boolean canBeMissing = false, float defaultValue = 0f)
		{
			if (table[key] == null)
			{
				if (canBeMissing)
				{
					return defaultValue;
				}
				else
				{
					throw new MissingPrototypeValueException(table, key, "Key is missing");
				}
			}

			try
			{
				return Convert.ToSingle(table[key]);
			}
			catch (FormatException e)
			{
				throw new MissingPrototypeValueException(table, key, string.Format("Expected a float, but the value ('{0}') isn't one", table[key]));
			}
		}

		private static int ReadLuaInt(LuaTable table, String key, Boolean canBeMissing = false, int defaultValue = 0)
		{
			if (table[key] == null)
			{
				if (canBeMissing)
				{
					return defaultValue;
				}
				else
				{
					throw new MissingPrototypeValueException(table, key, "Key is missing");
				}
			}

			try
			{
				return Convert.ToInt32(table[key]);
			}
			catch (FormatException e)
			{
				throw new MissingPrototypeValueException(table, key, String.Format("Expected an Int32, but the value ('{0]') isn't one", table[key]));
			}
		}

		private static string ReadLuaString(LuaTable table, String key, Boolean canBeMissing = false, String defaultValue = null)
		{
			if (table[key] == null)
			{
				if (canBeMissing)
				{
					return defaultValue;
				}
				else
				{
					throw new MissingPrototypeValueException(table, key, "Key is missing");
				}
			}

			return Convert.ToString(table[key]);
		}

		private static LuaTable ReadLuaLuaTable(LuaTable table, String key, Boolean canBeMissing = false)
		{
			if (table[key] == null)
			{
				if (canBeMissing)
				{
					return null;
				}
				else
				{
					throw new MissingPrototypeValueException(table, key, "Key is missing");
				}
			}

			try
			{
				return table[key] as LuaTable;
			}
			catch (Exception e)
			{
				throw new MissingPrototypeValueException(table, key, "Could not convert key to LuaTable");
			}
		}

		private static void ReportErrors()
		{
			if (failedPathDirectories.Any())
			{
				ErrorLogging.LogLine("There were errors setting the lua path variable for the following directories:");
				foreach (String dir in DataCache.failedPathDirectories.Keys)
				{
					ErrorLogging.LogLine(String.Format("{0} ({1})", dir, DataCache.failedPathDirectories[dir].Message));
				}
			}

			if (failedFiles.Any())
			{
				ErrorLogging.LogLine("The following files could not be loaded due to errors:");
				foreach (String file in DataCache.failedFiles.Keys)
				{
					ErrorLogging.LogLine(String.Format("{0} ({1})", file, DataCache.failedFiles[file].Message));
				}
			}
		}

		private static void AddLuaPackagePath(Lua lua, string dir)
		{
			try
			{
				string luaCommand = String.Format("package.path = package.path .. ';{0}{1}?.lua'", dir, Path.DirectorySeparatorChar);
				luaCommand = luaCommand.Replace("\\", "\\\\");
				lua.DoString(luaCommand);
			}
			catch (Exception e)
			{
				failedPathDirectories[dir] = e;
			}
		}

		private static IEnumerable<String> getAllLuaFiles()
		{
			if (Directory.Exists(FactorioDataPath))
			{
				foreach (String file in Directory.GetFiles(FactorioDataPath, "*.lua", SearchOption.AllDirectories))
				{
					yield return file;
				}
			}
			if (Directory.Exists(AppDataModPath))
			{
				foreach (String file in Directory.GetFiles(AppDataModPath, "*.lua", SearchOption.AllDirectories))
				{
					yield return file;
				}
			}
		}

		private static void FindAllMods() //Vanilla game counts as a mod too.
		{
			foreach(String dir in Directory.EnumerateDirectories(FactorioDataPath))
			{
				ReadModInfoJson(dir);
			}
			foreach (String dir in Directory.EnumerateDirectories(AppDataModPath))
			{
				ReadModInfoJson(dir);
			}

			Mods.Sort((a, b) => //Really basic way of putting core and base at the top
			{
				if (a.name == "core") { return -1; }
				if (b.name == "core") { return 1; }
				if (a.name == "base") { return -1; }
				if (b.name == "base") { return 1; }
				return 0;
			});
		}

		private static void ReadModInfoJson(String dir)
		{
			if (!File.Exists(Path.Combine(dir, "info.json")))
			{
				return;
			}
			String json = File.ReadAllText(Path.Combine(dir, "info.json"));
			Mod newMod = JsonConvert.DeserializeObject<Mod>(json);
			newMod.dir = dir;
			Mods.Add(newMod);
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

		private static void LoadLocaleFiles(String locale = "en")
		{
			foreach (Mod mod in Mods)
			{
				String localeDir = Path.Combine(mod.dir, "locale", locale);
				if (Directory.Exists(localeDir))
				{
					foreach (String file in Directory.GetFiles(localeDir, "*.cfg"))
					{
						try
						{
							using (StreamReader fStream = new StreamReader(file))
							{
								string currentIniSection = "none";

								while (!fStream.EndOfStream)
								{
									String line = fStream.ReadLine();
									if (line.StartsWith("[") && line.EndsWith("]"))
									{
										currentIniSection = line.Trim('[', ']');
									}
									else
									{
										if (!LocaleFiles.ContainsKey(currentIniSection))
										{
											LocaleFiles.Add(currentIniSection, new Dictionary<string, string>());
										}
										String[] split = line.Split('=');
										if (split.Count() == 2)
										{
											LocaleFiles[currentIniSection][split[0]] = split[1];
										}
									}
								}
							}
						}
						catch (Exception e)
						{
							failedFiles[file] = e;
						}
					}
				}
			}
		}

		private static void LoadItemNames(String category)
		{
			foreach (var kvp in LocaleFiles[category])
			{
				if (Items.ContainsKey(kvp.Key))
				{
					Items[kvp.Key].FriendlyName = kvp.Value;
				}
			}
		}

		private static void LoadRecipeNames(String locale = "en")
		{
			foreach (var kvp in LocaleFiles["recipe-name"])
			{
				KnownRecipeNames[kvp.Key] = kvp.Value;
			}
		}

		private static void LoadEntityNames(String locale = "en")
		{
			foreach (var kvp in LocaleFiles["entity-name"])
			{
				if (Assemblers.ContainsKey(kvp.Key))
				{
					Assemblers[kvp.Key].FriendlyName = kvp.Value;
				}
				if (Miners.ContainsKey(kvp.Key))
				{
					Miners[kvp.Key].FriendlyName = kvp.Value;
				}
			}
		}

		private static void LoadModuleNames(String locale = "en")
		{
			foreach (var kvp in LocaleFiles["item-name"])
			{
				if (Modules.ContainsKey(kvp.Key))
				{
					Modules[kvp.Key].FriendlyName = kvp.Value;
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
				fullPath = Mods.FirstOrDefault(m => m.name == splitPath[0]).dir;

				if (!String.IsNullOrEmpty(fullPath))
				{
					for (int i = 1; i < splitPath.Count(); i++) //Skip the first split section because it's the mod name, not a directory
					{
						fullPath = Path.Combine(fullPath, splitPath[i]);
					}
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
				//Set alpha to 255, also lighten the colours to make them more pastel-y
				result = Color.FromArgb(255, result.R + (255 - result.R) / 2, result.G + (255 - result.G) / 2, result.B + (255 - result.B) / 2);
				colourCache.Add(icon, result);
			}

			return result;
		}

		private static void InterpretLuaItem(String name, LuaTable values)
		{
			Item newItem = new Item(name);
			newItem.Icon = LoadImage(ReadLuaString(values, "icon", true));

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
			try
			{
				float time = ReadLuaFloat(values, "energy_required", true, 0.5f);
				Dictionary<Item, float> ingredients = extractIngredientsFromLuaRecipe(values);
				Dictionary<Item, float> results = extractResultsFromLuaRecipe(values);

				Recipe newRecipe = new Recipe(name, time == 0.0f ? defaultRecipeTime : time, ingredients, results);

				newRecipe.Category = ReadLuaString(values, "category", true, "crafting");

				String iconFile = ReadLuaString(values, "icon", true);
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
			catch (MissingPrototypeValueException e)
			{
				ErrorLogging.LogLine(String.Format("Error reading value '{0}' from recipe prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
			}
		}

		private static void InterpretAssemblingMachine(String name, LuaTable values)
		{
			try
			{
				Assembler newAssembler = new Assembler(name);

				newAssembler.Icon = LoadImage(ReadLuaString(values, "icon", true));
				newAssembler.MaxIngredients = ReadLuaInt(values, "ingredient_count");
				newAssembler.ModuleSlots = ReadLuaInt(values, "module_slots", true, 0);
				newAssembler.Speed = ReadLuaFloat(values, "crafting_speed");

				LuaTable effects = ReadLuaLuaTable(values, "allowed_effects", true);
				if (effects != null)
				{
					foreach (String effect in effects.Values)
					{
						newAssembler.AllowedEffects.Add(effect);
					}
				}
				LuaTable categories = ReadLuaLuaTable(values, "crafting_categories");
				foreach (String category in categories.Values)
				{
					newAssembler.Categories.Add(category);
				}

				Assemblers.Add(newAssembler.Name, newAssembler);
			}
			catch (MissingPrototypeValueException e)
			{
				ErrorLogging.LogLine(String.Format("Error reading value '{0}' from assembler prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
			}
		}

		private static void InterpretFurnace(String name, LuaTable values)
		{
			try
			{
				Assembler newFurnace = new Assembler(name);

				newFurnace.Icon = LoadImage(ReadLuaString(values, "icon", true));
				newFurnace.MaxIngredients = 1;
				newFurnace.ModuleSlots = ReadLuaInt(values, "module_slots", true, 0);
				newFurnace.Speed = ReadLuaFloat(values, "crafting_speed");

				LuaTable categories = ReadLuaLuaTable(values, "crafting_categories");
				foreach (String category in categories.Values)
				{
					newFurnace.Categories.Add(category);
				}

				Assemblers.Add(newFurnace.Name, newFurnace);
			}
			catch (MissingPrototypeValueException e)
			{
				ErrorLogging.LogLine(String.Format("Error reading value '{0}' from furnace prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
			}
		}

		private static void InterpretMiner(String name, LuaTable values)
		{
			try
			{
				Miner newMiner = new Miner(name);

				newMiner.Icon = LoadImage(ReadLuaString(values, "icon", true));
				newMiner.MiningPower = ReadLuaFloat(values, "mining_power");
				newMiner.Speed = ReadLuaFloat(values, "mining_speed");
				newMiner.ModuleSlots = ReadLuaInt(values, "module_slots", true, 0);

				LuaTable categories = ReadLuaLuaTable(values, "resource_categories");
				if (categories != null)
				{
					foreach (String category in categories.Values)
					{
						newMiner.ResourceCategories.Add(category);
					}
				}

				Miners.Add(name, newMiner);
			}
			catch (MissingPrototypeValueException e)
			{
				ErrorLogging.LogLine(String.Format("Error reading value '{0}' from miner prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
			}
		}

		private static void InterpretResource(String name, LuaTable values)
		{
			try
			{
				Resource newResource = new Resource(name);
				newResource.Category = ReadLuaString(values, "category", true, "basic-solid");
				LuaTable minableTable = ReadLuaLuaTable(values, "minable", true);
				newResource.Hardness = ReadLuaFloat(minableTable, "hardness");
				newResource.Time = ReadLuaFloat(minableTable, "mining_time");

				if (minableTable["result"] != null)
				{
					newResource.result = ReadLuaString(minableTable, "result");
				}
				else
				{
					try
					{
						newResource.result = ((minableTable["results"] as LuaTable)[1] as LuaTable)["name"] as String;
					}
					catch (Exception e)
					{
						throw new MissingPrototypeValueException(minableTable, "results", e.Message);
					}
				}

				Resources.Add(name, newResource);
			}
			catch (MissingPrototypeValueException e)
			{
				ErrorLogging.LogLine(String.Format("Error reading value '{0}' from resource prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
			}
		}

		private static void InterpretModule(String name, LuaTable values)
		{
			try
			{
				float speedBonus = 0f;

				LuaTable effectTable = ReadLuaLuaTable(values, "effect");
				LuaTable speed = ReadLuaLuaTable(effectTable, "speed", true);
				if (speed != null)
				{
					speedBonus = ReadLuaFloat(speed, "bonus", true, -1f);
				}

				if (speed == null || speedBonus <= 0)
				{
					return;
				}

				Modules.Add(name, new Module(name, speedBonus));
			}
			catch (MissingPrototypeValueException e)
			{
				ErrorLogging.LogLine(String.Format("Error reading value '{0}' from module prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
			}
		}

		private static Dictionary<Item, float> extractResultsFromLuaRecipe(LuaTable values)
		{
			Dictionary<Item, float> results = new Dictionary<Item, float>();
			if (values["result"] != null)
			{
				String resultName = ReadLuaString(values, "result");
				float resultCount = ReadLuaFloat(values, "result_count", true);
				if (resultCount == 0f)
				{
					resultCount = 1f;
				}
				results.Add(LoadItemFromRecipe(resultName), resultCount);
			}
			else
			{
				var resultEnumerator = ReadLuaLuaTable(values, "results").GetEnumerator();
				while (resultEnumerator.MoveNext())
				{
					LuaTable resultTable = resultEnumerator.Value as LuaTable;
					results.Add(LoadItemFromRecipe(ReadLuaString(resultTable, "name")), ReadLuaFloat(resultTable, "amount"));
				}
			}

			return results;
		}

		private static Dictionary<Item, float> extractIngredientsFromLuaRecipe(LuaTable values)
		{
			Dictionary<Item, float> ingredients = new Dictionary<Item, float>();
			var ingredientEnumerator = ReadLuaLuaTable(values, "ingredients").GetEnumerator();
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
					name = ingredientTable[1] as String; //Name and amount often have no key in the prototype
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