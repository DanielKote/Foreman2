using Foreman.DataCaching.DataTypes;
using System.Collections.Generic;
using System.Linq;

namespace Foreman.Models {
    public class FuelSelector {
        public IReadOnlyList<IItem> FuelPriority { get { return fuelPriority; } }
        private readonly List<IItem> fuelPriority;

        public void LoadFuelPriority(List<IItem> fuelList) {
            foreach (IItem fuel in fuelList)
                UseFuel(fuel);
        }

        public void ClearFuels() {
            fuelPriority.Clear();
        }

        public void UseFuel(IItem? fuel) {
            if (fuel == null)
                return;

            fuelPriority.Remove(fuel);
            fuelPriority.Add(fuel);
        }

        public IItem? GetFuel(IAssembler? assembler) {
            if (assembler is null || !assembler.IsBurner)
                return null;

            //check for valid fuel in order from highest standards to lowest
            var fuel = assembler.Fuels.OrderBy(item => item.Available)
                .ThenBy(item => item.ProductionRecipes.Any(r => r.Enabled))
                .ThenBy(item => item.ProductionRecipes.Any(r => r.Available))
                .ThenBy(item => item.ProductionRecipes.Any(r => r.Assemblers.Any(a => a.Enabled)))
                .ThenBy(item => item.ProductionRecipes.Count > 0)
                .ThenBy(item => fuelPriority.IndexOf(item))
                .LastOrDefault();

            if (fuel is not null)
                UseFuel(fuel);
            return fuel;
        }

        public FuelSelector() {
            fuelPriority = [];
        }
    }
}
