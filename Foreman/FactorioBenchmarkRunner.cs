using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Foreman {
    internal readonly struct FactorioRunResult(string output, int exitCode, bool crashed) {
        public string Output { get; } = output;
        public int ExitCode { get; } = exitCode;
        public bool Crashed { get; } = crashed;
    }

    internal static class FactorioBenchmarkRunner {
        public const string AnotherInstanceMessage = "Is another instance already running?";

        public static bool IsAnotherInstanceRunning(string output) =>
            output.Contains(AnotherInstanceMessage, StringComparison.Ordinal);

        /// <summary>True when Factorio's stdout/stderr indicates a native crash (SIGSEGV, crash handler, etc.).</summary>
        public static bool IsCrashOutput(string output) {
            return !string.IsNullOrEmpty(output) && (output.Contains("Received SIGSEGV", StringComparison.Ordinal)
                || output.Contains("Factorio crashed", StringComparison.Ordinal)
                || output.Contains("Generating symbolized stacktrace", StringComparison.Ordinal)
                || output.Contains("Error CrashHandler.cpp", StringComparison.Ordinal)
                || output.Contains("CrashDump success", StringComparison.Ordinal));
        }

        public static FactorioRunResult Run(string exePath, string arguments, CancellationToken token, Action? onCancelled = null) {
            using var process = new Process();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = exePath;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.StandardOutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            process.Start();

            string resultString = "";
            while (!process.HasExited) {
                resultString += process.StandardOutput.ReadToEnd();
                if (token.IsCancellationRequested) {
                    process.Close();
                    onCancelled?.Invoke();
                    return new FactorioRunResult("", -1, crashed: false);
                }
                Thread.Sleep(100);
            }
            resultString += process.StandardOutput.ReadToEnd();
            int exitCode = process.ExitCode;
            bool crashed = IsCrashOutput(resultString) || (exitCode != 0 && resultString.Contains("Unexpected error occurred", StringComparison.Ordinal));
            return new FactorioRunResult(resultString, exitCode, crashed);
        }
    }
}
