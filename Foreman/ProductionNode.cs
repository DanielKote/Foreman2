using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Foreman
{
	public enum NodeType { Recipe, Supply, Consumer };
	public enum RateType { Auto, Manual };
	
	[Serializable]
	public abstract class ProductionNode : ISerializable
	{
		public static readonly int RoundingDP = 4;
		public ProductionGraph Graph { get; protected set; }
		public abstract String DisplayName { get; }
		public abstract IEnumerable<Item> Inputs { get; }
		public abstract IEnumerable<Item> Outputs { get; }
		public List<NodeLink> InputLinks = new List<NodeLink>();
		public List<NodeLink> OutputLinks = new List<NodeLink>();
		public abstract float GetExcessOutput(Item item);
		public abstract float GetUnsatisfiedDemand(Item item);
		public abstract float GetTotalOutput(Item item);
		public abstract float GetTotalDemand(Item item);
		public abstract float GetRateLimitedByInputs();
		public abstract float GetRateDemandedByOutputs();
		public RateType rateType = RateType.Auto;
		public float actualRate = 0f;
		public float desiredRate = 0f;

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
				total += link.Throughput;
			}
			return (float)Math.Round(total, RoundingDP);
		}

		public float GetUsedOutput(Item item)
		{
			float total = 0f;
			foreach (NodeLink link in OutputLinks.Where(l => l.Item == item))
			{
				total += link.Throughput;
			}
			return (float)Math.Round(total, RoundingDP);
		}
		
		public float GetUnusedOutput(Item item)
		{
			return GetTotalOutput(item) - GetUsedOutput(item);
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
			return (float)Math.Round(amount, RoundingDP);
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

		public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
	}

	public class RecipeNode : ProductionNode
	{
		public Recipe BaseRecipe { get; private set; }

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

		public override float GetRateLimitedByInputs()
		{
			if (BaseRecipe.IsMissingRecipe) return 0f;

			float total = float.PositiveInfinity;
			foreach (Item inputItem in Inputs)
			{
				total = Math.Min(total, GetTotalInput(inputItem) / BaseRecipe.Ingredients[inputItem]);
			}
			return ValidateRecipeRate(total);
		}

		public override float GetRateDemandedByOutputs()
		{
			if (BaseRecipe.IsMissingRecipe) return 0f;

			float total = 0f;
			foreach (Item outputItem in BaseRecipe.Results.Keys)
			{
				total = Math.Max(total, GetRequiredOutput(outputItem) / BaseRecipe.Results[outputItem]);
			}
			return ValidateRecipeRate(total);
		}

		public override float GetUnsatisfiedDemand(Item item)
		{
			if (BaseRecipe.IsMissingRecipe) return 0f;
			
			float itemRate = (desiredRate * BaseRecipe.Ingredients[item]) - GetTotalInput(item);
			return (float)Math.Round(itemRate, RoundingDP);
		}

		public override float GetExcessOutput(Item item)
		{
			if (BaseRecipe.IsMissingRecipe) return 0f;

			float itemRate = (actualRate * BaseRecipe.Results[item]) - GetUsedOutput(item);
			return (float)Math.Round(itemRate, RoundingDP);
		}

		public override float GetTotalDemand(Item item)
		{
			if (BaseRecipe.IsMissingRecipe
				|| !BaseRecipe.Ingredients.ContainsKey(item))
			{
				return 0f;
			}
						
			return (float)Math.Round(desiredRate * BaseRecipe.Ingredients[item], RoundingDP);
		}

		public override float GetTotalOutput(Item item)
		{
			if (BaseRecipe.IsMissingRecipe
				|| !BaseRecipe.Results.ContainsKey(item))
			{
				return 0f;
			}

			return (float)Math.Round(BaseRecipe.Results[item] * actualRate, RoundingDP);
		}

		//If the graph is showing amounts rather than rates, round up all fractions (because it doesn't make sense to do half a recipe, for example)
		private float ValidateRecipeRate(float amount)
		{
			if (Graph.SelectedAmountType == AmountType.FixedAmount)
			{
				return (float)Math.Ceiling(Math.Round(amount, RoundingDP)); //Subtracting a very small number stops the amount from getting rounded up due to FP errors. It's a bit hacky but it works for now.
			}
			else
			{
				return (float)Math.Round(amount, RoundingDP);
			}
		}

		public Dictionary<MachinePermutation, int> GetMinimumAssemblers()
		{
			var results = new Dictionary<MachinePermutation, int>();

			double requiredRate = GetRateDemandedByOutputs();
			if (requiredRate == double.PositiveInfinity)
			{
				return results;
			}
			requiredRate = Math.Round(requiredRate, RoundingDP);

			List<Assembler> allowedAssemblers = DataCache.Assemblers.Values
				.Where(a => a.Enabled)
				.Where(a => a.Categories.Contains(BaseRecipe.Category))
				.Where(a => a.MaxIngredients >= BaseRecipe.Ingredients.Count).ToList();

			List<MachinePermutation> allowedPermutations = new List<MachinePermutation>();

			foreach (Assembler assembler in allowedAssemblers)
			{
				allowedPermutations.AddRange(assembler.GetAllPermutations());
			}

			var sortedPermutations = allowedPermutations.OrderBy(a => a.GetRate(BaseRecipe.Time)).ToList();

			if (sortedPermutations.Any())
			{
				double totalRateSoFar = 0;

				while (totalRateSoFar < requiredRate)
				{
					double remainingRate = requiredRate - totalRateSoFar;

					MachinePermutation permutationToAdd = sortedPermutations.LastOrDefault(p => p.GetRate(BaseRecipe.Time) <= remainingRate);

					if (permutationToAdd != null)
					{
						int numberToAdd;
						if (Graph.OneAssemblerPerRecipe)
						{
							numberToAdd = Convert.ToInt32(Math.Ceiling(remainingRate / permutationToAdd.GetRate(BaseRecipe.Time)));
						}
						else
						{
							numberToAdd = Convert.ToInt32(Math.Floor(remainingRate / permutationToAdd.GetRate(BaseRecipe.Time)));
						}
						results.Add(permutationToAdd, numberToAdd);
					}
					else
					{
						permutationToAdd = sortedPermutations.FirstOrDefault(a => a.GetRate(BaseRecipe.Time) > remainingRate);
						int amount = Convert.ToInt32(Math.Ceiling(remainingRate / permutationToAdd.GetRate(BaseRecipe.Time)));
						if (results.ContainsKey(permutationToAdd))
						{
							results[permutationToAdd] += amount;
						}
						else
						{
							results.Add(permutationToAdd, amount);
						}
					}
					totalRateSoFar = 0;
					foreach (var a in results)
					{
						totalRateSoFar += a.Key.GetRate(BaseRecipe.Time) * a.Value;
					}
					totalRateSoFar = Math.Round(totalRateSoFar, RoundingDP);
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

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", "Recipe");
			info.AddValue("RecipeName", BaseRecipe.Name);
			info.AddValue("RateType", rateType);
			info.AddValue("ActualRate", actualRate);
		}
	}

	public class SupplyNode : ProductionNode
	{
		public Item SuppliedItem { get; private set; }

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

		public override float GetRateLimitedByInputs()
		{
			return actualRate;
		}

		public override float GetRateDemandedByOutputs()
		{
			float total = 0f;
			foreach (NodeLink link in OutputLinks)
			{
				total += link.Demand;
			}
			return total;
		}

		public override float GetUnsatisfiedDemand(Item item)
		{
			return 0f;
		}

		public override float GetExcessOutput(Item item)
		{
			if (rateType == RateType.Auto)
			{
				return 0f;
			}
			else
			{
				float excessSupply = actualRate;
				foreach (NodeLink link in OutputLinks.Where(l => l.Item == item))
				{
					excessSupply -= link.Throughput;
				}
				return (float)Math.Round(excessSupply, RoundingDP);
			}
		}

		public override float GetTotalDemand(Item item)
		{
			return 0f;
		}

		public override float GetTotalOutput(Item item)
		{
			if (SuppliedItem != item)
			{
				return 0f;
			}
			else
			{
				return (float)Math.Round(actualRate, RoundingDP);
			}
		}

		public override string DisplayName
		{
			get { return SuppliedItem.FriendlyName; }
		}

		public Dictionary<MachinePermutation, int> GetMinimumMiners()
		{
			Dictionary<MachinePermutation, int> results = new Dictionary<MachinePermutation, int>();

			Resource resource = DataCache.Resources.Values.FirstOrDefault(r => r.result == SuppliedItem.Name);
			if (resource == null)
			{
				return results;
			}

			List<Miner> allowedMiners = DataCache.Miners.Values
				.Where(m => m.Enabled)
				.Where(m => m.ResourceCategories.Contains(resource.Category)).ToList();

			List<MachinePermutation> allowedPermutations = new List<MachinePermutation>();
			foreach (Miner miner in allowedMiners)
			{
				allowedPermutations.AddRange(miner.GetAllPermutations());
			}

			List<MachinePermutation> sortedPermutations = allowedPermutations.OrderBy(p => p.GetRate(resource.Time)).ToList();

			if (sortedPermutations.Any())
			{
				float requiredRate = GetRequiredOutput(SuppliedItem);
				MachinePermutation permutationToAdd = sortedPermutations.LastOrDefault(a => a.GetRate(resource.Time) < requiredRate);
				if (permutationToAdd != null)
				{
					int numberToAdd = Convert.ToInt32(Math.Ceiling(requiredRate / permutationToAdd.GetRate(resource.Time)));
					results.Add(permutationToAdd, numberToAdd);
				}
			}

			return results;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", "Supply");
			info.AddValue("ItemName", SuppliedItem.Name);
			info.AddValue("RateType", rateType);
			info.AddValue("ActualRate", actualRate);
		}
	}

	public class ConsumerNode : ProductionNode
	{
		public Item ConsumedItem { get; private set; }

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

		protected ConsumerNode(Item item, ProductionGraph graph)
			: base(graph)
		{
			ConsumedItem = item;
			rateType = RateType.Manual;
			actualRate = 1f;
		}

		public override float GetRateLimitedByInputs()
		{
			return GetTotalInput(ConsumedItem);
		}

		public override float GetRateDemandedByOutputs()
		{
			return 0f;
		}

		public override float GetUnsatisfiedDemand(Item item)
		{
			return (float)Math.Round(desiredRate - GetTotalInput(item), RoundingDP);
		}

		public override float GetExcessOutput(Item item)
		{
			return 0;
		}

		public override float GetTotalDemand(Item item)
		{
			if (ConsumedItem != item)
			{
				return 0f;
			}
			else
			{
				return (float)Math.Round(desiredRate, RoundingDP);
			}
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

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", "Consumer");
			info.AddValue("ItemName", ConsumedItem.Name);
			info.AddValue("RateType", rateType);
			info.AddValue("ActualRate", actualRate);
		}
	}
}
