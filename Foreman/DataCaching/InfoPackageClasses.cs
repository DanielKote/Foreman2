using System;
using System.Collections.Generic;

namespace Foreman.DataCaching {
    public class SaveFileInfo {
        public Dictionary<string, string> Mods { get; private set; }
        public Dictionary<string, bool> Technologies { get; private set; }
        public Dictionary<string, bool> Recipes { get; private set; }
        public SaveFileInfo() {
            Mods = [];
            Technologies = [];
            Recipes = [];
        }
    }

    public record struct PresetInfo(Dictionary<string, string>? ModList, bool ExpensiveRecipes, bool ExpensiveTechnology);

    public class PresetErrorPackage(Preset preset) : IComparable<PresetErrorPackage> {
        public Preset Preset { get; set; } = preset;
        public List<string> RequiredMods { get; set; } = [];
        public List<string> RequiredItems { get; set; } = [];
        public List<string> RequiredRecipes { get; set; } = [];
        public List<string> RequiredPlanting { get; set; } = [];
        public List<string> RequiredQualities { get; set; } = [];
        public List<string> MissingRecipes { get; set; } = [];
        public List<string> IncorrectRecipes { get; set; } = [];
        public List<string> ValidMissingRecipes { get; set; } = [];
        public List<string> MissingItems { get; set; } = [];
        //we ignore spoiling and burn results as they are part of item data and its not feasable to process them in the same way as recipes & plantResults. In any case, this will effect only the 'error' counter, not actual graph.
        public List<string> MissingPlanting { get; set; } = [];
        public List<string> ValidMissingPlanting { get; set; } = [];
        public List<string> IncorrectPlanting { get; set; } = [];
        public List<string> MissingQualities { get; set; } = [];
        public List<string> MissingMods { get; set; } = []; // in mod-name|version format
        public List<string> AddedMods { get; set; } = []; //in mod-name|version format
        public List<string> WrongVersionMods { get; set; } = []; //in mod-name|expected-version|preset-version format
        public int MICount { get { return MissingRecipes.Count + IncorrectRecipes.Count + MissingItems.Count + MissingPlanting.Count + IncorrectPlanting.Count + MissingQualities.Count; } }
        public int ErrorCount { get { return MICount + MissingMods.Count + AddedMods.Count + WrongVersionMods.Count; } }

        public int CompareTo(PresetErrorPackage? other) //useful for sorting the Presets by increasing severity (mods, items/recipes)
        {
            int modErrorComparison = MissingMods.Count.CompareTo(other?.MissingMods.Count);
            if (modErrorComparison != 0)
                return modErrorComparison;
            modErrorComparison = AddedMods.Count.CompareTo(other?.AddedMods.Count);
            return modErrorComparison != 0 ? modErrorComparison : MICount.CompareTo(other?.MICount);
        }

        public override bool Equals(object? obj) => obj is PresetErrorPackage other && CompareTo(other) == 0;

        public override int GetHashCode() => HashCode.Combine(Preset, ErrorCount, MICount);

        public static bool operator ==(PresetErrorPackage? left, PresetErrorPackage? right) =>
            ReferenceEquals(left, right) || (left is not null && right is not null && left.CompareTo(right) == 0);

        public static bool operator !=(PresetErrorPackage? left, PresetErrorPackage? right) => !(left == right);

        public static bool operator <(PresetErrorPackage left, PresetErrorPackage right) => left.CompareTo(right) < 0;

        public static bool operator <=(PresetErrorPackage left, PresetErrorPackage right) => left.CompareTo(right) <= 0;

        public static bool operator >(PresetErrorPackage left, PresetErrorPackage right) => left.CompareTo(right) > 0;

        public static bool operator >=(PresetErrorPackage left, PresetErrorPackage right) => left.CompareTo(right) >= 0;
    }
}
