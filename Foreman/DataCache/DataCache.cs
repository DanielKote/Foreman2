using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using Newtonsoft.Json.Linq;
using Foreman.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Foreman
{
    public static class DataCache
    {
        public enum GenerationType { ForemanMod, FactorioLUA }

        public static Dictionary<string, bool> Mods = new Dictionary<string, bool>();
        public static IReadOnlyDictionary<string, Technology> Technologies { get { return technologies; } }
        public static IReadOnlyDictionary<string, Group> Groups { get { return groups; } }
        public static IReadOnlyDictionary<string, Subgroup> Subgroups { get { return subgroups; } }
        public static IReadOnlyDictionary<string, Item> Items { get { return items; } }
        public static IReadOnlyDictionary<string, Recipe> Recipes { get { return recipes; } }
        public static IReadOnlyDictionary<string, Assembler> Assemblers { get { return assemblers; } }
        public static IReadOnlyDictionary<string, Miner> Miners { get { return miners; } }
        public static IReadOnlyDictionary<string, Resource> Resources { get { return resources; } }
        public static IReadOnlyDictionary<string, Module> Modules { get { return modules; } }

        public static Dictionary<string, Recipe> MissingRecipes = new Dictionary<string, Recipe>();
        public static Dictionary<string, Item> MissingItems = new Dictionary<string, Item>();
        public static Subgroup MissingSubgroup = new Subgroup("", new Group("","",""), "");

        public static Bitmap UnknownIcon { get { return IconProcessor.GetUnknownIcon(); } }

        public static event EventHandler DataLoaded;

        private static Dictionary<string, Technology> technologies;
        private static Dictionary<string, Group> groups;
        private static Dictionary<string, Subgroup> subgroups;
        private static Dictionary<string, Item> items;
        private static Dictionary<string, Recipe> recipes;
        private static Dictionary<string, Assembler> assemblers;
        private static Dictionary<string, Miner> miners;
        private static Dictionary<string, Resource> resources;
        private static Dictionary<string, Module> modules;

        private static Dictionary<string, Exception> failedFiles = new Dictionary<string, Exception>();
        private static Dictionary<string, Exception> failedPathDirectories = new Dictionary<string, Exception>();
        private static List<string> notFoundMods = new List<string>();
 
        private static GenerationType GetGenerationType() { return (GenerationType)(Settings.Default.GenerationType); }
        private static string ScriptOutPath { get { return Path.Combine(Settings.Default.FactorioUserDataPath, "script-output"); } }

        internal static async Task LoadAllData(IProgress<KeyValuePair<int, string>> progress, CancellationToken ctoken, bool reloadIconCache)
        {
            await Task.Run(() =>
            {
                Clear();

                JObject jsonData;
                JsonDataProcessor processor = new JsonDataProcessor();
                bool modLoadRequired = true;

                switch (GetGenerationType())
                {
                    case GenerationType.FactorioLUA:
                        /*
                        progress.Report(new KeyValuePair<int, string>(0, ""));
                        FactorioModsProcessor.LoadMods(progress, ctoken, 0, 15);
                        foreach (Mod mod in FactorioModsProcessor.Mods)
                            Mods.Add(mod.Name, mod.Enabled);

                        if (File.Exists(iconDataFile))
                            processor.LoadIconCache(iconDataFile);

                        jsonData = JObject.Parse(File.ReadAllText(setupFile));
                        processor.LoadData(jsonData, progress, ctoken, 15, 95);
                        processor.SaveIconCache(Path.Combine(ScriptOutPath, "ForemanFactorioIconData.dat"));
                        break;
                        */
                    case GenerationType.ForemanMod:
                    default:
                        //read in the data from the foreman mod (in the script output folder)
                        string setupFile = Path.Combine(ScriptOutPath, "ForemanFactorioSetup.txt");
                        string iconDataFile = Path.Combine(ScriptOutPath, "ForemanFactorioIconData.dat");
                        if (!File.Exists(setupFile))
                        {
                            MessageBox.Show("The setup file could not be found!\n\nEnsure you have the \"z-z-foremanexport\" mod installed, enabled, and started a new game with it. It should freeze the game for a bit (1sec for vanilla installation, 30sec+ for Angel/Bob with extras) before displaying \"Foreman export complete.\" ");
                            return;
                        }

                        if (File.Exists(iconDataFile) && !reloadIconCache)
                            modLoadRequired = !processor.LoadIconCache(iconDataFile);
                        else if (File.Exists(iconDataFile))
                            File.Delete(iconDataFile);

                        //failed to load cache (corrupt or doesnt exist), have to load mods / mod info
                        if (modLoadRequired)
                        {
                            progress.Report(new KeyValuePair<int, string>(0, ""));
                            FactorioModsProcessor.LoadMods(progress, ctoken, 0, 15);
                            foreach (Mod mod in FactorioModsProcessor.Mods)
                                Mods.Add(mod.Name, mod.Enabled);
                        }

                        jsonData = JObject.Parse(File.ReadAllText(setupFile));
                        processor.LoadData(jsonData, progress, ctoken, 15, 95);
                        break;
                }

                technologies = processor.Technologies;
                groups = processor.Groups;
                subgroups = processor.Subgroups;
                items = processor.Items;
                recipes = processor.Recipes;
                assemblers = processor.Assemblers;
                miners = processor.Miners;
                resources = processor.Resources;
                modules = processor.Modules;

                foreach(string mod in processor.IncludedMods)
                {
                    if (Mods.ContainsKey(mod))
                        Mods[mod] = true;
                    else
                    {
                        if(modLoadRequired)
                            notFoundMods.Add(mod);
                        Mods.Add(mod, true);
                    }
                }
                if(notFoundMods.Count == 0)
                    processor.SaveIconCache(Path.Combine(ScriptOutPath, "ForemanFactorioIconData.dat"));

                progress.Report(new KeyValuePair<int, string>(96, "Checking for cyclic recipes"));
                MarkCyclicRecipes();
                progress.Report(new KeyValuePair<int, string>(98, "Finalizing..."));

                UpdateRecipesAssemblerStatus();
                ReportErrors();

                DataLoaded?.Invoke(null, EventArgs.Empty);
                progress.Report(new KeyValuePair<int, string>(100, "Done!"));
            });

        }

        public static void Clear()
        {
            Mods.Clear();
            technologies?.Clear();
            groups?.Clear();
            subgroups?.Clear();
            items?.Clear();
            recipes?.Clear();
            assemblers?.Clear();
            miners?.Clear();
            resources?.Clear();
            modules?.Clear();

            MissingItems.Clear();
            MissingRecipes.Clear();

            failedFiles?.Clear();
            failedPathDirectories?.Clear();
            notFoundMods.Clear();
        }

        private static void ReportErrors()
        {
            if (failedPathDirectories.Any())
            {
                ErrorLogging.LogLine("There were errors setting the lua path variable for the following directories:");
                foreach (string dir in failedPathDirectories.Keys)
                    ErrorLogging.LogLine(String.Format("{0} ({1})", dir, failedPathDirectories[dir].Message));
            }

            if (failedFiles.Any())
            {
                ErrorLogging.LogLine("The following files could not be loaded due to errors:");
                foreach (string file in failedFiles.Keys)
                    ErrorLogging.LogLine(String.Format("{0} ({1})", file, failedFiles[file].Message));
            }

            if(notFoundMods.Any())
            {
                ErrorLogging.LogLine("The following mods could not be found on the drive:");
                string missingMods = "";
                foreach (string mod in notFoundMods)
                {
                    ErrorLogging.LogLine(mod);
                    missingMods += "\n  " + mod;
                }

                MessageBox.Show("Error during Icon creation! The following MODS are missing from the factorio directory:" + missingMods + "\n\nIt is recommended to ensure all the mods are present and restart.");
            }
        }


        public static void UpdateRecipesAssemblerStatus()
        {
            //very quick update on recipes to check for valid assemblers (if they have none, then they are so marked)
            foreach (Recipe recipe in Recipes.Values)
            {
                bool usable = false;
                foreach (Assembler assembler in Assemblers.Values)
                    usable |= assembler.Enabled && assembler.Categories.Contains(recipe.Category);
                recipe.HasEnabledAssemblers = usable;
                if (!usable)
                    Console.WriteLine(recipe);
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
