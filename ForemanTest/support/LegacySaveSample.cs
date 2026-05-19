namespace ForemanTest.support {
    internal static class LegacySaveSample {
        public const string FileName = "Seablock chart.fjson";

        public static string ResolvePath() => TestAssets.ResolvePath(FileName);
    }
}
