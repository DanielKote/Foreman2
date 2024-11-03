using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Foreman
{ 
    public readonly struct ItemQualityPair : IEquatable<ItemQualityPair>
    {
        public readonly Item Item;
        public readonly Quality Quality;

        public ItemQualityPair(Item item, Quality quality)
        {
            Item = item;
            Quality = quality;

            if (Item != null && Quality == null)
                Trace.Fail(string.Format("Attempted to create item quality pair with item {0} and no quality!", Item));
        }

        public override bool Equals(object obj) => obj is ItemQualityPair other && this.Equals(other);
        public bool Equals(ItemQualityPair other) => this.Item == other.Item && this.Quality == other.Quality;
        public override int GetHashCode() => Item.GetHashCode() + Quality.GetHashCode();
        public static bool operator ==(ItemQualityPair lhs, ItemQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(ItemQualityPair lhs, ItemQualityPair rhs) => !(lhs == rhs);
        public override string ToString() { return Item.ToString() + " (" + Quality.ToString() + ")"; }

        public string FriendlyName
        {
            get
            {
                if (Quality == Quality.Owner.DefaultQuality)
                    return Item.FriendlyName;
                else
                    return Item.FriendlyName + " (" + Quality.FriendlyName + ")";
            }
        }
        public Bitmap Icon
        {
            get
            {
                if (Item == null)
                    return null;
                return Quality == Quality.Owner.DefaultQuality ? Item.Icon : IconCacheProcessor.CombinedQualityIcon(Item.Icon, Quality.Icon);
            }
        }
    }

    public readonly struct ModuleQualityPair : ISerializable
    {
        public readonly Module Module;
        public readonly Quality Quality;

        public ModuleQualityPair(Module module, Quality quality)
        {
            Module = module;
            Quality = quality;

            if (Module != null && Quality == null)
                Trace.Fail(string.Format("Attempted to create Module quality pair with Module {0} and no quality!", Module));
        }

        public override bool Equals(object obj) => obj is ModuleQualityPair other && this.Equals(other);
        public bool Equals(ModuleQualityPair other) => this.Module == other.Module && this.Quality == other.Quality;
        public override int GetHashCode() => Module.GetHashCode() + Quality.GetHashCode();
        public static bool operator ==(ModuleQualityPair lhs, ModuleQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(ModuleQualityPair lhs, ModuleQualityPair rhs) => !(lhs == rhs);
        public override string ToString() { return Module.ToString() + " (" + Quality.ToString() + ")"; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Module.Name);
            info.AddValue("Quality", Quality.Name);
        }

        public string FriendlyName
        {
            get
            {
                if (Quality == Quality.Owner.DefaultQuality)
                    return Module.FriendlyName;
                else
                    return Module.FriendlyName + " (" + Quality.FriendlyName + ")";
            }
        }
        public Bitmap Icon
        {
            get
            {
                if (Module == null)
                    return null;
                return Quality == Quality.Owner.DefaultQuality ? Module.Icon : IconCacheProcessor.CombinedQualityIcon(Module.Icon, Quality.Icon);
            }
        }
    }

    public readonly struct AssemblerQualityPair
    {
        public readonly Assembler Assembler;
        public readonly Quality Quality;

        public AssemblerQualityPair(Assembler assembler, Quality quality)
        {
            Assembler = assembler;
            Quality = quality;

            if (Assembler != null && Quality == null)
                Trace.Fail(string.Format("Attempted to create Assembler quality pair with Assembler {0} and no quality!", Assembler));
        }

        public override bool Equals(object obj) => obj is AssemblerQualityPair other && this.Equals(other);
        public bool Equals(AssemblerQualityPair other) => this.Assembler == other.Assembler && this.Quality == other.Quality;
        public override int GetHashCode() => Assembler.GetHashCode() + Quality.GetHashCode();
        public static bool operator ==(AssemblerQualityPair lhs, AssemblerQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(AssemblerQualityPair lhs, AssemblerQualityPair rhs) => !(lhs == rhs);
        public override string ToString() { return Assembler.ToString() + " (" + Quality.ToString() + ")"; }

        public string FriendlyName
        {
            get
            {
                if (Quality == Quality.Owner.DefaultQuality)
                    return Assembler.FriendlyName;
                else
                    return Assembler.FriendlyName + " (" + Quality.FriendlyName + ")";
            }
        }
        public Bitmap Icon
        {
            get
            {
                if (Assembler == null)
                    return null;
                return Quality == Quality.Owner.DefaultQuality ? Assembler.Icon : IconCacheProcessor.CombinedQualityIcon(Assembler.Icon, Quality.Icon);
            }
        }
    }

    public readonly struct BeaconQualityPair
    {
        public readonly Beacon Beacon;
        public readonly Quality Quality;

        public BeaconQualityPair(string comment) {
            Beacon = null;
            Quality = null;
        }

        public BeaconQualityPair(Beacon beacon, Quality quality)
        {
            if (beacon == null || quality == null)
                throw new NullReferenceException("null error - Beacon: " + nameof(Beacon) + " Quality: " + nameof(Quality));

            Beacon = beacon;
            Quality = quality;
        }

        public override bool Equals(object obj) => obj is BeaconQualityPair other && this.Equals(other);
        public bool Equals(BeaconQualityPair other) => this.Beacon == other.Beacon && this.Quality == other.Quality;
        public override int GetHashCode() => Beacon.GetHashCode() + Quality.GetHashCode();
        public static bool operator ==(BeaconQualityPair lhs, BeaconQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(BeaconQualityPair lhs, BeaconQualityPair rhs) => !(lhs == rhs);
        public override string ToString() { return Beacon.ToString() + " (" + Quality.ToString() + ")"; }
        public static implicit operator bool(BeaconQualityPair bp) => bp != null && bp.Beacon != null && bp.Quality != null;

        public string FriendlyName
        {
            get
            {
                if (Quality == Quality.Owner.DefaultQuality)
                    return Beacon.FriendlyName;
                else
                    return Beacon.FriendlyName + " (" + Quality.FriendlyName + ")";
            }
        }
        public Bitmap Icon
        {
            get
            {
                if (Beacon == null)
                    return null;
                return Quality == Quality.Owner.DefaultQuality ? Beacon.Icon : IconCacheProcessor.CombinedQualityIcon(Beacon.Icon, Quality.Icon);
            }
        }
    }

    public readonly struct RecipeQualityPair
    {
        public readonly Recipe Recipe;
        public readonly Quality Quality;

        public RecipeQualityPair(Recipe recipe, Quality quality)
        {
            Recipe = recipe;
            Quality = quality;

            if (Recipe != null && Quality == null)
                Trace.Fail(string.Format("Attempted to create recipe quality pair with recipe {0} and no quality!", recipe));
        }

        public override bool Equals(object obj) => obj is RecipeQualityPair other && this.Equals(other);
        public bool Equals(RecipeQualityPair other) => this.Recipe == other.Recipe && this.Quality == other.Quality;
        public override int GetHashCode() => Recipe.GetHashCode() + Quality.GetHashCode();
        public static bool operator ==(RecipeQualityPair lhs, RecipeQualityPair rhs) => lhs.Equals(rhs);
        public static bool operator !=(RecipeQualityPair lhs, RecipeQualityPair rhs) => !(lhs == rhs);
        public override string ToString() { return Recipe.ToString() + " (" + Quality.ToString() + ")"; }

        public string FriendlyName
        {
            get
            {
                if (Quality == Quality.Owner.DefaultQuality)
                    return Recipe.FriendlyName;
                else
                    return Recipe.FriendlyName + " (" + Quality.FriendlyName + ")";
            }
        }
        public Bitmap Icon
        {
            get
            {
                if (Recipe == null)
                    return null;
                return Quality == Quality.Owner.DefaultQuality ? Recipe.Icon : IconCacheProcessor.CombinedQualityIcon(Recipe.Icon, Quality.Icon);
            }
        }
    }
}
