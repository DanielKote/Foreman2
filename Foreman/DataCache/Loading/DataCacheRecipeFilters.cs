using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Foreman {
    /// <summary>Recipe name filters applied during post-load availability pass.</summary>
    internal static class DataCacheRecipeFilters {
        public static readonly Regex[] WhiteList = { new Regex("^empty-barrel$") };
        public static readonly Regex[] BlackList =
        {
            new Regex("-barrel$"),
            new Regex("^deadlock-packrecipe-"),
            new Regex("^deadlock-unpackrecipe-"),
            new Regex("^deadlock-plastic-packaging$")
        };
        public static readonly KeyValuePair<string, Regex>[] RecyclingItemNameBlackList =
        {
            new KeyValuePair<string, Regex>("barrel", new Regex("-barrel$"))
        };
    }
}