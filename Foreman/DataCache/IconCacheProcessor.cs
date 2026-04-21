using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace Foreman {
    public struct IconInfo(string iconPath, int iconSize) {
        public readonly string IconPath = iconPath;
        public readonly int IconSize = iconSize;
        public double IconScale = 1;
        public Point IconOffset = new(0, 0);
        public Color IconTint = IconCacheProcessor.NoTint;

        public void SetIconTint(double a, double r, double g, double b) {
            a = a <= 1 ? a * 255 : a;
            r = r <= 1 ? r * 255 : r;
            g = g <= 1 ? g * 255 : g;
            b = b <= 1 ? b * 255 : b;
            IconTint = Color.FromArgb((int) a, (int) r, (int) g, (int) b);
        }
    }

    public class IconCacheProcessor : IDisposable {
        internal static readonly Color NoTint = Color.White;

        public int TotalPathCount { get; private set; }
        public int FailedPathCount { get; private set; }

        private Dictionary<string, IconColorPair> _myIconCache = new();

        private Dictionary<string, string> _folderLinks = new();
        private Dictionary<string, ZipArchiveEntry> _archiveFileLinks = new();
        private List<ZipArchive> _openedArchives = [];
        // just so we don't have to load the same file multiple times
        private Dictionary<string, Bitmap> _bitmapCache = new();

        public bool PrepareModPaths(Dictionary<string, string> modSet, string modsPath, string dataPath, CancellationToken token) {
            _folderLinks.Clear();
            _archiveFileLinks.Clear();
            _bitmapCache.Clear();

            // factorio checks for folder <name>_<version>, then folder <name> then zip <name>_<version>
            // if zip file, then the actual files can either be in the root of zip, or in <name> folder, or in <name>_<version> folder
            // NOTE: versions are of type v1.v2.v3 where each number can have any amount of leading zeros

            foreach (var mod in modSet) {
                if (token.IsCancellationRequested)
                    return false;

                var versionMatch = string.Join(".", mod.Value.Split('.').Select(s => "0*" + int.Parse(s)));

                var folders = Directory.GetDirectories(modsPath);
                var files = Directory.GetFiles(modsPath);

                var foundFolder = folders.FirstOrDefault(f => Regex.IsMatch(
                    Path.GetFileName(f).ToLower(),
                    $"{mod.Key}_{versionMatch}")) ?? folders.FirstOrDefault(f => Path.GetFileName(f).ToLower() == mod.Key
                );

                if (foundFolder != null)
                    _folderLinks.Add("__" + mod.Key.ToLower() + "__", foundFolder);
                else {
                    var foundFile = files.FirstOrDefault(f =>
                        Regex.IsMatch(Path.GetFileName(f).ToLower(), $"{mod.Key}_{versionMatch}.zip"));
                    if (foundFile == null) {
                        if (mod.Key.ToLower() != "core" && mod.Key.ToLower() != "base" && mod.Key.ToLower() != "elevated-rails" &&
                            mod.Key.ToLower() != "quality" && mod.Key.ToLower() != "space-age")
                            return false;
                        continue;
                    }

                    // for zip files, since we have to iterate through them for each file
                    // we might as well make a full link of every possible filepath to given entry

                    var zip = ZipFile.Open(foundFile, ZipArchiveMode.Read);
                    _openedArchives.Add(zip);
                    foreach (var zipEntry in zip.Entries) {
                        if (zipEntry.Name == "") // folder
                            continue;

                        var brokenPath = new LinkedList<string>();
                        var filePath = zipEntry.FullName;
                        while (filePath != "") {
                            brokenPath.AddFirst(Path.GetFileName(filePath));
                            filePath = Path.GetDirectoryName(filePath);
                        }

                        brokenPath.First.Value = "__" + mod.Key.ToLower() + "__";
                        _archiveFileLinks.Add(Path.Combine(brokenPath.ToArray()).ToLower(), zipEntry);
                    }
                }
            }

            _folderLinks.Add("__core__", Path.Combine(dataPath, "core"));
            _folderLinks.Add("__base__", Path.Combine(dataPath, "base"));
            _folderLinks.Add("__elevated-rails__", Path.Combine(dataPath, "elevated-rails"));
            _folderLinks.Add("__quality__", Path.Combine(dataPath, "quality"));
            _folderLinks.Add("__space-age__", Path.Combine(dataPath, "space-age"));

            return true;
        }

        public bool CreateIconCache(JObject iconJObject, string cachePath, IProgress<KeyValuePair<int, string>> progress, CancellationToken token,
            int startingPercent, int endingPercent) {
            TotalPathCount = 0;
            FailedPathCount = 0;

            _myIconCache.Clear();
            _bitmapCache.Clear();

            var totalCount =
                iconJObject["technologies"].Count() +
                iconJObject["recipes"].Count() +
                iconJObject["items"].Count() +
                iconJObject["fluids"].Count() +
                iconJObject["entities"].Count() +
                iconJObject["groups"].Count() +
                iconJObject["qualities"].Count();

            progress.Report(new KeyValuePair<int, string>(startingPercent, "Creating icons."));
            var counter = 0;
            foreach (var iconJToken in iconJObject["technologies"].ToList()) {
                if (token.IsCancellationRequested) return false;
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 256);
            }

            foreach (var iconJToken in iconJObject["recipes"].ToList()) {
                if (token.IsCancellationRequested) return false;
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 32);
            }

            foreach (var iconJToken in iconJObject["items"].ToList()) {
                if (token.IsCancellationRequested) return false;
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 32);
            }

            foreach (var iconJToken in iconJObject["fluids"].ToList()) {
                if (token.IsCancellationRequested) return false;
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 32);
            }

            foreach (var iconJToken in iconJObject["entities"].ToList()) {
                if (token.IsCancellationRequested) return false;
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 64);
            }

            foreach (var iconJToken in iconJObject["groups"].ToList()) {
                if (token.IsCancellationRequested) return false;
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 64);
            }

            foreach (var iconJToken in iconJObject["qualities"].ToList()) {
                if (token.IsCancellationRequested) return false;
                progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
                ProcessIcon(iconJToken, 32);
            }

            IconCache.SaveIconCache(cachePath, _myIconCache);

            return FailedPathCount == 0;
        }

        private void ProcessIcon(JToken objJToken, int defaultIconSize) {
            if (objJToken["icon_data"].Type == JTokenType.Null)
                return;

            var iconName = (string) objJToken["icon_name"];
            var iconData = new IconColorPair(null, Color.Black);

            var iconDataJToken = objJToken["icon_data"];

            var mainIconPath = (string) iconDataJToken["icon"];
            var baseIconSize = iconDataJToken["icon_size"].Type == JTokenType.Null ? 32 : (int) iconDataJToken["icon_size"];

            var iconInfo = new IconInfo(mainIconPath, baseIconSize);
            iconInfo.IconScale = defaultIconSize / iconInfo.IconSize;

            var iconInfos = new List<IconInfo>();
            var iconJTokens = iconDataJToken["icons"].ToList();
            foreach (var iconJToken in iconJTokens) {
                var picon = new IconInfo((string) iconJToken["icon"],
                    iconJToken["icon_size"].Type == JTokenType.Null ? baseIconSize : (int) iconJToken["icon_size"]);
                picon.IconScale = iconJToken["scale"].Type == JTokenType.Null ? defaultIconSize / picon.IconSize : (double) iconJToken["scale"];

                picon.IconOffset = new Point((int) iconJToken["shift"][0], (int) iconJToken["shift"][1]);
                picon.SetIconTint((double) iconJToken["tint"][3], (double) iconJToken["tint"][0], (double) iconJToken["tint"][1],
                    (double) iconJToken["tint"][2]);
                iconInfos.Add(picon);
            }

            if (!_myIconCache.ContainsKey(iconName))
                _myIconCache.Add(iconName, GetIconAndColor(iconInfo, iconInfos, defaultIconSize));
        }


        public IconColorPair GetIconAndColor(IconInfo iconInfo, List<IconInfo> iconInfos, int defaultCanvasSize) {
            iconInfos ??= [];

            // just some upscaling for icons (item icons are set at 32x32, but they look better at 64x64)
            double iconCanvasScale = defaultCanvasSize == 32 ? 2 : 1;
            var iconCanvasSize = (int) (defaultCanvasSize * iconCanvasScale);

            // if there are no icons, use the single icon
            if (iconInfos.Count == 0)
                iconInfos.Add(iconInfo);

            //quick check to ensure it isn't a null icon
            var empty = true;
            foreach (var ii in iconInfos) {
                if (!string.IsNullOrEmpty(ii.IconPath))
                    empty = false;
            }

            if (empty)
                return new IconColorPair(null, Color.Black);

            // prepare the canvas - we will add each successive icon/layer on top of it

            var canvas = new Bitmap(iconCanvasSize, iconCanvasSize, PixelFormat.Format32bppPArgb);
            var canvasData = canvas.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height), ImageLockMode.ReadWrite, canvas.PixelFormat);
            var cBpp = Image.GetPixelFormatSize(canvas.PixelFormat) / 8;
            var bCount = canvasData.Stride * canvas.Height;
            var canvasPixels = new byte[bCount];
            var ptrCanvasFPixel = canvasData.Scan0;
            Marshal.Copy(ptrCanvasFPixel, canvasPixels, 0, canvasPixels.Length);
            var heightInPixels = canvasData.Height;
            var widthInBytes = canvasData.Width * cBpp;

            foreach (var ii in iconInfos) {
                // load the image and prep it for processing

                var iconSize = ii.IconSize > 0 ? ii.IconSize : iconInfo.IconSize;
                var iconDrawSize = (int) (iconSize * (ii.IconScale > 0 ? ii.IconScale : (double) defaultCanvasSize / iconSize));
                iconDrawSize = (int) (iconDrawSize * iconCanvasScale);

                var iconImage = LoadImageFromMod(ii.IconPath, iconDrawSize);
                if (iconImage == null)
                    continue;

                // draw the icon onto a layer (that we will apply tint to and blend with canvas)

                var layerSlice = new Bitmap(canvas.Width, canvas.Height, canvas.PixelFormat);
                using (var g = Graphics.FromImage(layerSlice))
                    g.DrawImageUnscaled(iconImage, iconCanvasSize / 2 - iconDrawSize / 2 + ii.IconOffset.X,
                        iconCanvasSize / 2 - iconDrawSize / 2 + ii.IconOffset.Y);

                // grab the layer data

                var layerData = layerSlice.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height), ImageLockMode.ReadOnly, canvas.PixelFormat);
                var layerPixels = new byte[bCount];
                var ptrLayerFPixel = layerData.Scan0;
                Marshal.Copy(ptrLayerFPixel, layerPixels, 0, layerPixels.Length);

                // blend -> for each value in 0->1 (so when multiplying, you have to divide by 255 if in 0->255)
                // newCanvas(A/R/G/B) = Layer(A/R/G/B) * tint(A/R/G/B)   +   oldCanvas(A/R/G/B) * (1 - tint(A) * Layer(A))
                // https://www.factorio.com/blog/post/fff-172

                for (var y = 0; y < heightInPixels; y++) {
                    var currentLine = y * canvasData.Stride;
                    for (var x = 0; x < widthInBytes; x = x + cBpp) {
                        var canvasMulti = 255 - ii.IconTint.A * layerPixels[currentLine + x + 3] / 255;
                        canvasPixels[currentLine + x + 0] = (byte) Math.Min(255,
                            layerPixels[currentLine + x + 0] * ii.IconTint.B / 255 +
                            canvasPixels[currentLine + x + 0] * canvasMulti / 255);
                        canvasPixels[currentLine + x + 1] = (byte) Math.Min(255,
                            layerPixels[currentLine + x + 1] * ii.IconTint.G / 255 +
                            canvasPixels[currentLine + x + 1] * canvasMulti / 255);
                        canvasPixels[currentLine + x + 2] = (byte) Math.Min(255,
                            layerPixels[currentLine + x + 2] * ii.IconTint.R / 255 +
                            canvasPixels[currentLine + x + 2] * canvasMulti / 255);
                        canvasPixels[currentLine + x + 3] = (byte) Math.Min(255,
                            layerPixels[currentLine + x + 3] * ii.IconTint.A / 255 +
                            canvasPixels[currentLine + x + 3] * canvasMulti / 255);
                    }
                }

                layerSlice.UnlockBits(layerData);
            }

            // we are done adding all the layers, so copy the canvas data

            Marshal.Copy(canvasPixels, 0, ptrCanvasFPixel, canvasPixels.Length);
            canvas.UnlockBits(canvasData);

            // at this point we need to convert the canvas into a non-alpha multiplied format due to winforms having issues with it

            var result = new Bitmap(canvas.Width, canvas.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
                g.DrawImageUnscaled(canvas, 0, 0);

            // finally, calculate the average color (yes, it comes out a bit different due to inclusion of transparency)

            var averageColor = GetAverageColor(result);

            // if the image is too bright, add a border to it. Honestly, this is never done anymore -
            // it was useful before layer blending was fixed and some icons came out... white.

            if (averageColor.GetBrightness() > 0.9)
                result = AddBorder(result);
            if (averageColor.GetBrightness() > 0.7)
                averageColor = Color.FromArgb(255, (int) (averageColor.R * 0.7), (int) (averageColor.G * 0.7), (int) (averageColor.B * 0.7));

            return new IconColorPair(result, averageColor);
        }

        // NOTE: must make sure we use pre-multiplied alpha
        private Bitmap LoadImageFromMod(string fileName, int resultSize = 32) {
            if (string.IsNullOrEmpty(fileName))
                return null;
            fileName = fileName.ToLower().Replace("/", "\\");

            // found this error in krastorio - apparently factorio ignores multiple slashes in file name

            while (fileName.IndexOf("\\\\", StringComparison.Ordinal) != -1)
                fileName = fileName.Replace("\\\\", "\\");

            // if the image isn't currently in the cache, process it and add it to cache

            if (!_bitmapCache.ContainsKey(fileName)) {
                TotalPathCount++;
                var origin = fileName.Substring(0, fileName.IndexOf("__", 2, StringComparison.Ordinal) + 2);
                var file = fileName.Substring(fileName.IndexOf("__", 2, StringComparison.Ordinal) + 3);

                if (_folderLinks.TryGetValue(origin, out var link)) {
                    file = Path.Combine(link, file);
                    try {
                        _bitmapCache.Add(fileName, new Bitmap(file));
                    } catch {
                        _bitmapCache.Add(fileName, null);
                        FailedPathCount++;
                        ErrorLogging.LogLine("IconCacheProcessor: given fileName not found in mod folders: " + fileName);
                    }
                } else if (_archiveFileLinks.ContainsKey(fileName)) {
                    try {
                        _bitmapCache.Add(fileName, new Bitmap(_archiveFileLinks[fileName].Open()));
                    } catch {
                        _bitmapCache.Add(fileName, null);
                        FailedPathCount++;
                        ErrorLogging.LogLine("IconCacheProcessor: given fileName not found in mod folders: " + fileName);
                    }
                } else {
                    FailedPathCount++;
                    _bitmapCache.Add(fileName, null);
                    ErrorLogging.LogLine("IconCacheProcessor: given fileName not found in mod folders: " + fileName);
                }
            }

            if (_bitmapCache[fileName] == null)
                return null;

            // get the requested image from the cache and draw it to correct size.

            var image = _bitmapCache[fileName];
            var bmp = new Bitmap(resultSize, resultSize, PixelFormat.Format32bppPArgb);
            using var g = Graphics.FromImage(bmp);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, new Rectangle(0, 0, resultSize * image.Width / image.Height, resultSize));

            return bmp;
        }

        private Color GetAverageColor(Bitmap icon) {
            if (icon == null)
                return Color.Black;

            var iconData = icon.LockBits(new Rectangle(0, 0, icon.Width, icon.Height), ImageLockMode.ReadOnly, icon.PixelFormat);
            var bytesPerPixel = Image.GetPixelFormatSize(icon.PixelFormat) / 8;
            var byteCount = iconData.Stride * icon.Height;
            var iconPixels = new byte[byteCount];
            var ptrFirstPixel = iconData.Scan0;
            Marshal.Copy(ptrFirstPixel, iconPixels, 0, iconPixels.Length);
            var heightInPixels = iconData.Height;
            var widthInBytes = iconData.Width * bytesPerPixel;

            int[] totalPixel = [0, 0, 0, 0];
            // just to avoid div by 0 in case of completely empty bitmap
            var totalCounter = 1;
            for (var y = 0; y < heightInPixels; y++) {
                var currentLine = y * iconData.Stride;
                for (var x = 0; x < widthInBytes; x = x + bytesPerPixel) {
                    // ignore transparent pixels
                    if (iconPixels[currentLine + x + 3] <= 10)
                        continue;

                    totalPixel[3] += iconPixels[currentLine + x]; // B
                    totalPixel[2] += iconPixels[currentLine + x + 1]; // G
                    totalPixel[1] += iconPixels[currentLine + x + 2]; // R
                    totalCounter++;
                }
            }

            for (var i = 1; i < 4; i++) {
                totalPixel[i] /= totalCounter;
                totalPixel[i] = Math.Min(totalPixel[i], 255);
            }

            icon.UnlockBits(iconData);

            return Color.FromArgb(255, totalPixel[1], totalPixel[2], totalPixel[3]);
        }

        // border is drawn on a new layer as 
        private const int iconBorder = 1;

        private Bitmap AddBorder(Bitmap icon) {
            var canvas = new Bitmap(icon.Width, icon.Height, icon.PixelFormat);
            var iconData = icon.LockBits(new Rectangle(0, 0, icon.Width, icon.Height), ImageLockMode.ReadOnly, icon.PixelFormat);
            var canvasData = canvas.LockBits(new Rectangle(0, 0, icon.Width, icon.Height), ImageLockMode.WriteOnly, icon.PixelFormat);

            // same for both
            var bytesPerPixel = Image.GetPixelFormatSize(icon.PixelFormat) / 8;
            var byteCount = iconData.Stride * icon.Height;

            var iconPixels = new byte[byteCount];
            var canvasPixels = new byte[byteCount];

            var ptrFirstPixel = iconData.Scan0;
            Marshal.Copy(ptrFirstPixel, iconPixels, 0, iconPixels.Length);
            var heightInPixels = iconData.Height;
            var widthInBytes = iconData.Width * bytesPerPixel;

            for (var y = iconBorder; y < heightInPixels - iconBorder; y++) {
                var currentLine = y * iconData.Stride;
                for (var x = iconBorder * bytesPerPixel; x < widthInBytes - iconBorder * bytesPerPixel; x += bytesPerPixel) {
                    // check if A >= 10
                    if (iconPixels[currentLine + x + 3] <= 11)
                        continue;

                    for (var iy = -iconBorder; iy <= iconBorder; iy++) {
                        for (var ix = -iconBorder * bytesPerPixel; ix <= iconBorder * bytesPerPixel; ix += bytesPerPixel) {
                            var currentCanvasIndex = currentLine + iy * iconData.Stride + x + ix;
                            canvasPixels[currentCanvasIndex] = 64;
                            canvasPixels[currentCanvasIndex + 1] = 64;
                            canvasPixels[currentCanvasIndex + 2] = 64;
                            canvasPixels[currentCanvasIndex + 3] = 64;
                        }
                    }
                }
            }

            ptrFirstPixel = canvasData.Scan0;
            Marshal.Copy(canvasPixels, 0, ptrFirstPixel, canvasPixels.Length);
            icon.UnlockBits(iconData);
            canvas.UnlockBits(canvasData);

            //draw the processed icon (singular) onto the main canvas

            using var g = Graphics.FromImage(canvas);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImageUnscaled(icon, 0, 0);

            return canvas;
        }

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing) {
            if (_disposedValue)
                return;

            if (disposing) {
                _archiveFileLinks.Clear();
                _archiveFileLinks = null;

                foreach (var bitmap in _bitmapCache.Values)
                    bitmap?.Dispose();
                _bitmapCache.Clear();
                _bitmapCache = null;

                foreach (var zip in _openedArchives)
                    zip.Dispose();
                _openedArchives.Clear();
                _openedArchives = null;
            }

            _disposedValue = true;
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private static Dictionary<KeyValuePair<Bitmap, Bitmap>, Bitmap> _combinedBitmapDictionary = new();
        private const double qualitySizeMultiplier = 0.5;

        public static Bitmap CombinedQualityIcon(Bitmap baseIcon, Bitmap qualityIcon) {
            if (baseIcon == null)
                return null;

            if (_combinedBitmapDictionary.TryGetValue(new KeyValuePair<Bitmap, Bitmap>(baseIcon, qualityIcon), out var combinedBitmap))
                return combinedBitmap;

            //combine the two bitmaps
            var canvas = new Bitmap(baseIcon.Width, baseIcon.Height, baseIcon.PixelFormat);
            using (var g = Graphics.FromImage(canvas)) {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(baseIcon, new Rectangle(0, 0, baseIcon.Width, baseIcon.Height));
                g.DrawImage(qualityIcon, new Rectangle(
                    (int) (baseIcon.Width * (1 - qualitySizeMultiplier)),
                    (int) (baseIcon.Height * (1 - qualitySizeMultiplier)),
                    (int) (baseIcon.Width * qualitySizeMultiplier),
                    (int) (baseIcon.Height * qualitySizeMultiplier))
                );
            }

            _combinedBitmapDictionary.Add(new KeyValuePair<Bitmap, Bitmap>(baseIcon, qualityIcon), canvas);
            return canvas;
        }
    }
}