using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	public class Recipe
	{
		public String Name { get; private set; }
		public float Time { get; private set; }
		public String Category { get; set; }
		public Dictionary<Item, float> Results { get; private set; }
		public Dictionary<Item, float> Ingredients { get; private set; }
		public Boolean IsMissingRecipe = false;
		private Bitmap uniqueIcon = null;
		public Bitmap Icon
		{
			get
			{
				if (uniqueIcon != null)
				{
					return uniqueIcon;
				}
				else if (Results.Count == 1)
				{
					return Results.Keys.First().Icon;
				}
				else
				{
					return DataCache.UnknownIcon;
				}
			}
			set
			{
				uniqueIcon = value;
			}
		}
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
			if (!(obj is Recipe))
			{
				return false;
			}
			
			return (obj as Recipe) == this;
		}

		public static bool operator ==(Recipe recipe1, Recipe recipe2)
		{
			if (object.ReferenceEquals(recipe1, recipe2))
			{
				return true;
			}

			if ((object)recipe1 == null || (object)recipe2 == null)
			{
				return false;
			}

			return recipe1.Name == recipe2.Name;
		}

		public static bool operator !=(Recipe recipe1, Recipe recipe2)
		{
			return !(recipe1 == recipe2);
		}
	}
}
