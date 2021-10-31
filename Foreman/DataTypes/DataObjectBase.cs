using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Foreman
{
    public abstract class DataObjectBase : IComparable<DataObjectBase>
    {
		protected DataCache myCache { get; private set; }

        public string Name { get; private set; }
		public string Order { get; private set; }

		public virtual string LFriendlyName { get; private set; }
		public virtual string FriendlyName { get; private set; }

		public DataObjectBase(DataCache dCache, string name, string friendlyName, string order)
        {
			myCache = dCache;
			Name = name;
			FriendlyName = friendlyName;
			LFriendlyName = friendlyName.ToLower();

			Order = order;
			Icon = DataCache.UnknownIcon;
			AverageColor = Color.Black;
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

        private static readonly Color DefaultAverageColor = Color.Black;
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
			int orderCompare = Order.CompareTo(other.Order);
			if (orderCompare == 0)
				return FriendlyName.CompareTo(other.FriendlyName);
			return orderCompare;
        }
	}
}
