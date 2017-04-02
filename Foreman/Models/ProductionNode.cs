using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Foreman
{
	public enum NodeType { Recipe, Supply, Consumer };
	public enum RateType { Auto, Manual };
	
	[Serializable]
	public abstract partial class ProductionNode : ISerializable
	{
		public static readonly int RoundingDP = 4;
		public ProductionGraph Graph { get; protected set; }
		public abstract String DisplayName { get; }
		public abstract IEnumerable<Item> Inputs { get; }
		public abstract IEnumerable<Item> Outputs { get; }
        public double SpeedBonus { get; internal set; }
        public double ProductivityBonus { get; set; }

        public List<NodeLink> InputLinks = new List<NodeLink>();
		public List<NodeLink> OutputLinks = new List<NodeLink>();
		public RateType rateType = RateType.Auto;

        // The rate the solver calculated is appropriate for this node.
        public float actualRate = 0f;

        // If the rateType is manual, this field contains the rate the user desires.
		public float desiredRate = 0f;

        // The calculated rate at which the given item is consumed by this node. This may not match
        // the desired amount!
        public abstract float GetConsumeRate(Item item);

        // The calculated rate at which the given item is consumed by this node. This may not match
        // the desired amount!
        public abstract float GetSupplyRate(Item item);

		protected ProductionNode(ProductionGraph graph)
		{
			Graph = graph;
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

        public virtual float ProductivityMultiplier()
        {
            return (float)(1.0 + ProductivityBonus);
        }

        internal double GetProductivityBonus()
        {
            return 1.5;
        }

        public float GetSuppliedRate(Item item)
        {
            return (float)InputLinks.Where(x => x.Item == item).Sum(x => x.Throughput);
        }

        internal bool OverSupplied(Item item)
        {
            return (Math.Round(GetConsumeRate(item), 2) < Math.Round(GetSuppliedRate(item), 2));
        }

        internal bool ManualRateNotMet()
        {
            // TODO: Hard-coded epsilon is gross :(
            return rateType == RateType.Manual && Math.Abs(actualRate - desiredRate) > 0.0001;
        }
    }

	public partial class RecipeNode : ProductionNode
	{
		public Recipe BaseRecipe { get; private set; }
		public Assembler Assembler { get; set; }

		public abstract class ModuleFilterBase : ISerializable
		{
			public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
			public abstract IEnumerable<MachinePermutation> GetAllPermutations(IEnumerable<Assembler> allowedAssemblers, Recipe recipe);
			public abstract String Name { get; }

			public static ModuleFilterBase Load(JToken token)
			{
				ModuleFilterBase filter = ModuleBestFilter;

				if (token["ModuleFilterType"] != null)
				{
					switch ((String)token["ModuleFilterType"])
					{
						case "Best":
							filter = ModuleBestFilter;
							break;
						case "None":
							filter = ModuleNoneFilter;
							break;
						case "Specific":
							if (token["Module"] != null)
							{
								var moduleKey = (String)token["Module"];
								if (DataCache.Modules.ContainsKey(moduleKey))
								{
									filter = new ModuleSpecificFilter(DataCache.Modules[moduleKey]);
								}
							}
							break;
					}
				}

				return filter;
			}
		}

		public class ModuleSpecificFilter : ModuleFilterBase
		{
			public Module Module { get; set; }

			public ModuleSpecificFilter(Module module)
			{
				this.Module = module;
			}
			
			public override String Name { get { return Module.Name; } }

			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("ModuleFilterType", "Specific");
				info.AddValue("Module", Module.Name);
			}

			public override IEnumerable<MachinePermutation> GetAllPermutations(IEnumerable<Assembler> allowedAssemblers, Recipe recipe)
			{
                Trace.Assert(this.Module.AllowedIn(recipe), "This module not allowed for this recipe. Shouldn't be able to get here.");

				List<MachinePermutation> allowedPermutations = new List<MachinePermutation>();
				foreach (Assembler assembler in allowedAssemblers)
				{
					allowedPermutations.Add(new MachinePermutation(assembler, Enumerable.Repeat(this.Module, assembler.ModuleSlots).ToList()));
				}

				return allowedPermutations.OrderBy(a =>
                {
                    if (Module.ProductivityBonus > Module.SpeedBonus)
                    {
                        return a.GetAssemblerProductivity();
                    } else
                    {
                        return a.GetAssemblerRate(recipe.Time, 0);
                    }
                });
			}
		}

		private class ModuleBestFilterImpl : ModuleFilterBase
		{
			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("ModuleFilterType", "Best");
			}

			public override String Name { get { return "Best"; } }

			public override IEnumerable<MachinePermutation> GetAllPermutations(IEnumerable<Assembler> allowedAssemblers, Recipe recipe)
			{
				List<MachinePermutation> allowedPermutations = new List<MachinePermutation>();
				foreach (Assembler assembler in allowedAssemblers)
				{
					allowedPermutations.AddRange(assembler.GetAllPermutations(recipe));
				}

				return allowedPermutations.OrderBy(a => a.GetAssemblerRate(recipe.Time, 0));
			}
		}

        // TODO: Probably can share with BestFilterImptl
		private class ModuleProductivityFilterImpl : ModuleFilterBase
		{
			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("ModuleFilterType", "Most Productive");
			}

			public override String Name { get { return "Most Productive"; } }

			public override IEnumerable<MachinePermutation> GetAllPermutations(IEnumerable<Assembler> allowedAssemblers, Recipe recipe)
			{
				List<MachinePermutation> allowedPermutations = new List<MachinePermutation>();
				foreach (Assembler assembler in allowedAssemblers)
				{
					allowedPermutations.AddRange(assembler.GetAllPermutations(recipe));
				}

                return allowedPermutations.OrderBy(a => a.GetAssemblerProductivity());
			}
		}

		private class ModuleNoneFilterImpl : ModuleFilterBase
		{
			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("ModuleFilterType", "None");
			}

			public override String Name { get { return "None"; } }

			public override IEnumerable<MachinePermutation> GetAllPermutations(IEnumerable<Assembler> allowedAssemblers, Recipe recipe)
			{
				List<MachinePermutation> allowedPermutations = new List<MachinePermutation>();
				foreach (Assembler assembler in allowedAssemblers)
				{
					allowedPermutations.Add(new MachinePermutation(assembler, new List<Module>()));
				}

				return allowedPermutations;
			}
		}
		public ModuleFilterBase ModuleFilter { get; set; }

		public static ModuleFilterBase ModuleBestFilter { get { return new ModuleBestFilterImpl(); } }
		public static ModuleFilterBase ModuleNoneFilter { get { return new ModuleNoneFilterImpl(); } }
		public static ModuleFilterBase ModuleProductivityFilter { get { return new ModuleProductivityFilterImpl(); } }

		protected RecipeNode(Recipe baseRecipe, ProductionGraph graph)
			: base(graph)
		{
			BaseRecipe = baseRecipe;
			ModuleFilter = ModuleBestFilter;
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

			double requiredRate = actualRate;
			if (requiredRate == double.PositiveInfinity)
			{
				return results;
			}
			requiredRate = Math.Round(requiredRate, RoundingDP);

			List<Assembler> allowedAssemblers;

			if (Assembler != null)
			{
				allowedAssemblers = new List<Assembler>();
				allowedAssemblers.Add(this.Assembler);
			}
			else
			{
				allowedAssemblers = DataCache.Assemblers.Values
					.Where(a => a.Enabled)
					.Where(a => a.Categories.Contains(BaseRecipe.Category))
					.Where(a => a.MaxIngredients >= BaseRecipe.Ingredients.Count).ToList();
			}

            List<MachinePermutation> allowedPermutations = ModuleFilter.GetAllPermutations(allowedAssemblers, BaseRecipe).ToList();

            var sortedPermutations = allowedPermutations.ToList();

            if (sortedPermutations.Any())
            {
                double totalRateSoFar = 0;
                var beaconBonus = (float)SpeedBonus;

                while (totalRateSoFar < requiredRate)
                {
                    double remainingRate = requiredRate - totalRateSoFar;

                    MachinePermutation permutationToAdd = sortedPermutations.LastOrDefault(p => p.GetAssemblerRate(BaseRecipe.Time, beaconBonus) <= remainingRate);

                    if (permutationToAdd != null)
                    {
                        int numberToAdd;
                        Debug.WriteLine(permutationToAdd.GetAssemblerRate(BaseRecipe.Time, beaconBonus));
                        if (Graph.OneAssemblerPerRecipe || Assembler != null)
                        {
                            numberToAdd = Convert.ToInt32(Math.Ceiling(remainingRate / permutationToAdd.GetAssemblerRate(BaseRecipe.Time, beaconBonus)));
                        }
                        else
                        {
                            numberToAdd = Convert.ToInt32(Math.Floor(remainingRate / permutationToAdd.GetAssemblerRate(BaseRecipe.Time, beaconBonus)));
                        }
                        if (!results.ContainsKey(permutationToAdd))
                        {
                            results.Add(permutationToAdd, numberToAdd);
                        }
                        else
                        {
                            results[permutationToAdd] += numberToAdd;
                        }
                    }
                    else
                    {
                        permutationToAdd = sortedPermutations.FirstOrDefault(a => a.GetAssemblerRate(BaseRecipe.Time, beaconBonus) > remainingRate);
                        int amount = Convert.ToInt32(Math.Ceiling(remainingRate / permutationToAdd.GetAssemblerRate(BaseRecipe.Time, beaconBonus)));
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
                        totalRateSoFar += a.Key.GetAssemblerRate(BaseRecipe.Time, beaconBonus) * a.Value;
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
			info.AddValue("SpeedBonus", SpeedBonus);
			info.AddValue("ProductivityBonus", ProductivityBonus);
			info.AddValue("RateType", rateType);
			info.AddValue("ActualRate", actualRate);
            if (rateType == RateType.Manual)
            {
                info.AddValue("DesiredRate", desiredRate);
            }
			if (Assembler != null)
			{
				info.AddValue("Assembler", Assembler.Name);
			}
			ModuleFilter.GetObjectData(info, context);
		}

        public override float GetConsumeRate(Item item)
        {
			if (BaseRecipe.IsMissingRecipe
				|| !BaseRecipe.Ingredients.ContainsKey(item))
			{
				return 0f;
			}

			return (float)Math.Round(BaseRecipe.Ingredients[item] * actualRate, RoundingDP);
        }

        public override float GetSupplyRate(Item item)
        {
			if (BaseRecipe.IsMissingRecipe
				|| !BaseRecipe.Results.ContainsKey(item))
			{
				return 0f;
			}

			return (float)Math.Round(BaseRecipe.Results[item] * actualRate * ProductivityMultiplier(), RoundingDP);
        }

        public override float ProductivityMultiplier()
        {
            var assemblers = this.GetMinimumAssemblers();
            var assemblerBonus = 0.0;
            /*
            if (assemblers.Count == 1)
            {
                var permutation = assemblers.First().Key;
                assemblerBonus = permutation.GetAssemblerProductivity();
            }
            */

            return (float)(1.0 + ProductivityBonus + assemblerBonus);
        }
    }

	public partial class SupplyNode : ProductionNode
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
                // TODO: Find correct recipe to pass in here. Needed to disallow productivity modules.
				allowedPermutations.AddRange(miner.GetAllPermutations(null));
			}

			List<MachinePermutation> sortedPermutations = allowedPermutations.OrderBy(p => p.GetMinerRate(resource)).ToList();

			if (sortedPermutations.Any())
			{
				float requiredRate = GetSupplyRate(SuppliedItem);
				MachinePermutation permutationToAdd = sortedPermutations.LastOrDefault(a => a.GetMinerRate(resource) < requiredRate);
				if (permutationToAdd != null)
				{
					int numberToAdd = Convert.ToInt32(Math.Ceiling(requiredRate / permutationToAdd.GetMinerRate(resource)));
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
            if (rateType == RateType.Manual)
            {
                info.AddValue("DesiredRate", desiredRate);
            }
		}

        public override float GetConsumeRate(Item item)
        {
            Trace.Fail(String.Format("{0} supplier does not consume {1}, nothing should be asking for the rate!", SuppliedItem.FriendlyName, item.FriendlyName));
            return 0;
        }

        public override float GetSupplyRate(Item item)
        {
            if (SuppliedItem != item)
                Trace.Fail(String.Format("{0} supplier does not supply {1}, nothing should be asking for the rate!", SuppliedItem.FriendlyName, item.FriendlyName));

			return (float)Math.Round(actualRate, RoundingDP);
        }
	}

	public partial class ConsumerNode : ProductionNode
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
            if (rateType == RateType.Manual)
            {
                info.AddValue("DesiredRate", desiredRate);
            }
		}

        public override float GetConsumeRate(Item item)
        {
            if (ConsumedItem != item)
                Trace.Fail(String.Format("{0} consumer does not consume {1}, nothing should be asking for the rate!", ConsumedItem.FriendlyName, item.FriendlyName));

			return (float)Math.Round(actualRate, RoundingDP);
        }

        public override float GetSupplyRate(Item item)
        {
            Trace.Fail(String.Format("{0} consumer does not supply {1}, nothing should be asking for the rate!", ConsumedItem.FriendlyName, item.FriendlyName));

            return 0;
        }
	}
}
