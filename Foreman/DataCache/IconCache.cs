using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	[Serializable]
	public struct IconColorPair
	{
		public Bitmap Icon;
		public Color Color;
		public IconColorPair(Bitmap icon, Color color)
		{
			this.Icon = icon;
			this.Color = color;
		}
	}
	[Serializable]
	public class IconBitmapCollection
	{
		public Dictionary<string, IconColorPair> Icons;
		public IconBitmapCollection() { Icons = new Dictionary<string, IconColorPair>(); }
	}


	public static class IconCache
	{
		private static Bitmap unknownIcon;
		public static Bitmap GetUnknownIcon()
		{
			if (unknownIcon == null)
				unknownIcon = GetIcon(Path.Combine("Graphics", "UnknownIcon.png"), 32);
			return unknownIcon;
		}
		public static Bitmap GetIcon(string path, int size)
		{
			try
			{
				using (Bitmap image = new Bitmap(path)) //If you don't do this, the file is locked for the lifetime of the bitmap
				{
					Bitmap bmp = new Bitmap(size, size);
					using (Graphics g = Graphics.FromImage(bmp))
						g.DrawImage(image, new Rectangle(0, 0, (size * image.Width / image.Height), size));
					return bmp;
				}
			}
			catch (Exception) { return new Bitmap(size, size); }
		}


		public static void SaveIconCache(string path, Dictionary<string, IconColorPair> iconCache)
		{
			IconBitmapCollection iCollection = new IconBitmapCollection();

			foreach (KeyValuePair<string, IconColorPair> iconKVP in iconCache)
				iCollection.Icons.Add(iconKVP.Key, iconKVP.Value);

			if (File.Exists(path))
				File.Delete(path);
			using (Stream stream = File.Open(path, FileMode.Create, FileAccess.Write))
			{
				var binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(stream, iCollection);
			}
		}

		public static async Task<Dictionary<string, IconColorPair>> LoadIconCache(string path, IProgress<KeyValuePair<int, string>> progress, int startingPercent, int endingPercent)
		{
			Dictionary<string, IconColorPair> iconCache = new Dictionary<string, IconColorPair>();
			await Task.Run(() =>
			{
				try
				{
					using (Stream stream = File.Open(path, FileMode.Open))
					{
						var binaryFormatter = new BinaryFormatter();
						IconBitmapCollection iCollection = (IconBitmapCollection)binaryFormatter.Deserialize(stream);

						int totalCount = iCollection.Icons.Count();
						int counter = 0;
						foreach (KeyValuePair<string, IconColorPair> iconKVP in iCollection.Icons)
						{
							progress.Report(new KeyValuePair<int, string>(startingPercent + (endingPercent - startingPercent) * counter++ / totalCount, "Loading Icons..."));
							iconCache.Add(iconKVP.Key, iconKVP.Value);
						}
					}
				}
				catch //there was an error reading the cache. Just ignore it and continue (we will have to load the icons from the files directly)
				{
					iconCache.Clear();
					MessageBox.Show("Icon cache was corrupted. All icons will be empty.\nRecommendation: delete preset and import new one?");
				}
			});
			return iconCache;
		}
	}
}
