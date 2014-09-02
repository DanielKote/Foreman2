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

		public bool CanUltimatelyTakeFrom(ProductionNode node)
		{
			Queue<ProductionNode> Q = new Queue<ProductionNode>();
			HashSet<ProductionNode> V = new HashSet<ProductionNode>();

			V.Add(this);
			Q.Enqueue(this);

			while (Q.Any())
			{
				ProductionNode t = Q.Dequeue();
				if (t == node)
				{
					return true;
				}
				foreach (NodeLink e in t.InputLinks)
				{
					ProductionNode u = e.Supplier;
					if (!V.Contains(u))
					{
						V.Add(u);
						Q.Enqueue(u);
					}
				}
			}
			return false;
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
			return ValidateRecipeRate(rate);
		}

		public float GetRateRequiredByOutputs()
		{
			float rate = 0;
			foreach (Item outputItem in BaseRecipe.Results.Keys)
			{
				rate = Math.Max(rate, GetRequiredOutput(outputItem) / BaseRecipe.Results[outputItem]);
			}
			return ValidateRecipeRate(rate);
		}

		public override float GetUnsatisfiedDemand(Item item)
		{
			float rate = Math.Min(CompletionAmountLimit, GetRateRequiredByOutputs());
			float itemRate = (rate * BaseRecipe.Ingredients[item]) - GetTotalInput(item);
			return itemRate;
		}

		public override float GetExcessOutput(Item item)
		{
			float rate = Math.Min(CompletionAmountLimit, GetRateAllowedByInputs());
			float itemRate = (rate * BaseRecipe.Results[item]) - GetUsedOutput(item);
			return itemRate;
		}

		public override float GetRequiredInput(Item item)
		{
			float rate = Math.Min(CompletionAmountLimit, GetRateRequiredByOutputs());
			return rate * BaseRecipe.Ingredients[item];
		}

        public override float GetTotalOutput(Item item)
        {
            float rate = GetRateAllowedByInputs();
            return BaseRecipe.Results[item] * rate;
        }

		//If the graph is showing amounts rather than rates, round up all fractions (because it doesn't make sense to require half an item, for example)
		private float ValidateRecipeRate(float amount)
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

		public Dictionary<ProductionEntity, int> GetMinimumAssemblers()
		{
			Dictionary<ProductionEntity, int> results = new Dictionary<ProductionEntity, int>();

			double requiredRate = GetRateRequiredByOutputs();
			List<Assembler> sortedAssemblers = DataCache.Assemblers.Values
				.Where(a => a.Enabled)
				.Where(a => a.Categories.Contains(BaseRecipe.Category))
				.Where(a => a.MaxIngredients >= BaseRecipe.Ingredients.Count)
				.OrderBy(a => a.GetRate(BaseRecipe)).ToList();

			if (sortedAssemblers.Any())
			{
				double totalRateSoFar = 0;

				while (totalRateSoFar < requiredRate)
				{
					double remainingRate = requiredRate - totalRateSoFar;
					Assembler assemblerToAdd = sortedAssemblers.LastOrDefault(a => a.GetRate(BaseRecipe) <= remainingRate);

					if (assemblerToAdd != null)
					{
						int numberToAdd;
						if (Graph.OneAssemblerPerRecipe)
						{
							numberToAdd = Convert.ToInt32(Math.Ceiling(remainingRate / assemblerToAdd.GetRate(BaseRecipe)));
						}
						else
						{
							numberToAdd = Convert.ToInt32(Math.Floor(remainingRate / assemblerToAdd.GetRate(BaseRecipe)));
						}
						results.Add(assemblerToAdd, numberToAdd);
					}
					else
					{
						assemblerToAdd = sortedAssemblers.FirstOrDefault(a => a.GetRate(BaseRecipe) > remainingRate);
						int amount = Convert.ToInt32(Math.Ceiling(remainingRate / assemblerToAdd.GetRate(BaseRecipe)));
						if (results.ContainsKey(assemblerToAdd))
						{
							results[assemblerToAdd] += amount;
						}
						else
						{
							results.Add(assemblerToAdd, amount);
						}
					}
					totalRateSoFar = 0;
					foreach (var a in results)
					{
						totalRateSoFar += (a.Key as Assembler).GetRate(BaseRecipe) * a.Value;
					}
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
		
		public Dictionary<ProductionEntity, int> GetMinimumMiners()
		{
			Dictionary<ProductionEntity, int> results = new Dictionary<ProductionEntity, int>();

			Resource resource = DataCache.Resources.Values.FirstOrDefault(r => r.result == SuppliedItem.Name);
			if (resource == null)
			{
				return results;
			}

			List<Miner> sortedMiners = DataCache.Miners.Values
				.Where(m => m.Enabled)
				.Where(m => m.ResourceCategories.Contains(resource.Category))
				.OrderBy(m => m.GetRate(resource)).ToList();

			if (sortedMiners.Any())
			{
				float requiredRate = GetRequiredOutput(SuppliedItem);
				Miner minerToAdd = sortedMiners.LastOrDefault(a => a.GetRate(resource) < requiredRate);
				if (minerToAdd != null)
				{
					int numberToAdd = Convert.ToInt32(Math.Ceiling(requiredRate / minerToAdd.GetRate(resource)));
					results.Add(minerToAdd, numberToAdd);
				}
			}

			return results;
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