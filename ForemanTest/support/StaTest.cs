using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace ForemanTest.support {
    /// <summary>Runs WinForms UI code on an STA thread (required for Control drag-drop and OLE).</summary>
    internal static class StaTest {
        public static void Run(Action body) {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA) {
                body();
                return;
            }

            Exception? error = null;
            var thread = new Thread(() => {
                try {
                    body();
                } catch (Exception ex) {
                    error = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (error is not null)
                ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

}
