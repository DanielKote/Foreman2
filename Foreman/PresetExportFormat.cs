using Foreman.DataCaching;
using System.Windows.Forms;

namespace Foreman {
    /// <summary>
    /// Version of the foremanexport JSON schema. Bump <see cref="CurrentVersion"/> when export layout changes;
    /// keep in sync with <c>FOREMAN_EXPORT_VERSION</c> in foremanexport instrument-control.lua.
    /// </summary>
    public static class PresetExportFormat {
        public const string VersionPropertyName = "foreman_export_version";

        /// <summary>Latest export schema version understood by this Foreman build.</summary>
        public const int CurrentVersion = 1;

        public static int ReadVersion(DataCache? dc) => dc?.Version ?? 0;

        public static bool IsOutdated(DataCache? dc) => dc is null || dc.Version < CurrentVersion;

        public static void ShowOutdatedWarningIfNeeded(DataCache? dc) {
            if (!IsOutdated(dc))
                return;

            UserMessages.Show(
                "Your preset was exported with an older version of Foreman. " +
                "We recommend you regenerate it in the Settings menu to ensure you benefit from our improvements and fixes.",
                "Preset update recommended",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
