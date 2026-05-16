using System;
using System.IO;
using System.Linq;

namespace ForemanTest.support {
    internal static class FlowchartSample {
        public const string FileName = "Flowchart.fjson";
        public const string PresetName = "Factorio 2.0 Space Age";

        public static string ResolvePath() {
            string[] candidates =
            [
                Path.Combine(AppContext.BaseDirectory, FileName),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", FileName))
            ];
            return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
        }
    }
}