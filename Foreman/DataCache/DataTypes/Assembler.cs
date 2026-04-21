using System.Collections.Generic;

namespace Foreman {
    public interface Assembler : EntityObjectBase {
        IReadOnlyCollection<Recipe> RecipesView { get; }
        double BaseSpeedBonus { get; }
        double BaseProductivityBonus { get; }
        double BaseConsumptionBonus { get; }
        double BasePollutionBonus { get; }
        double BaseQualityBonus { get; }

        bool AllowBeacons { get; }
        bool AllowModules { get; }
    }

    internal class AssemblerPrototype(DataCache dCache, string name, string friendlyName, EntityType type, EnergySource source, bool isMissing = false)
        : EntityObjectBasePrototype(dCache, name, friendlyName, type, source, isMissing), Assembler {
        public IReadOnlyCollection<Recipe> RecipesView => Recipes;

        public double BaseSpeedBonus { get; set; } = 0;
        public double BaseProductivityBonus { get; set; } = 0;
        public double BaseConsumptionBonus { get; set; } = 0;
        public double BasePollutionBonus { get; set; } = 0;
        public double BaseQualityBonus { get; set; } = 0;

        // assumed to be default? no info in LUA
        public bool AllowBeacons { get; internal set; } = false;
        // assumed to be default? no info in LUA
        public bool AllowModules { get; internal set; } = false;

        internal HashSet<RecipePrototype> Recipes { get; private set; } = [];

        public override string ToString() {
            return $"Assembler: {Name}";
        }
    }
}