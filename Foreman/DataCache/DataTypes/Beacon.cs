using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Foreman
{
	public interface Beacon : DataObjectBase
	{
		int ModuleSlots { get; }
		float Effectivity { get; }

		IReadOnlyCollection<Module> ValidModules { get; }
		IReadOnlyCollection<Item> AssociatedItems { get; }

		bool Enabled { get; set; }
		bool IsMissing { get; }
	}

	public class BeaconPrototype : DataObjectBasePrototype, Beacon
	{
		public int ModuleSlots { get; set; }
		public float Effectivity { get; set; }

		public IReadOnlyCollection<Module> ValidModules { get { return validModules; } }
		public IReadOnlyCollection<Item> AssociatedItems { get { return associatedItems; } }

		public bool Enabled { get; set; }
		public bool IsMissing { get; private set; }
		public override bool Available { get { return associatedItems.FirstOrDefault(i => i.Available) != null; } set { } }

		internal HashSet<ModulePrototype> validModules { get; private set; }
		internal List<ItemPrototype> associatedItems { get; private set; } //should honestly only be 1, but knowing modders....

		public BeaconPrototype(DataCache dCache, string name, string friendlyName, bool isMissing = false) : base(dCache, name, friendlyName, "-")
		{
			IsMissing = isMissing;

			validModules = new HashSet<ModulePrototype>();
			associatedItems = new List<ItemPrototype>();
		}

		public override string ToString() { return string.Format("Beacon: {0}", Name); }
	}
}
