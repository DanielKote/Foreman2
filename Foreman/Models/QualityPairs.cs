using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace Foreman {
    public readonly struct ItemQualityPair : IEquatable<ItemQualityPair> {
        public readonly Item Item;
        public readonly Quality Quality;

        public ItemQualityPair(string comment) {
            Item = null;
            Quality = null;
        }

        public ItemQualityPair(Item item, Quality quality) {
            Item = item;
            Quality = quality;

            if (Item == null || Quality == null)
                throw new NullReferenceException($"null error - Item: {nameof(Item)} Quality: {nameof(Quality)}");
        }

        public override bool Equals(object obj) => obj is ItemQualityPair other && Equals(other);
        public bool Equals(ItemQualityPair other) => Item == other.Item && Quality == other.Quality;
        public override int GetHashCode() => Item.GetHashCode() + Quality.GetHashCode();
        public static bool operator ==(ItemQualityPair lhs, ItemQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(ItemQualityPair lhs, ItemQualityPair rhs) => !(lhs == rhs);
        public static implicit operator bool(ItemQualityPair bp) => bp is { Item: not null, Quality: not null };

        public override string ToString() {
            return $"{Item} ({Quality})";
        }

        public string FriendlyName => Quality != Quality.Owner.DefaultQuality
            ? $"{Item.FriendlyName} ({Quality.FriendlyName})"
            : Item.FriendlyName;

        public Bitmap Icon {
            get {
                if (Item == null)
                    return null;

                return Quality != Quality.Owner.DefaultQuality
                    ? IconCacheProcessor.CombinedQualityIcon(Item.Icon, Quality.Icon)
                    : Item.Icon;
            }
        }
    }

    public readonly struct ModuleQualityPair : ISerializable {
        public readonly Module Module;
        public readonly Quality Quality;

        public ModuleQualityPair(Module module, Quality quality) {
            Module = module;
            Quality = quality;

            if (Module == null || Quality == null)
                throw new NullReferenceException($"null error - Module: {nameof(Module)} Quality: {nameof(Quality)}");
        }

        public override bool Equals(object obj) => obj is ModuleQualityPair other && Equals(other);
        public bool Equals(ModuleQualityPair other) => Module == other.Module && Quality == other.Quality;
        public override int GetHashCode() => Module.GetHashCode() + Quality.GetHashCode();
        public static bool operator ==(ModuleQualityPair lhs, ModuleQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(ModuleQualityPair lhs, ModuleQualityPair rhs) => !(lhs == rhs);
        public static implicit operator bool(ModuleQualityPair bp) => bp is { Module: not null, Quality: not null };

        public override string ToString() {
            return $"{Module} ({Quality})";
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Name", Module.Name);
            info.AddValue("Quality", Quality.Name);
        }

        public string FriendlyName => Quality != Quality.Owner.DefaultQuality
            ? $"{Module.FriendlyName} ({Quality.FriendlyName})"
            : Module.FriendlyName;

        public Bitmap Icon {
            get {
                if (Module == null)
                    return null;

                return Quality != Quality.Owner.DefaultQuality
                    ? IconCacheProcessor.CombinedQualityIcon(Module.Icon, Quality.Icon)
                    : Module.Icon;
            }
        }
    }

    public readonly struct AssemblerQualityPair {
        public readonly Assembler Assembler;
        public readonly Quality Quality;

        public AssemblerQualityPair(Assembler assembler, Quality quality) {
            Assembler = assembler;
            Quality = quality;

            if (Assembler == null || Quality == null)
                throw new NullReferenceException($"null error - Assembler: {nameof(Assembler)} Quality: {nameof(Quality)}");
        }

        public override bool Equals(object obj) => obj is AssemblerQualityPair other && Equals(other);
        public bool Equals(AssemblerQualityPair other) => Assembler == other.Assembler && Quality == other.Quality;
        public override int GetHashCode() => Assembler.GetHashCode() + Quality.GetHashCode();
        public static bool operator ==(AssemblerQualityPair lhs, AssemblerQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(AssemblerQualityPair lhs, AssemblerQualityPair rhs) => !(lhs == rhs);
        public static implicit operator bool(AssemblerQualityPair bp) => bp is { Assembler: not null, Quality: not null };

        public override string ToString() {
            return $"{Assembler} ({Quality})";
        }

        public string FriendlyName => Quality != Quality.Owner.DefaultQuality
            ? $"{Assembler.FriendlyName} ({Quality.FriendlyName})"
            : Assembler.FriendlyName;

        public Bitmap Icon {
            get {
                if (Assembler == null)
                    return null;

                return Quality != Quality.Owner.DefaultQuality
                    ? IconCacheProcessor.CombinedQualityIcon(Assembler.Icon, Quality.Icon)
                    : Assembler.Icon;
            }
        }
    }

    public readonly struct BeaconQualityPair {
        public readonly Beacon Beacon;
        public readonly Quality Quality;

        public BeaconQualityPair(string comment) {
            Beacon = null;
            Quality = null;
        }

        public BeaconQualityPair(Beacon beacon, Quality quality) {
            Beacon = beacon;
            Quality = quality;

            if (Beacon == null || Quality == null)
                throw new NullReferenceException($"null error - Beacon: {nameof(Beacon)} Quality: {nameof(Quality)}");
        }

        public override bool Equals(object obj) => obj is BeaconQualityPair other && Equals(other);
        public bool Equals(BeaconQualityPair other) => Beacon == other.Beacon && Quality == other.Quality;
        public override int GetHashCode() => Beacon.GetHashCode() + Quality.GetHashCode();
        public static bool operator ==(BeaconQualityPair lhs, BeaconQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(BeaconQualityPair lhs, BeaconQualityPair rhs) => !(lhs == rhs);
        public static implicit operator bool(BeaconQualityPair bp) => bp is { Beacon: not null, Quality: not null };

        public override string ToString() {
            return $"{Beacon} ({Quality})";
        }

        public string FriendlyName => Quality != Quality.Owner.DefaultQuality
            ? $"{Beacon.FriendlyName} ({Quality.FriendlyName})"
            : Beacon.FriendlyName;

        public Bitmap Icon {
            get {
                if (Beacon == null)
                    return null;

                return Quality != Quality.Owner.DefaultQuality
                    ? IconCacheProcessor.CombinedQualityIcon(Beacon.Icon, Quality.Icon)
                    : Beacon.Icon;
            }
        }
    }

    public readonly struct RecipeQualityPair {
        public readonly Recipe Recipe;
        public readonly Quality Quality;

        public RecipeQualityPair(string comment) {
            Recipe = null;
            Quality = null;
        }

        public RecipeQualityPair(Recipe recipe, Quality quality) {
            Recipe = recipe;
            Quality = quality;

            if (Recipe == null || Quality == null)
                throw new NullReferenceException($"null error - Recipe: {nameof(Recipe)} Quality: {nameof(Quality)}");
        }

        public override bool Equals(object obj) => obj is RecipeQualityPair other && Equals(other);
        public bool Equals(RecipeQualityPair other) => Recipe == other.Recipe && Quality == other.Quality;
        public override int GetHashCode() => Recipe.GetHashCode() + Quality.GetHashCode();
        public static bool operator ==(RecipeQualityPair lhs, RecipeQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(RecipeQualityPair lhs, RecipeQualityPair rhs) => !(lhs == rhs);
        public static implicit operator bool(RecipeQualityPair bp) => bp is { Recipe: not null, Quality: not null };

        public override string ToString() {
            return $"{Recipe} ({Quality})";
        }

        public string FriendlyName => Quality != Quality.Owner.DefaultQuality
            ? $"{Recipe.FriendlyName} ({Quality.FriendlyName})"
            : Recipe.FriendlyName;

        public Bitmap Icon {
            get {
                if (Recipe == null)
                    return null;

                return Quality != Quality.Owner.DefaultQuality
                    ? IconCacheProcessor.CombinedQualityIcon(Recipe.Icon, Quality.Icon)
                    : Recipe.Icon;
            }
        }
    }
}