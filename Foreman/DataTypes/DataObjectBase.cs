using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Foreman
{
    public abstract class DataObjectBase : IComparable<DataObjectBase>
    {
		private static readonly char[] orderSeparators = { '[', ']' };

		protected DataCache myCache { get; private set; }

        public string Name { get; private set; }

		public virtual string LFriendlyName { get; private set; }
		public virtual string FriendlyName { get; private set; }

		private string[] OrderCompareArray;

		public DataObjectBase(DataCache dCache, string name, string friendlyName, string order)
        {
			myCache = dCache;
			Name = name;
			FriendlyName = friendlyName;
			LFriendlyName = friendlyName.ToLower();

			Icon = DataCache.UnknownIcon;
			AverageColor = Color.Black;

			OrderCompareArray = order.Split(orderSeparators);
        }

		public void SetIconAndColor(IconColorPair icp) { SetIconAndColor(icp.Icon, icp.Color); }
		public void SetIconAndColor(Bitmap icon, Color averageColor) //usefull if icon average color has already been calculated
		{
			if(icon != null)
            {
				this.Icon = icon;

				this.AverageColor = averageColor;
            }
			else
            {
				this.Icon = DataCache.UnknownIcon;
				this.AverageColor = averageColor;
            }
		}

        public virtual Color AverageColor { get; private set; }
		public virtual Bitmap Icon { get; private set; }

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
		public int CompareTo(DataObjectBase other)
        {
			//order comparison is apparently quite convoluted - any time we have brackets ([ or ]), it signifies a different order part.
			//each part is compared char-by-char, and in the case of the longer string it goes first.
			//same thing for sections?
			for (int i = 0; i < this.OrderCompareArray.Length && i < other.OrderCompareArray.Length; i++)
            {
				for (int j = 0; j < this.OrderCompareArray[i].Length && j < other.OrderCompareArray[i].Length; j++)
                {
					int result = this.OrderCompareArray[i][j].CompareTo(other.OrderCompareArray[i][j]);
					if (result != 0)
						return result;
                }
				if (this.OrderCompareArray[i].Length != other.OrderCompareArray[i].Length)
					return (this.OrderCompareArray[i].Length > other.OrderCompareArray[i].Length) ? -1 : 1;
            }
			if (this.OrderCompareArray.Length != other.OrderCompareArray.Length)
				return (this.OrderCompareArray.Length > other.OrderCompareArray.Length) ? -1 : 1;

			return LFriendlyName.CompareTo(other.LFriendlyName);
        }
	}
}
