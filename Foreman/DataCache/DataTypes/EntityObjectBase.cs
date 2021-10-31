using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
	//burner & fluid burner types add fuel & burnt remains to the recipe node they are part of.
	//electric types add to the energy consumption (electic calculations) totals
	//heat types add a special 'heat' item to the recipe node they are part of (similar to burner types) -> in fact to simplify things they are handled as a burner with a specific burn item of 'heat'
	//void are considered as electric types with 0 electricity use
	public enum EnergySource { Burner, FluidBurner, Electric, Heat, Void }
	public enum EntityType { Miner, Assembler, Beacon, Boiler, Generator, BurnerGenerator, Reactor, ERROR }

	public interface EntityObjectBase : DataObjectBase
	{
		IReadOnlyCollection<Module> Modules { get; }
		IReadOnlyCollection<Item> Fuels { get; }
		IReadOnlyCollection<Item> AssociatedItems { get; }

		EntityType EntityType { get; }
		EnergySource EnergySource { get; }
		bool IsBurner { get; }

		bool IsMissing { get; }

		double Speed { get; }

		int ModuleSlots { get; }
		double NeighbourBonus { get; }

		double EnergyDrain { get; }
		double EnergyConsumption { get; }
		double EnergyProduction { get; }
		double ConsumptionEffectivity { get; }

		//steam generators
		double OperationTemperature { get; }

		double Pollution { get; }
	}

	internal class EntityObjectBasePrototype : DataObjectBasePrototype, EntityObjectBase
	{
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

			Pollution = 0;
		}

		public double GetFuelConsumptionRate(Item fuel)
		{
			if ((EnergySource != EnergySource.Burner && EnergySource != EnergySource.FluidBurner) || !fuels.Contains(fuel) || fuel.FuelValue <= 0)
				return 0;

			return EnergyConsumption * ConsumptionEffectivity / fuel.FuelValue;
		}
	}
}
