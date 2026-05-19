using System.Collections.Generic;

namespace Foreman.Models.Nodes {
    /// <summary>Shared error/warning text for supplier and consumer nodes (identical flag layout).</summary>
    internal static class ItemQualityNodeMessages {
        internal const int ItemMissing = 0b_0000_0000_0001;
        internal const int QualityMissing = 0b_0000_0000_0010;
        internal const int InvalidLinks = 0b_1000_0000_0000;
        internal const int ItemUnavailable = 0b_0000_0000_0001;
        internal const int ItemDisabled = 0b_0000_0000_0010;
        internal const int QualityUnavailable = 0b_0000_0000_0100;
        internal const int QualityDisabled = 0b_0000_0000_1000;

        public static List<string> GetErrors(ItemQualityPair item, int errorSet) {
            List<string> errors = [];
            if (item.Item is null || item.Quality is null)
                return errors;
            if ((errorSet & ItemMissing) != 0)
                errors.Add(string.Format(DisplayCulture.Format, "> Item \"{0}\" doesnt exist in preset!", item.Item.FriendlyName));
            if ((errorSet & QualityMissing) != 0)
                errors.Add(string.Format(DisplayCulture.Format, "> Quality \"{0}\" doesnt exist in preset!", item.Quality.FriendlyName));
            if ((errorSet & InvalidLinks) != 0)
                errors.Add("> Some links are invalid!");
            return errors;
        }

        public static List<string> GetWarnings(ItemQualityPair item, int warningSet) {
            List<string> warnings = [];
            if (item.Quality is null)
                return warnings;
            if ((warningSet & QualityUnavailable) != 0)
                warnings.Add(string.Format(DisplayCulture.Format, "> Quality \"{0}\" isnt available in regular gameplay.", item.Quality.FriendlyName));
            else if ((warningSet & QualityDisabled) != 0)
                warnings.Add(string.Format(DisplayCulture.Format, "> Quality \"{0}\" isnt currently enabled.", item.Quality.FriendlyName));
            if (item.Item is not null) {
                if ((warningSet & ItemDisabled) != 0)
                    warnings.Add(string.Format(DisplayCulture.Format, "> Item \"{0}\" isnt currently enabled.", item.Item.FriendlyName));
                if ((warningSet & ItemUnavailable) != 0)
                    warnings.Add(string.Format(DisplayCulture.Format, "> Item \"{0}\" is unavailable in regular play.", item.Item.FriendlyName));
            }
            return warnings;
        }
    }
}
