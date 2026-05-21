using System;
using System.Windows.Forms;

namespace Foreman.Controls {
    /// <summary>96 DPI design-time metrics for the item/recipe chooser (scaled at runtime).</summary>
    internal static class ChooserLayout {
        public const float BaselineDpi = 96f;

        public const int DesignCellPixels = 40;
        public const int DesignGroupIconPixels = 64;
        /// <summary>96 DPI baseline; prefer <see cref="GetVerticalScrollbarWidth"/> at runtime.</summary>
        public const int DesignScrollbarWidth = 17;

        public static int GetVerticalScrollbarWidth() => SystemInformation.VerticalScrollBarWidth;
        public const int DesignChooserWidth = ChooserIconGrid.ColumnCount * DesignCellPixels + DesignScrollbarWidth + 6;
        public const int DesignChromeExtraHeight = 96;
        public const int DesignFilterTextWidth = 127;
        public const int DesignQualityComboWidth = 146;
        public const int DesignItemIconPixels = 40;
        public const int DesignMinCellPixels = 18;
        public const int DesignMinGroupIconPixels = 24;
        public const int DesignFooterButtonHeightPixels = 38;
        public const int DesignMinFooterButtonHeightPixels = 22;
        public const float DesignFooterButtonFontSizePoints = 8.25f;
        public const float DesignMinFooterButtonFontSizePoints = 6f;
        public const int DesignMinVisibleRows = 4;

        public static int DesignGridOuterWidth =>
            ChooserIconGrid.ColumnCount * DesignCellPixels + DesignScrollbarWidth;

        public static int DesignGridOuterHeight =>
            ChooserIconGrid.VisibleRowCount * DesignCellPixels;

        public static float GetScaleFactor(Control control) {
            int dpi = control.DeviceDpi;
            if (dpi <= 0 && control.Parent != null)
                dpi = control.Parent.DeviceDpi;
            if (dpi <= 0)
                dpi = (int)BaselineDpi;
            return dpi / BaselineDpi;
        }

        public static int Scale(Control control, int logicalPixels) =>
            Math.Max(1, (int)Math.Round(logicalPixels * GetScaleFactor(control)));

        public static int GroupIconSizeForCell(int cellSize, int designGroupSize, int minGroupSize) {
            int fromCell = (int)Math.Round(cellSize * (DesignGroupIconPixels / (float)DesignCellPixels));
            return Math.Min(designGroupSize, Math.Max(minGroupSize, fromCell));
        }

        public static int FooterButtonHeightForCell(int cellSize, int designFooterHeight, int minFooterHeight) {
            int fromCell = (int)Math.Round(cellSize * (DesignFooterButtonHeightPixels / (float)DesignCellPixels));
            return Math.Max(minFooterHeight, Math.Min(designFooterHeight, fromCell));
        }

        public static float FooterButtonFontSizeForCell(int cellSize, int designCellPixels, float designFontSize, float minFontSize) {
            float fromCell = cellSize * (designFontSize / designCellPixels);
            return Math.Max(minFontSize, Math.Min(designFontSize, fromCell));
        }
    }
}
