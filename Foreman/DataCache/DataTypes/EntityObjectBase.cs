using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Foreman {
    //burner & fluid burner types add fuel & burnt remains to the recipe node they are part of.
    //electric types add to the energy consumption (electic calculations) totals
    //heat types add a special 'heat' item to the recipe node they are part of (similar to burner types) -> in fact to simplify things they are handled as a burner with a specific burn item of 'heat'
    //void are considered as electric types with 0 electricity use
    public enum EnergySource { Burner, FluidBurner, Electric, Heat, Void }
    public enum EntityType { Miner, OffshorePump, Assembler, Beacon, Boiler, Generator, BurnerGenerator, Reactor, Rocket, ERROR }

    public interface EntityObjectBase : DataObjectBase {
        IReadOnlyCollection<Module> Modules { get; }
        IReadOnlyCollection<Item> Fuels { get; }
        IReadOnlyCollection<Item> AssociatedItems { get; }
        IReadOnlyDictionary<string, double> Pollution { get; }

        EntityType EntityType { get; }
        string GetEntityTypeName(bool plural);
        EnergySource EnergySource { get; }
        bool IsBurner { get; }
        bool IsTemperatureFluidBurner { get; }
        fRange FluidFuelTemperatureRange { get; }

        double GetBaseFuelConsumptionRate(Item? fuel, Quality quality, double temperature = double.NaN);

        bool IsMissing { get; }

        double GetSpeed(Quality quality);

        int ModuleSlots { get; }

        double GetEnergyDrain();
        double GetEnergyConsumption(Quality quality);
        double GetEnergyProduction(Quality quality);

        double ConsumptionEffectivity { get; }

        //steam generators
        double OperationTemperature { get; }
        //reactors
        double NeighbourBonus { get; }
    }

    internal class EntityObjectBasePrototype : DataObjectBasePrototype, EntityObjectBase {
        private bool availableOverride;
        public override bool Available { get { return availableOverride || associatedItems.Any(i => i.productionRecipes.Any(r => r.Available)); } set { availableOverride = value; } }

        public IReadOnlyCollection<Module> Modules { get { return modules; } }
        public IReadOnlyCollection<Item> Fuels { get { return fuels; } }
        public IReadOnlyCollection<Item> AssociatedItems { get { return associatedItems; } }
        public IReadOnlyDictionary<string, double> Pollution { get { return pollution; } }

        internal HashSet<ModulePrototype> modules { get; private set; }
        internal HashSet<ItemPrototype> fuels { get; private set; }
        internal List<ItemPrototype> associatedItems { get; private set; } //should honestly only be 1, but knowing modders....
        internal Dictionary<string, double> pollution { get; private set; }

        public EntityType EntityType { get; private set; }
        public EnergySource EnergySource { get; internal set; }
        public bool IsMissing { get; internal set; }
        public bool IsBurner { get { return (EnergySource == EnergySource.Burner || EnergySource == EnergySource.FluidBurner || EnergySource == EnergySource.Heat); } }
        public bool IsTemperatureFluidBurner { get; set; }
        public fRange FluidFuelTemperatureRange { get; set; }

        internal Dictionary<Quality, double> speed { get; private set; }
        internal Dictionary<Quality, double> energyConsumption { get; private set; }
        internal Dictionary<Quality, double> energyProduction { get; private set; }

        public double GetSpeed(Quality quality) { return speed.ContainsKey(quality) ? (speed[quality] > 0 ? speed[quality] : 1) : 1; }

        public int ModuleSlots { get; internal set; }
        public double NeighbourBonus { get; internal set; }

        internal double energyDrain;
        public double GetEnergyDrain() { return energyDrain; }
        public double GetEnergyConsumption(Quality quality) {
            if (this is BeaconPrototype)
                return quality.BeaconPowerMultiplier * (energyConsumption.ContainsKey(quality) ? energyConsumption[quality] : 1000);
            else
                return energyConsumption.ContainsKey(quality) ? energyConsumption[quality] : 1000;
        }
        public double GetEnergyProduction(Quality quality) { return energyConsumption.ContainsKey(quality) ? energyProduction[quality] : 0; }

        public double ConsumptionEffectivity { get; internal set; }
        public double OperationTemperature { get; internal set; }

        public EntityObjectBasePrototype(DataCache dCache, string name, string friendlyName, EntityType type, EnergySource source, bool isMissing) : base(dCache, name, friendlyName, "-") {
            availableOverride = false;

            modules = new HashSet<ModulePrototype>();
            fuels = new HashSet<ItemPrototype>();
            associatedItems = new List<ItemPrototype>();
            pollution = new Dictionary<string, double>();

            speed = new Dictionary<Quality, double>();
            energyConsumption = new Dictionary<Quality, double>();
            energyProduction = new Dictionary<Quality, double>();

            IsMissing = isMissing;
            EntityType = type;
            EnergySource = source;

            //just some base defaults -> helps prevent overflow errors during solving if the assembler is a missing entity
            ModuleSlots = 0;
            NeighbourBonus = 0;
            ConsumptionEffectivity = 1f;
            OperationTemperature = double.MaxValue;
            FluidFuelTemperatureRange = new fRange(double.MinValue, double.MaxValue);

        }

        public double GetBaseFuelConsumptionRate(Item? fuel, Quality quality, double temperature = double.NaN) {
            if (IsMissing)
                return 0.01; //prevents failure from importing a recipe node with set fuel without a valid assembler

            if ((EnergySource != EnergySource.Burner && EnergySource != EnergySource.FluidBurner && EnergySource != EnergySource.Heat))
                return 0.01; // Trace.Fail(string.Format("Cant ask for fuel consumption rate on a non-burner! {0}", this));
            else if (fuel is null || !fuels.Contains(fuel))
                return 0.01; // Trace.Fail(string.Format("Invalid fuel! {0} for entity {1}", fuel, this));
            else if (!IsTemperatureFluidBurner)
                return GetEnergyConsumption(quality) / (fuel.FuelValue * ConsumptionEffectivity);
            else if (!double.IsNaN(temperature) && (fuel is Fluid fluidFuel) && (temperature > fluidFuel.DefaultTemperature) && (fluidFuel.SpecificHeatCapacity > 0)) //temperature burn of liquid
                return GetEnergyConsumption(quality) / ((temperature - fluidFuel.DefaultTemperature) * fluidFuel.SpecificHeatCapacity * ConsumptionEffectivity);
            return 0.01;

            //0.01 is returned in case of error and prevents the solver from crashing. These errors will be noted on the node, so dont have to worry about them here.
        }

        public string GetEntityTypeName(bool plural) {
            if (plural) {
                switch (EntityType) {
                    case EntityType.Assembler:
                        return "Assemblers";
                    case EntityType.Beacon:
                        return "Beacons";
                    case EntityType.Boiler:
                        return "Boilers";
                    case EntityType.BurnerGenerator:
                        return "Generators";
                    case EntityType.Generator:
                        return "Generators";
                    case EntityType.Miner:
                        return "Miners";
                    case EntityType.OffshorePump:
                        return "Offshore Pumps";
                    case EntityType.Reactor:
                        return "Reactors";
                    case EntityType.Rocket:
                        return "Rockets";
                    default:
                        return "";
                }
            } else {
                switch (EntityType) {
                    case EntityType.Assembler:
                        return "Assembler";
                    case EntityType.Beacon:
                        return "Beacon";
                    case EntityType.Boiler:
                        return "Boiler";
                    case EntityType.BurnerGenerator:
                        return "Generator";
                    case EntityType.Generator:
                        return "Generator";
                    case EntityType.Miner:
                        return "Miner";
                    case EntityType.OffshorePump:
                        return "Offshore Pump";
                    case EntityType.Reactor:
                        return "Reactor";
                    case EntityType.Rocket:
                        return "Rocket";
                    default:
                        return "";
                }
            }
        }

    }
}