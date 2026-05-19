using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using System;
using System.Drawing;

namespace Foreman.Models {
    public readonly struct ItemQualityPair : IEquatable<ItemQualityPair> {
        public IItem? Item { get; }
        public IQuality? Quality { get; }
        public ItemQualityPair() {
            Item = null;
            Quality = null;
        }

        public ItemQualityPair(IItem item, IQuality quality) {
            Item = item;
            Quality = quality;

            if (Item == null || Quality == null)
                throw new InvalidOperationException("null error - Item: " + nameof(Item) + " Quality: " + nameof(Quality));
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
                return Quality == Quality?.Owner.DefaultQuality ? (Item?.FriendlyName) : Item?.FriendlyName + " (" + Quality?.FriendlyName + ")";
            }
        }
        public Bitmap? Icon {
            get {
                return Item is null || Quality is null
                    ? null
                    : Quality == Quality.Owner.DefaultQuality ? Item.Icon : IconCacheProcessor.CombinedQualityIcon(Item.Icon, Quality.Icon);
            }
        }
    }

    public readonly struct ModuleQualityPair {
        public IModule Module { get; }
        public IQuality Quality { get; }
        public ModuleQualityPair(IModule module, IQuality quality) {
            Module = module;
            Quality = quality;

            if (Module == null || Quality == null)
                throw new InvalidOperationException("null error - Module: " + nameof(Module) + " Quality: " + nameof(Quality));
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
                return Quality == Quality.Owner.DefaultQuality ? Module.FriendlyName : Module.FriendlyName + " (" + Quality.FriendlyName + ")";
            }
        }
        public Bitmap? Icon {
            get {
                return Module == null
                    ? null
                    : Quality == Quality.Owner.DefaultQuality ? Module.Icon : IconCacheProcessor.CombinedQualityIcon(Module.Icon, Quality.Icon);
            }
        }
    }

    public readonly struct AssemblerQualityPair {
        public IAssembler Assembler { get; }
        public IQuality Quality { get; }
        public AssemblerQualityPair(IAssembler assembler, IQuality quality) {
            Assembler = assembler;
            Quality = quality;

            if (Assembler == null || Quality == null)
                throw new InvalidOperationException("null error - Assembler: " + nameof(Assembler) + " Quality: " + nameof(Quality));
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
                return Quality == Quality.Owner.DefaultQuality ? Assembler.FriendlyName : Assembler.FriendlyName + " (" + Quality.FriendlyName + ")";
            }
        }
        public Bitmap? Icon {
            get {
                return Assembler == null
                    ? null
                    : Quality == Quality.Owner.DefaultQuality ? Assembler.Icon : IconCacheProcessor.CombinedQualityIcon(Assembler.Icon, Quality.Icon);
            }
        }
    }

    public readonly struct BeaconQualityPair {
        public IBeacon? Beacon { get; }
        public IQuality? Quality { get; }
        public BeaconQualityPair() {
            Beacon = null;
            Quality = null;
        }

        public BeaconQualityPair(IBeacon beacon, IQuality quality) {
            Beacon = beacon;
            Quality = quality;

            if (Beacon == null || Quality == null)
                throw new InvalidOperationException("null error - Beacon: " + nameof(Beacon) + " Quality: " + nameof(Quality));
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
                return Quality is null
                    ? null
                    : Quality == Quality.Owner.DefaultQuality ? (Beacon?.FriendlyName) : Beacon?.FriendlyName + " (" + Quality.FriendlyName + ")";
            }
        }
        public Bitmap? Icon {
            get {
                return Beacon is null || Quality is null
                    ? null
                    : Quality == Quality.Owner.DefaultQuality ? Beacon.Icon : IconCacheProcessor.CombinedQualityIcon(Beacon.Icon, Quality.Icon);
            }
        }
    }

    public readonly struct RecipeQualityPair {
        public IRecipe? Recipe { get; }
        public IQuality? Quality { get; }
        public RecipeQualityPair() {
            Recipe = null;
            Quality = null;
        }

        public RecipeQualityPair(IRecipe recipe, IQuality quality) {
            Recipe = recipe;
            Quality = quality;

            if (Recipe == null || Quality == null)
                throw new InvalidOperationException("null error - Recipe: " + nameof(Recipe) + " Quality: " + nameof(Quality));
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
                return Quality == Quality?.Owner.DefaultQuality ? (Recipe?.FriendlyName) : Recipe?.FriendlyName + " (" + Quality?.FriendlyName + ")";
            }
        }
        public Bitmap? Icon {
            get {
                return Recipe is null || Quality is null
                    ? null
                    : Quality == Quality.Owner.DefaultQuality ? Recipe.Icon : IconCacheProcessor.CombinedQualityIcon(Recipe.Icon, Quality.Icon);
            }
        }
    }
}
