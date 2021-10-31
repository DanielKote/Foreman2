using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
	public class FuelSelector
	{
		public IReadOnlyList<Item> FuelPriority { get { return fuelPriority; } }
		private List<Item> fuelPriority;

		public void LoadFuelPriority(List<Item> fuelList)
		{
			foreach (Item fuel in fuelList)
				UseFuel(fuel);
		}

		public void ClearFuels()
		{
			fuelPriority.Clear();
		}

		public void UseFuel(Item fuel)
		{
			if (fuel == null)
				return;

			fuelPriority.Remove(fuel);
			fuelPriority.Add(fuel);
		}

		public Item GetFuel(Assembler assembler)
		{
			if (assembler == null || !assembler.IsBurner)
				return null;

			//check for valid fuel in order from highest standards to lowest
			Item fuel = fuelPriority.LastOrDefault(item => item.FuelsAssemblers.Contains(assembler) && item.ProductionRecipes.FirstOrDefault(r => r.Enabled && r.HasEnabledAssemblers) != null);
			if (fuel == null)
				fuel = assembler.Fuels.FirstOrDefault(i => i.ProductionRecipes.FirstOrDefault(r => r.Enabled && r.HasEnabledAssemblers) != null);
			if(fuel == null)
				fuel = fuelPriority.LastOrDefault(item => item.FuelsAssemblers.Contains(assembler) && item.ProductionRecipes.FirstOrDefault(r => r.Enabled) != null);
			if(fuel == null)
				fuel = assembler.Fuels.FirstOrDefault(i => i.ProductionRecipes.FirstOrDefault(r => r.Enabled) != null);
			if(fuel == null)
				fuel = fuelPriority.LastOrDefault(item => item.FuelsAssemblers.Contains(assembler) && item.ProductionRecipes.Count > 0);
			if(fuel == null)
				fuel = assembler.Fuels.FirstOrDefault(i => i.ProductionRecipes.Count > 0);
			if(fuel == null)
				fuel = assembler.Fuels.FirstOrDefault();

			if (fuel != null)
				UseFuel(fuel);
			return fuel;
		}

		public FuelSelector()
		{
			fuelPriority = new List<Item>();
		}
	}
}
