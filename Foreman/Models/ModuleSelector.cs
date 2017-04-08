using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections;

namespace Foreman
{
    public abstract class ModuleSelector : ISerializable
    {
        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
        public abstract String Name { get; }

        public static ModuleSelector Fastest { get { return new ModuleSelectorFastest(); } }
        public static ModuleSelector None { get { return new ModuleSelectorNone(); } }
        public static ModuleSelector Productive { get { return new ModuleSelectorProductivity(); } }

        public static ModuleSelector Load(JToken token)
        {
            ModuleSelector filter = Fastest;

            if (token["ModuleFilterType"] != null)
            {
                switch ((String)token["ModuleFilterType"])
                {
                    case "Best":
                        filter = Fastest;
                        break;
                    case "None":
                        filter = None;
                        break;
                    case "Most Productive":
                        filter = Productive;
                        break;
                    case "Specific":
                        if (token["Module"] != null)
                        {
                            var moduleKey = (String)token["Module"];
                            if (DataCache.Modules.ContainsKey(moduleKey))
                            {
                                filter = new ModuleSpecificFilter(DataCache.Modules[moduleKey]);
                            }
                        }
                        break;
                }
            }

            return filter;
        }

        protected abstract IEnumerable<Module> availableModules();

        public IEnumerable<Module> For(Recipe recipe, int moduleSlots)
        {
            var modules = availableModules()
                .Where(m => m.Enabled)
                .Where(m => m.AllowedIn(recipe))
                .Take(1);

            return Enumerable.Repeat(modules, moduleSlots)
                .SelectMany(x => x);
        }

        public static ModuleSelector Specific(Module module)
        {
            return new ModuleSpecificFilter(module);
        }

        private class ModuleSpecificFilter : ModuleSelector
        {
            public Module Module { get; set; }

            public ModuleSpecificFilter(Module module)
            {
                this.Module = module;
            }
            
            public override String Name { get { return Module.Name; } }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("ModuleFilterType", "Specific");
                info.AddValue("Module", Module.Name);
            }

            protected override IEnumerable<Module> availableModules()
            {
                return Enumerable.Repeat(Module, 1);
            }
        }

        private class ModuleSelectorFastest : ModuleSelector
        {
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("ModuleFilterType", "Best");
            }

            public override String Name { get { return "Fastest"; } }

            protected override IEnumerable<Module> availableModules()
            {
                return DataCache.Modules.Values
                    .OrderBy(m => -m.SpeedBonus);
            }
        }

        private class ModuleSelectorProductivity : ModuleSelector
        {
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("ModuleFilterType", "Most Productive");
            }

            public override String Name { get { return "Most Productive"; } }

            protected override IEnumerable<Module> availableModules()
            {
                return DataCache.Modules.Values
                    .OrderBy(m => -m.ProductivityBonus);
            }
        }

        private class ModuleSelectorNone : ModuleSelector
        {
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("ModuleFilterType", "None");
            }

            public override String Name { get { return "None"; } }

            protected override IEnumerable<Module> availableModules()
            {
                return Enumerable.Empty<Module>();
            }
        }
    }
}
