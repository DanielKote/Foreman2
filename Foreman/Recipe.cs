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

	//public class Ingredient
	//{
	//    public String ItemName { get; private set; }
	//    public float Amount { get; private set; }

	//    public Ingredient(String name, float number)
	//    {
	//        ItemName = name;
	//        Amount = number;
	//    }

	//    public override string ToString()
	//    {
	//        return String.Format("Ingredient: {0} ({1})", ItemName, Amount);
	//    }
	//}
}
