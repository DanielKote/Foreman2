using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foreman
{
	public enum NodeType { Recipe, Supply, Consumer };

	public abstract class ProductionNode
	{
		public ProductionGraph Graph { get; protected set; }
		public abstract String DisplayName { get; }
		public abstract IEnumerable<Item> Inputs { get; }
		public abstract IEnumerable<Item> Outputs { get; }
		public List<NodeLink> InputLinks = new List<NodeLink>();
		public List<NodeLink> OutputLinks = new List<NodeLink>();
		public abstract float GetExcessOutput(Item item);
		public abstract float GetUnsatisfiedDemand(Item item);
		public abstract float GetTotalOutput(Item item);
		public abstract float GetRequiredInput(Item item);

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
		
		public float GetUsedOutput(Item item)
		{
			float total = 0f;
			foreach (NodeLink link in OutputLinks.Where(l => l.Item == item))
			{
				total += link.Amount;
			}
			return total;
		}

		public float GetRequiredOutput(Item item)
		{
			float amount = 0;
			foreach (NodeLink link in OutputLinks)
			{
				if (link.Item == item)
				{
					amount += link.Demand;
				}
			}
			return amount;
		}

		public bool CanUltimatelyTakeFrom(ProductionNode node) // Breadth-first search would probably be a better algorithm.
		{
			int thisIndex = Graph.Nodes.IndexOf(this);	//I should somehow cache this index in the graph so I don't have to do a linear search each time.
			int otherIndex = Graph.Nodes.IndexOf(node);

			return (Graph.PathMatrix[otherIndex, thisIndex] > 0) ;
		}

		public void Destroy()
		{
			foreach (NodeLink link in InputLinks.ToList().Union(OutputLinks.ToList()))
			{
				link.Destroy();
			}
			Graph.Nodes.Remove(this);
			Graph.InvalidateCaches();
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
			float rate = float.PositiveInfinity;
			foreach (Item inputItem in BaseRecipe.Ingredients.Keys)
			{
				rate = Math.Min(rate, GetTotalInput(inputItem) / BaseRecipe.Ingredients[inputItem]);
			}
			return rate;
		}

		public float GetRateRequiredByOutputs()
		{
			float rate = 0;
			foreach (Item outputItem in BaseRecipe.Results.Keys)
			{
				rate = Math.Max(rate, GetRequiredOutput(outputItem) / BaseRecipe.Results[outputItem]);
			}
			return rate;
		}

		public override float GetUnsatisfiedDemand(Item item)
		{
			float rate = Math.Min(CompletionAmountLimit, GetRateRequiredByOutputs());
			float itemRate = ValidateItemAmount(rate * BaseRecipe.Ingredients[item]) - GetTotalInput(item);
			return itemRate;
		}

		public override float GetExcessOutput(Item item)
		{
			float rate = Math.Min(CompletionAmountLimit, GetRateAllowedByInputs());
			float itemRate = ValidateItemAmount(rate * BaseRecipe.Results[item]) - GetUsedOutput(item);
			return itemRate;
		}

		public override float GetRequiredInput(Item item)
		{
			float rate = Math.Min(CompletionAmountLimit, GetRateRequiredByOutputs());
			return ValidateItemAmount(rate * BaseRecipe.Ingredients[item]);
		}

        public override float GetTotalOutput(Item item)
        {
            float rate = GetRateAllowedByInputs();
            return BaseRecipe.Results[item] * rate;
        }

		//If the graph is showing amounts rather than rates, round up all fractions (because it doesn't make sense to require half an item, for example)
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

		public Dictionary<Assembler, int> GetMinimumAssemblers(OptimisationGoal goal)
		{
			Dictionary<Assembler, int> results = new Dictionary<Assembler, int>();
			
			if (goal == OptimisationGoal.Count)
			{
				float requiredRate = GetRateRequiredByOutputs();
				List<Assembler> sortedAssemblers = DataCache.Assemblers.Values.OrderBy(a => a.GetRate(BaseRecipe)).Reverse().ToList();

				float totalRateSoFar = 0;
				foreach (Assembler assembler in sortedAssemblers)
				{
					float thisRate = assembler.GetRate(BaseRecipe);
					results.Add(assembler, Convert.ToInt32(Math.Ceiling(requiredRate / thisRate)));
					break;
				}
			}

			return results;
		}

		public override string DisplayName
		{
			get { return BaseRecipe.FriendlyName; }
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

		public override float GetUnsatisfiedDemand(Item item)
		{
			return 0f;
		}

		public override float GetExcessOutput(Item item)
		{
			float excessSupply = SupplyAmount;
			foreach (NodeLink link in OutputLinks.Where(l => l.Item == item))
			{
				excessSupply -= link.Amount;
			}
			return excessSupply;
		}

		public override float GetRequiredInput(Item item)
		{
			return 0f;
		}

		public override float GetTotalOutput(Item item)
		{
			return SupplyAmount;
		}

		public override string DisplayName
		{
			get { return SuppliedItem.FriendlyName; }
		}
	}

	public class ConsumerNode : ProductionNode
	{
		public Item ConsumedItem { get; private set; }
		public float ConsumptionAmount = 1f;

		public override string DisplayName
		{
			get { return ConsumedItem.FriendlyName; }
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

		public override float GetUnsatisfiedDemand(Item item)
		{
			float excessDemand = ConsumptionAmount;
			foreach (NodeLink link in InputLinks.Where(l => l.Item == item))
			{
				excessDemand -= link.Amount;
			}
			return excessDemand;
		}

		public override float GetExcessOutput(Item item)
		{
			return 0;
		}

		public override float GetRequiredInput(Item item)
		{
			return ConsumptionAmount;
		}

		public override float GetTotalOutput(Item item)
		{
			return 0;
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