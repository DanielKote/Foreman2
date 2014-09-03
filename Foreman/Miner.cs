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

		public float GetRate(Resource resource)
		{
			//According to http://www.factorioforums.com/wiki/index.php?title=Mining_drill
			return (MiningPower - resource.Hardness) * Speed / resource.Time;
		}
	}
}