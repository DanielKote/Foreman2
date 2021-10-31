using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Compression;

namespace Foreman
{

	public struct IconInfo
	{
		public string iconPath;
		public int iconSize;
		public double iconScale;
		public Point iconOffset;
		public Color iconTint;

		public IconInfo(string iconPath, int iconSize)
		{
			this.iconPath = iconPath;
			this.iconSize = iconSize;
			this.iconScale = iconSize > 0 ? 32 / iconSize : 1;
			this.iconOffset = new Point(0, 0);
			iconTint = IconCacheProcessor.NoTint;
		}

		public void SetIconTint(double a, double r, double g, double b)
		{
			a = (a <= 1 ? a * 255 : a);
			r = (r <= 1 ? r * 255 : r);
			g = (g <= 1 ? g * 255 : g);
			b = (b <= 1 ? b * 255 : b);
			iconTint = Color.FromArgb((int)a, (int)r, (int)g, (int)b);
		}
	}

	public class IconCacheProcessor : IDisposable
	{
		internal static readonly Color NoTint = Color.FromArgb(255, 255, 255, 255);

		public int TotalPathCount { get; private set; }
		public int FailedPathCount { get; private set; }

		private Dictionary<string, IconColorPair> myIconCache;

		private Dictionary<string, string> folderLinks;
		private Dictionary<string, ZipArchiveEntry> archiveFileLinks;
		private List<ZipArchive> openedArchives;
		private Dictionary<string, Bitmap> bitmapCache; //just so we dont have to load the same file multiple times

		public IconCacheProcessor()
		{
			TotalPathCount = 0;
			FailedPathCount = 0;

			myIconCache = new Dictionary<string, IconColorPair>();

			folderLinks = new Dictionary<string, string>();
			archiveFileLinks = new Dictionary<string, ZipArchiveEntry>();
			openedArchives = new List<ZipArchive>();
			bitmapCache = new Dictionary<string, Bitmap>();
		}

		public bool PrepareModPaths(Dictionary<string, string> modSet, string modsPath, string dataPath, CancellationToken token)
		{
			folderLinks.Clear();
			archiveFileLinks.Clear();
			bitmapCache.Clear();

			bool success = true;
			//factorio checks for foldeer <name>_<version>, then folder <name> then zip <name>_<version>
			//if zip, then the actual files can either be in the root of zip, or in <name> foler, or in <name>_<version> folder
			foreach (KeyValuePair<string, string> mod in modSet)
			{
				if (token.IsCancellationRequested)
					return false;

				if (Directory.Exists(Path.Combine(modsPath, mod.Key + "_" + mod.Value)))
					folderLinks.Add("__" + mod.Key.ToLower() + "__", Path.Combine(modsPath, mod.Key + "_" + mod.Value));
				else if (Directory.Exists(Path.Combine(modsPath, mod.Key)))
					folderLinks.Add("__" + mod.Key.ToLower() + "__", Path.Combine(modsPath, mod.Key));
				else if (File.Exists(Path.Combine(modsPath, mod.Key + "_" + mod.Value + ".zip")))
				{
					//for zip files, since we have to iterate through them for each file we might as well make a full link of every possible filepath to given entry
					ZipArchive zip = ZipFile.Open(Path.Combine(modsPath, mod.Key + "_" + mod.Value + ".zip"), ZipArchiveMode.Read);
					openedArchives.Add(zip);
					foreach (ZipArchiveEntry zentity in zip.Entries)
					{
						if (zentity.Name == "")
							continue; //folder

						LinkedList<string> brokenPath = new LinkedList<string>();
						string filePath = zentity.FullName;
						while (filePath != "")
						{
							brokenPath.AddFirst(Path.GetFileName(filePath));
							filePath = Path.GetDirectoryName(filePath);
						}
						brokenPath.First.Value = "__" + mod.Key.ToLower() + "__";
						archiveFileLinks.Add(Path.Combine(brokenPath.ToArray()).ToLower(), zentity);
					}
				}
				else if (mod.Key.ToLower() != "core" && mod.Key.ToLower() != "base" && mod.Key.ToLower() != "foremanexport") success = false;
			}
			folderLinks.Add("__core__", Path.Combine(dataPath, "core"));
			folderLinks.Add("__base__", Path.Combine(dataPath, "base"));
			return success;
		}

		public bool CreateIconCache(JObject iconJObject, string cachePath, IProgress<KeyValuePair<int, string>> progress, CancellationToken token, int startingPercent, int endingPercent)
		{
			TotalPathCount = 0;
			FailedPathCount = 0;

			myIconCache.Clear();
			bitmapCache.Clear();

			int totalCount =
				iconJObject["technologies"].Count() +
				iconJObject["recipes"].Count() +
				iconJObject["items"].Count() +
				iconJObject["fluids"].Count() +
				iconJObject["groups"].Count();

			progress.Report(new KeyValuePair<int, string>(startingPercent, "Creating icons."));
			int counter = 0;
			foreach (var iconJToken in iconJObject["technologies"].ToList())
			{
				if (token.IsCancellationRequested) return false;
				progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
				ProcessIcon(iconJToken, 256);
			}
			foreach (var iconJToken in iconJObject["recipes"].ToList())
			{
				if (token.IsCancellationRequested) return false;
				progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
				ProcessIcon(iconJToken, 32);
			}
			foreach (var iconJToken in iconJObject["items"].ToList())
			{
				if (token.IsCancellationRequested) return false;
				progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
				ProcessIcon(iconJToken, 32);
			}
			foreach (var iconJToken in iconJObject["fluids"].ToList())
			{
				if (token.IsCancellationRequested) return false;
				progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
				ProcessIcon(iconJToken, 32);
			}
			foreach (var iconJToken in iconJObject["groups"].ToList())
			{
				if (token.IsCancellationRequested) return false;
				progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, ""));
				ProcessIcon(iconJToken, 64);
			}

			IconCache.SaveIconCache(cachePath, myIconCache);

			return (FailedPathCount == 0);
		}

		private void ProcessIcon(JToken objJToken, int defaultIconSize)
		{
			if (objJToken["icon_data"].Type != JTokenType.Null)
			{
				string iconName = (string)objJToken["icon_name"];
				IconColorPair iconData = new IconColorPair(null, Color.Black);

				JToken iconDataJToken = objJToken["icon_data"];

				string mainIconPath = (string)iconDataJToken["icon"];
				int baseIconSize = (iconDataJToken["icon_size"].Type == JTokenType.Null) ? 32 : (int)iconDataJToken["icon_size"];

				IconInfo iicon = new IconInfo(mainIconPath, baseIconSize);

				List<IconInfo> iicons = new List<IconInfo>();
				List<JToken> iconJTokens = iconDataJToken["icons"].ToList();
				foreach (var iconJToken in iconJTokens)
				{
					IconInfo picon = new IconInfo((string)iconJToken["icon"], (iconJToken["icon_size"].Type == JTokenType.Null) ? baseIconSize : (int)iconJToken["icon_size"]);
					picon.iconScale = (iconJToken["scale"].Type == JTokenType.Null) ? defaultIconSize / picon.iconSize : (double)iconJToken["scale"];

					picon.iconOffset = new Point((int)iconJToken["shift"][0], (int)iconJToken["shift"][1]);
					picon.SetIconTint((double)iconJToken["tint"][3], (double)iconJToken["tint"][0], (double)iconJToken["tint"][1], (double)iconJToken["tint"][2]);
					iicons.Add(picon);
				}
				myIconCache.Add(iconName, GetIconAndColor(iicon, iicons, defaultIconSize));
			}
		}


		public IconColorPair GetIconAndColor(IconInfo iinfo, List<IconInfo> iinfos, int defaultCanvasSize)
		{
			if (iinfos == null)
				iinfos = new List<IconInfo>();
			double IconCanvasScale = defaultCanvasSize == 32 ? 2 : 1; //just some upscailing for icons (64x64 look better)
			int IconCanvasSize = (int)(defaultCanvasSize * IconCanvasScale);

			if (iinfos.Count == 0) //if there are no icons, use the single icon
				iinfos.Add(iinfo);

			//quick check to ensure it isnt a null icon
			bool empty = true;
			foreach (IconInfo ii in iinfos)
			{
				if (!string.IsNullOrEmpty(ii.iconPath))
					empty = false;
			}
			if (empty)
				return new IconColorPair(null, Color.Black);

			//prepare the canvas - we will add each successive icon/layer on top of it
			Bitmap canvas = new Bitmap(IconCanvasSize, IconCanvasSize, PixelFormat.Format32bppPArgb);
			BitmapData canvasData = canvas.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height), ImageLockMode.ReadWrite, canvas.PixelFormat);
			int cBPP = Bitmap.GetPixelFormatSize(canvas.PixelFormat) / 8;
			int bCount = canvasData.Stride * canvas.Height;
			byte[] canvasPixels = new byte[bCount];
			IntPtr ptrCanvasFPixel = canvasData.Scan0;
			Marshal.Copy(ptrCanvasFPixel, canvasPixels, 0, canvasPixels.Length);
			int heightInPixels = canvasData.Height;
			int widthInBytes = canvasData.Width * cBPP;

			foreach (IconInfo ii in iinfos)
			{
				//load the image and prep it for processing
				int iconSize = ii.iconSize > 0 ? ii.iconSize : iinfo.iconSize;
				int iconDrawSize = (int)(iconSize * (ii.iconScale > 0 ? ii.iconScale : (double)defaultCanvasSize / iconSize));
				iconDrawSize = (int)(iconDrawSize * IconCanvasScale);

				Bitmap iconImage = LoadImageFromMod(ii.iconPath, iconDrawSize);
				if (iconImage == null)
					continue;

				//draw the icon onto a layer (that we will apply tint to and blend with canvas)
				Bitmap layerSlice = new Bitmap(canvas.Width, canvas.Height, canvas.PixelFormat);
				using (Graphics g = Graphics.FromImage(layerSlice))
					g.DrawImageUnscaled(iconImage, (IconCanvasSize / 2) - (iconDrawSize / 2) + ii.iconOffset.X, (IconCanvasSize / 2) - (iconDrawSize / 2) + ii.iconOffset.Y);

				//grab the layer data
				BitmapData layerData = layerSlice.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height), ImageLockMode.ReadOnly, canvas.PixelFormat);
				byte[] layerPixels = new byte[bCount];
				IntPtr ptrLayerFPixel = layerData.Scan0;
				Marshal.Copy(ptrLayerFPixel, layerPixels, 0, layerPixels.Length);

				//blend -> for each value in 0->1 (so when multiplying, you have to divide by 255 if in 0->255)
				//newCanvas(A/R/G/B) = Layer(A/R/G/B) * tint(A/R/G/B)   +   oldCanvas(A/R/G/B) * (1 - tint(A) * Layer(A))
				//https://www.factorio.com/blog/post/fff-172
				for (int y = 0; y < heightInPixels; y++)
				{
					int currentLine = y * canvasData.Stride;
					for (int x = 0; x < widthInBytes; x = x + cBPP)
					{
						int canvasMulti = 255 - (ii.iconTint.A * layerPixels[currentLine + x + 3] / 255);
						canvasPixels[currentLine + x + 0] = (byte)Math.Min(255,
							(layerPixels[currentLine + x + 0] * ii.iconTint.B / 255) +
							(canvasPixels[currentLine + x + 0] * canvasMulti / 255));
						canvasPixels[currentLine + x + 1] = (byte)Math.Min(255,
							(layerPixels[currentLine + x + 1] * ii.iconTint.G / 255) +
							(canvasPixels[currentLine + x + 1] * canvasMulti / 255));
						canvasPixels[currentLine + x + 2] = (byte)Math.Min(255,
							(layerPixels[currentLine + x + 2] * ii.iconTint.R / 255) +
							(canvasPixels[currentLine + x + 2] * canvasMulti / 255));
						canvasPixels[currentLine + x + 3] = (byte)Math.Min(255,
							(layerPixels[currentLine + x + 3] * ii.iconTint.A / 255) +
							(canvasPixels[currentLine + x + 3] * canvasMulti / 255));

					}
				}
				layerSlice.UnlockBits(layerData);
			}

			//we are done adding all the layers, so copy the canvas data
			Marshal.Copy(canvasPixels, 0, ptrCanvasFPixel, canvasPixels.Length);
			canvas.UnlockBits(canvasData);

			//at this point we need to convert the canvas into a non-alpha multiplied format due to winforms having issues with it
			Bitmap result = new Bitmap(canvas.Width, canvas.Height, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(result))
				g.DrawImageUnscaled(canvas, 0, 0);

			//finally, calculate the average color (yes, it comes out a bit different due to inclusion of transparency)
			Color averageColor = GetAverageColor(result);
			if (averageColor.GetBrightness() > 0.9)
				result = AddBorder(result); //if the image is too bright, add a border to it. Honestly, this is never done anymore - it was useful before layer blending was fixed and some icons came out... white.
			if (averageColor.GetBrightness() > 0.7)
				averageColor = Color.FromArgb(255, (int)(averageColor.R * 0.7), (int)(averageColor.G * 0.7), (int)(averageColor.B * 0.7));

			return new IconColorPair(result, averageColor);
		}

		private Bitmap LoadImageFromMod(string fileName, int resultSize = 32) //NOTE: must make sure we use pre-multiplied alpha
		{
			if (String.IsNullOrEmpty(fileName))
				return null;
			fileName = fileName.ToLower().Replace("/", "\\");
			while (fileName.IndexOf("\\\\") != -1) //found this error in krastorio - apparently factorio ignores multiple slashes in file name
				fileName = fileName.Replace("\\\\", "\\");

			//if the image isnt currently in the cache, process it and add it to cache
			if (!bitmapCache.ContainsKey(fileName))
			{
				TotalPathCount++;
				string origin = fileName.Substring(0, fileName.IndexOf("__", 2) + 2);
				string file = fileName.Substring(fileName.IndexOf("__", 2) + 3);

				if (folderLinks.ContainsKey(origin))
				{

					file = Path.Combine(folderLinks[origin], file);
					try { bitmapCache.Add(fileName, new Bitmap(file)); }
					catch
					{
						bitmapCache.Add(fileName, null);
						FailedPathCount++;
						ErrorLogging.LogLine("IconCacheProcessor: given fileName not found in mod folders: " + fileName);
					}

				}
				else if (archiveFileLinks.ContainsKey(fileName))
				{
					try { bitmapCache.Add(fileName, new Bitmap(archiveFileLinks[fileName].Open())); }
					catch
					{
						bitmapCache.Add(fileName, null);
						FailedPathCount++;
						ErrorLogging.LogLine("IconCacheProcessor: given fileName not found in mod folders: " + fileName);
					}

				}
				else
				{
					FailedPathCount++;
					bitmapCache.Add(fileName, null);
					ErrorLogging.LogLine("IconCacheProcessor: given fileName not found in mod folders: " + fileName);
				}
			}

			if (bitmapCache[fileName] == null)
				return null;

			//get the requested image from the cache and draw it to correct size.
			Bitmap image = bitmapCache[fileName];
			Bitmap bmp = new Bitmap(resultSize, resultSize, PixelFormat.Format32bppPArgb);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.DrawImage(image, new Rectangle(0, 0, (resultSize * image.Width / image.Height), resultSize));
			}
			return bmp;
		}

		private Color GetAverageColor(Bitmap icon)
		{
			if (icon == null)
				return Color.Black;

			BitmapData iconData = icon.LockBits(new Rectangle(0, 0, icon.Width, icon.Height), ImageLockMode.ReadOnly, icon.PixelFormat);
			int bytesPerPixel = Bitmap.GetPixelFormatSize(icon.PixelFormat) / 8;
			int byteCount = iconData.Stride * icon.Height;
			byte[] iconPixels = new byte[byteCount];
			IntPtr ptrFirstPixel = iconData.Scan0;
			Marshal.Copy(ptrFirstPixel, iconPixels, 0, iconPixels.Length);
			int heightInPixels = iconData.Height;
			int widthInBytes = iconData.Width * bytesPerPixel;

			int[] totalPixel = { 0, 0, 0, 0 };
			int totalCounter = 1; //just to avoid div by 0 in case of completely empty bitmap
			for (int y = 0; y < heightInPixels; y++)
			{
				int currentLine = y * iconData.Stride;
				for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
				{
					if (iconPixels[currentLine + x + 3] > 10) //ignore transparent pixels
					{
						totalPixel[3] += iconPixels[currentLine + x];     //B
						totalPixel[2] += iconPixels[currentLine + x + 1]; //G
						totalPixel[1] += iconPixels[currentLine + x + 2]; //R
						totalCounter++;
					}
				}
			}
			for (int i = 1; i < 4; i++)
			{
				totalPixel[i] /= totalCounter;
				totalPixel[i] = Math.Min(totalPixel[i], 255);
			}
			icon.UnlockBits(iconData);

			return Color.FromArgb(255, totalPixel[1], totalPixel[2], totalPixel[3]);
		}

		private const int iconBorder = 1; //border is drawn on a new layer as 
		private Bitmap AddBorder(Bitmap icon)
		{
			Bitmap canvas = new Bitmap(icon.Width, icon.Height, icon.PixelFormat);
			BitmapData iconData = icon.LockBits(new Rectangle(0, 0, icon.Width, icon.Height), ImageLockMode.ReadOnly, icon.PixelFormat);
			BitmapData canvasData = canvas.LockBits(new Rectangle(0, 0, icon.Width, icon.Height), ImageLockMode.WriteOnly, icon.PixelFormat);
			int bytesPerPixel = Bitmap.GetPixelFormatSize(icon.PixelFormat) / 8; //same for both
			int byteCount = iconData.Stride * icon.Height; //same for both
			byte[] iconPixels = new byte[byteCount];
			byte[] canvasPixels = new byte[byteCount];

			IntPtr ptrFirstPixel = iconData.Scan0;
			Marshal.Copy(ptrFirstPixel, iconPixels, 0, iconPixels.Length);
			int heightInPixels = iconData.Height;
			int widthInBytes = iconData.Width * bytesPerPixel;

			for (int y = iconBorder; y < heightInPixels - iconBorder; y++)
			{
				int currentLine = y * iconData.Stride;
				for (int x = iconBorder * bytesPerPixel; x < widthInBytes - iconBorder * bytesPerPixel; x += bytesPerPixel)
				{
					if (iconPixels[currentLine + x + 3] > 11) //check if A >= 10
					{
						for (int iy = -iconBorder; iy <= iconBorder; iy++)
						{
							for (int ix = -iconBorder * bytesPerPixel; ix <= iconBorder * bytesPerPixel; ix += bytesPerPixel)
							{
								int currentCanvasIndex = currentLine + iy * iconData.Stride + x + ix;
								canvasPixels[currentCanvasIndex] = 64;
								canvasPixels[currentCanvasIndex + 1] = 64;
								canvasPixels[currentCanvasIndex + 2] = 64;
								canvasPixels[currentCanvasIndex + 3] = 64;
							}
						}
					}
				}
			}
			ptrFirstPixel = canvasData.Scan0;
			Marshal.Copy(canvasPixels, 0, ptrFirstPixel, canvasPixels.Length);
			icon.UnlockBits(iconData);
			canvas.UnlockBits(canvasData);

			//draw the processed icon (singluar) onto the main canvas
			using (Graphics g = Graphics.FromImage(canvas))
			{
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.DrawImageUnscaled(icon, 0, 0);
			}

			return canvas;
		}

		private bool disposedValue;
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					archiveFileLinks.Clear();
					archiveFileLinks = null;

					foreach (Bitmap bitmap in bitmapCache.Values)
						bitmap?.Dispose();
					bitmapCache.Clear();
					bitmapCache = null;

					foreach (ZipArchive zip in openedArchives)
						zip.Dispose();
					openedArchives.Clear();
					openedArchives = null;
				}
				disposedValue = true;
			}
		}
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
