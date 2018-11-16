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
using Foreman.Properties;
using System.ComponentModel;
using System.Threading;

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

        private static String DataPath { get { return Path.Combine(Properties.Settings.Default.FactorioPath, "data"); } }
        private static String ModPath { get { return Properties.Settings.Default.FactorioModPath; } }

        public static List<Mod> Mods = new List<Mod>();
        public static List<Language> Languages = new List<Language>();

        public static string Difficulty = "normal";

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

        public static void LoadAllData(List<String> enabledMods, CancellableProgress progress)
        {
            Clear();

	        progress.Report(0);
            using (Lua lua = new Lua())
            {
                FindAllMods(enabledMods, progress);

                AddLuaPackagePath(lua, Path.Combine(DataPath, "core", "lualib")); //Core lua functions
                String basePackagePath = lua["package.path"] as String;

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

                var modSettingsLua = ReadModSettings();
                lua.DoString(modSettingsLua);

                foreach (String filename in new String[] { "data.lua", "data-updates.lua", "data-final-fixes.lua" })
                {
                    foreach (Mod mod in Mods.Where(m => m.Enabled))
                    {
                        //Mods use relative paths, but if more than one mod is in package.path at once this can be ambiguous
                        lua["package.path"] = basePackagePath;
                        AddLuaPackagePath(lua, mod.dir);

                        //Because many mods use the same path to refer to different files, we need to clear the 'loaded' table so Lua doesn't think they're already loaded
                        lua.DoString(@"
							for k, v in pairs(package.loaded) do
								package.loaded[k] = false
							end");

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

                foreach (String type in new List<String> { "item", "fluid", "capsule", "module", "ammo", "gun", "armor", "blueprint", "deconstruction-item", "mining-tool", "repair-tool", "tool" })
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
                if (UnknownIcon == null)
                {
                    UnknownIcon = new Bitmap(32, 32);
                    using (Graphics g = Graphics.FromImage(UnknownIcon))
                    {
                        g.FillRectangle(Brushes.White, 0, 0, 32, 32);
                    }
                }

                LoadAllLanguages();
                LoadLocaleFiles();
            }

            MarkCyclicRecipes();
            progress.Report(100);

            ReportErrors();
        }

        private static string ReadModSettings()
        {
            var sb = new StringBuilder();
            sb.AppendLine("settings = {}");
            sb.AppendLine("settings.startup = {}");

            var settingsFile = Path.Combine(Properties.Settings.Default.FactorioModPath, "mod-settings.dat");

            if (!File.Exists(settingsFile))
            {
                ErrorLogging.LogLine($"Unable to find mod-settings.dat: {settingsFile}");
                return sb.ToString();
            }

            ushort major;
            ushort minor;
            ushort patch;
            ushort dev;

            FactorioPropertyTree propTree;

            using (BinaryReader reader = new BinaryReader(File.Open(settingsFile, FileMode.Open)))
            {
                major = reader.ReadUInt16();
                minor = reader.ReadUInt16();
                patch = reader.ReadUInt16();
                dev = reader.ReadUInt16();

                propTree = FactorioPropertyTree.Read(reader);
            }

            var startup = (Dictionary<string, FactorioPropertyTree>)((Dictionary<string, FactorioPropertyTree>)propTree.Content)["startup"].Content;

            const string braces = @"{}";

            foreach (var x in startup)
            {
                var format = $"settings.startup[\"{x.Key}\"]";
                sb.AppendLine($"{format} = {braces}");
                var valPropTree = ((Dictionary<string, FactorioPropertyTree>)x.Value.Content)["value"].Content;
                var type = valPropTree.GetType();
                var value = "";
                if (type.Name == "String")
                {
                    value = '"' + valPropTree.ToString() + '"';
                }
                else if (type.Name == "Boolean" || type.Name == "Double" || type.Name == "Float" || type.Name == "Integer")
                {
                    value = valPropTree.ToString();
                }
                else
                {
                    throw new Exception($"Unknown type {type.Name} found while parsing settings for key {x.Key}");
                }
                
                var assignment = $"{format}.value = {value}";
                sb.AppendLine(assignment);
            }

            return sb.ToString();
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
                string escapedDir = (dir + Path.DirectorySeparatorChar).Replace(@"\", @"\\").Replace("'", @"\'");
                string luaCommand = String.Format("package.path = package.path .. ';{0}?.lua'", escapedDir);
                lua.DoString(luaCommand);
            }
            catch (Exception e)
            {
                failedPathDirectories[dir] = e;
            }
        }

        public class ModOnDisk
        {
            public readonly string Location;
            public readonly ModType Type;

            public enum ModType { DIRECTORY, ZIP };

            public ModOnDisk(string location, ModType type)
            {
                this.Location = location;
                this.Type = type;
            }

            public static IEnumerable<ModOnDisk> EnumerateDirectories(string path)
            {
                if (Directory.Exists(path))
                {
                    return Directory.EnumerateDirectories(path).Select(x => new ModOnDisk(x, ModType.DIRECTORY));
                } else
                {
                    return Empty();
                }
            }

            public static IEnumerable<ModOnDisk> EnumerateZips(string path)
            {
                if (Directory.Exists(path))
                {
                    return Directory.EnumerateFiles(path, "*.zip").Select(x => new ModOnDisk(x, ModType.ZIP));
                } else
                {
                    return Empty();
                }
            }

            public static IEnumerable<ModOnDisk> Empty()
            {
                return Enumerable.Empty<ModOnDisk>();
            }
        }

        private static void reportingProgress<T>(CancellableProgress progress, int percentCoverage, IEnumerable<T> xs, Action<T> f) {
            float total = xs.Count();
            float i = 0;
            foreach (T x in xs)
            {
                if (progress.IsCancellationRequested)
                {
                    break;
                }
                f.Invoke(x);
                i += 1;
                progress.Report((int)(i / total * percentCoverage));
            }
        }

		private static void FindAllMods(List<String> enabledMods, CancellableProgress progress) //Vanilla game counts as a mod too.
		{
            String modPath = Properties.Settings.Default.FactorioModPath;
            IEnumerable<ModOnDisk> mods = ModOnDisk.Empty();

            mods = mods.Concat(ModOnDisk.EnumerateDirectories(DataPath));
            mods = mods.Concat(ModOnDisk.EnumerateDirectories(modPath));
            mods = mods.Concat(ModOnDisk.EnumerateZips(modPath));

            reportingProgress(progress, 50, mods, mod =>
            {
                switch (mod.Type) {
                    case ModOnDisk.ModType.DIRECTORY:
                        ReadModInfoFile(mod.Location);
                        break;
                    case ModOnDisk.ModType.ZIP:
                        ReadModInfoZip(mod.Location);
                        break;
                }
            });

            if (progress.IsCancellationRequested)
            {
                return;
            }

            Dictionary<String, bool> enabledModsFromFile = new Dictionary<string, bool>();

            string modListFile = Path.Combine(Properties.Settings.Default.FactorioModPath, "mod-list.json");
            if (File.Exists(modListFile))
            {
                String json = File.ReadAllText(modListFile);
                dynamic parsedJson = JsonConvert.DeserializeObject(json);
                foreach (var mod in parsedJson.mods)
                {
                    String name = mod.name;
                    Boolean enabled = (bool)mod.enabled;
                    enabledModsFromFile.Add(name, enabled);
                }
            }

            if (enabledMods != null)
            {
                foreach (Mod mod in Mods)
                {
                    mod.Enabled = enabledMods.Contains(mod.Name);
                }
            }
            else
            {
                Dictionary<String, String> splitModStrings = new Dictionary<string, string>();
                foreach (String s in Properties.Settings.Default.EnabledMods)
                {
                    var split = s.Split('|');
                    splitModStrings.Add(split[0], split[1]);
                }
                foreach (Mod mod in Mods)
                {
                    if (splitModStrings.ContainsKey(mod.Name))
                    {
                        mod.Enabled = (splitModStrings[mod.Name] == "True");
                    }
                    else if (enabledModsFromFile.ContainsKey(mod.Name))
                    {
                        mod.Enabled = enabledModsFromFile[mod.Name];
                    }
                    else
                    {
                        mod.Enabled = true;
                    }
                }
            }

            DependencyGraph modGraph = new DependencyGraph(Mods);
            modGraph.DisableUnsatisfiedMods();
            Mods = modGraph.SortMods();

            progress.Report(75);
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

                    if (!Version.TryParse(split[token], out newDependency.Version))
                    {
                        newDependency.Version = new Version(0, 0, 0, 0);
                    }
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
                                            LocaleFiles[currentIniSection][split[0].Trim()] = split[1].Trim();
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
            if (String.IsNullOrEmpty(fileName))
            { return null; }

            string fullPath = "";
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
                string[] splitPath = fileName.Split('/');
                splitPath[0] = splitPath[0].Trim('_');

                if (Mods.Any(m => m.Name == splitPath[0]))
                {
                    fullPath = Mods.First(m => m.Name == splitPath[0]).dir;
                }

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
            if (String.IsNullOrEmpty(name))
            {
                return;
            }
            Item newItem = new Item(name);
            var fileName = ReadLuaString(values, "icon", true);
            if (fileName == null)
            {
                var icons = ReadLuaLuaTable(values, "icons", true);
                if (icons != null)
                {
                    // TODO: Figure out how to either composite multiple icons
                    var first = (LuaTable)icons?[1];
                    if (first != null)
                    {
                        fileName = ReadLuaString(first, "icon", true);
                    }
                }
            }

            newItem.Icon = LoadImage(fileName);

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
                var timeSource = values[Difficulty] == null ? values : ReadLuaLuaTable(values, Difficulty, true);
                if (timeSource == null)
                {
                    ErrorLogging.LogLine($"Error reading recipe '{name}', unable to locate data table.");
                    return;
                }

                float time = ReadLuaFloat(timeSource, "energy_required", true, 0.5f);

                Dictionary<Item, float> ingredients = extractIngredientsFromLuaRecipe(values);
                Dictionary<Item, float> results = extractResultsFromLuaRecipe(values);

                if (name == null)
                    name = results.ElementAt(0).Key.Name;
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
                if (newAssembler.ModuleSlots == 0)
                {
                    var moduleTable = ReadLuaLuaTable(values, "module_specification", true);
                    if (moduleTable != null)
                    {
                        newAssembler.ModuleSlots = ReadLuaInt(moduleTable, "module_slots", true, 0);
                    }
                }
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

                foreach (String s in Properties.Settings.Default.EnabledAssemblers)
                {
                    if (s.Split('|')[0] == name)
                    {
                        newAssembler.Enabled = (s.Split('|')[1] == "True");
                    }
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
                if (newFurnace.ModuleSlots == 0)
                {
                    var moduleTable = ReadLuaLuaTable(values, "module_specification", true);
                    if (moduleTable != null)
                    {
                        newFurnace.ModuleSlots = ReadLuaInt(moduleTable, "module_slots", true, 0);
                    }
                }
                newFurnace.Speed = ReadLuaFloat(values, "crafting_speed", true, -1f);
                if (newFurnace.Speed == -1f)
                {   //In case we're still on Factorio 0.10
                    newFurnace.Speed = ReadLuaFloat(values, "smelting_speed");
                }

                LuaTable categories = ReadLuaLuaTable(values, "crafting_categories", true);
                if (categories == null)
                {   //Another 0.10 compatibility thing.
                    categories = ReadLuaLuaTable(values, "smelting_categories");
                }
                foreach (String category in categories.Values)
                {
                    newFurnace.Categories.Add(category);
                }

                foreach (String s in Properties.Settings.Default.EnabledAssemblers)
                {
                    if (s.Split('|')[0] == name)
                    {
                        newFurnace.Enabled = (s.Split('|')[1] == "True");
                    }
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
                if (newMiner.ModuleSlots == 0)
                {
                    var moduleTable = ReadLuaLuaTable(values, "module_specification", true);
                    if (moduleTable != null)
                    {
                        newMiner.ModuleSlots = ReadLuaInt(moduleTable, "module_slots", true, 0);
                    }
                }

                LuaTable categories = ReadLuaLuaTable(values, "resource_categories");
                if (categories != null)
                {
                    foreach (String category in categories.Values)
                    {
                        newMiner.ResourceCategories.Add(category);
                    }
                }

                foreach (String s in Properties.Settings.Default.EnabledMiners)
                {
                    if (s.Split('|')[0] == name)
                    {
                        newMiner.Enabled = (s.Split('|')[1] == "True");
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
                float productivityBonus = 0f;

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

		LuaTable productivity = ReadLuaLuaTable(effectTable, "productivity", true);
		if (productivity != null)
		{
			productivityBonus = ReadLuaFloat(productivity, "bonus", true, -1f);
		}

                var limitations = ReadLuaLuaTable(values, "limitation", true);
                List<String> allowedIn = null;
                if (limitations != null)
                {
                    allowedIn = new List<string>();
                    foreach (var recipe in limitations.Values)
                    {
                        allowedIn.Add((string)recipe);
                    }
                }

                Module newModule = new Module(name, speedBonus, productivityBonus, allowedIn);

                foreach (String s in Properties.Settings.Default.EnabledModules)
                {
                    if (s.Split('|')[0] == name)
                    {
                        newModule.Enabled = (s.Split('|')[1] == "True");
                    }
                }

                Modules.Add(name, newModule);
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

            LuaTable source = null;

            if (values[Difficulty] == null)
                source = values;
            else
            {
                var difficultyTable = ReadLuaLuaTable(values, Difficulty, true);
                if (difficultyTable?["result"] != null || difficultyTable?["results"] != null)
                    source = difficultyTable;
            }

            if (source?["result"] != null)
            {
                String resultName = ReadLuaString(source, "result");
                float resultCount = ReadLuaFloat(source, "result_count", true);
                if (resultCount == 0f)
                {
                    resultCount = 1f;
                }
                results.Add(FindOrCreateUnknownItem(resultName), resultCount);
            }
            else
            {
                // If we can't read results, try difficulty/results
                LuaTable resultsTable = ReadLuaLuaTable(source, "results", true);

                if (resultsTable != null)
                {
                    var resultEnumerator = resultsTable.GetEnumerator();
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
                            amount = ReadLuaFloat(resultTable, "amount");
                            //Just the average yield. Maybe in the future it should show more information about the probability
                            var probability = ReadLuaFloat(resultTable, "probability", true, 1.0f);
                            amount *= probability;
                        }
                        else if (resultTable["amount_min"] != null)
                        {
                            float probability = ReadLuaFloat(resultTable, "probability", true, 1f);
                            float amount_min = ReadLuaFloat(resultTable, "amount_min");
                            float amount_max = ReadLuaFloat(resultTable, "amount_max");
                            amount = ((amount_min + amount_max) / 2f) * probability;        //Just the average yield. Maybe in the future it should show more information about the probability
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
                else
                {
                    ErrorLogging.LogLine($"Error reading results from {values}.");
                }
            }
            return results;
        }

        private static Dictionary<Item, float> extractIngredientsFromLuaRecipe(LuaTable values)
        {
            Dictionary<Item, float> ingredients = new Dictionary<Item, float>();

            LuaTable ingredientsTable = ReadLuaLuaTable(values, "ingredients", true) ??
                                        ReadLuaLuaTable(ReadLuaLuaTable(values, Difficulty), "ingredients");

            var ingredientEnumerator = ingredientsTable.GetEnumerator();
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

        private static void MarkCyclicRecipes()
        {
            ProductionGraph testGraph = new ProductionGraph();
            foreach (Recipe recipe in Recipes.Values)
            {
                var node = RecipeNode.Create(recipe, testGraph);
            }
            testGraph.CreateAllPossibleInputLinks();
            foreach (var scc in testGraph.GetStronglyConnectedComponents(true))
            {
                foreach (var node in scc)
                {
                    ((RecipeNode)node).BaseRecipe.IsCyclic = true;
                }
            }
        }
    }
}
