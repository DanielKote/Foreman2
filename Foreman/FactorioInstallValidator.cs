using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;

namespace Foreman {
    public static class FactorioInstallValidator {
        public static bool TryValidateExecutable(string factorioExePath, [NotNullWhen(false)] out string? userMessage) {
            userMessage = null;
            if (!File.Exists(factorioExePath)) {
                userMessage = "Could not find factorio.exe. Please select a valid Factorio install location.";
                return false;
            }

            FileVersionInfo factorioVersionInfo = FileVersionInfo.GetVersionInfo(factorioExePath);
            if (factorioVersionInfo.ProductMajorPart < 2) {
                userMessage = "Factorio Version below 2.0 can not be used with this version of Foreman. Please use Factorio 2.0 or newer. Alternatively download dev.13 or under of foreman 2.0 for pre factorio 2.0.";
                ErrorLogging.LogLine(string.Format("Factorio version 0.x or 1.x instead of 2.x - use Foreman dev.13 or below for these factorio installs. ({0})", factorioVersionInfo.ProductVersion));
                return false;
            }

            if (factorioVersionInfo.ProductMajorPart > 2) {
                userMessage = "Factorio Version 3.x+ can not be used with this version of Foreman. Sit tight and wait for update...\nYou can also try to msg me on discord (u\\DanielKotes) if for some reason I am not already aware of this.";
                ErrorLogging.LogLine(string.Format("Factorio version 3.x+ isnt supported. ({0})", factorioVersionInfo.ProductVersion));
                return false;
            }

            if (factorioVersionInfo.ProductMinorPart < 0 || (factorioVersionInfo.ProductMinorPart == 0 && factorioVersionInfo.ProductBuildPart < 7)) {
                userMessage = "Factorio version (" + factorioVersionInfo.ProductVersion + ") can not be used with Foreman. Please use Factorio 2.0.7 or newer.";
                ErrorLogging.LogLine(string.Format("Factorio version was too old. {0} instead of 2.0.7+", factorioVersionInfo.ProductVersion));
                return false;
            }

            return true;
        }
    }
}