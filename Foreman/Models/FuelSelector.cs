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

			Item fuel = fuelPriority.LastOrDefault(item => item.FuelsAssemblers.Contains(assembler)); //last added -> most recent
			if (fuel == null)
				fuel = assembler.ValidFuels.FirstOrDefault();
			if(fuel != null)
				UseFuel(fuel);

			return fuel;
		}

		public FuelSelector()
		{
			fuelPriority = new List<Item>();
		}
	}
}
