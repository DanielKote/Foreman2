using System;
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
			Item fuel = assembler.Fuels.OrderBy(item => item.Available)
				.ThenBy(item => item.ProductionRecipes.FirstOrDefault(r => r.Enabled) != null)
				.ThenBy(item => item.ProductionRecipes.FirstOrDefault(r => r.Available) != null)
				.ThenBy(item => item.ProductionRecipes.FirstOrDefault(r => r.Assemblers.FirstOrDefault(a => a.Enabled) != null) != null)
				.ThenBy(item => item.ProductionRecipes.Count > 0)
				.ThenBy(item => fuelPriority.IndexOf(item))
				.LastOrDefault();

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
