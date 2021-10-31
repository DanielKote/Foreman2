
using System.Collections.Generic;

namespace Foreman
{
	public interface ProductionEntity : DataObjectBase
	{
		bool Enabled { get; set; }
		float Speed { get; }
		float BaseProductivityBonus { get; }

		int ModuleSlots { get; }
		IReadOnlyCollection<Module> ValidModules { get; }
    }

	public abstract class ProductionEntityPrototype : DataObjectBasePrototype, ProductionEntity
	{
		public bool Enabled { get; set; }
		public float Speed { get; set; }
		public float BaseProductivityBonus { get; set; }

		public int ModuleSlots { get; set; }
		public IReadOnlyCollection<Module> ValidModules { get { return validModules; } }

		internal HashSet<ModulePrototype> validModules { get; private set; }

		public ProductionEntityPrototype(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName, "-")
        {
			validModules = new HashSet<ModulePrototype>();
		}

	}
}
