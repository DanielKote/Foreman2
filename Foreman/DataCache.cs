using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using Foreman.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLua;

namespace Foreman
{
    internal class Language
    {
        private string localName;
        public string Name;

        public string LocalName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(localName))
                    return localName;
                return Name;
            }
            set { localName = value; }
        }
    }

    internal static class DataCache
    {
        private const float defaultRecipeTime = 0.5f;

        public static List<Mod> Mods = new List<Mod>();
        public static List<Language> Languages = new List<Language>();

        public static Dictionary<string, Item> Items = new Dictionary<string, Item>();
        public static Dictionary<string, Recipe> Recipes = new Dictionary<string, Recipe>();
        public static Dictionary<string, Assembler> Assemblers = new Dictionary<string, Assembler>();
        public static Dictionary<string, Miner> Miners = new Dictionary<string, Miner>();
        public static Dictionary<string, Resource> Resources = new Dictionary<string, Resource>();
        public static Dictionary<string, Module> Modules = new Dictionary<string, Module>();
        public static Dictionary<string, Inserter> Inserters = new Dictionary<string, Inserter>();
        private static readonly Dictionary<Bitmap, Color> colourCache = new Dictionary<Bitmap, Color>();
        public static Bitmap UnknownIcon;

        public static Dictionary<string, Dictionary<string, string>> LocaleFiles =
            new Dictionary<string, Dictionary<string, string>>();

        public static Dictionary<string, Exception> failedFiles = new Dictionary<string, Exception>();
        public static Dictionary<string, Exception> failedPathDirectories = new Dictionary<string, Exception>();

        public static Dictionary<string, byte[]> zipHashes = new Dictionary<string, byte[]>();

        private static string DataPath => Path.Combine(Settings.Default.FactorioPath, "data");

        private static string ModPath => Settings.Default.FactorioModPath;

        public static void LoadAllData(List<string> enabledMods)
        {
            Clear();

            using (var lua = new Lua())
            {
                FindAllMods(enabledMods);

                AddLuaPackagePath(lua, Path.Combine(DataPath, "core", "lualib")); //Core lua functions
                var basePackagePath = lua["package.path"] as string;

                var dataloaderFile = Path.Combine(DataPath, "core", "lualib", "dataloader.lua");
                try
                {
                    lua.DoFile(dataloaderFile);
                }
                catch (Exception e)
                {
                    failedFiles[dataloaderFile] = e;
                    ErrorLogging.LogLine(
                        $"Error loading dataloader.lua. This file is required to load any values from the prototypes. Message: '{e.Message}'");
                    return;
                }

                lua.DoString(@"
					function module(modname,...)
					end
	
					require ""util""
					util = {}
					util.table = {}
					util.table.deepcopy = table.deepcopy
					util.multiplystripes = multiplystripes
                    util.by_pixel = by_pixel
                    util.format_number = format_number
                    util.increment = increment

                    function log(...)
                    end

                    defines = {}
                    defines.difficulty_settings = {}
                    defines.difficulty_settings.recipe_difficulty = {}
                    defines.difficulty_settings.technology_difficulty = {}
                    defines.difficulty_settings.recipe_difficulty.normal = 1
                    defines.difficulty_settings.technology_difficulty.normal = 1
                    defines.direction = {}
                    defines.direction.north = 1
                    defines.direction.east = 2
                    defines.direction.south = 3
                    defines.direction.west = 4
");

                foreach (var filename in new[] {"data.lua", "data-updates.lua", "data-final-fixes.lua"})
                    foreach (var mod in Mods.Where(m => m.Enabled))
                    {
                        //Mods use relative paths, but if more than one mod is in package.path at once this can be ambiguous
                        lua["package.path"] = basePackagePath;
                        AddLuaPackagePath(lua, mod.dir);

                        //Because many mods use the same path to refer to different files, we need to clear the 'loaded' table so Lua doesn't think they're already loaded
                        lua.DoString(@"
							for k, v in pairs(package.loaded) do
								package.loaded[k] = false
							end");

                        var dataFile = Path.Combine(mod.dir, filename);
                        if (File.Exists(dataFile))
                            try
                            {
                                lua.DoFile(dataFile);
                            }
                            catch (Exception e)
                            {
                                failedFiles[dataFile] = e;
                            }
                    }

                //------------------------------------------------------------------------------------------
                // Lua files have all been executed, now it's time to extract their data from the lua engine
                //------------------------------------------------------------------------------------------

                foreach (
                    var type in
                    new List<string>
                    {
                        "item",
                        "fluid",
                        "capsule",
                        "module",
                        "ammo",
                        "gun",
                        "armor",
                        "blueprint",
                        "deconstruction-item",
                        "mining-tool",
                        "repair-tool",
                        "tool"
                    })
                    InterpretItems(lua, type);

                var recipeTable = lua.GetTable("data.raw")["recipe"] as LuaTable;
                if (recipeTable != null)
                {
                    var recipeEnumerator = recipeTable.GetEnumerator();
                    while (recipeEnumerator.MoveNext())
                        InterpretLuaRecipe(recipeEnumerator.Key as string, recipeEnumerator.Value as LuaTable);
                }

                var assemblerTable = lua.GetTable("data.raw")["assembling-machine"] as LuaTable;
                if (assemblerTable != null)
                {
                    var assemblerEnumerator = assemblerTable.GetEnumerator();
                    while (assemblerEnumerator.MoveNext())
                        InterpretAssemblingMachine(assemblerEnumerator.Key as string,
                            assemblerEnumerator.Value as LuaTable);
                }

                var furnaceTable = lua.GetTable("data.raw")["furnace"] as LuaTable;
                if (furnaceTable != null)
                {
                    var furnaceEnumerator = furnaceTable.GetEnumerator();
                    while (furnaceEnumerator.MoveNext())
                        InterpretFurnace(furnaceEnumerator.Key as string, furnaceEnumerator.Value as LuaTable);
                }

                var minerTable = lua.GetTable("data.raw")["mining-drill"] as LuaTable;
                if (minerTable != null)
                {
                    var minerEnumerator = minerTable.GetEnumerator();
                    while (minerEnumerator.MoveNext())
                        InterpretMiner(minerEnumerator.Key as string, minerEnumerator.Value as LuaTable);
                }

                var resourceTable = lua.GetTable("data.raw")["resource"] as LuaTable;
                if (resourceTable != null)
                {
                    var resourceEnumerator = resourceTable.GetEnumerator();
                    while (resourceEnumerator.MoveNext())
                        InterpretResource(resourceEnumerator.Key as string, resourceEnumerator.Value as LuaTable);
                }

                var moduleTable = lua.GetTable("data.raw")["module"] as LuaTable;
                if (moduleTable != null)
                    foreach (string moduleName in moduleTable.Keys)
                        InterpretModule(moduleName, moduleTable[moduleName] as LuaTable);

                UnknownIcon = LoadImage("UnknownIcon.png");
                if (UnknownIcon == null)
                {
                    UnknownIcon = new Bitmap(32, 32);
                    using (var g = Graphics.FromImage(UnknownIcon))
                    {
                        g.FillRectangle(Brushes.White, 0, 0, 32, 32);
                    }
                }

                LoadAllLanguages();
                LoadLocaleFiles();
            }

            MarkCyclicRecipes();

            ReportErrors();
        }

        private static void LoadAllLanguages()
        {
            var dirList = Directory.EnumerateDirectories(Path.Combine(Mods.First(m => m.Name == "core").dir, "locale"));

            foreach (var dir in dirList)
            {
                var newLanguage = new Language {Name = Path.GetFileName(dir)};
                try
                {
                    var infoJson = File.ReadAllText(Path.Combine(dir, "info.json"));
                    newLanguage.LocalName = (string) JObject.Parse(infoJson)["language-name"];
                }
                catch
                {
                }
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

        private static float ReadLuaFloat(LuaTable table, string key, bool canBeMissing = false, float defaultValue = 0f)
        {
            if (table[key] == null)
                if (canBeMissing)
                    return defaultValue;
                else
                    throw new MissingPrototypeValueException(table, key, "Key is missing");

            try
            {
                return Convert.ToSingle(table[key]);
            }
            catch (FormatException)
            {
                throw new MissingPrototypeValueException(table, key,
                    $"Expected a float, but the value ('{table[key]}') isn't one");
            }
        }

        private static int ReadLuaInt(LuaTable table, string key, bool canBeMissing = false, int defaultValue = 0)
        {
            if (table[key] == null)
                if (canBeMissing)
                    return defaultValue;
                else
                    throw new MissingPrototypeValueException(table, key, "Key is missing");

            try
            {
                return Convert.ToInt32(table[key]);
            }
            catch (FormatException)
            {
                throw new MissingPrototypeValueException(table, key,
                    $"Expected an Int32, but the value ('{table[key]}') isn't one");
            }
        }

        private static string ReadLuaString(LuaTable table, string key, bool canBeMissing = false,
            string defaultValue = null)
        {
            if (table[key] == null)
                if (canBeMissing)
                    return defaultValue;
                else
                    throw new MissingPrototypeValueException(table, key, "Key is missing");

            return Convert.ToString(table[key]);
        }

        private static LuaTable ReadLuaLuaTable(LuaTable table, string key, bool canBeMissing = false)
        {
            if (table[key] == null)
                if (canBeMissing)
                    return null;
                else
                    throw new MissingPrototypeValueException(table, key, "Key is missing");

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
                foreach (var dir in failedPathDirectories.Keys)
                    ErrorLogging.LogLine($"{dir} ({failedPathDirectories[dir].Message})");
            }

            if (failedFiles.Any())
            {
                ErrorLogging.LogLine("The following files could not be loaded due to errors:");
                foreach (var file in failedFiles.Keys)
                    ErrorLogging.LogLine($"{file} ({failedFiles[file].Message})");
            }
        }

        private static void AddLuaPackagePath(Lua lua, string dir)
        {
            try
            {
                var luaCommand = $"package.path = package.path .. ';{dir}{Path.DirectorySeparatorChar}?.lua'";
                luaCommand = luaCommand.Replace("\\", "\\\\");
                lua.DoString(luaCommand);
            }
            catch (Exception e)
            {
                failedPathDirectories[dir] = e;
            }
        }

        private static IEnumerable<string> getAllLuaFiles()
        {
            if (Directory.Exists(ModPath))
                foreach (var file in Directory.GetFiles(DataPath, "*.lua", SearchOption.AllDirectories))
                    yield return file;
            if (Directory.Exists(ModPath))
                foreach (var file in Directory.GetFiles(ModPath, "*.lua", SearchOption.AllDirectories))
                    yield return file;
        }

        private static void FindAllMods(List<string> enabledMods) //Vanilla game counts as a mod too.
        {
            if (Directory.Exists(DataPath))
                foreach (var dir in Directory.EnumerateDirectories(DataPath))
                    ReadModInfoFile(dir);
            if (Directory.Exists(Settings.Default.FactorioModPath))
            {
                foreach (var dir in Directory.EnumerateDirectories(Settings.Default.FactorioModPath))
                    ReadModInfoFile(dir);
                foreach (var zipFile in Directory.EnumerateFiles(Settings.Default.FactorioModPath, "*.zip"))
                    ReadModInfoZip(zipFile);
            }

            var enabledModsFromFile = new Dictionary<string, bool>();

            var modListFile = Path.Combine(Settings.Default.FactorioModPath, "mod-list.json");
            if (File.Exists(modListFile))
            {
                var json = File.ReadAllText(modListFile);
                dynamic parsedJson = JsonConvert.DeserializeObject(json);
                foreach (var mod in parsedJson.mods)
                {
                    string name = mod.name;
                    var enabled = (bool) mod.enabled;
                    enabledModsFromFile.Add(name, enabled);
                }
            }

            if (enabledMods != null)
            {
                foreach (var mod in Mods)
                    mod.Enabled = enabledMods.Contains(mod.Name);
            }
            else
            {
                var splitModStrings = new Dictionary<string, string>();
                foreach (var s in Settings.Default.EnabledMods)
                {
                    var split = s.Split('|');
                    splitModStrings.Add(split[0], split[1]);
                }
                foreach (var mod in Mods)
                    if (splitModStrings.ContainsKey(mod.Name))
                        mod.Enabled = splitModStrings[mod.Name] == "True";
                    else if (enabledModsFromFile.ContainsKey(mod.Name))
                        mod.Enabled = enabledModsFromFile[mod.Name];
                    else
                        mod.Enabled = true;
            }

            var modGraph = new DependencyGraph(Mods);
            modGraph.DisableUnsatisfiedMods();
            Mods = modGraph.SortMods();
        }

        private static void ReadModInfoFile(string dir)
        {
            try
            {
                if (!File.Exists(Path.Combine(dir, "info.json")))
                    return;
                var json = File.ReadAllText(Path.Combine(dir, "info.json"));
                ReadModInfo(json, dir);
            }
            catch (Exception)
            {
                ErrorLogging.LogLine($"The mod at '{dir}' has an invalid info.json file");
            }
        }

        private static void UnzipMod(string modZipFile)
        {
            var fullPath = Path.GetFullPath(modZipFile);
            byte[] hash;
            var needsExtraction = false;

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

            var outputDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(modZipFile));

            if (needsExtraction)
                using (var zip = ZipStorer.Open(modZipFile, FileAccess.Read))
                {
                    foreach (var fileEntry in zip.ReadCentralDir())
                        zip.ExtractFile(fileEntry, Path.Combine(outputDir, fileEntry.FilenameInZip));
                }
        }

        private static void ReadModInfoZip(string zipFile)
        {
            UnzipMod(zipFile);

            var file =
                Directory.EnumerateFiles(Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(zipFile)),
                    "info.json", SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(file))
                return;
            ReadModInfo(File.ReadAllText(file), Path.GetDirectoryName(file));
        }

        private static void ReadModInfo(string json, string dir)
        {
            var newMod = JsonConvert.DeserializeObject<Mod>(json);
            newMod.dir = dir;

            if (!Version.TryParse(newMod.version, out newMod.parsedVersion))
                newMod.parsedVersion = new Version(0, 0, 0, 0);
            ParseModDependencies(newMod);

            Mods.Add(newMod);
        }

        private static void ParseModDependencies(Mod mod)
        {
            if (mod.Name == "base")
                mod.dependencies.Add("core");

            foreach (var depString in mod.dependencies)
            {
                var token = 0;

                var newDependency = new ModDependency();

                var split = depString.Split(' ');

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

                    if (!Version.TryParse(split[token], out newDependency.Version))
                        newDependency.Version = new Version(0, 0, 0, 0);
                    token++;
                }

                mod.parsedDependencies.Add(newDependency);
            }
        }

        private static void InterpretItems(Lua lua, string typeName)
        {
            var itemTable = lua.GetTable("data.raw")[typeName] as LuaTable;

            var table = lua.GetTable("data.raw")["solar-panel"] as LuaTable;

            if (itemTable != null)
            {
                var enumerator = itemTable.GetEnumerator();
                while (enumerator.MoveNext())
                    InterpretLuaItem(enumerator.Key as string, enumerator.Value as LuaTable);
            }
        }

        public static void LoadLocaleFiles(string locale = "en")
        {
            foreach (var mod in Mods.Where(m => m.Enabled))
            {
                var localeDir = Path.Combine(mod.dir, "locale", locale);
                if (Directory.Exists(localeDir))
                    foreach (var file in Directory.GetFiles(localeDir, "*.cfg"))
                        try
                        {
                            using (var fStream = new StreamReader(file))
                            {
                                var currentIniSection = "none";

                                while (!fStream.EndOfStream)
                                {
                                    var line = fStream.ReadLine();
                                    if (line.StartsWith("[") && line.EndsWith("]"))
                                    {
                                        currentIniSection = line.Trim('[', ']');
                                    }
                                    else
                                    {
                                        if (!LocaleFiles.ContainsKey(currentIniSection))
                                            LocaleFiles.Add(currentIniSection, new Dictionary<string, string>());
                                        var split = line.Split('=');
                                        if (split.Count() == 2)
                                            LocaleFiles[currentIniSection][split[0].Trim()] = split[1].Trim();
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

        private static Bitmap LoadImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            var fullPath = "";
            if (File.Exists(fileName))
            {
                fullPath = fileName;
            }
            else if (File.Exists(Application.StartupPath + "\\" + fileName))
            {
                fullPath = Application.StartupPath + "\\" + fileName;
            }
            else
            {
                var splitPath = fileName.Split('/');
                splitPath[0] = splitPath[0].Trim('_');

                if (Mods.Any(m => m.Name == splitPath[0]))
                    fullPath = Mods.First(m => m.Name == splitPath[0]).dir;

                if (!string.IsNullOrEmpty(fullPath))
                    for (var i = 1; i < splitPath.Count(); i++)
                        //Skip the first split section because it's the mod name, not a directory
                        fullPath = Path.Combine(fullPath, splitPath[i]);
            }

            try
            {
                using (var image = new Bitmap(fullPath))
                    //If you don't do this, the file is locked for the lifetime of the bitmap
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
                return Color.LightGray;

            Color result;
            if (colourCache.ContainsKey(icon))
            {
                result = colourCache[icon];
            }
            else
            {
                using (var pixel = new Bitmap(1, 1))
                {
                    using (var g = Graphics.FromImage(pixel))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(icon, new Rectangle(0, 0, 1, 1));
                            //Scale the icon down to a 1-pixel image, which does the averaging for us
                        result = pixel.GetPixel(0, 0);
                    }
                }
                //Set alpha to 255, also lighten the colours to make them more pastel-y
                result = Color.FromArgb(255, result.R + (255 - result.R)/2, result.G + (255 - result.G)/2,
                    result.B + (255 - result.B)/2);
                colourCache.Add(icon, result);
            }

            return result;
        }

        private static void InterpretLuaItem(string name, LuaTable values)
        {
            if (string.IsNullOrEmpty(name))
                return;
            var newItem = new Item(name) {Icon = LoadImage(ReadLuaString(values, "icon", true))};

            var localNames = ReadLuaLuaTable(values, "localised_name", true);
            if (localNames != null)
            {
                if ((localNames[1] as string)?.Contains(".filled-barrel") == true)
                    return;
            }

            if (!Items.ContainsKey(name))
                Items.Add(name, newItem);
        }

        //This is only if a recipe references an item that isn't in the item prototypes (which shouldn't really happen)
        private static Item FindOrCreateUnknownItem(string itemName)
        {
            Item newItem;
            if (!Items.ContainsKey(itemName))
                Items.Add(itemName, newItem = new Item(itemName));
            else
                newItem = Items[itemName];
            return newItem;
        }

        private static void InterpretLuaRecipe(string name, LuaTable values)
        {
            try
            {
                var localNames = ReadLuaLuaTable(values, "localised_name", true);
                if (localNames != null)
                {
                    var localName = localNames[1] as string;
                    if (localName?.Contains(".fill-barrel") == true || localName?.Contains(".empty-filled-barrel") == true)
                        return;
                }

                float time;

                time = values["normal"] != null
                    ? ReadLuaFloat(ReadLuaLuaTable(values, "normal"), "energy_required", true, 0.5f)
                    : ReadLuaFloat(values, "energy_required", true, 0.5f);

                var ingredients = extractIngredientsFromLuaRecipe(values);
                var results = extractResultsFromLuaRecipe(values);

                if (name == null)
                    name = results.ElementAt(0).Key.Name;
                var newRecipe = new Recipe(name, time == 0.0f ? defaultRecipeTime : time, ingredients, results)
                {
                    Category = ReadLuaString(values, "category", true, "crafting")
                };


                var iconFile = ReadLuaString(values, "icon", true);
                if (iconFile != null)
                {
                    var icon = LoadImage(iconFile);
                    newRecipe.Icon = icon;
                }

                foreach (var result in results.Keys)
                    result.Recipes.Add(newRecipe);

                Recipes.Add(newRecipe.Name, newRecipe);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(
                    $"Error reading value '{e.Key}' from recipe prototype '{name}'. Returned error message: '{e.Message}'");
            }
        }

        private static void InterpretAssemblingMachine(string name, LuaTable values)
        {
            try
            {
                var newAssembler = new Assembler(name)
                {
                    Icon = LoadImage(ReadLuaString(values, "icon", true)),
                    MaxIngredients = ReadLuaInt(values, "ingredient_count"),
                    ModuleSlots = ReadLuaInt(values, "module_slots", true, 0)
                };

                if (newAssembler.ModuleSlots == 0)
                {
                    var moduleTable = ReadLuaLuaTable(values, "module_specification", true);
                    if (moduleTable != null)
                        newAssembler.ModuleSlots = ReadLuaInt(moduleTable, "module_slots", true, 0);
                }
                newAssembler.Speed = ReadLuaFloat(values, "crafting_speed");

                var effects = ReadLuaLuaTable(values, "allowed_effects", true);
                if (effects != null)
                    foreach (string effect in effects.Values)
                        newAssembler.AllowedEffects.Add(effect);
                var categories = ReadLuaLuaTable(values, "crafting_categories");
                foreach (string category in categories.Values)
                    newAssembler.Categories.Add(category);

                foreach (var s in Settings.Default.EnabledAssemblers)
                    if (s.Split('|')[0] == name)
                        newAssembler.Enabled = s.Split('|')[1] == "True";

                Assemblers.Add(newAssembler.Name, newAssembler);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(
                    $"Error reading value '{e.Key}' from assembler prototype '{name}'. Returned error message: '{e.Message}'");
            }
        }

        private static void InterpretFurnace(string name, LuaTable values)
        {
            try
            {
                var newFurnace = new Assembler(name)
                {
                    Icon = LoadImage(ReadLuaString(values, "icon", true)),
                    MaxIngredients = 1,
                    ModuleSlots = ReadLuaInt(values, "module_slots", true, 0)
                };

                if (newFurnace.ModuleSlots == 0)
                {
                    var moduleTable = ReadLuaLuaTable(values, "module_specification", true);
                    if (moduleTable != null)
                        newFurnace.ModuleSlots = ReadLuaInt(moduleTable, "module_slots", true, 0);
                }
                newFurnace.Speed = ReadLuaFloat(values, "crafting_speed", true, -1f);
                if (newFurnace.Speed == -1f)
                    newFurnace.Speed = ReadLuaFloat(values, "smelting_speed");

                var categories = ReadLuaLuaTable(values, "crafting_categories", true);
                if (categories == null)
                    categories = ReadLuaLuaTable(values, "smelting_categories");
                foreach (string category in categories.Values)
                    newFurnace.Categories.Add(category);

                foreach (var s in Settings.Default.EnabledAssemblers)
                    if (s.Split('|')[0] == name)
                        newFurnace.Enabled = s.Split('|')[1] == "True";

                Assemblers.Add(newFurnace.Name, newFurnace);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(
                    $"Error reading value '{e.Key}' from furnace prototype '{name}'. Returned error message: '{e.Message}'");
            }
        }

        private static void InterpretMiner(string name, LuaTable values)
        {
            try
            {
                var newMiner = new Miner(name)
                {
                    Icon = LoadImage(ReadLuaString(values, "icon", true)),
                    MiningPower = ReadLuaFloat(values, "mining_power"),
                    Speed = ReadLuaFloat(values, "mining_speed"),
                    ModuleSlots = ReadLuaInt(values, "module_slots", true, 0)
                };

                if (newMiner.ModuleSlots == 0)
                {
                    var moduleTable = ReadLuaLuaTable(values, "module_specification", true);
                    if (moduleTable != null)
                        newMiner.ModuleSlots = ReadLuaInt(moduleTable, "module_slots", true, 0);
                }

                var categories = ReadLuaLuaTable(values, "resource_categories");
                if (categories != null)
                    foreach (string category in categories.Values)
                        newMiner.ResourceCategories.Add(category);

                foreach (var s in Settings.Default.EnabledMiners)
                    if (s.Split('|')[0] == name)
                        newMiner.Enabled = s.Split('|')[1] == "True";

                Miners.Add(name, newMiner);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(
                    $"Error reading value '{e.Key}' from miner prototype '{name}'. Returned error message: '{e.Message}'");
            }
        }

        private static void InterpretResource(string name, LuaTable values)
        {
            try
            {
                if (values["minable"] == null)
                    return; //This means the resource is not usable by miners and is therefore not useful to us
                var newResource = new Resource(name) {Category = ReadLuaString(values, "category", true, "basic-solid")};
                var minableTable = ReadLuaLuaTable(values, "minable", true);
                newResource.Hardness = ReadLuaFloat(minableTable, "hardness");
                newResource.Time = ReadLuaFloat(minableTable, "mining_time");

                if (minableTable["result"] != null)
                    newResource.result = ReadLuaString(minableTable, "result");
                else
                    try
                    {
                        newResource.result = ((minableTable["results"] as LuaTable)[1] as LuaTable)["name"] as string;
                    }
                    catch (Exception e)
                    {
                        throw new MissingPrototypeValueException(minableTable, "results", e.Message);
                    }

                Resources.Add(name, newResource);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(
                    $"Error reading value '{e.Key}' from resource prototype '{name}'. Returned error message: '{e.Message}'");
            }
        }

        private static void InterpretModule(string name, LuaTable values)
        {
            try
            {
                var speedBonus = 0f;

                var effectTable = ReadLuaLuaTable(values, "effect");
                var speed = ReadLuaLuaTable(effectTable, "speed", true);
                if (speed != null)
                    speedBonus = ReadLuaFloat(speed, "bonus", true, -1f);

                if ((speed == null) || (speedBonus <= 0))
                    return;

                var newModule = new Module(name, speedBonus);

                foreach (var s in Settings.Default.EnabledModules)
                    if (s.Split('|')[0] == name)
                        newModule.Enabled = s.Split('|')[1] == "True";

                Modules.Add(name, newModule);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(
                    $"Error reading value '{e.Key}' from module prototype '{name}'. Returned error message: '{e.Message}'");
            }
        }

        private static void InterpretInserter(string name, LuaTable values)
        {
            try
            {
                var rotationSpeed = ReadLuaFloat(values, "rotation_speed");
                var newInserter = new Inserter(name)
                {
                    RotationSpeed = rotationSpeed,
                    Icon = LoadImage(ReadLuaString(values, "icon", true))
                };

                Inserters.Add(name, newInserter);
            }
            catch (MissingPrototypeValueException e)
            {
                ErrorLogging.LogLine(
                    $"Error reading value '{e.Key}' from inserter prototype '{name}'. Returned error message: '{e.Message}'");
            }
        }

        private static Dictionary<Item, float> extractResultsFromLuaRecipe(LuaTable values)
        {
            var results = new Dictionary<Item, float>();
            if (values["result"] != null)
            {
                var resultName = ReadLuaString(values, "result");
                var resultCount = ReadLuaFloat(values, "result_count", true);
                if (resultCount == 0f)
                    resultCount = 1f;
                results.Add(FindOrCreateUnknownItem(resultName), resultCount);
            }
            else if (ReadLuaLuaTable(values, "results", true) == null)
            {
                var table = ReadLuaLuaTable(values, "normal");
                if (table["result"] == null) return results;
                var resultName = ReadLuaString(table, "result");
                var resultCount = ReadLuaFloat(table, "result_count", true);
                if (resultCount == 0f)
                {
                    resultCount = 1f;
                }
                results.Add(FindOrCreateUnknownItem(resultName), resultCount);
            }
            else
            {
                //If we can't read results, read normal/results.
                LuaTable luaTable = ReadLuaLuaTable(values, "results", true) ??
                                    ReadLuaLuaTable(ReadLuaLuaTable(values, "normal"), "results");

                var resultEnumerator = luaTable.GetEnumerator();
                while (resultEnumerator.MoveNext())
                {
                    var resultTable = resultEnumerator.Value as LuaTable;
                    Item result;
                    if (resultTable["name"] != null)
                        result = FindOrCreateUnknownItem(ReadLuaString(resultTable, "name"));
                    else
                        result = FindOrCreateUnknownItem((string) resultTable[1]);

                    var amount = 0f;
                    if (resultTable["amount"] != null)
                    {
                        amount = ReadLuaFloat(resultTable, "amount");
                    }
                    else if (resultTable["amount_min"] != null)
                    {
                        var probability = ReadLuaFloat(resultTable, "probability", true, 1f);
                        var amount_min = ReadLuaFloat(resultTable, "amount_min");
                        var amount_max = ReadLuaFloat(resultTable, "amount_max");
                        amount = (amount_min + amount_max)/2f*probability;
                            //Just the average yield. Maybe in the future it should show more information about the probability
                    }
                    else
                    {
                        amount = Convert.ToSingle(resultTable[2]);
                    }

                    if (results.ContainsKey(result))
                        results[result] += amount;
                    else
                        results.Add(result, amount);
                }
            }
            return results;
        }

        private static Dictionary<Item, float> extractIngredientsFromLuaRecipe(LuaTable values)
        {
            var ingredients = new Dictionary<Item, float>();

            //if ingredients can't be read, read normal.ingredients instead
            LuaTable ingredientsTable = ReadLuaLuaTable(values, "ingredients", true) ??
                ReadLuaLuaTable(ReadLuaLuaTable(values, "normal"), "ingredients");

            var ingredientEnumerator = ingredientsTable.GetEnumerator();
            while (ingredientEnumerator.MoveNext())
            {
                var ingredientTable = ingredientEnumerator.Value as LuaTable;
                string name;
                float amount;
                if (ingredientTable["name"] != null)
                    name = ingredientTable["name"] as string;
                else
                    name = ingredientTable[1] as string; //Name and amount often have no key in the prototype
                if (ingredientTable["amount"] != null)
                    amount = Convert.ToSingle(ingredientTable["amount"]);
                else
                    amount = Convert.ToSingle(ingredientTable[2]);
                var ingredient = FindOrCreateUnknownItem(name);
                if (!ingredients.ContainsKey(ingredient))
                    ingredients.Add(ingredient, amount);
                else
                    ingredients[ingredient] += amount;
            }

            return ingredients;
        }

        private static void MarkCyclicRecipes()
        {
            var testGraph = new ProductionGraph();
            foreach (var recipe in Recipes.Values)
            {
                var node = RecipeNode.Create(recipe, testGraph);
            }
            testGraph.CreateAllPossibleInputLinks();
            foreach (var scc in testGraph.GetStronglyConnectedComponents(true))
                foreach (var node in scc)
                    ((RecipeNode) node).BaseRecipe.IsCyclic = true;
        }

        private class MissingPrototypeValueException : Exception
        {
            public readonly string Key;
            public LuaTable Table;

            public MissingPrototypeValueException(LuaTable table, string key, string message = "")
                : base(message)
            {
                Table = table;
                Key = key;
            }
        }
    }
}