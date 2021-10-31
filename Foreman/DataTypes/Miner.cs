using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	public class Resource
	{
		public String Name { get; private set; }
		public String Category { get; set; }
		public float Hardness { get; set;}
		public float Time { get; set;}
		public String result { get; set;}

		public Resource(String name)
		{
			Name = name;
			Hardness = 0.5f;
		}
	}

	public class Miner : ProductionEntity
	{
		public List<String> ResourceCategories { get; private set; }
		public float MiningPower { get; set; }

		public Miner(String name)
		{
			Name = name;
			ResourceCategories = new List<string>();
			Enabled = true;
		}

		public float GetRate(Resource resource, IEnumerable<Module> modules)
		{
			double finalSpeed = this.Speed;
			foreach (Module module in modules.Where(m => m != null))
			{
				finalSpeed += module.SpeedBonus * this.Speed;
			}

			//According to https://wiki.factorio.com/Mining
			double timeForOneItem = resource.Time / finalSpeed;

			timeForOneItem = Math.Ceiling(timeForOneItem * 60d) / 60d;   //Round up to the nearest tick, since mining can't start until the start of a new tick

			return (float)(1d / timeForOneItem);
		}
	}
}