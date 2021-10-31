using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace Foreman
{
	public class Resource : DataObjectBase
	{
		public IReadOnlyCollection<Item> ResultingItems { get { return resultingItems; } }
		public IReadOnlyCollection<Miner> ValidMiners { get { return validMiners; } }

		public string Category { get; set; }

		public float Hardness { get; set;}
		public float Time { get; set;}

		private HashSet<Miner> validMiners;
		private HashSet<Item> resultingItems;

		public Resource(DataCache dCache, string name) : base(dCache, name, name, "-")
		{
			Hardness = 0.5f;
			validMiners = new HashSet<Miner>();
			resultingItems = new HashSet<Item>();
		}

		public void AddResult(Item item)
        {
			resultingItems.Add(item);
			item.InternalOneWayAddMiningResource(this);
		}

		internal void InternalOneWayRemoveResult(Item item) //only from delete calls
        {
			resultingItems.Remove(item);
			item.InternalOneWayRemoveMiningResource(this);
        }

		public void AddMiner(Miner miner)
        {
			validMiners.Add(miner);
			miner.InternalOneWayAddResource(this);
        }
		internal void InternalOneWayRemoveMiner(Miner miner) //only from delete calls
		{
			validMiners.Remove(miner);
        }
	}

	public class Miner : ProductionEntity
	{
		public IReadOnlyCollection<Resource> MineableResources { get { return mineableResources; } }
		public float MiningPower { get; set; }
		public Item AssociatedItem { get { return myCache.Items[Name]; } }

		private HashSet<Resource> mineableResources;

		public Miner(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName)
		{
			mineableResources = new HashSet<Resource>();
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

		internal void InternalOneWayAddResource(Resource resource) //only called from Resource
        {
			mineableResources.Add(resource);
        }
		internal void InternalOneWayRemoveResource(Resource resource) //only from delete calls
		{
			mineableResources.Remove(resource);
        }
	}
}