using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{

	public abstract class ProductionEntity
	{
		public String Name { get; protected set; }
		public bool Enabled { get; set; }
		public Bitmap Icon { get; set; }
		public int ModuleSlots { get; set; }
		public float Speed { get; set; }
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
	}

	public class Assembler : ProductionEntity
	{
		public List<String> Categories { get; private set; }
		public int MaxIngredients { get; set; }
		public List<string> AllowedEffects { get; private set; }

		public Assembler(String name)
		{
			Enabled = true;
			Name = name;
			Categories = new List<string>();
			AllowedEffects = new List<string>();
		}

		public float GetRate(Recipe recipe)
		{
			return 1 / recipe.Time * Speed;
		}

		public override string ToString()
		{
			return String.Format("Assembler: {0}", Name);
		}
	}

	public class Module
	{
		public bool Enabled { get; set; }
		public float SpeedBonus { get; set; }
		public string Name { get; private set; }
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

		public Module(String name, float speedBonus)
		{
			SpeedBonus = speedBonus;
			Name = name;
		}
	}
}
