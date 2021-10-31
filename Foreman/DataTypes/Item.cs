using System;
using System.Collections.Generic;
using System.Drawing;

namespace Foreman
{
	public class Item : DataObjectBase, IComparable<Item>
	{
		public static string[] itemLocaleCategories = { "item-name", "fluid-name", "entity-name", "equipment-name" };

		public HashSet<Recipe> ProductionRecipes { get; private set; }
		public HashSet<Recipe> ConsumptionRecipes { get; private set; }
		public Boolean IsMissingItem = false;

		public double Temperature { get; set; } //for liquids

		public new Bitmap Icon
        {
			get
			{
				if (base.Icon == null)
					base.Icon = DataCache.UnknownIcon;
				return base.Icon;
            }
			set { base.Icon = value; }
        }

		public Item(string name, string lname) : base(name, lname)
		{
			localeCategories = itemLocaleCategories;
			ProductionRecipes = new HashSet<Recipe>();
			ConsumptionRecipes = new HashSet<Recipe>();
		}

		public override string ToString() { return String.Format("Item: {0}", Name); }
		public int CompareTo(Item other) { return base.CompareTo(other); }
	}
}
