using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foreman
{
    public class ModuleSelector
    {
        public enum Style { None, Speed, Productivity, Efficiency }
        public static readonly string[] StyleNames = new string[] { "None", "Speed", "Productivity", "Efficiency" };

        public Style SelectionStyle { get; set; }

        public List<Module> GetModules(Assembler assembler, Recipe recipe)
        {
            List<Module> moduleList = new List<Module>();

            return moduleList;
        }
    }
}
