using System;
using System.Drawing;
using System.Linq;

namespace Foreman
{
	public interface DataObjectBase : IComparable<DataObjectBase>
	{
		DataCache Owner { get; }

		string Name { get; }
		string LFriendlyName { get; }
		string FriendlyName { get; }

		bool Available { get; }
		bool Enabled { get; set; }

		Bitmap Icon { get; }
		Color AverageColor { get; }
		void SetIconAndColor(IconColorPair icp);
	}

	public abstract class DataObjectBasePrototype : DataObjectBase
	{
		private static readonly char[] orderSeparators = { '[', ']' };

		public DataCache Owner { get; private set; }

		public string Name { get; private set; }
		public string LFriendlyName { get; private set; }
		public string FriendlyName { get; private set; }

		public virtual bool Available { get; set; }
		public bool Enabled { get; set; }

		private string[] OrderCompareArray;

		public DataObjectBasePrototype(DataCache dCache, string name, string friendlyName, string order)
		{
			Owner = dCache;
			Name = name;
			FriendlyName = friendlyName;
			LFriendlyName = friendlyName.ToLower();

			Available = true;
			Enabled = true;

			Icon = DataCache.UnknownIcon;
			AverageColor = Color.Black;

			OrderCompareArray = order.Split(orderSeparators).Where(s => !string.IsNullOrEmpty(s)).ToArray();
		}

		public void SetIconAndColor(IconColorPair icp)
		{
			if (icp.Icon != null)
				this.Icon = icp.Icon;
			else
				this.Icon = DataCache.UnknownIcon;

			this.AverageColor = icp.Color;
		}

		public Color AverageColor { get; private set; }
		public Bitmap Icon { get; private set; }

		public override bool Equals(object obj)
		{
			return (obj as DataObjectBasePrototype) == this;
		}

		public static bool operator ==(DataObjectBasePrototype doBase1, DataObjectBasePrototype doBase2)
		{
			if (ReferenceEquals(doBase1, doBase2))
				return true;
			if ((object)doBase1 == null || (object)doBase2 == null)
				return false;
			if (doBase1.GetType() != doBase2.GetType())
				return false;
			return doBase1.Name == doBase2.Name;
		}

		public static bool operator !=(DataObjectBasePrototype recipe1, DataObjectBasePrototype recipe2)
		{
			return !(recipe1 == recipe2);
		}

		public override int GetHashCode() { return Name.GetHashCode(); }
		public int CompareTo(DataObjectBase other)
		{
			if (other is DataObjectBasePrototype otherP)
			{

				//order comparison is apparently quite convoluted - any time we have brackets ([ or ]), it signifies a different order part.
				//each part is compared char-by-char, and in the case of the longer string it goes first.
				//in terms of sections, the sorter section goes first (ex: a[0] goes before a[0]-1)
				for (int i = 0; i < this.OrderCompareArray.Length && i < otherP.OrderCompareArray.Length; i++)
				{
					for (int j = 0; j < this.OrderCompareArray[i].Length && j < otherP.OrderCompareArray[i].Length; j++)
					{
						int result = this.OrderCompareArray[i][j].CompareTo(otherP.OrderCompareArray[i][j]);
						if (result != 0)
							return result;
					}
					if (this.OrderCompareArray[i].Length != otherP.OrderCompareArray[i].Length)
						return (this.OrderCompareArray[i].Length > otherP.OrderCompareArray[i].Length) ? -1 : 1;
				}
				if (this.OrderCompareArray.Length != otherP.OrderCompareArray.Length)
					return (this.OrderCompareArray.Length < otherP.OrderCompareArray.Length) ? -1 : 1;

				return LFriendlyName.CompareTo(otherP.LFriendlyName);
			}
			return 0;
		}
	}
}
