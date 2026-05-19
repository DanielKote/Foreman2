using Foreman.DataCaching.DataTypes;
using System.Collections.Generic;
using System.Linq;

namespace Foreman.Models {
    public class AssemblerSelector {
        public enum Style { Worst, WorstNonBurner, WorstBurner, Best, BestNonBurner, BestBurner, MostModules }
        public static readonly string[] StyleNames = ["Worst", "Worst Non-Burner", "Worst Burner", "Best", "Best Non-Burner", "Best Burner", "Most Modules"];

        public Style DefaultSelectionStyle { get; set; }

        public AssemblerSelector() { DefaultSelectionStyle = Style.WorstNonBurner; }


        public IAssembler GetAssembler(IRecipe recipe) { return GetAssembler(recipe, DefaultSelectionStyle); }
        public static IAssembler GetAssembler(IRecipe recipe, Style style) { return GetOrderedAssemblerList(recipe, style).First(); }
        public List<IAssembler> GetOrderedAssemblerList(IRecipe recipe) { return GetOrderedAssemblerList(recipe, DefaultSelectionStyle); }

        public static List<IAssembler> GetOrderedAssemblerList(IRecipe recipe, Style style) {
            if (style == Style.MostModules) {
                return [.. recipe.Assemblers
                    .OrderByDescending(a => a.Enabled)
                    .ThenByDescending(a => a.Available)
                    .ThenByDescending(a => ((a.ModuleSlots * 1000000) + (a.Owner.DefaultQuality is not null ? a.GetSpeed(a.Owner.DefaultQuality) : 1)))];
            } else {
                int orderDirection;
                bool includeNonBurners;
                bool includeBurners;

                switch (style) {
                    case Style.Worst:
                        orderDirection = -1;
                        includeNonBurners = true;
                        includeBurners = true;
                        break;
                    case Style.WorstBurner:
                        orderDirection = -1;
                        includeNonBurners = false;
                        includeBurners = true;
                        break;
                    case Style.WorstNonBurner:
                        orderDirection = -1;
                        includeNonBurners = true;
                        includeBurners = false;
                        break;
                    case Style.Best:
                        orderDirection = 1;
                        includeNonBurners = true;
                        includeBurners = true;
                        break;
                    case Style.BestBurner:
                        orderDirection = 1;
                        includeNonBurners = false;
                        includeBurners = true;
                        break;
                    case Style.BestNonBurner:
                    default:
                        orderDirection = 1;
                        includeNonBurners = true;
                        includeBurners = false;
                        break;
                }
                return [.. recipe.Assemblers
                    .OrderByDescending(a => a.Enabled)
                    .ThenByDescending(a => (a.IsBurner && includeBurners) || (!a.IsBurner && includeNonBurners))
                    .ThenByDescending(a => a.Available)
                    .ThenByDescending(a => orderDirection * (((a.Owner.DefaultQuality is not null ? a.GetSpeed(a.Owner.DefaultQuality) : 1) * 1000000) + a.ModuleSlots))];
            }
        }
    }
}
