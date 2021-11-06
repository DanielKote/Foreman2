using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Foreman
{
	//burner & fluid burner types add fuel & burnt remains to the recipe node they are part of.
	//electric types add to the energy consumption (electic calculations) totals
	//heat types add a special 'heat' item to the recipe node they are part of (similar to burner types) -> in fact to simplify things they are handled as a burner with a specific burn item of 'heat'
	//void are considered as electric types with 0 electricity use
	public enum EnergySource { Burner, FluidBurner, Electric, Heat, Void }
	public enum EntityType { Miner, OffshorePump, Assembler, Beacon, Boiler, Generator, BurnerGenerator, Reactor, Rocket, ERROR }

	public interface EntityObjectBase : DataObjectBase
	{
		IReadOnlyCollection<Module> Modules { get; }
		IReadOnlyCollection<Item> Fuels { get; }
		IReadOnlyCollection<Item> AssociatedItems { get; }

		EntityType EntityType { get; }
		string GetEntityTypeName(bool plural);
		EnergySource EnergySource { get; }
		bool IsBurner { get; }
		bool IsTemperatureFluidBurner { get; }
		fRange FluidFuelTemperatureRange { get; } 

		double GetBaseFuelConsumptionRate(Item fuel, double temperature = double.NaN);

		bool IsMissing { get; }

		double Speed { get; }

		int ModuleSlots { get; }

		double EnergyDrain { get; }
		double EnergyConsumption { get; }
		double EnergyProduction { get; }
		double ConsumptionEffectivity { get; }
		double Pollution { get; }

		//steam generators
		double OperationTemperature { get; }
		//reactors
		double NeighbourBonus { get; }
	}

	internal class EntityObjectBasePrototype : DataObjectBasePrototype, EntityObjectBase
	{
		private bool availableOverride;
		public override bool Available { get { return availableOverride || associatedItems.Any(i => i.productionRecipes.Any(r => r.Available)); } set { availableOverride = value; } }

		public IReadOnlyCollection<Module> Modules { get { return modules; } }
		public IReadOnlyCollection<Item> Fuels { get { return fuels; } }
		public IReadOnlyCollection<Item> AssociatedItems { get { return associatedItems; } }

		internal HashSet<ModulePrototype> modules { get; private set; }
		internal HashSet<ItemPrototype> fuels { get; private set; }
		internal List<ItemPrototype> associatedItems { get; private set; } //should honestly only be 1, but knowing modders....

		public EntityType EntityType { get; private set; }
		public EnergySource EnergySource { get; internal set; }
		public bool IsMissing { get; internal set; }
		public bool IsBurner { get { return (EnergySource == EnergySource.Burner || EnergySource == EnergySource.FluidBurner || EnergySource == EnergySource.Heat); } }
		public bool IsTemperatureFluidBurner { get; set; }
		public fRange FluidFuelTemperatureRange { get; set; }

		public double Speed { get; internal set; }

		public int ModuleSlots { get; internal set; }
		public double NeighbourBonus { get; internal set; }

		public double EnergyDrain { get; internal set; } //per second
		public double EnergyConsumption { get; internal set; }
		public double EnergyProduction { get; internal set; }
		public double ConsumptionEffectivity { get; internal set; }
		public double OperationTemperature { get; internal set; }

		public double Pollution { get; internal set; }

		public EntityObjectBasePrototype(DataCache dCache, string name, string friendlyName, EntityType type, EnergySource source, bool isMissing) : base(dCache, name, friendlyName, "-")
		{
			availableOverride = false;

			modules = new HashSet<ModulePrototype>();
			fuels = new HashSet<ItemPrototype>();
			associatedItems = new List<ItemPrototype>();

			IsMissing = isMissing;
			EntityType = type;
			EnergySource = source;

			//just some base defaults -> helps prevent overflow errors during solving if the assembler is a missing entity
			Speed = 1f;
			ModuleSlots = 0;
			NeighbourBonus = 0;
			EnergyDrain = 0; //passive use (pretty much electricity only)
			EnergyConsumption = 1000; //default value to prevent issues with missing objects
			EnergyProduction = 0;
			ConsumptionEffectivity = 1f;
			OperationTemperature = double.MaxValue;
			FluidFuelTemperatureRange = new fRange(double.MinValue, double.MaxValue);

			Pollution = 0;
		}

		public double GetBaseFuelConsumptionRate(Item fuel, double temperature = double.NaN)
		{
			if ((EnergySource != EnergySource.Burner && EnergySource != EnergySource.FluidBurner && EnergySource != EnergySource.Heat))
				Trace.Fail(string.Format("Cant ask for fuel consumption rate on a non-burner! {0}", this));
			else if (!fuels.Contains(fuel))
				Trace.Fail(string.Format("Invalid fuel! {0} for entity {1}", fuel, this));
			else if (!IsTemperatureFluidBurner)
				return EnergyConsumption / (fuel.FuelValue * ConsumptionEffectivity);
			else if (!double.IsNaN(temperature) && (fuel is Fluid fluidFuel) && (temperature > fluidFuel.DefaultTemperature) && (fluidFuel.SpecificHeatCapacity > 0)) //temperature burn of liquid
				return EnergyConsumption / ((temperature - fluidFuel.DefaultTemperature) * fluidFuel.SpecificHeatCapacity * ConsumptionEffectivity);
			return 0.01; // we cant have a 0 consumption rate as that would mess with the solver.
		}

		public string GetEntityTypeName(bool plural)
		{
			if (plural)
			{
				switch (EntityType)
				{
					case EntityType.Assembler: return "Assemblers";
					case EntityType.Beacon: return "Beacons";
					case EntityType.Boiler: return "Boilers";
					case EntityType.BurnerGenerator: return "Generators";
					case EntityType.Generator: return "Generators";
					case EntityType.Miner: return "Miners";
					case EntityType.OffshorePump: return "Offshore Pumps";
					case EntityType.Reactor: return "Reactors";
					case EntityType.Rocket: return "Rockets";
					default: return "";
				}
			}
			else
			{
				switch (EntityType)
				{
					case EntityType.Assembler: return "Assembler";
					case EntityType.Beacon: return "Beacon";
					case EntityType.Boiler: return "Boiler";
					case EntityType.BurnerGenerator: return "Generator";
					case EntityType.Generator: return "Generator";
					case EntityType.Miner: return "Miner";
					case EntityType.OffshorePump: return "Offshore Pump";
					case EntityType.Reactor: return "Reactor";
					case EntityType.Rocket: return "Rocket";
					default: return "";
				}
			}
		}

	}
}
