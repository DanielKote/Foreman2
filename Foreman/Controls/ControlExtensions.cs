using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman.Controls {
    public static class ControlExtensions {
        static public void UIThread(this Control control, Action code) {
            if (control.InvokeRequired) {
                control.BeginInvoke(code);
                return;
            }
            code.Invoke();
        }

        static public void UIThreadInvoke(this Control control, Action code) {
            if (control.InvokeRequired) {
                control.Invoke(code);
                return;
            }
            code.Invoke();
        }

        /// <summary>Runs <paramref name="action"/> on the control's UI thread (required after <c>ConfigureAwait(false)</c>).</summary>
        static public Task InvokeOnUiThreadAsync(this Control control, Action action) {
            if (control.InvokeRequired)
                return control.InvokeAsync(action);
            action();
            return Task.CompletedTask;
        }

    }
}
