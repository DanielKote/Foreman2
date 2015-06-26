using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	public class MachinePermutation
	{
		public ProductionEntity assembler;
		public List<Module> modules;

		public float GetRate(float timeDivisor)
		{
			float speed = assembler.Speed;

			foreach (Module module in modules.OfType<Module>())
			{
				speed += module.SpeedBonus * assembler.Speed;
			}

			return 1 / timeDivisor * speed;
		}

		public MachinePermutation(ProductionEntity machine, ICollection<Module> modules)
		{
			assembler = machine;
			this.modules = modules.ToList();
		}
	}

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

		public float GetRate(float timeDivisor)
		{
			return 1 / timeDivisor * Speed;
		}

		public IEnumerable<MachinePermutation> GetAllPermutations()
		{
			yield return new MachinePermutation(this, new List<Module>());

			Module[] currentModules = new Module[ModuleSlots];

			if (ModuleSlots <= 0)
			{
				yield break;
			}

			foreach (Module module in DataCache.Modules.Values.Where(m => m.Enabled))
			{
				for (int i = 0; i < ModuleSlots; i++)
				{
					currentModules[i] = module;
					yield return new MachinePermutation(this, currentModules);
				}
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

		public override string ToString()
		{
			return String.Format("Assembler: {0}", Name);
		}
	}

	public class Module
	{
		public Bitmap Icon
		{
			get
			{
				//For each module there should be a corresponding item with the icon already loaded.
				return DataCache.Items[Name].Icon;
			}
		}
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
			Enabled = true;
		}
	}
}
