using System;
using System.Collections.Generic;

namespace Foreman {
    // http://procbits.com/2010/08/25/benchmarking-c-apps-algorithms
    public static class BenchmarkTimer {
        private static Stack<BenchmarkData> _startStack = new();

        public static void Start() {
            _startStack.Push(new BenchmarkData());
        }

        public static void Start(string label) {
            var bd = new BenchmarkData() { Label = label };
            _startStack.Push(bd);
        }

        public static TimeSpan Stop() {
            var stop = DateTime.Now;
            var startBd = _startStack.Pop();
            return stop - startBd.DateTime;
        }

        public static void StopAndOutput() {
            var stop = DateTime.Now;
            var startBd = _startStack.Pop();

            var delta = stop - startBd.DateTime;

            const string lbl = "{0}: {1} ms";
            Console.WriteLine(lbl, startBd.Label, delta.TotalMilliseconds);
        }

        private class BenchmarkData {
            public DateTime DateTime { get; set; } = DateTime.Now;
            public string Label { get; set; } = "";
        }
    }
}