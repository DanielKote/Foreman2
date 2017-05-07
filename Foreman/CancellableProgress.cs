using System;
using System.Threading;

namespace Foreman
{
    // Kind of mad I need this class. Don't want to thread two variables through code that needs it.
    // Hopefully there's a better built-in way to do this?
    internal class CancellableProgress
    {
        private IProgress<int> progress;
        private CancellationToken cancelToken;

        public bool IsCancellationRequested
        {
            get
            {
                return cancelToken.IsCancellationRequested;
            }
        }

        public CancellableProgress(IProgress<int> progress, CancellationToken cancelToken)
        {
            this.progress = progress;
            this.cancelToken = cancelToken;
        }

        public void Report(int amount)
        {
            progress.Report(amount);
        }
    }
}