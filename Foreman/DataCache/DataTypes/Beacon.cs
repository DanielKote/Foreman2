using System.Collections.Generic;

namespace Foreman
{
    public interface Beacon : DataObjectBase
    {
        int ModuleSlots { get; }
        float Effectivity { get; }
        IReadOnlyCollection<Module> ValidModules { get; }
        Item AssociatedItem { get; }
    }

    public class BeaconPrototype : DataObjectBasePrototype, Beacon
    {
        public int ModuleSlots { get; set; }
        public float Effectivity { get; set; }
        public IReadOnlyCollection<Module> ValidModules { get { return validModules; } }
        public Item AssociatedItem { get { return myCache.Items[Name]; } }

        internal HashSet<ModulePrototype> validModules { get; private set; }


        public BeaconPrototype(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName, "-")
        {
            validModules = new HashSet<ModulePrototype>();
        }
    }
}
