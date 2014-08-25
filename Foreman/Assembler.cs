using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	public class Assembler
	{
		public String Name { get; private set; }
		public Bitmap Icon { get; set; }
		public List<String> Categories { get; private set; }
		public float Speed { get; set; }
		public int MaxIngredients { get; set; }
		public int ModuleSlots { get; set; }
		public List<string> AllowedEffects { get; private set; }
		private String friendlyName;
		public String FriendlyName
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(friendlyName))
				{
					return friendlyName;
				}
				else
				{
					return Name;
				}
			}
			set
			{
				friendlyName = value;
			}
		}

		public Assembler(String name)
		{
			Name = name;
			Categories = new List<string>();
			AllowedEffects = new List<string>();
		}

		public float GetRate(Recipe recipe)
		{
			return 1 / recipe.Time * (1 + Speed);
		}

		public override string ToString()
		{
			return String.Format("Assembler: {0}", Name);
		}
	}
}
