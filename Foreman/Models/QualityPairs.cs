using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Foreman {
    public readonly struct ItemQualityPair : IEquatable<ItemQualityPair> {
        public readonly Item? Item;
        public readonly Quality? Quality;

        public ItemQualityPair(string comment) {
            Item = null;
            Quality = null;
        }

        public ItemQualityPair(Item item, Quality quality) {
            Item = item;
            Quality = quality;

            if (Item == null || Quality == null)
                throw new NullReferenceException("null error - Item: " + nameof(Item) + " Quality: " + nameof(Quality));
        }

        public override bool Equals(object? obj) => obj is ItemQualityPair other && Equals(other);
        public bool Equals(ItemQualityPair other) => Item == other.Item && Quality == other.Quality;
        public override int GetHashCode() => HashCode.Combine(Item, Quality);
        public static bool operator ==(ItemQualityPair lhs, ItemQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(ItemQualityPair lhs, ItemQualityPair rhs) => !(lhs == rhs);
        public static implicit operator bool(ItemQualityPair bp) => bp.Item != null && bp.Quality != null;
        public override string ToString() => Item?.ToString() + " (" + Quality?.ToString() + ")";

        public string? FriendlyName {
            get {
                if (Quality == Quality?.Owner.DefaultQuality)
                    return Item?.FriendlyName;
                else
                    return Item?.FriendlyName + " (" + Quality?.FriendlyName + ")";
            }
        }
        public Bitmap? Icon {
            get {
                if (Item is null || Quality is null)
                    return null;
                return Quality == Quality.Owner.DefaultQuality ? Item.Icon : IconCacheProcessor.CombinedQualityIcon(Item.Icon, Quality.Icon);
            }
        }
    }

    public readonly struct ModuleQualityPair {
        public readonly Module Module;
        public readonly Quality Quality;

        public ModuleQualityPair(Module module, Quality quality) {
            Module = module;
            Quality = quality;

            if (Module == null || Quality == null)
                throw new NullReferenceException("null error - Module: " + nameof(Module) + " Quality: " + nameof(Quality));
        }

        public override bool Equals(object? obj) => obj is ModuleQualityPair other && Equals(other);
        public bool Equals(ModuleQualityPair other) => Module == other.Module && Quality == other.Quality;
        public override int GetHashCode() => HashCode.Combine(Module, Quality);
        public static bool operator ==(ModuleQualityPair lhs, ModuleQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(ModuleQualityPair lhs, ModuleQualityPair rhs) => !(lhs == rhs);
        public static implicit operator bool(ModuleQualityPair bp) => bp.Module != null && bp.Quality != null;
        public override string ToString() => Module.ToString() + " (" + Quality.ToString() + ")";

        public string FriendlyName {
            get {
                if (Quality == Quality.Owner.DefaultQuality)
                    return Module.FriendlyName;
                else
                    return Module.FriendlyName + " (" + Quality.FriendlyName + ")";
            }
        }
        public Bitmap? Icon {
            get {
                if (Module == null)
                    return null;
                return Quality == Quality.Owner.DefaultQuality ? Module.Icon : IconCacheProcessor.CombinedQualityIcon(Module.Icon, Quality.Icon);
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
                throw new NullReferenceException("null error - Assembler: " + nameof(Assembler) + " Quality: " + nameof(Quality));
        }

        public override bool Equals(object? obj) => obj is AssemblerQualityPair other && Equals(other);
        public bool Equals(AssemblerQualityPair other) => Assembler == other.Assembler && Quality == other.Quality;
        public override int GetHashCode() => HashCode.Combine(Assembler, Quality);
        public static bool operator ==(AssemblerQualityPair lhs, AssemblerQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(AssemblerQualityPair lhs, AssemblerQualityPair rhs) => !(lhs == rhs);
        public static implicit operator bool(AssemblerQualityPair bp) => bp.Assembler != null && bp.Quality != null;
        public override string ToString() => Assembler.ToString() + " (" + Quality.ToString() + ")";

        public string FriendlyName {
            get {
                if (Quality == Quality.Owner.DefaultQuality)
                    return Assembler.FriendlyName;
                else
                    return Assembler.FriendlyName + " (" + Quality.FriendlyName + ")";
            }
        }
        public Bitmap? Icon {
            get {
                if (Assembler == null)
                    return null;
                return Quality == Quality.Owner.DefaultQuality ? Assembler.Icon : IconCacheProcessor.CombinedQualityIcon(Assembler.Icon, Quality.Icon);
            }
        }
    }

    public readonly struct BeaconQualityPair {
        public readonly Beacon? Beacon;
        public readonly Quality? Quality;

        public BeaconQualityPair(string comment) {
            Beacon = null;
            Quality = null;
        }

        public BeaconQualityPair(Beacon beacon, Quality quality) {
            Beacon = beacon;
            Quality = quality;

            if (Beacon == null || Quality == null)
                throw new NullReferenceException("null error - Beacon: " + nameof(Beacon) + " Quality: " + nameof(Quality));
        }

        public override bool Equals(object? obj) => obj is BeaconQualityPair other && Equals(other);
        public bool Equals(BeaconQualityPair other) => Beacon == other.Beacon && Quality == other.Quality;
        public override int GetHashCode() => HashCode.Combine(Beacon, Quality);
        public static bool operator ==(BeaconQualityPair lhs, BeaconQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(BeaconQualityPair lhs, BeaconQualityPair rhs) => !(lhs == rhs);
        public static implicit operator bool(BeaconQualityPair bp) => bp.Beacon != null && bp.Quality != null;
        public override string ToString() => Beacon?.ToString() + " (" + Quality?.ToString() + ")";

        public string? FriendlyName {
            get {
                if (Quality is null)
                    return null;
                if (Quality == Quality.Owner.DefaultQuality)
                    return Beacon?.FriendlyName;
                else
                    return Beacon?.FriendlyName + " (" + Quality.FriendlyName + ")";
            }
        }
        public Bitmap? Icon {
            get {
                if (Beacon is null || Quality is null)
                    return null;
                return Quality == Quality.Owner.DefaultQuality ? Beacon.Icon : IconCacheProcessor.CombinedQualityIcon(Beacon.Icon, Quality.Icon);
            }
        }
    }

    public readonly struct RecipeQualityPair {
        public readonly Recipe? Recipe;
        public readonly Quality? Quality;

        public RecipeQualityPair(string comment) {
            Recipe = null;
            Quality = null;
        }

        public RecipeQualityPair(Recipe recipe, Quality quality) {
            Recipe = recipe;
            Quality = quality;

            if (Recipe == null || Quality == null)
                throw new NullReferenceException("null error - Recipe: " + nameof(Recipe) + " Quality: " + nameof(Quality));
        }

        public override bool Equals(object? obj) => obj is RecipeQualityPair other && Equals(other);
        public bool Equals(RecipeQualityPair other) => Recipe == other.Recipe && Quality == other.Quality;
        public override int GetHashCode() => HashCode.Combine(Recipe, Quality);
        public static bool operator ==(RecipeQualityPair lhs, RecipeQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(RecipeQualityPair lhs, RecipeQualityPair rhs) => !(lhs == rhs);
        public static implicit operator bool(RecipeQualityPair bp) => bp.Recipe != null && bp.Quality != null;
        public override string ToString() => Recipe?.ToString() + " (" + Quality?.ToString() + ")";

        public string? FriendlyName {
            get {
                if (Quality == Quality?.Owner.DefaultQuality)
                    return Recipe?.FriendlyName;
                else
                    return Recipe?.FriendlyName + " (" + Quality?.FriendlyName + ")";
            }
        }
        public Bitmap? Icon {
            get {
                if (Recipe is null || Quality is null)
                    return null;
                return Quality == Quality.Owner.DefaultQuality ? Recipe.Icon : IconCacheProcessor.CombinedQualityIcon(Recipe.Icon, Quality.Icon);
            }
        }
    }
}