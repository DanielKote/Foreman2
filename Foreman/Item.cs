using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	public class Item
	{
		public static List<String> localeCategories = new List<String> { "item-name", "fluid-name", "entity-name", "equipment-name" };

		public String Name { get; private set; }
		public HashSet<Recipe> Recipes { get; private set; }
		public Bitmap Icon { get; set; }
		public String FriendlyName
		{
			get
			{
				foreach (String category in localeCategories)
				{
					if (DataCache.LocaleFiles[category].ContainsKey(Name))
					{
						return DataCache.LocaleFiles[category][Name];
					}
				}

				return Name;
			}
		}
		public Boolean IsMissingItem = false;

		private Item()
		{
			Name = "";
		}

		public Item(String name)
		{
			Name = name;
			Recipes = new HashSet<Recipe>();
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
	}
}
