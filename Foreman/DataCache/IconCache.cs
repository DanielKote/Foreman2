using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    [Serializable]
    public struct IconColorPair {
        public Bitmap? Icon;
        public Color Color;
        public IconColorPair(Bitmap? icon, Color color) {
            this.Icon = icon;
            this.Color = color;
        }
    }

    public static class IconCache {
        public static Bitmap UnknownIcon {
            get {
                if (field is null)
                    field = GetIcon(Path.Combine("Graphics", "UnknownIcon.png"), 32);
                return field;
            }
        }
        public static Bitmap SpoilageIcon {
            get {
                if (field == null)
                    field = GetIcon(Path.Combine("Graphics", "SpoilAssembler.png"), 96);
                return field;

            }
        }
        public static Bitmap PlantingIcon {
            get {
                if (field == null)
                    field = GetIcon(Path.Combine("Graphics", "PlantAssembler.png"), 96);
                return field;

            }
        }

        public static Bitmap GetIcon(string path, int size) {
            Bitmap? bmp = null;
            try {
                using (Bitmap image = new Bitmap(path)) //If you don't do this, the file is locked for the lifetime of the bitmap
                {
                    bmp = new Bitmap(size, size, image.PixelFormat);
                    using (Graphics g = Graphics.FromImage(bmp))
                        g.DrawImage(image, new Rectangle(0, 0, (size * image.Width / image.Height), size));
                    return bmp;
                }
            } catch (Exception ex) {
                bmp?.Dispose();
                ErrorLogging.LogException(ex, string.Format("IconCache.GetIcon failed for '{0}' (size {1})", path, size));
                return new Bitmap(size, size);
            }
        }

        public static Bitmap CombineIcons(Bitmap aIcon, Bitmap bIcon, int size, bool diagonalSlice = true) {
            Bitmap result = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(result)) {
                using (GraphicsPath tlPath = new GraphicsPath()) {
                    tlPath.AddLine(0, 0, 0, size);
                    tlPath.AddLine(0, size, size, 0);
                    tlPath.AddLine(size, 0, 0, 0);
                    if (diagonalSlice)
                        g.Clip = new Region(tlPath);
                    if (aIcon != null)
                        g.DrawImage(aIcon, 0, 0, size, size);
                }

                using (GraphicsPath trPath = new GraphicsPath()) {
                    trPath.AddLine(size, size, 0, size);
                    trPath.AddLine(0, size, size, 0);
                    trPath.AddLine(size, 0, size, size);
                    if (diagonalSlice)
                        g.Clip = new Region(trPath);
                    if (bIcon != null)
                        g.DrawImage(bIcon, 0, 0, size, size);
                }
            }
            return result;
        }

        public static Task SaveIconCacheAsync(string path, Dictionary<string, IconColorPair> iconCache, CancellationToken cancellationToken = default) =>
            ForemanIconCacheFile.WriteAsync(path, iconCache, cancellationToken);

        public static async Task<Dictionary<string, IconColorPair>> LoadIconCache(string path, IProgress<KeyValuePair<int, string>> progress, int startingPercent, int endingPercent) {
            try {
                if (!File.Exists(path))
                    return [];
                if (!ForemanIconCacheFile.IsFoicFile(path))
                    throw new InvalidDataException("Unrecognized icon cache format.");

                int lastReportedPercent = startingPercent - 1;
                var iconProgress = new Progress<(int Decoded, int Total)>(state => {
                    int percent = startingPercent + (int)((endingPercent - startingPercent) * (double)state.Decoded / Math.Max(state.Total, 1));
                    if (percent <= lastReportedPercent)
                        return;
                    lastReportedPercent = percent;
                    progress.Report(new(percent, "Loading Icons..."));
                });
                return await ForemanIconCacheFile.ReadAsync(path, iconProgress);
            } catch (Exception ex) {
                ErrorLogging.LogException(ex, $"Failed to load icon cache from {path}");
                UserMessages.Show(
                    $"The icon cache \"{Path.GetFileName(path)}\" could not be read.\n\n" +
                    "Re-import the preset to rebuild the cache.",
                    "Icon cache unreadable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return [];
            }
        }
    }
}