using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Foreman {
    internal static class FactorioBenchmarkRunner {
        public const string AnotherInstanceMessage = "Is another instance already running?";

        public static bool IsAnotherInstanceRunning(string output) =>
            output.Contains(AnotherInstanceMessage, StringComparison.Ordinal);

        public static string Run(string exePath, string arguments, CancellationToken token, Action? onCancelled = null) {
            using Process process = new Process();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = exePath;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.Start();

            string resultString = "";
            while (!process.HasExited) {
                resultString += process.StandardOutput.ReadToEnd();
                if (token.IsCancellationRequested) {
                    process.Close();
                    onCancelled?.Invoke();
                    return "";
                }
                Thread.Sleep(100);
            }
            return resultString;
        }
    }
}