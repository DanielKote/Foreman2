using System.Collections.Generic;

namespace Foreman.DataCaching.DataTypes {
    public interface IAssembler : IEntityObjectBase {
        IReadOnlyCollection<IRecipe> Recipes { get; }
        double BaseSpeedBonus { get; }
        double BaseProductivityBonus { get; }
        double BaseConsumptionBonus { get; }
        double BasePollutionBonus { get; }
        double BaseQualityBonus { get; }

        bool AllowBeacons { get; }
        bool AllowModules { get; }
    }

    internal class AssemblerPrototype(DataCache dCache, string name, string friendlyName, EntityType type, EnergySource source, bool isMissing = false) : EntityObjectBasePrototype(dCache, name, friendlyName, type, source, isMissing), IAssembler {
        public IReadOnlyCollection<IRecipe> Recipes { get { return _recipes; } }
        public double BaseSpeedBonus { get; set; }
        public double BaseProductivityBonus { get; set; }
        public double BaseConsumptionBonus { get; set; }
        public double BasePollutionBonus { get; set; }
        public double BaseQualityBonus { get; set; }

        public bool AllowBeacons { get; internal set; }  //assumed to be default? no info in LUA
        public bool AllowModules { get; internal set; }  //assumed to be default? no info in LUA

        private readonly HashSet<RecipePrototype> _recipes = [];
        internal HashSet<RecipePrototype> RecipesInternal => _recipes;

        public override string ToString() {
            return string.Format(CultureInfo.InvariantCulture, "Assembler: {0}", Name);
        }
    }
}
