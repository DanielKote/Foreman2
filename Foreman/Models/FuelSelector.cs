using System.Collections.Generic;
using System.Linq;

namespace Foreman {
    public class FuelSelector {
        public IReadOnlyList<Item> FuelPriority => _fuelPriority;

        private List<Item> _fuelPriority = [];

        public void LoadFuelPriority(List<Item> fuelList) {
            foreach (var fuel in fuelList)
                UseFuel(fuel);
        }

        public void ClearFuels() {
            _fuelPriority.Clear();
        }

        public void UseFuel(Item fuel) {
            if (fuel == null)
                return;

            _fuelPriority.Remove(fuel);
            _fuelPriority.Add(fuel);
        }

        public Item GetFuel(Assembler assembler) {
            if (assembler == null || !assembler.IsBurner)
                return null;

            // check for valid fuel in order from the highest standards to lowest
            var fuel = assembler.Fuels.OrderBy(item => item.Available)
                .ThenBy(item => item.ProductionRecipes.Any(r => r.Enabled))
                .ThenBy(item => item.ProductionRecipes.Any(r => r.Available))
                .ThenBy(item => item.ProductionRecipes.Any(r => r.Assemblers.Any(a => a.Enabled)))
                .ThenBy(item => item.ProductionRecipes.Count > 0)
                .ThenBy(item => _fuelPriority.IndexOf(item))
                .LastOrDefault();

            if (fuel != null)
                UseFuel(fuel);
            return fuel;
        }
    }
}