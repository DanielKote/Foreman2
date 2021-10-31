
namespace Foreman
{
	public interface ProductionEntity : DataObjectBase
	{
		bool Enabled { get; set; }
		int ModuleSlots { get; }
		float Speed { get; }
    }

	public abstract class ProductionEntityPrototype : DataObjectBasePrototype
	{
		public bool Enabled { get; set; }
		public int ModuleSlots { get; set; }
		public float Speed { get; set; }

		public ProductionEntityPrototype(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName, "-") { }

	}
}
