using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    [Serializable]
    public struct IconColorPair {
        public Bitmap? Icon;
        public Color Color;
        public byte[] IconBytes {
            get {
                using var stream = new MemoryStream();
                Icon?.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
            set {
                using var stream = new MemoryStream(value);
                Icon = new Bitmap(stream);
            }
        }
        public int ColorBytes {
            get => Color.ToArgb();
            set {
                Color = Color.FromArgb(value);
            }
        }
        public IconColorPair(Bitmap? icon, Color color) {
            this.Icon = icon;
            this.Color = color;
        }
    }
    [Serializable]
    public class IconBitmapCollection {
        [JsonInclude]
        public Dictionary<string, IconColorPair> Icons;
        public IconBitmapCollection() { Icons = new Dictionary<string, IconColorPair>(); }
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
            } catch (Exception) {
                bmp?.Dispose();
                return new Bitmap(size, size);
            }
        }

        public static Bitmap ConbineIcons(Bitmap aIcon, Bitmap bIcon, int size, bool diagonalSlice = true) {
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


        public static void SaveIconCache(string path, Dictionary<string, IconColorPair> iconCache) {
            IconBitmapCollection iCollection = new IconBitmapCollection();

            foreach (KeyValuePair<string, IconColorPair> iconKVP in iconCache)
                iCollection.Icons.Add(iconKVP.Key, iconKVP.Value);

            if (File.Exists(path))
                File.Delete(path);
            using (Stream stream = File.Open(path, FileMode.Create, FileAccess.Write)) {
                JsonSerializer.Serialize(stream, iCollection);
            }
        }

        private static bool StreamLooksLikeJson(Stream stream) {
            long start = stream.Position;
            try {
                int b;
                while ((b = stream.ReadByte()) >= 0) {
                    if (b is ' ' or '\t' or '\r' or '\n')
                        continue;
                    return b == '{';
                }
                return false;
            } finally {
                stream.Position = start;
            }
        }

        public static async Task<Dictionary<string, IconColorPair>> LoadIconCache(string path, IProgress<KeyValuePair<int, string>> progress, int startingPercent, int endingPercent) {
            Dictionary<string, IconColorPair> iconCache = [];
            await Task.Run(() => {
                try {
                    using Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    if (!StreamLooksLikeJson(stream)) {
                        iconCache.Clear();
                        MessageBox.Show(
                            $"The icon cache \"{Path.GetFileName(path)}\" was created by an older Foreman build and cannot be read after upgrading to .NET.\n\n" +
                            "Delete that file in the Presets folder (or re-import the preset) to rebuild the cache. " +
                            "Icons will be loaded from game files until then.",
                            "Icon cache format changed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }

                    IconBitmapCollection? iCollection = JsonSerializer.Deserialize<IconBitmapCollection>(stream);
                    if (iCollection?.Icons is null)
                        throw new InvalidDataException("Icon cache JSON did not contain any icons.");

                    int totalCount = iCollection.Icons.Count;
                    int counter = 0;
                    foreach (KeyValuePair<string, IconColorPair> iconKVP in iCollection.Icons) {
                        progress.Report(new(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, "Loading Icons..."));
                        iconCache.Add(iconKVP.Key, iconKVP.Value);
                    }
                } catch (Exception ex) {
                    iconCache.Clear();
                    ErrorLogging.LogLine($"Failed to load icon cache from {path}: {ex}");
                    MessageBox.Show(
                        $"The icon cache \"{Path.GetFileName(path)}\" could not be read.\n\n" +
                        "Delete that file in the Presets folder (or re-import the preset) to rebuild the cache. " +
                        "Icons will be loaded from game files until then.",
                        "Icon cache unreadable",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            });
            return iconCache;
        }
    }
}