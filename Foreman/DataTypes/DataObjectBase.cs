using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Foreman
{
    public abstract class DataObjectBase : IComparable<DataObjectBase>
    {
		internal string[] localeCategories;
        public string Name { get; private set; }
        public string LName { get; set; }
		public string Order { get; private set; }
		public virtual Subgroup MySubgroup { get; private set; }
		public virtual Group MyGroup { get { return MySubgroup.MyGroup; } }

		private string friendlyName;
		private string lfriendlyName; //lower case
		public string LFriendlyName { get { if (string.IsNullOrEmpty(lfriendlyName)) lfriendlyName = FriendlyName.ToLower(); return lfriendlyName; } }
		public string FriendlyName
		{
			get
			{
				if (string.IsNullOrEmpty(friendlyName))
				{
					if (!String.IsNullOrEmpty(LName))
						friendlyName = LName;
					else if (localeCategories != null)
					{
						string calcName = "";

						foreach (String category in localeCategories)
						{
							string[] SplitName = Name.Split('\f');
							if (DataCache.LocaleFiles.ContainsKey(category) && DataCache.LocaleFiles[category].ContainsKey(SplitName[0]))
							{
								if (DataCache.LocaleFiles[category][SplitName[0]].Contains("__"))
									calcName = Regex.Replace(DataCache.LocaleFiles[category][SplitName[0]], "__.+?__", "").Replace("_", "").Replace("-", " ");
								else
									calcName = DataCache.LocaleFiles[category][SplitName[0]];
								if (SplitName.Length > 1)
									calcName += " (" + SplitName + "*)";
							}
						}
						friendlyName = calcName;
					}
					else
						friendlyName = Name;
				}
				return friendlyName;
			}
		}

		public DataObjectBase(string name, string lname, Subgroup subGroup, string order)
        {
			Name = name;
			LName = lname;
			MySubgroup = subGroup;

			Order = order;
			AverageColor = Color.Black;
        }

		public void SetIconAndColor(IconColorPair icp) { SetIconAndColor(icp.Icon, icp.Color); }
		public void SetIconAndColor(Bitmap icon, Color averageColor) //usefull if icon average color has already been calculated
		{
			if(icon != null)
            {
				this.icon = icon;
				this.AverageColor = averageColor;
            }
			else
            {
				this.icon = DataCache.UnknownIcon;
				this.AverageColor = averageColor;
            }
		}

        private static readonly Color DefaultAverageColor = Color.Black;
        public Color AverageColor { get; private set; }

        private Bitmap icon;
        public Bitmap Icon
		{
			get { return icon; } 
			set
			{
				if (value != null)
				{
					icon = value;
					AverageColor = IconProcessor.GetAverageColor(value);
				}
				else
                {
					icon = DataCache.UnknownIcon; //even if null
					AverageColor = DefaultAverageColor;
                }
			}
		}

		public override bool Equals(object obj)
		{
			return (obj as DataObjectBase) == this;
		}

		public static bool operator ==(DataObjectBase doBase1, DataObjectBase doBase2)
		{
			if (ReferenceEquals(doBase1, doBase2))
				return true;
			if ((object)doBase1 == null || (object)doBase2 == null)
				return false;
			if (doBase1.GetType() != doBase2.GetType())
				return false;
			return doBase1.Name == doBase2.Name;
		}

		public static bool operator !=(DataObjectBase recipe1, DataObjectBase recipe2)
		{
			return !(recipe1 == recipe2);
		}

		public override int GetHashCode() { return Name.GetHashCode(); }
		public int CompareTo(DataObjectBase other) { return Order.CompareTo(other.Order); }
	}
}
