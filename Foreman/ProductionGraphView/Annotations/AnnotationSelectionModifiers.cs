using System.Windows.Forms;

namespace Foreman {
    internal static class AnnotationSelectionModifiers {
        public static bool IsRemoveFromSelection =>
            (Control.ModifierKeys & Keys.Alt) != 0;

        public static bool IsAddToSelection =>
            (Control.ModifierKeys & Keys.Control) != 0;

        public static bool IsReplaceSelection =>
            !IsRemoveFromSelection && !IsAddToSelection;
    }
}