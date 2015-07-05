using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Foreman
{
	class Language
	{
		public String Name;
		private String localName;
		public String LocalName
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(localName))
				{
					return localName;
				}
				else
				{
					return Name;
				}
			}
			set
			{
				localName = value;
			}
		}
	}

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

		private static String DataPath { get { return Properties.Settings.Default.FactorioDataPath; } }
		private static String ModPath { get { return Properties.Settings.Default.FactorioModPath; } }

		public static List<Mod> Mods = new List<Mod>();
		public static List<Language> Languages = new List<Language>();
		
		public static Dictionary<String, Item> Items = new Dictionary<String, Item>();
		public static Dictionary<String, Recipe> Recipes = new Dictionary<String, Recipe>();
		public static Dictionary<String, Assembler> Assemblers = new Dictionary<string, Assembler>();
		public static Dictionary<String, Miner> Miners = new Dictionary<string, Miner>();
		public static Dictionary<String, Resource> Resources = new Dictionary<string, Resource>();
		public static Dictionary<String, Module> Modules = new Dictionary<string, Module>();
		public static Dictionary<String, Inserter> Inserters = new Dictionary<string, Inserter>();

		private const float defaultRecipeTime = 0.5f;
		private static Dictionary<Bitmap, Color> colourCache = new Dictionary<Bitmap, Color>();
		public static Bitmap UnknownIcon;
		public static Dictionary<String, Dictionary<String, String>> LocaleFiles = new Dictionary<string, Dictionary<string, string>>();

		public static Dictionary<String, Exception> failedFiles = new Dictionary<string, Exception>();
		public static Dictionary<String, Exception> failedPathDirectories = new Dictionary<string, Exception>();

		public static Dictionary<String, byte[]> zipHashes = new Dictionary<string, byte[]>();

		public static void LoadAllData(List<String> enabledMods)
		{
			Clear();

			using (Lua lua = new Lua())
			{
				FindAllMods(enabledMods);

				foreach (Mod mod in Mods.Where(m => m.Enabled))
				{
					AddLuaPackagePath(lua, mod.dir); //Prototype folder matches package hierarchy so this is enough.
				}
				AddLuaPackagePath(lua, Path.Combine(DataPath, "core", "lualib")); //Core lua functions

				String dataloaderFile = Path.Combine(DataPath, "core", "lualib", "dataloader.lua");
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

				Regex requiresRegex = new Regex("require *\\(?(\"|')(?<modulename>[^\\.\"']+)(\"|')\\)?");

				foreach (String filename in new String[] { "data.lua", "data-updates.lua", "data-final-fixes.lua" })
				{
					foreach (Mod mod in Mods.Where(m => m.Enabled))
					{
						String dataFile = Path.Combine(mod.dir, filename);
						if (File.Exists(dataFile))
						{
							try
							{
								String fileContents = File.ReadAllText(dataFile);

								var matches = requiresRegex.Matches(fileContents);	//This whole section is a dirty hack to force lua to run modules that it thinks have already been loaded (e.g. "config.lua" in DyTech)
								foreach (Match match in matches)
								{
									String moduleName = match.Groups["modulename"].Value;
									String moduleFileName = Path.Combine(mod.dir, match.Groups["modulename"].Value + ".lua");
									if (File.Exists(moduleFileName))
									{
										lua.DoFile(moduleFileName);
									}
								}

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

				LoadAllLanguages();
				LoadLocaleFiles();
			}

			ReportErrors();
		}

		private static void LoadAllLanguages()
		{
			var dirList = Directory.EnumerateDirectories(Path.Combine(Mods.First(m => m.Name == "core").dir, "locale"));

			foreach (String dir in dirList)
			{
				Language newLanguage = new Language();
				newLanguage.Name = Path.GetFileName(dir);
				try
				{
					String infoJson = File.ReadAllText(Path.Combine(dir, "info.json"));
					newLanguage.LocalName = (String)JObject.Parse(infoJson)["language-name"];
				}
				catch { }
				Languages.Add(newLanguage);
			}
		}

		public static void Clear()
		{
			Mods.Clear();
			Items.Clear();
			Recipes.Clear();
			Assemblers.Clear();
			Miners.Clear();
			Resources.Clear();
			Modules.Clear();
			colourCache.Clear();
			LocaleFiles.Clear();
			failedFiles.Clear();
			failedPathDirectories.Clear();
			Inserters.Clear();
			Languages.Clear();
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
			catch (FormatException)
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
			catch (FormatException)
			{
				throw new MissingPrototypeValueException(table, key, String.Format("Expected an Int32, but the value ('{0}') isn't one", table[key]));
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
			catch (Exception)
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
			if (Directory.Exists(ModPath))
			{
				foreach (String file in Directory.GetFiles(DataPath, "*.lua", SearchOption.AllDirectories))
				{
					yield return file;
				}
			}
			if (Directory.Exists(ModPath))
			{
				foreach (String file in Directory.GetFiles(ModPath, "*.lua", SearchOption.AllDirectories))
				{
					yield return file;
				}
			}
		}

		private static void FindAllMods(List<String> enabledMods) //Vanilla game counts as a mod too.
		{
			if (Directory.Exists(DataPath))
			{
				foreach (String dir in Directory.EnumerateDirectories(DataPath))
				{
					ReadModInfoFile(dir);
				}
			}
			if (Directory.Exists(Properties.Settings.Default.FactorioModPath))
			{
				foreach (String dir in Directory.EnumerateDirectories(Properties.Settings.Default.FactorioModPath))
				{
					ReadModInfoFile(dir);
				}
				foreach (String zipFile in Directory.EnumerateFiles(Properties.Settings.Default.FactorioModPath, "*.zip"))
				{
					ReadModInfoZip(zipFile);
					
				}
				string modListFile = Path.Combine(Properties.Settings.Default.FactorioModPath, "mod-list.json");
				if (File.Exists(modListFile))
				{
					String json = File.ReadAllText(modListFile);
					dynamic parsedJson = JsonConvert.DeserializeObject(json);
					foreach (var mod in parsedJson.mods)
					{
						string modName = mod.name;
						bool enabled = mod.enabled;

						if (Mods.Any(m => m.Name == modName))
						{
							Mods.First(m => m.Name == modName).Enabled = enabled;
						}
					}
				}
			}

			if (enabledMods != null)
			{
				foreach (Mod mod in Mods)
				{
					mod.Enabled = enabledMods.Contains(mod.Name);
				}
			}

			DependencyGraph modGraph = new DependencyGraph(Mods);
			modGraph.DisableUnsatisfiedMods();
			Mods = modGraph.SortMods();
		}
		
		private static void ReadModInfoFile(String dir)
		{
			try
			{
				if (!File.Exists(Path.Combine(dir, "info.json")))
				{
					return;
				}
				String json = File.ReadAllText(Path.Combine(dir, "info.json"));
				ReadModInfo(json, dir);
			}
			catch (Exception)
			{
				ErrorLogging.LogLine(String.Format("The mod at '{0}' has an invalid info.json file", dir));
			}
		}

		private static void UnzipMod(String modZipFile)
		{
			String fullPath = Path.GetFullPath(modZipFile);
			byte[] hash;
			Boolean needsExtraction = false;

			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(fullPath))
				{
					hash = md5.ComputeHash(stream);
				}
			}

			if (zipHashes.ContainsKey(fullPath))
			{
				if (!zipHashes[fullPath].SequenceEqual(hash))
				{
					needsExtraction = true;
					zipHashes[fullPath] = hash;
				}
			}
			else
			{
				needsExtraction = true;
				zipHashes.Add(fullPath, hash);
			}

			String outputDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(modZipFile));

			if (needsExtraction)
			{
				using (ZipStorer zip = ZipStorer.Open(modZipFile, FileAccess.Read))
				{
					foreach (var fileEntry in zip.ReadCentralDir())
					{
						zip.ExtractFile(fileEntry, Path.Combine(outputDir, fileEntry.FilenameInZip));
					}
				}
			}
		}

		private static void ReadModInfoZip(String zipFile)
		{
			UnzipMod(zipFile);

			String file = Directory.EnumerateFiles(Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(zipFile)), "info.json", SearchOption.AllDirectories).FirstOrDefault();
			if (String.IsNullOrWhiteSpace(file))
			{
				return;
			}
			ReadModInfo(File.ReadAllText(file), Path.GetDirectoryName(file));
		}

		private static void ReadModInfo(String json, String dir)
		{
			Mod newMod = JsonConvert.DeserializeObject<Mod>(json);
			newMod.dir = dir;

			if (!Version.TryParse(newMod.version, out newMod.parsedVersion))
			{
				newMod.parsedVersion = new Version(0, 0, 0, 0);
			}
			ParseModDependencies(newMod);

			Mods.Add(newMod);
		}

		private static void ParseModDependencies(Mod mod)
		{
			if (mod.Name == "base")
			{
				mod.dependencies.Add("core");
			}

			foreach (String depString in mod.dependencies)
			{
				int token = 0;

				ModDependency newDependency = new ModDependency();

				string[] split = depString.Split(' ');

				if (split[token] == "?")
				{
					newDependency.Optional = true;
					token++;
				}

				newDependency.ModName = split[token];
				token++;

				if (split.Count() == token + 2)
				{
					switch (split[token])
					{
						case "=":
							newDependency.VersionType = DependencyType.EqualTo;
							break;
						case ">":
							newDependency.VersionType = DependencyType.GreaterThan;
							break;
						case ">=":
							newDependency.VersionType = DependencyType.GreaterThanOrEqual;
							break;
					}
					token++;

					newDependency.Version = Version.Parse(split[token]);
					token++;
				}

				mod.parsedDependencies.Add(newDependency);
			}
		}
		
		private static void InterpretItems(Lua lua, String typeName)
		{
			LuaTable itemTable = lua.GetTable("data.raw")[typeName] as LuaTable;

			var table = lua.GetTable("data.raw")["solar-panel"] as LuaTable;

			if (itemTable != null)
			{
				var enumerator = itemTable.GetEnumerator();
				while (enumerator.MoveNext())
				{
					InterpretLuaItem(enumerator.Key as String, enumerator.Value as LuaTable);
				}
			}
		}

		public static void LoadLocaleFiles(String locale = "en")
		{
			foreach (Mod mod in Mods.Where(m => m.Enabled))
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
				fullPath = Mods.FirstOrDefault(m => m.Name == splitPath[0]).dir;

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
				using (Bitmap image = new Bitmap(fullPath)) //If you don't do this, the file is locked for the lifetime of the bitmap
				{
					return new Bitmap(image);
				}
			}
			catch (Exception)
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
		private static Item FindOrCreateUnknownItem(String itemName)
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
				newFurnace.Speed = ReadLuaFloat(values, "crafting_speed", true, -1f);
				if (newFurnace.Speed == -1f)
				{	//In case we're still on Factorio 0.10
					newFurnace.Speed = ReadLuaFloat(values, "smelting_speed");
				}

				LuaTable categories = ReadLuaLuaTable(values, "crafting_categories", true);
				if (categories == null)
				{	//Another 0.10 compatibility thing.
					categories = ReadLuaLuaTable(values, "smelting_categories");
				}
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
				if (values["minable"] == null)
				{
					return; //This means the resource is not usable by miners and is therefore not useful to us
				}
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

		private static void InterpretInserter(String name, LuaTable values)
		{
			try
			{
				float rotationSpeed = ReadLuaFloat(values, "rotation_speed");
				Inserter newInserter = new Inserter(name);
				newInserter.RotationSpeed = rotationSpeed;
				newInserter.Icon = LoadImage(ReadLuaString(values, "icon", true));

				Inserters.Add(name, newInserter);
			}
			catch (MissingPrototypeValueException e)
			{
				ErrorLogging.LogLine(String.Format("Error reading value '{0}' from inserter prototype '{1}'. Returned error message: '{2}'", e.Key, name, e.Message));
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
				results.Add(FindOrCreateUnknownItem(resultName), resultCount);
			}
			else
			{
				var resultEnumerator = ReadLuaLuaTable(values, "results").GetEnumerator();
				while (resultEnumerator.MoveNext())
				{
					LuaTable resultTable = resultEnumerator.Value as LuaTable;
					Item result;
					if (resultTable["name"] != null)
					{
						result = FindOrCreateUnknownItem(ReadLuaString(resultTable, "name"));
					}
					else
					{
						result = FindOrCreateUnknownItem((string)resultTable[1]);
					}

					float amount = 0f;
						if (resultTable["amount"] != null)
						{
							if (results.ContainsKey(result))
							{
								results[result] += ReadLuaFloat(resultTable, "amount");
							}
							else
							{
								results.Add(result, ReadLuaFloat(resultTable, "amount"));
							}
						}
						else if (resultTable["probability"] != null)
						{
							float amount_min = ReadLuaFloat(resultTable, "amount_min");
							float amount_max = ReadLuaFloat(resultTable, "amount_max");
							float probability = ReadLuaFloat(resultTable, "probability");
							amount = ((amount_min + amount_max) / 2) * probability; //Just the average yield. Maybe in the future it should show more information about the probability
						}
						else
						{
							amount = Convert.ToSingle(resultTable[2]);
						}

						if (results.ContainsKey(result))
						{
							results[result] += amount;
						}
						else
						{
							results.Add(result, amount);
						}
					
					
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
				Item ingredient = FindOrCreateUnknownItem(name);
				if (!ingredients.ContainsKey(ingredient))
				{
					ingredients.Add(ingredient, amount);
				}
				else
				{
					ingredients[ingredient] += amount;
				}
			}

			return ingredients;
		}
	}
}