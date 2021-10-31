using System;
using System.Collections.Generic;

namespace Foreman
{
    public class Technology : DataObjectBase
    {
        public IReadOnlyCollection<Technology> Prerequisites { get { return prerequisites; } }
		public IReadOnlyCollection<Technology> PostTechs { get { return postTechs; } }
        public IReadOnlyCollection<Recipe> UnlockedRecipes { get { return unlockedRecipes; } }

		private bool enabled = false;
		private bool locked = false;
		public bool Enabled { get { return enabled; } set { enabled = value && !Locked; } }
		public bool Locked { get { return locked; } set { locked = value; if (value) enabled = false; } } //cant be enabled if locked

		private HashSet<Technology> prerequisites;
		private HashSet<Technology> postTechs;
		private HashSet<Recipe> unlockedRecipes;

		public Technology(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName, "-")
		{
			prerequisites = new HashSet<Technology>();
			postTechs = new HashSet<Technology>();
			unlockedRecipes = new HashSet<Recipe>();
		}

		public void AddPrerequisite(Technology tech)
        {
			prerequisites.Add(tech);
			tech.postTechs.Add(this);
        }

		internal void InternalOneWayRemovePrerequisite(Technology tech)
        {
			prerequisites.Remove(tech);
        }

		internal void InternalOneWayRemovePostTech(Technology tech)
        {
			postTechs.Remove(tech);
        }

		internal void InternalOneWayAddRecipe(Recipe recipe) //only called from Recipe
        {
			unlockedRecipes.Add(recipe);
        }

		internal void InternalOneWayRemoveRecipe(Recipe recipe) //only called from Recipe
        {
			unlockedRecipes.Remove(recipe);
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
