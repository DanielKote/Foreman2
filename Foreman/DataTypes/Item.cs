using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace Foreman
{
	public class Item
	{
		public static List<String> localeCategories = new List<String> { "item-name", "fluid-name", "entity-name", "equipment-name" };

		public String Name { get; private set; }
		public string LName { get; set; }
		public HashSet<Recipe> ProductionRecipes { get; private set; }
		public HashSet<Recipe> ConsumptionRecipes { get; private set; }
		public double Temperature { get; set; } //for liquids

		private Bitmap icon;
		public Bitmap Icon { get { return icon; } set { icon = value; AverageColor = IconProcessor.GetAverageColor(value); } }
		private static readonly Color DefaultAverageColor = Color.Black;
		public Color AverageColor { get; private set; }

		public String FriendlyName
		{
			get
			{
				if (!String.IsNullOrEmpty(LName))
					return LName;

				string calcName = "";
				string localeString = "";
				foreach (String category in localeCategories)
				{
					string[] SplitName = Name.Split('\f');
					if (DataCache.LocaleFiles.ContainsKey(category) && DataCache.LocaleFiles[category].ContainsKey(SplitName[0]))
					{
						localeString = DataCache.LocaleFiles[category][SplitName[0]];
						if (DataCache.LocaleFiles[category][SplitName[0]].Contains("__"))
							calcName = Regex.Replace(DataCache.LocaleFiles[category][SplitName[0]], "__.+?__", "").Replace("_", "").Replace("-", " ");
						else
							calcName =  DataCache.LocaleFiles[category][SplitName[0]];
						if(SplitName.Length > 1)
							calcName += " (" + SplitName + "*)";
					}

				}
				return calcName;
				Console.WriteLine(Name + ": >>" + LName + "<< compared: >>" + calcName + "<<. LOCALE STRING: >>"+localeString+"<<");
				return LName;
			}
		}
		public Boolean IsMissingItem = false;

		public Item(String name)
		{
			Name = name;
			ProductionRecipes = new HashSet<Recipe>();
			ConsumptionRecipes = new HashSet<Recipe>();
			AverageColor = DefaultAverageColor;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Item))
			{
				return false;
			}
			return this == (Item)obj;
		}

		public static bool operator ==(Item item1, Item item2)
		{
			if (System.Object.ReferenceEquals(item1, item2))
			{
				return true;
			}
			if ((object)item1 == null || (object)item2 == null)
			{
				return false;
			}

			return item1.Name == item2.Name;
		}

		public static bool operator !=(Item item1, Item item2)
		{
			return !(item1 == item2);
		}

		public override string ToString()
		{
			return String.Format("Item: {0}", Name);
		}

		private const int iconBorder = 1;
		private static readonly Color iconBorderColor = Color.FromArgb(150, 100, 100, 100);
		/*
		private void ProcessIcon(Bitmap inicon)
        {
			if(inicon == null)
            {
				icon = null;
				return;
            }

			if (TooBright)
			{

				icon = new Bitmap(inicon.Width, inicon.Height, PixelFormat.Format32bppArgb);
				LBitmap licon = new LBitmap(icon);
				licon.LockBits();

				LBitmap linicon = new LBitmap(inicon);
				linicon.LockBits();

				//set up the shadow
				for (int y = 0; y < linicon.Height; y++)
				{
					for (int x = 1; x < linicon.Width; x++)
					{
						if (linicon.GetPixel(x, y).A >= 10)
						{
							for (int sy = Math.Max(0, y - iconBorder); sy <= Math.Min(icon.Height - 1, y + iconBorder); sy++)
								for (int sx = Math.Max(0, x - iconBorder); sx <= Math.Min(icon.Width - 1, x + iconBorder); sx++)
									licon.SetPixel(sx, sy, iconBorderColor);

						}
					}
				}
				licon.UnlockBits();
				linicon.UnlockBits();

				using (Graphics g = Graphics.FromImage(icon))
				{
					g.DrawImage(inicon, 0, 0, icon.Width, icon.Height);
				}
			}
			else
				icon = inicon;
		}*/
	}
}
