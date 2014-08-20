using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foreman
{
	public abstract class ProductionNode
	{
		public ProductionGraph Graph { get; protected set; }
		public abstract String DisplayName { get; }
		public abstract IEnumerable<Item> Inputs { get; }
		public abstract IEnumerable<Item> Outputs { get; }
		public List<NodeLink> InputLinks = new List<NodeLink>();
		public List<NodeLink> OutputLinks = new List<NodeLink>();
		public abstract float GetExcessSupply(Item item);
		public abstract float GetExcessDemand(Item item);
		public abstract Dictionary<Item, float> GetExcessSupply();
		public abstract Dictionary<Item, float> GetExcessDemand();
		public abstract void MinimiseInputs();

		protected ProductionNode(ProductionGraph graph)
		{
			Graph = graph;
		}

		public bool TakesFrom(ProductionNode node)
		{
			return node.OutputLinks.Any(l => l.Consumer == this);
		}

		public bool GivesTo(ProductionNode node)
		{
			return node.InputLinks.Any(l => l.Supplier == this);
		}

		public float GetTotalInput(Item item)
		{
			float total = 0f;
			foreach (NodeLink link in InputLinks.Where(l => l.Item == item))
			{
				total += link.Amount;
			}
			return total;
		}

		public float GetTotalOutput(Item item)
		{
			float total = 0f;
			foreach (NodeLink link in OutputLinks.Where(l => l.Item == item))
			{
				total += link.Amount;
			}
			return total;
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
		public float CompletionAmountLimit = float.PositiveInfinity;

		protected RecipeNode(Recipe baseRecipe, ProductionGraph graph)
			: base(graph)
		{
			BaseRecipe = baseRecipe;
		}

		public override IEnumerable<Item> Inputs
		{
			get
			{
				foreach (Item item in BaseRecipe.Ingredients.Keys)
				{
					yield return item;
				}
			}
		}

		public override IEnumerable<Item> Outputs
		{
			get
			{
				foreach (Item item in BaseRecipe.Results.Keys)
				{
					yield return item;
				}
			}
		}

		public static RecipeNode Create(Recipe baseRecipe, ProductionGraph graph)
		{
			RecipeNode node = new RecipeNode(baseRecipe, graph);
			node.Graph.Nodes.Add(node);
			node.Graph.InvalidateCaches();
			return node;
		}

		public float GetRateAllowedByInputs()
		{
			float rate = 0;
			foreach (Item inputItem in BaseRecipe.Ingredients.Keys)
			{
				rate = Math.Max(rate, GetTotalInput(inputItem) / BaseRecipe.Ingredients[inputItem]);
			}
			return rate;
		}

		public float GetRateRequiredByOutputs()
		{
			float rate = 0;
			foreach (Item outputItem in BaseRecipe.Results.Keys)
			{
				rate += Math.Max(rate, GetTotalOutput(outputItem) / BaseRecipe.Results[outputItem]);
			}
			return rate;
		}

		public override void MinimiseInputs()
		{
			float rate = GetRateRequiredByOutputs();

			foreach (NodeLink link in InputLinks)
			{
				link.Amount = ValidateItemAmount(rate * BaseRecipe.Ingredients[link.Item]);
			}
		}

		public override float GetExcessDemand(Item item)
		{
			float rate = Math.Min(CompletionAmountLimit, GetRateRequiredByOutputs());
			float itemRate = ValidateItemAmount(rate * BaseRecipe.Ingredients[item]) - GetTotalInput(item);
			return itemRate;
		}

		public override float GetExcessSupply(Item item)
		{
			float rate = Math.Min(CompletionAmountLimit, GetRateAllowedByInputs());
			float itemRate = ValidateItemAmount(rate * BaseRecipe.Results[item]) - GetTotalOutput(item);
			return itemRate;
		}

		private float ValidateItemAmount(float amount)
		{
			if (Graph.SelectedAmountType == AmountType.FixedAmount)
			{
				return (float)Math.Ceiling(amount - 0.00001f); //Subtracting a very small number stops the amount from getting rounded up due to FP errors. It's a bit hacky but it works for now.
			}
			else
			{
				return amount;
			}
		}

		public override Dictionary<Item, float> GetExcessDemand()
		{
			Dictionary<Item, float> excessDemand = new Dictionary<Item, float>();
			foreach (Item inputItem in BaseRecipe.Ingredients.Keys)
			{
				excessDemand.Add(inputItem, GetExcessDemand(inputItem));
			}
			return excessDemand;
		}

		public override Dictionary<Item, float> GetExcessSupply()
		{
			Dictionary<Item, float> excessSupply = new Dictionary<Item, float>();
			foreach (Item outputItem in BaseRecipe.Results.Keys)
			{
				excessSupply.Add(outputItem, GetExcessSupply(outputItem));
			}
			return excessSupply;
		}

		public override string DisplayName
		{
			get { return BaseRecipe.Name; }
		}
				
		public override string ToString()
		{
			return String.Format("Recipe Tree Node: {0}", BaseRecipe.Name);
		}
	}

	public class SupplyNode : ProductionNode
	{
		public Item SuppliedItem { get; private set; }
		public float SupplyAmount = float.PositiveInfinity;

		protected SupplyNode(Item item, ProductionGraph graph)
			: base(graph)
		{
			SuppliedItem = item;
		}

		public override IEnumerable<Item> Inputs
		{
			get { return new List<Item>(); }
		}

		public override IEnumerable<Item> Outputs
		{
			get { yield return SuppliedItem; }
		}

		public static SupplyNode Create(Item item, ProductionGraph graph)
		{
			SupplyNode node = new SupplyNode(item, graph);
			node.Graph.Nodes.Add(node);
			node.Graph.InvalidateCaches();
			return node;
		}

		public override float GetExcessDemand(Item item)
		{
			return 0f;
		}

		public override float GetExcessSupply(Item item)
		{
			float excessSupply = SupplyAmount;
			foreach (NodeLink link in OutputLinks.Where(l => l.Item == item))
			{
				excessSupply -= link.Amount;
			}
			return excessSupply;
		}

		public override Dictionary<Item, float> GetExcessDemand()
		{
			return new Dictionary<Item, float>();
		}

		public override Dictionary<Item, float> GetExcessSupply()
		{
			Dictionary<Item, float> demands = new Dictionary<Item, float>();
			demands.Add(SuppliedItem, GetExcessDemand(SuppliedItem));
			return demands;
		}

		public override void MinimiseInputs()
		{
			//No inputs to minimise.
		}

		public override string DisplayName
		{
			get { return SuppliedItem.Name; }
		}
	}

	public class ConsumerNode : ProductionNode
	{
		public Item ConsumedItem { get; private set; }
		public float ConsumptionAmount = 1f;

		public override string DisplayName
		{
			get { return ConsumedItem.Name; }
		}

		public override IEnumerable<Item> Inputs
		{
			get { yield return ConsumedItem; }
		}

		public override IEnumerable<Item> Outputs
		{
			get { return new List<Item>(); }
		}

		protected ConsumerNode(Item item, ProductionGraph graph) : base(graph)
		{
			ConsumedItem = item;
		}

		public override Dictionary<Item, float> GetExcessDemand()
		{
			Dictionary<Item, float> demands = new Dictionary<Item, float>();
			demands.Add(ConsumedItem, GetExcessDemand(ConsumedItem));
			return demands;
		}

		public override Dictionary<Item, float> GetExcessSupply()
		{
			return new Dictionary<Item, float>();
		}

		public override float GetExcessDemand(Item item)
		{
			float excessDemand = ConsumptionAmount;
			foreach (NodeLink link in InputLinks.Where(l => l.Item == item))
			{
				excessDemand -= link.Amount;
			}
			return excessDemand;
		}

		public override float GetExcessSupply(Item item)
		{
			return 0;
		}

		public override void MinimiseInputs()
		{
			foreach (NodeLink link in InputLinks)
			{
				link.Amount = ConsumptionAmount;
			}
		}

		public static ConsumerNode Create(Item item, ProductionGraph graph)
		{
			ConsumerNode node = new ConsumerNode(item, graph);
			node.Graph.Nodes.Add(node);
			node.Graph.InvalidateCaches();
			return node;
		}
	}
}
