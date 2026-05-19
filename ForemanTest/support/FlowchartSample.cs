namespace ForemanTest.support {
    internal static class FlowchartSample {
        public const string FileName = "Flowchart.fjson";
        public const string PresetName = "Factorio 2.0 Space Age";

        public static string ResolvePath() => TestAssets.ResolvePath(FileName);
    }
}
