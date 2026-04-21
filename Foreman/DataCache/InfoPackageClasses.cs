using System;
using System.Collections.Generic;

namespace Foreman {
    public class SaveFileInfo {
        public Dictionary<string, string> Mods { get; private set; } = new();
        public Dictionary<string, bool> Technologies { get; private set; } = new();
        public Dictionary<string, bool> Recipes { get; private set; } = new();
    }

    public struct PresetInfo(Dictionary<string, string> modList, bool eRecipes, bool eTech) {
        public Dictionary<string, string> ModList { get; set; } = modList;
        public bool ExpensiveRecipes { get; set; } = eRecipes;
        public bool ExpensiveTechnology { get; set; } = eTech;
    }

    public class PresetErrorPackage(Preset preset) : IComparable<PresetErrorPackage> {
        public Preset Preset = preset;

        public List<string> RequiredMods = [];
        public List<string> RequiredItems = [];
        public List<string> RequiredRecipes = [];
        public List<string> RequiredPlanting = [];
        public List<string> RequiredQualities = [];

        public List<string> MissingRecipes = [];
        public List<string> IncorrectRecipes = [];
        // any recipes that were missing previously but have been found to fit in this current preset
        public List<string> ValidMissingRecipes = [];

        public List<string> MissingItems = [];

        // we ignore spoiling and burn results as they are part of item data, and it's not feasible to process them in the same way as recipes & plantResults.
        // In any case, this will affect only the 'error' counter, not actual graph.
        public List<string> MissingPlanting = [];
        // any planting processes that were missing previously but have been found to fit in this current preset
        public List<string> ValidMissingPlanting = [];
        public List<string> IncorrectPlanting = [];
        public List<string> MissingQualities = [];
        // in mod-name|version format
        public List<string> MissingMods = [];
        // in mod-name|version format
        public List<string> AddedMods = [];
        // in mod-name|expected-version|preset-version format
        public List<string> WrongVersionMods = [];

        public int MiCount => MissingRecipes.Count
            + IncorrectRecipes.Count
            + MissingItems.Count
            + MissingPlanting.Count
            + IncorrectPlanting.Count
            + MissingQualities.Count;

        public int ErrorCount => MiCount + MissingMods.Count + AddedMods.Count + WrongVersionMods.Count;

        // useful for sorting the Presets by increasing severity (mods, items/recipes)
        public int CompareTo(PresetErrorPackage other) {
            var modErrorComparison = MissingMods.Count.CompareTo(other.MissingMods.Count);
            if (modErrorComparison != 0)
                return modErrorComparison;
            modErrorComparison = AddedMods.Count.CompareTo(other.AddedMods.Count);
            return modErrorComparison != 0
                ? modErrorComparison
                : MiCount.CompareTo(other.MiCount);
        }
    }
}