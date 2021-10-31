using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Foreman
{
	public class RecipeShort
	{
		public string Name;
		public List<string> Ingredients;
		public List<string> Results;

		public RecipeShort(Recipe recipe)
		{
			Name = recipe.Name;
			Ingredients = recipe.Ingredients.Keys.Select(item => item.Name).ToList();
			Results = recipe.Results.Keys.Select(item => item.Name).ToList();
		}
	}

	public class Recipe : DataObjectBase
	{
		public static string[] recipeLocaleCategories = { "recipe-name" };

		public Subgroup MySubgroup { get; internal set; }

		public float Time { get; set; }
		public string Category { get; set; }
		public Dictionary<Item, float> Results { get; private set; }
		public Dictionary<Item, float> Ingredients { get; private set; }
		public bool IsAvailableAtStart { get; set; }
		public bool IsCyclic { get; set; }
		public bool IsMissingRecipe { get { return DataCache.MissingRecipes.ContainsKey(Name); } }

		public bool Hidden { get; set; }

		public bool HasEnabledAssemblers { get; set; }

		public new Bitmap Icon
		{
			get
			{
				if (base.Icon == null)
				{
					if (Results.Count == 1)
						base.Icon = Results.Keys.First().Icon;
					else
						base.Icon = DataCache.UnknownIcon;
				}
				return base.Icon;
			}
			set { base.Icon = value; }
		}

		public Recipe(string name, string lname, Subgroup subgroup, string order) : base(name, lname, order)
		{
			localeCategories = recipeLocaleCategories;

			MySubgroup = subgroup;
			MySubgroup.Recipes.Add(this);

			this.Time = 0.5f;
			this.Ingredients = new Dictionary<Item, float>();
			this.Results = new Dictionary<Item, float>();
			this.HasEnabledAssemblers = false;

			this.Hidden = false;
			this.IsAvailableAtStart = false;
			this.IsCyclic = false;
		}

		public override string ToString() { return String.Format("Recipe: {0}", Name); }
	}
}
