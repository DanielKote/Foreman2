using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foreman
{
	public class Recipe
	{
		public String Name { get; private set; }
		public float Time { get; private set; }
		public Dictionary<Item, float> Results { get; set; }
		public Dictionary<Item, float> Ingredients { get; set; }
		public String FriendlyName
		{
			get
			{
				if (DataCache.KnownRecipeNames.ContainsKey(Name))
				{
					return DataCache.KnownRecipeNames[Name];
				}
				else if (Results.Count == 1)
				{
					return Results.Keys.First().FriendlyName;
				}
				else
				{
					return Name;
				}
			}
		}

		public Recipe(String name, float time, Dictionary<Item, float> ingredients, Dictionary<Item, float> results)
		{
			this.Name = name;
			this.Time = time;
			this.Ingredients = ingredients;
			this.Results = results;
		}

		public override int GetHashCode()
		{
			return this.Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj is Recipe)
			{
				return (obj as Recipe).Name == this.Name;
			}

			return false;
		}
	}
}
