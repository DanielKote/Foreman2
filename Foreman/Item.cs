using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	public class Item
	{
		public String Name { get; private set; }
		public HashSet<Recipe> Recipes { get; private set; }
		public Bitmap Icon { get; set; }

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
			if (obj == null)
			{
				return false;
			}

			if (obj is Item)
			{
				return (obj as Item).Name == Name;
			}

			return false;
		}
	}
}
