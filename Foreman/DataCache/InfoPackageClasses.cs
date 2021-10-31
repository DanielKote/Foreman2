using System;
using System.Collections.Generic;

namespace Foreman
{
    public class SaveFileInfo
    {
        public Dictionary<string, string> Mods { get; private set; }
        public Dictionary<string, bool> Technologies { get; private set; }
        public Dictionary<string, bool> Recipes { get; private set; }
        public SaveFileInfo()
        {
            Mods = new Dictionary<string, string>();
            Technologies = new Dictionary<string, bool>();
            Recipes = new Dictionary<string, bool>();
        }
    }

    public struct PresetInfo
    {
        public Dictionary<string, string> ModList { get; set; }
        public bool ExpensiveRecipes { get; set; }
        public bool ExpensiveTechnology { get; set; }
        public PresetInfo(Dictionary<string, string> modList, bool ERecipes, bool ETech) { ModList = modList; ExpensiveRecipes = ERecipes; ExpensiveTechnology = ETech; }
    }

    public class PresetErrorPackage : IComparable<PresetErrorPackage>
    {
        public Preset Preset;

        public List<string> RequiredMods;
        public List<string> RequiredItems;
        public List<string> RequiredRecipes;

        public List<string> MissingRecipes;
        public List<string> IncorrectRecipes;
        public List<string> ValidMissingRecipes; //any recipes that were missing previously but have been found to fit in this current preset
        public List<string> MissingItems;
        public List<string> MissingMods;
        public List<string> AddedMods;
        public List<string> WrongVersionMods;

        public int ErrorCount { get { return MissingRecipes.Count + IncorrectRecipes.Count + MissingItems.Count + MissingMods.Count + AddedMods.Count + WrongVersionMods.Count; } }
        public int MICount { get { return MissingRecipes.Count + IncorrectRecipes.Count + MissingItems.Count; } }

        public PresetErrorPackage(Preset preset)
        {
            Preset = preset;
            RequiredMods = new List<string>();
            RequiredItems = new List<string>();
            RequiredRecipes = new List<string>();

            MissingRecipes = new List<string>();
            IncorrectRecipes = new List<string>();
            ValidMissingRecipes = new List<string>();
            MissingItems = new List<string>();
            MissingMods = new List<string>(); // in mod-name|version format
            AddedMods = new List<string>(); //in mod-name|version format
            WrongVersionMods = new List<string>(); //in mod-name|expected-version|preset-version format
        }

        public int CompareTo(PresetErrorPackage other) //usefull for sorting the Presets by increasing severity (mods, items/recipes)
        {
            int modErrorComparison = this.MissingMods.Count.CompareTo(other.MissingMods.Count);
            if (modErrorComparison != 0)
                return modErrorComparison;
            modErrorComparison = this.AddedMods.Count.CompareTo(other.AddedMods.Count);
            if (modErrorComparison != 0)
                return modErrorComparison;
            return this.MICount.CompareTo(other.MICount);
        }
    }
}
