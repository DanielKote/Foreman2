using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foreman
{
    public class AssemblerSelector
    {
        public enum Style { Worst, WorstNonBurner, Best, BestNonBurner}
        public static readonly string[] StyleNames = new string[] { "Worst", "Worst non-Burer", "Best", "Best non-Burner" };

        public Style SelectionStyle { get; set; } 

        public Assembler GetAssembler(Recipe recipe)
        {
            return null;
        }
    }
}
