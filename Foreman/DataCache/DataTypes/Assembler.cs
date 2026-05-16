using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman {
    public interface Assembler : EntityObjectBase {
        IReadOnlyCollection<Recipe> Recipes { get; }
        double BaseSpeedBonus { get; }
        double BaseProductivityBonus { get; }
        double BaseConsumptionBonus { get; }
        double BasePollutionBonus { get; }
        double BaseQualityBonus { get; }

        bool AllowBeacons { get; }
        bool AllowModules { get; }
    }

    internal class AssemblerPrototype : EntityObjectBasePrototype, Assembler {
        public IReadOnlyCollection<Recipe> Recipes { get { return recipes; } }
        public double BaseSpeedBonus { get; set; }
        public double BaseProductivityBonus { get; set; }
        public double BaseConsumptionBonus { get; set; }
        public double BasePollutionBonus { get; set; }
        public double BaseQualityBonus { get; set; }

        public bool AllowBeacons { get; internal set; }
        public bool AllowModules { get; internal set; }

        internal HashSet<RecipePrototype> recipes { get; private set; }

        public AssemblerPrototype(DataCache dCache, string name, string friendlyName, EntityType type, EnergySource source, bool isMissing = false) : base(dCache, name, friendlyName, type, source, isMissing) {
            BaseSpeedBonus = 0;
            BaseProductivityBonus = 0;
            BaseConsumptionBonus = 0;
            BasePollutionBonus = 0;
            BaseQualityBonus = 0;

            AllowBeacons = false; //assumed to be default? no info in LUA
            AllowModules = false; //assumed to be default? no info in LUA

            recipes = new HashSet<RecipePrototype>();
        }

        public override string ToString() {
            return String.Format("Assembler: {0}", Name);
        }
    }
}