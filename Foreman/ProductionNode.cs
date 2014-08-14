using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foreman
{
	public abstract class ProductionNode
	{
		public ProductionGraph Graph { get; protected set; }
		public abstract Dictionary<Item, float> Inputs { get; }
		public abstract Dictionary<Item, float> Outputs { get; }
		public abstract float OutputRate(Item item);
		public abstract float InputRate(Item item);
		public abstract void AddOutput(Item item, float rate);

		public ProductionNode(ProductionGraph graph)
		{
			Graph = graph;
		}

		public bool CanTakeFrom(ProductionNode node)
		{
			foreach (Item item in Inputs.Keys)
			{
				if (node.OutputRate(item) > 0.0f)
				{
					return true;
				}
			}
			return false;
		}

		public bool CanUltimatelyTakeFrom(ProductionNode node) // Breadth-first search would probably be a better algorithm.
		{
			int thisIndex = Graph.Nodes.IndexOf(this);	//I should somehow cache this index in the graph so I don't have to do a linear search each time.
			int otherIndex = Graph.Nodes.IndexOf(node);

			return (Graph.PathMatrix[otherIndex, thisIndex] > 0) ;
		}
	}

	public class RecipeNode : ProductionNode
	{
		public Recipe BaseRecipe { get; private set; }
		public float Rate { get; set; }

		public RecipeNode(Recipe baseRecipe, ProductionGraph graph)
			: base(graph)
		{
			BaseRecipe = baseRecipe;
		}

		public override Dictionary<Item, float> Outputs
		{
			get
			{
				var dict = new Dictionary<Item, float>();
				foreach (Item item in BaseRecipe.Results.Keys)
				{
					dict.Add(item, BaseRecipe.Results[item] * Rate);
				}
				return dict;
			}
		}

		public override Dictionary<Item, float> Inputs
		{
			get
			{
				var dict = new Dictionary<Item, float>();
				foreach (Item item in BaseRecipe.Ingredients.Keys)
				{
					dict.Add(item, BaseRecipe.Ingredients[item] * Rate);
				}
				return dict;
			}
		}

		public override void AddOutput(Item item, float rate)
		{
			if (BaseRecipe.Results.ContainsKey(item))
			{
				Rate += rate / BaseRecipe.Results[item];
			}
			else
			{
				throw new InvalidOperationException(String.Format("Tried make a RecipeNode ({0}) output an item ({1}) that the recipe doesn't produce", BaseRecipe.Name, item.Name));
			}
		}

		public override float InputRate(Item item)
		{
			if (BaseRecipe.Ingredients.ContainsKey(item))
			{
				return BaseRecipe.Ingredients[item] * Rate;
			}
			else
			{
				return 0.0f;
			}
		}

		public override float OutputRate(Item item)
		{
			if (BaseRecipe.Results.ContainsKey(item))
			{
				return BaseRecipe.Results[item] * Rate;
			}
			else
			{
				return 0.0f;
			}
		}

		public override string ToString()
		{
			return String.Format("Recipe Tree Node: {0}", BaseRecipe.Name);
		}
	}

	// For items that can are created outside the production network
	public class SupplyNode : ProductionNode
	{
		public Item SuppliedItem;
		public float SupplyRate;

		public SupplyNode(Item item, float rate, ProductionGraph graph)
			: base(graph)
		{
			SuppliedItem = item;
			SupplyRate = rate;
		}

		public override Dictionary<Item, float> Outputs
		{
			get
			{
				var dict = new Dictionary<Item, float>();
				dict.Add(SuppliedItem, SupplyRate);
				return dict;
			}
		}

		public override Dictionary<Item, float> Inputs
		{
			get
			{
				return new Dictionary<Item, float>();
			}
		}

		public override void AddOutput(Item item, float rate)
		{
			if (item == SuppliedItem)
			{
				SupplyRate += rate;
			}
			else
			{
				throw new InvalidOperationException(String.Format("Tried to add {0} to a supply node that can only output {1}.", item.Name, SuppliedItem.Name));
			}
		}

		public override float InputRate(Item item)
		{
			return 0.0f;
		}

		public override float OutputRate(Item item)
		{
			if (item == SuppliedItem)
			{
				return SupplyRate;
			}
			else
			{
				return 0.0f;
			}
		}
	}

	public class ConsumerNode : ProductionNode
	{
		public Item ConsumedItem { get; private set; }
		public float ConsumptionRate { get; set; }

		public ConsumerNode(Item item, float rate, ProductionGraph graph) : base(graph)
		{
			ConsumedItem = item;
			ConsumptionRate = rate;
		}

		public override Dictionary<Item, float> Inputs
		{
			get
			{
				var dict = new Dictionary<Item, float>();
				dict.Add(ConsumedItem, ConsumptionRate);
				return dict;
			}
		}
		public override Dictionary<Item, float> Outputs
		{
			get
			{
				return new Dictionary<Item, float>();
			}
		}

		public override float InputRate(Item item)
		{
			if (item == ConsumedItem)
			{
				return ConsumptionRate;
			}
			else
			{
				return 0.0f;
			}
		}

		public override float OutputRate(Item item)
		{
			return 0.0f;
		}

		public override void AddOutput(Item item, float rate)
		{
			if (item == ConsumedItem)
			{
				ConsumptionRate += rate;
			}
			else
			{
				throw new InvalidOperationException(String.Format("Tried to add output for item {0} to a node that only takes inputs", item.Name));
			}
		}
	}
}
