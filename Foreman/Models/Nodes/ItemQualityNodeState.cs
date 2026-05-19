namespace Foreman.Models.Nodes {
    /// <summary>Shared error/warning evaluation for supplier and consumer nodes (identical flag layout).</summary>
    internal static class ItemQualityNodeState {
        public static void Evaluate(
            ItemQualityPair pair,
            bool allLinksValid,
            bool allLinksConnected,
            out int errorSet,
            out int warningSet,
            out NodeState state) {
            errorSet = 0;
            warningSet = 0;

            if (pair.Item?.IsMissing is true)
                errorSet |= ItemQualityNodeMessages.ItemMissing;
            if (pair.Quality?.Available is not true)
                errorSet |= ItemQualityNodeMessages.QualityMissing;
            if (!allLinksValid)
                errorSet |= ItemQualityNodeMessages.InvalidLinks;

            if (errorSet != 0) {
                state = NodeState.Error;
                return;
            }

            if (pair.Quality?.Enabled is not true)
                warningSet |= ItemQualityNodeMessages.QualityDisabled;
            if (pair.Item?.Available is not true)
                warningSet |= ItemQualityNodeMessages.ItemUnavailable;
            if (pair.Item?.Enabled is not true)
                warningSet |= ItemQualityNodeMessages.ItemDisabled;

            if (warningSet != 0) {
                state = NodeState.Warning;
                return;
            }

            state = allLinksConnected ? NodeState.Clean : NodeState.MissingLink;
        }
    }
}
