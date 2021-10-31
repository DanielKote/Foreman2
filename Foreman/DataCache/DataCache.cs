using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Foreman.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Foreman
{
    public class Language
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

    public static class DataCache
    {
        public enum GenerationType { ForemanMod, FactorioLUA }

        public static List<Mod> Mods = new List<Mod>();
        public static List<Language> Languages = new List<Language>();
        public static Dictionary<String, Dictionary<String, String>> LocaleFiles = new Dictionary<string, Dictionary<string, string>>();

        public static Dictionary<string, Technology> Technologies = new Dictionary<string, Technology>();
        public static Dictionary<string, Item> Items = new Dictionary<String, Item>();
        public static Dictionary<string, Recipe> Recipes = new Dictionary<String, Recipe>();
        public static Dictionary<string, Assembler> Assemblers = new Dictionary<string, Assembler>();
        public static Dictionary<string, Miner> Miners = new Dictionary<string, Miner>();
        public static Dictionary<string, Resource> Resources = new Dictionary<string, Resource>();
        public static Dictionary<string, Module> Modules = new Dictionary<string, Module>();

        public static Dictionary<string, Recipe> MissingRecipes = new Dictionary<string, Recipe>(); //both are used when loading a graph file with unknown / missing items / recipes.
        public static Dictionary<string, Item> MissingItems = new Dictionary<string, Item>();

        public static Dictionary<String, Exception> failedFiles = new Dictionary<string, Exception>();
        public static Dictionary<String, Exception> failedPathDirectories = new Dictionary<string, Exception>();
 
        public static Bitmap UnknownIcon;

        private static Dictionary<String, byte[]> zipHashes = new Dictionary<string, byte[]>();
        private static GenerationType GetGenerationType() { return (GenerationType)(Settings.Default.GenerationType); }

        private static String DataPath { get { return Path.Combine(Settings.Default.FactorioPath, "data"); } }
        private static String ModPath { get { return Path.Combine(Settings.Default.FactorioUserDataPath, "mods"); } }
        private static string ScriptOutPath { get { return Path.Combine(Settings.Default.FactorioUserDataPath, "script-output"); } }
        private static String ExtractionPath { get { return ModPath; } }

        private static readonly List<Tuple<string, DependencyType>> DependencyTypeTokens = new List<Tuple<string, DependencyType>>
        {
            Tuple.Create("(?)", DependencyType.OptionalHidden),
            Tuple.Create("?", DependencyType.Optional),
            Tuple.Create("!", DependencyType.Incompatible)
        };
        private static readonly List<Tuple<string, VersionOperator>> VersionOperatorTokens = new List<Tuple<string, VersionOperator>>
        {
            // Order is important to match the 'largest' token first
            Tuple.Create(VersionOperator.GreaterThanOrEqual.Token(), VersionOperator.GreaterThanOrEqual),
            Tuple.Create(VersionOperator.LessThanOrEqual.Token(), VersionOperator.LessThanOrEqual),
            Tuple.Create(VersionOperator.GreaterThan.Token(), VersionOperator.GreaterThan),
            Tuple.Create(VersionOperator.LessThan.Token(), VersionOperator.LessThan),
            Tuple.Create(VersionOperator.EqualTo.Token(), VersionOperator.EqualTo)
        };

        internal static async Task LoadAllData(bool defaultEnabled, IProgress<KeyValuePair<int, string>> progress, CancellationToken ctoken)
        {
            await Task.Run(() =>
            {
                Clear();

                progress.Report(new KeyValuePair<int, string>(0,""));

                FindAllMods(progress, ctoken, 0, 15); //gets mod enable status from Properties.Settings.Default
                LoadAllLanguages();
                LoadLocaleFiles(Properties.Settings.Default.Language);
                UnknownIcon = IconProcessor.GetUnknownIcon();

                JObject jsonData;
                int jsonStartingPercent = 15;
                switch (GetGenerationType())
                {
                    case DataCache.GenerationType.FactorioLUA:
                        jsonStartingPercent = 30; //15% goes to processing factorio lua
                        FactorioLuaProcessor flp = new FactorioLuaProcessor();
                        jsonData = flp.LoadData(progress, ctoken, 0, jsonStartingPercent);
                        break;
                    case DataCache.GenerationType.ForemanMod:
                    default:
                        //read in the data from the foreman mod (in the script output folder)
                        string setupFile = Path.Combine(ScriptOutPath, "ForemanFactorioSetup.txt");
                        if (!File.Exists(setupFile))
                        {
                            MessageBox.Show("The setup file could not be found!\n\nEnsure you have the \"z-z-foremanexport\" mod installed, enabled, and started a new game with it. It should freeze the game for a bit (1sec for vanilla installation, 30sec+ for Angel/Bob with extras) before displaying \"Foreman export complete.\" ");
                            return;
                        }
                        jsonData = JObject.Parse(File.ReadAllText(setupFile));
                        break;
                }
                JsonDataProcessor processor = new JsonDataProcessor();
                processor.LoadData(jsonData, defaultEnabled, progress, ctoken, jsonStartingPercent, 95);

                foreach (KeyValuePair<string, Technology> kvp in processor.GetTechnologies())
                    Technologies.Add(kvp.Key, kvp.Value);
                foreach (KeyValuePair<string, Item> kvp in processor.GetItems())
                    Items.Add(kvp.Key, kvp.Value);
                foreach (KeyValuePair<string, Recipe> kvp in processor.GetRecipes())
                    Recipes.Add(kvp.Key, kvp.Value);
                foreach (KeyValuePair<string, Assembler> kvp in processor.GetAssemblers())
                    Assemblers.Add(kvp.Key, kvp.Value);
                foreach (KeyValuePair<string, Miner> kvp in processor.GetMiners())
                    Miners.Add(kvp.Key, kvp.Value);
                foreach (KeyValuePair<string, Resource> kvp in processor.GetResources())
                    Resources.Add(kvp.Key, kvp.Value);
                foreach (KeyValuePair<string, Module> kvp in processor.GetModules())
                    Modules.Add(kvp.Key, kvp.Value);

                foreach (KeyValuePair<string, Exception> kvp in processor.GetFileExceptions())
                    failedFiles.Add(kvp.Key, kvp.Value);
                foreach (KeyValuePair<string, Exception> kvp in processor.GetPathExceptions())
                    failedPathDirectories.Add(kvp.Key, kvp.Value);

                //IncorporateMiners(); //miners are inserted as part of recipes & assembly machines

                progress.Report(new KeyValuePair<int, string>(96, "Checking for cyclic recipes"));
                MarkCyclicRecipes();
                ReportErrors();
                progress.Report(new KeyValuePair<int, string>(100, "Done!"));
            });
        }

        public static void Clear()
        {
            Mods.Clear();
            Technologies.Clear();
            Items.Clear();
            Recipes.Clear();
            Assemblers.Clear();
            Miners.Clear();
            Resources.Clear();
            Modules.Clear();
            LocaleFiles.Clear();
            Languages.Clear();

            MissingItems.Clear();
            MissingRecipes.Clear();

            failedFiles.Clear();
            failedPathDirectories.Clear();
        }

        private static void FindAllMods(IProgress<KeyValuePair<int,string>> progress, CancellationToken ctoken, int startingPercent, int endingPercent) //Vanilla game counts as a mod too.
        {
            progress.Report(new KeyValuePair<int, string>(startingPercent, "Preparing Mods"));

            String modPath = ModPath;
            IEnumerable<ModOnDisk> mods = ModOnDisk.Empty();

            mods = mods.Concat(ModOnDisk.EnumerateDirectories(DataPath));
            mods = mods.Concat(ModOnDisk.EnumerateDirectories(modPath));
            mods = mods.Concat(ModOnDisk.EnumerateZips(modPath));

            IEnumerable<ModOnDisk> latestMods = ChangeModsToOnlyLatest(mods);

            List<string> enabledMods = new List<string>();
            enabledMods.Add("base");
            enabledMods.Add("core");
            if (Settings.Default.EnabledMods.Count > 0)
            {
                foreach (String s in Settings.Default.EnabledMods)
                    enabledMods.Add(s);
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

            int totalCount = latestMods.Count();
            int counter = 0;
            foreach (ModOnDisk mod in latestMods)
            {
                switch (mod.Type)
                {
                    case ModOnDisk.ModType.DIRECTORY:
                        ReadModInfoFile(mod.Location);
                        break;
                    case ModOnDisk.ModType.ZIP:
                        // Unzipping is very expensive only do if we have to
                        if (enabledMods.Contains(mod.Id))
                            ReadModInfoZip(mod.Location);
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
                if (ctoken.IsCancellationRequested)
                    return;
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
            }

            foreach (Mod mod in Mods)
                mod.Enabled = enabledMods.Contains(mod.Name);

            DependencyGraph modGraph = new DependencyGraph(Mods);
            modGraph.DisableUnsatisfiedMods();
            Mods = modGraph.SortMods();

            progress.Report(new KeyValuePair<int, string>(endingPercent,""));
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

        public static void LoadLocaleFiles(String locale)
        {
            foreach (Mod mod in Mods.Where(m => m.Enabled))
            {
                String localeDir = Path.Combine(mod.dir, "locale", locale);
                if (!Directory.Exists(localeDir))
                    localeDir = Path.Combine(mod.dir, "locale", "en"); //try for a default english if the one we want doesnt exist

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
                    ErrorLogging.LogLine(e.Message);
                }
                this.Location = location;
                this.Type = type;
            }

            public static IEnumerable<ModOnDisk> EnumerateDirectories(string path)
            {
                if (Directory.Exists(path))
                {
                    return Directory.EnumerateDirectories(path).Select(x => new ModOnDisk(x, ModType.DIRECTORY));
                }
                else
                {
                    return Empty();
                }
            }

            public static IEnumerable<ModOnDisk> EnumerateZips(string path)
            {
                if (Directory.Exists(path))
                {
                    return Directory.EnumerateFiles(path, "*.zip").Select(x => new ModOnDisk(x, ModType.ZIP));
                }
                else
                {
                    return Empty();
                }
            }

            public static IEnumerable<ModOnDisk> Empty()
            {
                return Enumerable.Empty<ModOnDisk>();
            }
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
                String outputDir = Path.Combine(ExtractionPath, Path.GetFileNameWithoutExtension(modZipFile));
                if (!Directory.Exists(outputDir))
                {
                    ZipFile.ExtractToDirectory(modZipFile, outputDir);
                }

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

            String modUnzippedFolder = Path.Combine(ExtractionPath, Path.GetFileNameWithoutExtension(zipFile));
            String file = Directory.EnumerateFiles(modUnzippedFolder, "info.json", SearchOption.AllDirectories).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(file))
            {
                ErrorLogging.LogLine(String.Format("Could not find info.json at '{0}' for mod zipFile '{1}'", modUnzippedFolder, zipFile));
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

            foreach (string depString in mod.dependencies)
            {
                ModDependency newDependency = new ModDependency();

                string trimmedDepString = depString.Trim();
                var dependencyTypeToken = DependencyTypeTokens.FirstOrDefault(t => trimmedDepString.StartsWith(t.Item1));
                newDependency.Type = dependencyTypeToken?.Item2 ?? DependencyType.Required;

                string modNameWithVersion = dependencyTypeToken != null ?
                    trimmedDepString.Substring(dependencyTypeToken.Item1.Length).TrimStart() : trimmedDepString;
                var indexOfVersionOperatorToken = VersionOperatorTokens
                    .Select(t => new { Token = t.Item1, Index = modNameWithVersion.IndexOf(t.Item1), Operator = t.Item2 })
                    .FirstOrDefault(v => v.Index > 0);

                if (indexOfVersionOperatorToken != null)
                {
                    newDependency.ModName = modNameWithVersion
                        .Substring(0, indexOfVersionOperatorToken.Index)
                        .TrimEnd();
                    newDependency.VersionOperator = indexOfVersionOperatorToken.Operator;

                    string versionString = modNameWithVersion
                        .Substring(indexOfVersionOperatorToken.Index + indexOfVersionOperatorToken.Token.Length)
                        .TrimStart();
                    if (!Version.TryParse(versionString, out newDependency.Version))
                    {
                        ErrorLogging.LogLine(String.Format("Mod '{0}' has malformed dependency '{1}'", mod.Name, depString));
                        return;
                    }
                }
                else
                {
                    newDependency.ModName = modNameWithVersion;
                }

                mod.parsedDependencies.Add(newDependency);
            }
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
