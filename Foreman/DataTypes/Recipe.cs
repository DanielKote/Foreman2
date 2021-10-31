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
			Ingredients = recipe.IngredientsSet.Keys.Select(item => item.Name).ToList();
			Results = recipe.ResultsSet.Keys.Select(item => item.Name).ToList();
		}
	}

	public class Recipe : DataObjectBase
	{
		public static string[] recipeLocaleCategories = { "recipe-name" };

		public Subgroup MySubgroup { get; protected set; }

		public float Time { get; set; }
		public string Category { get; set; }

		public IReadOnlyDictionary<Item, float> ResultsSet { get { return resultsSet; } }
		public IReadOnlyDictionary<Item, float> IngredientsSet { get { return ingredientsSet; } }
		public IReadOnlyList<Item> ResultsList { get { return resultsList; } }
		public IReadOnlyList<Item> IngredientsList { get { return ingredientsList; } }

		private Dictionary<Item, float> resultsSet;
		private List<Item> resultsList;
		private Dictionary<Item, float> ingredientsSet;
		private List<Item> ingredientsList;

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
					if (ResultsSet.Count == 1)
						base.Icon = ResultsSet.Keys.First().Icon;
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
			this.ingredientsSet = new Dictionary<Item, float>();
			this.resultsSet = new Dictionary<Item, float>();
			this.ingredientsList = new List<Item>();
			this.resultsList = new List<Item>();
			this.HasEnabledAssemblers = false;

			this.Hidden = false;
			this.IsAvailableAtStart = false;
			this.IsCyclic = false;
		}

		public void AddIngredient(Item item, float quantity)
        {
			if (ingredientsSet.ContainsKey(item))
				ingredientsSet[item] += quantity;
			else
            {
				ingredientsSet.Add(item, quantity);
				ingredientsList.Add(item);
				item.InternalAddConsumptionRecipe(this);
            }
        }

		public bool DeleteIngredient(Item item)
        {
			if (!ingredientsSet.ContainsKey(item))
				return false;
			ingredientsSet.Remove(item);
			ingredientsList.Remove(item);
			item.InternalRemoveConsumptionRecipe(this);
			return true;
        }

		public void AddResult(Item item, float quantity)
		{
			if (resultsSet.ContainsKey(item))
				resultsSet[item] += quantity;
			else
			{
				resultsSet.Add(item, quantity);
				resultsList.Add(item);
				item.InternalAddProductionRecipe(this);
			}
		}

		public bool DeleteResult(Item item)
		{
			if (!resultsSet.ContainsKey(item))
				return false;
			resultsSet.Remove(item);
			resultsList.Remove(item);
			item.InternalRemoveProductionRecipe(this);
			return true;
		}

		public override string ToString() { return String.Format("Recipe: {0}", Name); }
	}
}
