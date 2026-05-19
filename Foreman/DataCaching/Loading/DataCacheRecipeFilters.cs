using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Foreman.DataCaching.Loading {
    /// <summary>IRecipe name filters applied during post-load availability pass.</summary>
    internal static class DataCacheRecipeFilters {
        public static readonly Regex[] WhiteList = [new("^empty-barrel$")];
        public static readonly Regex[] BlackList =
        [
            new("-barrel$"),
            new("^deadlock-packrecipe-"),
            new("^deadlock-unpackrecipe-"),
            new("^deadlock-plastic-packaging$")
        ];
        public static readonly KeyValuePair<string, Regex>[] RecyclingItemNameBlackList =
        [
            new("barrel", new Regex("-barrel$"))
        ];
    }
}
