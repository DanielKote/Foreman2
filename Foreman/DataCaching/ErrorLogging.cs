using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Foreman.DataCaching {
    /// <summary>Append-only error log (errorlog.txt). Caught exceptions use <see cref="LogException"/>; UI shows generic text.</summary>
    public static class ErrorLogging {
        public static void ClearLog() {
            if (File.Exists(Path.Combine(Application.StartupPath, "errorlog.txt")))
                Utf8File.WriteAllText(Path.Combine(Application.StartupPath, "errorlog.txt"), "");
        }

        public static void LogLine(string message) {
            string line = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "]: " + message + "\n";
            try {
                Utf8File.AppendAllText(Path.Combine(Application.StartupPath, "errorlog.txt"), line);
            } catch (Exception writeFailure) {
                Trace.WriteLine("Failed to write errorlog.txt: " + writeFailure);
                Trace.WriteLine(line.TrimEnd());
            }
        }

        public static void LogException(Exception ex, string? context = null) {
            if (string.IsNullOrEmpty(context))
                LogLine(ex.ToString());
            else
                LogLine(context + ": " + ex);
        }
    }
}
