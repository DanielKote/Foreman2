using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace Foreman
{
	public interface Resource : DataObjectBase
    {
		IReadOnlyCollection<Item> ResultingItems { get; }
		IReadOnlyCollection<Miner> ValidMiners { get; }
		float Time { get; }
	}

	public interface Miner : ProductionEntity
	{
		IReadOnlyCollection<Resource> MineableResources { get; }
		float MiningSpeed { get; }
		Item AssociatedItem { get; }
	}

	public class ResourcePrototype : DataObjectBasePrototype, Resource
	{
		public IReadOnlyCollection<Item> ResultingItems { get { return resultingItems; } }
		public IReadOnlyCollection<Miner> ValidMiners { get { return validMiners; } }

		public float Time { get; internal set;}

		internal HashSet<MinerPrototype> validMiners { get; private set; }
		internal HashSet<ItemPrototype> resultingItems { get; private set; }

		public ResourcePrototype(DataCache dCache, string name) : base(dCache, name, name, "-")
		{
			validMiners = new HashSet<MinerPrototype>();
			resultingItems = new HashSet<ItemPrototype>();
		}
	}

	public class MinerPrototype : ProductionEntityPrototype, Miner
	{
		public IReadOnlyCollection<Resource> MineableResources { get { return mineableResources; } }
		public float MiningSpeed { get; set; }
		public Item AssociatedItem { get { return myCache.Items[Name]; } }

		internal HashSet<ResourcePrototype> mineableResources;

		public MinerPrototype(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName)
		{
			mineableResources = new HashSet<ResourcePrototype>();
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