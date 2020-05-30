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

        private static String DataPath { get { return Path.Combine(Settings.Default.FactorioPath, "data"); } }
        private static String ModPath { get { return Settings.Default.FactorioModPath; } }

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
                AddLuaPackagePath(lua, Path.Combine(DataPath, "base")); // Base mod
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

                // Added a custom require function to support relative paths (angels refining was using this and a few others)
                // Defines is defined here: https://lua-api.factorio.com/latest/defines.html 
                // Also contains a special case for "__base__" that can resolve to the base mod
                lua.DoString(@"
                    local oldrequire = require

                    function relative_require(modname)
                      if string.match(modname, '__.+__/') then
                        return oldrequire(string.gsub(modname, '__.+__/', ''))
                      end
                      local regular_loader = package.searchers[2]
                      local loader = function(inner)
                        if string.match(modname, '(.*)%.') then
                          return regular_loader(string.match(modname, '(.*)%.') .. '.' .. inner)
                        end
                      end

                      table.insert(package.searchers, 1, loader)
                      local retval = oldrequire(modname)
                      table.remove(package.searchers, 1)

                      return retval
                    end
                    _G.require = relative_require

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

                    _G.unpack = table.unpack;

                    function log(...)
                    end

                    
                    defines = {}
                    defines.alert_type = {}
                    defines.behavior_result = {}
                    defines.build_check_type = {}
                    defines.chain_signal_state = {}
                    defines.chunk_generated_status = {}
                    defines.circuit_condition_index = {}
                    defines.circuit_connector_id = {}
                    defines.command = {}
                    defines.compound_command = {}
                    defines.control_behavior = {}
                    defines.control_behavior.inserter = {}
                    defines.control_behavior.inserter.circuit_mode_of_operation = {}
                    defines.control_behavior.inserter.hand_read_mode = {}
                    defines.control_behavior.logistics_container = {}
                    defines.control_behavior.logistics_container.circuit_mode_of_operation = {}
                    defines.control_behavior.lamp = {}
                    defines.control_behavior.lamp.circuit_mode_of_operation = {}
                    defines.control_behavior.mining_drill = {}
                    defines.control_behavior.mining_drill.resource_read_mode = {}
                    defines.control_behavior.transport_belt = {}
                    defines.control_behavior.transport_belt.content_read_mode = {}
                    defines.control_behavior.type = {}
                    defines.controllers = {}
                    defines.deconstruction_item = {}
                    defines.deconstruction_item.entity_filter_mode = {}
                    defines.deconstruction_item.tile_filter_mode = {}
                    defines.deconstruction_item.tile_selection_mode = {}
                    defines.difficulty = {}
                    defines.difficulty_settings = {}
                    defines.difficulty_settings.recipe_difficulty = {}
                    defines.difficulty_settings.technology_difficulty = {}
                    defines.difficulty_settings.recipe_difficulty.normal = 1
                    defines.difficulty_settings.technology_difficulty.normal = 1
                    defines.distraction = {}
                    defines.direction = {}
                    defines.direction.north = 1
                    defines.direction.east = 2
                    defines.direction.south = 3
                    defines.direction.west = 4
                    defines.entity_status = {}
                    defines.events = {}
                    defines.flow_precision_index = {}
                    defines.group_state = {}
                    defines.gui_type = {}
                    defines.input_action = {}
                    defines.inventory = {}
                    defines.logistic_member_index = {}
                    defines.logistic_mode = {}
                    defines.mouse_button_type = {}
                    defines.rail_connection_direction = {}
                    defines.rail_direction = {}
                    defines.render_mode	= {}
                    defines.rich_text_setting = {}
                    defines.riding = {}
                    defines.riding.acceleration = {}
                    defines.riding.direction = {}
                    defines.shooting = {}
                    defines.signal_state = {}
                    defines.train_state	 = {}
                    defines.transport_line = {}
                    defines.wire_connection_id = {}
                    defines.wire_type = {}
");

                // Should be mod settings generated by Factorio. It looks at settings.lua, settings-updates.lua, and settings-final-fixed.lua.
                var modSettingsLua = ReadModSettings();
                lua.DoString(modSettingsLua);

                IEnumerable<Mod> newEnabledMods = Mods.Where(m => m.Enabled);

                // Create global dictionary of mods
                String luaCode = "mods = {}\n";
                foreach (Mod m in newEnabledMods)
                {
                    luaCode += $"mods['{m.Name}'] = '{m.parsedVersion.ToString()}'\n";
                }
                lua.DoString(luaCode);

                // Note: All mods "settings" file is processed first then data, then date-updates, etc.
                // This allows things such as the base mod to generate barrel recipes after other mods have defined liquids
                foreach (String filename in new String[] { "data.lua", "data-updates.lua", "data-final-fixes.lua" })
                {
                    foreach (Mod mod in newEnabledMods)
                    {
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

                progress.Report(80);

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

            progress.Report(90);
            MarkCyclicRecipes();
            progress.Report(100);

            ReportErrors();
        }

        private static string ReadModSettings()
        {
            var sb = new StringBuilder();
            sb.AppendLine("settings = {}");
            sb.AppendLine("settings.startup = {}");

            var settingsFile = Path.Combine(Settings.Default.FactorioModPath, "mod-settings.dat");

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

                if (minor >= 17 )
                {
                    var noop = reader.ReadByte();
                }

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
                foreach (String dir in failedPathDirectories.Keys)
                {
                    ErrorLogging.LogLine(String.Format("{0} ({1})", dir, failedPathDirectories[dir].Message));
                }
            }

            if (failedFiles.Any())
            {
                ErrorLogging.LogLine("The following files could not be loaded due to errors:");
                foreach (String file in failedFiles.Keys)
                {
                    ErrorLogging.LogLine(String.Format("{0} ({1})", file, failedFiles[file].Message));
                }
            }
        }

        private static void AddLuaPackagePath(Lua lua, string dir)
        {
            try
            {
                string escapedDir = (dir + Path.DirectorySeparatorChar).Replace(@"\", @"\\").Replace("'", @"\'");
                // Inserting the given path at the front of package.path. Without this mods might import files from other mods.
                string luaCommand = String.Format("package.path = '{0}?.lua;' .. package.path", escapedDir);
                lua.DoString(luaCommand);
            }
            catch (Exception e)
            {
                failedPathDirectories[dir] = e;
            }
        }

        public class ModOnDisk
        {
            public readonly string Id;
            public readonly Version Version;
            public readonly string Location;
            public readonly ModType Type;

            public enum ModType { DIRECTORY, ZIP };

            public ModOnDisk(string location, ModType type)
            {
                String[] parts = Path.GetFileNameWithoutExtension(location).Split('_');
                this.Id = parts.Count() > 1 ? String.Join("_", parts.Take(parts.Length - 1)) : parts[0];
                try
                {
                    this.Version = new Version(parts.Last());
                }
                catch (Exception e)
                {
                    this.Version = new Version(0, 0);
                }
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
            String modPath = Settings.Default.FactorioModPath;
            IEnumerable<ModOnDisk> mods = ModOnDisk.Empty();

            mods = mods.Concat(ModOnDisk.EnumerateDirectories(DataPath));
            mods = mods.Concat(ModOnDisk.EnumerateDirectories(modPath));
            mods = mods.Concat(ModOnDisk.EnumerateZips(modPath));

            IEnumerable<ModOnDisk> latestMods = ChangeModsToOnlyLatest(mods);

            if (enabledMods == null)
            {
                enabledMods = new List<string>();
                if (Settings.Default.EnabledMods.Count > 0)
                {
                    foreach (String s in Settings.Default.EnabledMods)
                    {
                        var split = s.Split('|');
                        if (split[1] == "True")
                            enabledMods.Add(split[0]);
                    }
                }
                else
                {
                    string modListFile = Path.Combine(modPath, "mod-list.json");
                    if (File.Exists(modListFile))
                    {
                        String json = File.ReadAllText(modListFile);
                        dynamic parsedJson = JsonConvert.DeserializeObject(json);
                        foreach (var mod in parsedJson.mods)
                        {
                            if ((bool)mod.enabled)
                                enabledMods.Add((string)mod.name);
                        }
                    }
                }
            }

            reportingProgress(progress, 75, latestMods, mod =>
            {
                switch (mod.Type) {
                    case ModOnDisk.ModType.DIRECTORY:
                        ReadModInfoFile(mod.Location);
                        break;
                    case ModOnDisk.ModType.ZIP: 
                        // Unzipping is very expensive only do if we have to
                        if (enabledMods.Contains(mod.Id))
                        {
                            ReadModInfoZip(mod.Location);
                        }
                        else
                        {
                            Mod newMod = new Mod();
                            newMod.Id = mod.Id;
                            newMod.parsedVersion = mod.Version;
                            newMod.Name = newMod.Id;
                            newMod.Enabled = false;
                            Mods.Add(newMod);
                        }

                        break;
                }
            });

            if (progress.IsCancellationRequested)
            {
                return;
            }

            foreach (Mod mod in Mods)
            {
                mod.Enabled = enabledMods.Contains(mod.Name);
            }

            DependencyGraph modGraph = new DependencyGraph(Mods);
            modGraph.DisableUnsatisfiedMods();
            Mods = modGraph.SortMods();

            progress.Report(80);
        }

        private static IEnumerable<ModOnDisk> ChangeModsToOnlyLatest(IEnumerable<ModOnDisk> mods)
        {
            List<ModOnDisk> latest = new List<ModOnDisk>();
            foreach (ModOnDisk mod in mods)
            {
                ModOnDisk found = latest.Find(m => m.Id == mod.Id);
                if (found == null || found.Version < mod.Version)
                {
                    latest.Remove(found);
                    latest.Add(mod);
                }
            }
            return latest;
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

            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fullPath))
                {
                    hash = md5.ComputeHash(stream);
                }
            }

            if (!zipHashes.ContainsKey(fullPath) || !zipHashes[fullPath].SequenceEqual(hash))
            {
                String outputDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(modZipFile));
                if (!Directory.Exists(outputDir))
                    ZipFile.ExtractToDirectory(modZipFile, outputDir);

                if (zipHashes.ContainsKey(fullPath))
                    zipHashes[fullPath] = hash;
                else
                    zipHashes.Add(fullPath, hash);
            }
        }

        private static void ReadModInfoZip(String zipFile)
        {
            // Ran into a mod with a file that had a modified timestamp that causes the UnZip to fail. Not much we can do besides skip that mod.
            try
            {
                UnzipMod(zipFile);
            }
            catch (Exception e)
            {
                ErrorLogging.LogLine(String.Format("The mod with zip file '{0}' could not be extracted. Error: {1}", zipFile, e.Message));
                return;
            }

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
            if (Mods.Any(m => m.Name == newMod.Name))
            {
                ErrorLogging.LogLine(String.Format("Duplicate installed mod found '{0}' ignoring duplicate.", newMod.Name));
                return;
            }
            
            newMod.dir = dir;

            if (!Version.TryParse(newMod.version, out newMod.parsedVersion))
            {
                newMod.parsedVersion = new Version(0, 0, 0, 0);
            }
            Console.WriteLine("Parsing Mod " + newMod.Name);
            ParseModDependencies(newMod);

            Mods.Add(newMod);
        }

        private static void ParseModDependencies(Mod mod)
        {
            if (mod.Name == "base")
            {
                mod.dependencies.Add("core >= 0.0.0.0");
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
                        default:
                            ErrorLogging.LogLine(String.Format("Mod '{0}' has malformed dependency '{1}'", mod.Name, depString));
                            return;
                    }
                    token++;

                    if (!Version.TryParse(split[token], out newDependency.Version))
                    {
                        ErrorLogging.LogLine(String.Format("Mod '{0}' has malformed dependency '{1}'", mod.Name, depString));
                        return;
                    }
                    token++;
                }
                else
                {
                    ErrorLogging.LogLine(String.Format("Mod '{0}' has malformed dependency '{1}'", mod.Name, depString));
                    return;
                }

                mod.parsedDependencies.Add(newDependency);
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
            newItem.Icon = GetIcon(values);

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

        private static Bitmap GetIcon(LuaTable values, bool fallbackToUnknown = false)
        {
            string fileName = ReadLuaString(values, "icon", true);
            if (fileName == null)
            {
                var icons = ReadLuaLuaTable(values, "icons", true);
                if (icons != null)
                {
                    // TODO: Figure out how to composite multiple icons
                    LuaTable first = (LuaTable)icons?[1];
                    if (first != null)
                    {
                        fileName = ReadLuaString(first, "icon", true);
                    }
                }
            }


            if (fileName == null)
                return (fallbackToUnknown ? UnknownIcon : null);
            else
                return LoadImage(fileName);
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

                if (results == null)
                    return;

                if (name == null)
                    name = results.ElementAt(0).Key.Name;
                Recipe newRecipe = new Recipe(name, time == 0.0f ? defaultRecipeTime : time, ingredients, results);

                newRecipe.Category = ReadLuaString(values, "category", true, "crafting");
                newRecipe.Icon = GetIcon(values);

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
                newAssembler.Icon = GetIcon(values, true);

                // 0.17 compat, ingredient_count no longer required
                newAssembler.MaxIngredients = ReadLuaInt(values, "ingredient_count", true, 10);
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

                foreach (String s in Settings.Default.EnabledAssemblers)
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

                newFurnace.Icon = GetIcon(values, true);
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

                foreach (String s in Settings.Default.EnabledAssemblers)
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

                newMiner.Icon = GetIcon(values, true);
                // 0.17 compat, mining_power no longer required
                newMiner.MiningPower = ReadLuaFloat(values, "mining_power", true, 1);
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

                foreach (String s in Settings.Default.EnabledMiners)
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
                newResource.Hardness = ReadLuaFloat(minableTable, "hardness", true, 0.5f);
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

                foreach (String s in Settings.Default.EnabledModules)
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
                newInserter.Icon = GetIcon(values, true);

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

            if (source == null)
            {
                ErrorLogging.LogLine($"Error reading results from {values}.");
                return null;
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
                        else if (resultTable[1] != null)
                        {
                            result = FindOrCreateUnknownItem((string)resultTable[1]);
                        }
                        else
                        {
                            ErrorLogging.LogLine($"Error reading results from {values}.");
                            return null;
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
