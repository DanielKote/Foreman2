using System.Collections.Generic;

namespace Foreman
{
    public interface Beacon : DataObjectBase
    {
        int ModuleSlots { get; }
        float Effectivity { get; }
        IReadOnlyCollection<Module> ValidModules { get; }
    }

    public class BeaconPrototype : DataObjectBasePrototype, Beacon
    {
        public int ModuleSlots { get; set; }
        public float Effectivity { get; set; }
        public IReadOnlyCollection<Module> ValidModules { get { return validModules; } }

        internal HashSet<ModulePrototype> validModules { get; private set; }


        public BeaconPrototype(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName, "-")
        {
            validModules = new HashSet<ModulePrototype>();
        }
    }
}
