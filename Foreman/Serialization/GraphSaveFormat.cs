namespace Foreman.Serialization {
    /// <summary>JSON property names and object-type markers for Foreman graph save files.</summary>
    public static class GraphSaveFormat {
        /// <summary>Current .fjson schema version written by this build. Bump when the on-disk format changes.</summary>
        public const int SaveFormatVersion = 8;

        /// <summary>Oldest viewer/graph save version this build can still read.</summary>
        public const int MinReadableSaveVersion = 7;

        public const string ViewerObject = "ProductionGraphViewer";
        public const string GraphObject = "ProductionGraph";
        public const string NodeCopyObject = "NodeCopyOptions";
    }
}
