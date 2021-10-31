using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace Foreman
{
    public class Technology
    {
        public String Name { get; private set; }
        public string LName { get; set; }
        public HashSet<Technology> Prerequisites { get; private set; }
        public HashSet<Recipe> Recipes { get; private set; }

		private bool enabled = false;
		private bool locked = false;
		public bool Enabled { get { return enabled; } set { enabled = value && !Locked; } }
		public bool Locked { get { return locked; } set { locked = value; if (value) enabled = false; } } //cant be enabled if locked

		public Bitmap Icon { get; set; }

		private Technology()
		{
			Name = "";
		}

		public Technology(String name)
		{
			Name = name;
			Prerequisites = new HashSet<Technology>();
			Recipes = new HashSet<Recipe>();
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Technology))
			{
				return false;
			}
			return this == (Technology)obj;
		}

		public static bool operator ==(Technology item1, Technology item2)
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

		public static bool operator !=(Technology item1, Technology item2)
		{
			return !(item1 == item2);
		}

		public override string ToString()
		{
			return String.Format("Technology: {0}", Name);
		}

	}
}
