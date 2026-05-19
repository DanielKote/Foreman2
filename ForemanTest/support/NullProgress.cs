using System;
using System.Collections.Generic;

namespace ForemanTest.support {
    internal sealed class NullProgress : IProgress<KeyValuePair<int, string>> {
        public static NullProgress Instance { get; } = new NullProgress();
        public void Report(KeyValuePair<int, string> value) { }
    }
}
