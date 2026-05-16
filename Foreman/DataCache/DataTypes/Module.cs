using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Foreman {
    public interface Module : DataObjectBase {
        IReadOnlyCollection<Recipe> Recipes { get; }
        IReadOnlyCollection<Assembler> Assemblers { get; }
        IReadOnlyCollection<Beacon> Beacons { get; }
        IReadOnlyCollection<Recipe>? AvailableRecipes { get; }

        Item AssociatedItem { get; }

        double GetSpeedBonus(Quality quality);
        double GetProductivityBonus(Quality quality);
        double GetConsumptionBonus(Quality quality);
        double GetPolutionBonus(Quality quality);
        double GetQualityBonus(Quality quality);

        double GetSpeedBonus(int qualityLevel = 0);
        double GetProductivityBonus(int qualityLevel = 0);
        double GetConsumptionBonus(int qualityLevel = 0);
        double GetPolutionBonus(int qualityLevel = 0);
        double GetQualityBonus(int qualityLevel = 0);

        string Category { get; }

        int Tier { get; }

        bool IsMissing { get; }
    }

    public class ModulePrototype : DataObjectBasePrototype, Module {
        public IReadOnlyCollection<Recipe> Recipes => recipes;
        public IReadOnlyCollection<Assembler> Assemblers => assemblers;
        public IReadOnlyCollection<Beacon> Beacons => beacons;
        public IReadOnlyCollection<Recipe>? AvailableRecipes { get; private set; }
        public Item AssociatedItem => Owner.Items[Name];

        public double GetSpeedBonus(Quality quality) { return GetSpeedBonus(quality.Level); }
        public double GetProductivityBonus(Quality quality) { return GetProductivityBonus(quality.Level); }
        public double GetConsumptionBonus(Quality quality) { return GetConsumptionBonus(quality.Level); }
        public double GetPolutionBonus(Quality quality) { return GetPolutionBonus(quality.Level); }
        public double GetQualityBonus(Quality quality) { return GetQualityBonus(quality.Level); }

        public double GetSpeedBonus(int qualityLevel = 0) { return SpeedBonus <= 0 || qualityLevel == 0 ? SpeedBonus : Math.Truncate(SpeedBonus * (1 + (qualityLevel * 0.3)) * 100) / 100; }
        public double GetProductivityBonus(int qualityLevel = 0) { return ProductivityBonus <= 0 || qualityLevel == 0 ? ProductivityBonus : Math.Truncate(ProductivityBonus * (1 + (qualityLevel * 0.3)) * 100) / 100; }
        public double GetConsumptionBonus(int qualityLevel = 0) { return ConsumptionBonus >= 0 || qualityLevel == 0 ? ConsumptionBonus : Math.Truncate(ConsumptionBonus * (1 + (qualityLevel * 0.3)) * 100) / 100; }
        public double GetPolutionBonus(int qualityLevel = 0) { return PollutionBonus >= 0 || qualityLevel == 0 ? PollutionBonus : Math.Truncate(PollutionBonus * (1 + (qualityLevel * 0.3)) * 100) / 100; }
        public double GetQualityBonus(int qualityLevel = 0) { return QualityBonus <= 0 || qualityLevel == 0 ? QualityBonus : Math.Truncate(QualityBonus * (1 + (qualityLevel * 0.3)) * 100) / 100; }

        public double SpeedBonus { get; internal set; }
        public double ProductivityBonus { get; internal set; }
        public double ConsumptionBonus { get; internal set; }
        public double PollutionBonus { get; internal set; }
        public double QualityBonus { get; internal set; }

        public string Category { get; internal set; }

        public int Tier { get; set; }

        public bool IsMissing { get; private set; }
        public override bool Available { get { return AssociatedItem.Available; } set { } }

        internal HashSet<RecipePrototype> recipes { get; private set; }
        internal HashSet<AssemblerPrototype> assemblers { get; private set; }
        internal HashSet<BeaconPrototype> beacons { get; private set; }

        public ModulePrototype(DataCache dCache, string name, string friendlyName, bool isMissing = false) : base(dCache, name, friendlyName, "-") {
            Enabled = true;
            IsMissing = isMissing;

            SpeedBonus = 0;
            ProductivityBonus = 0;
            ConsumptionBonus = 0;
            PollutionBonus = 0;
            QualityBonus = 0;

            Category = "";

            recipes = new HashSet<RecipePrototype>();
            assemblers = new HashSet<AssemblerPrototype>();
            beacons = new HashSet<BeaconPrototype>();
        }

        internal void UpdateAvailabilities() {
            AvailableRecipes = new HashSet<Recipe>(recipes.Where(r => r.Enabled));
        }

        public override string ToString() { return string.Format("Module: {0}", Name); }
    }
}