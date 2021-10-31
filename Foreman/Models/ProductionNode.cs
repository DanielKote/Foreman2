using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
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
			//supply & consume > 1 ---> allow for 0.1% error
			//supply & consume [0.001 -> 1]  ---> allow for 2% error
			//supply & consume [0 ->0.001] ---> allow for any errors (as long as neither are 0)
			//supply & consume = 0 ---> no errors if both are exactly 0

			float consumeRate = GetConsumeRate(item);
			float supplyRate = GetSuppliedRate(item);
			if ((consumeRate == 0 && supplyRate == 0) || (supplyRate < 0.001 && supplyRate < 0.001))
				return false;
			return ((supplyRate - consumeRate) / supplyRate) > ((consumeRate > 1 && supplyRate > 1) ? 0.001f : 0.05f);
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
        // If null, best available assembler should be used, using speed (not number of module slots)
        // as a proxy since "best" can have varying definitions.
		public Assembler Assembler { get; set; }

        public ModuleSelector NodeModules { get; set; }

        internal Dictionary<MachinePermutation, int> GetAssemblers()
        {
            var assembler = Assembler;

            if (assembler == null)
			{
                assembler = DataCache.Assemblers.Values
                    .Where(a => a.Enabled)
                    .Where(a => a.Categories.Contains(BaseRecipe.Category))
                    .Where(a => a.MaxIngredients >= BaseRecipe.Ingredients.Count)
                    .OrderBy(a => -a.Speed)
                    .FirstOrDefault();
			}

            var ret = new Dictionary<MachinePermutation, int>();

            if (assembler != null) {
                var modules = NodeModules.For(BaseRecipe, assembler.ModuleSlots);
                var required = (int)Math.Ceiling(actualRate / assembler.GetRate(BaseRecipe.Time, (float)SpeedBonus, modules));
                ret.Add(new MachinePermutation(assembler, modules.ToList()), required);
            }

            return ret;
        }

		protected RecipeNode(Recipe baseRecipe, ProductionGraph graph)
			: base(graph)
		{
			BaseRecipe = baseRecipe;
			NodeModules = graph.defaultModuleSelector ?? ModuleSelector.None;
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
                info.AddValue("DesiredRate", desiredRate);
			if (Assembler != null)
				info.AddValue("Assembler", Assembler.Name);
			NodeModules.GetObjectData(info, context);
		}

		public override float GetConsumeRate(Item item)
        {
			if (BaseRecipe.IsMissingRecipe || !BaseRecipe.Ingredients.ContainsKey(item))
				return 0f;
			return (float)Math.Round(BaseRecipe.Ingredients[item] * actualRate, RoundingDP);
        }

        public override float GetSupplyRate(Item item)
        {
			if (BaseRecipe.IsMissingRecipe || !BaseRecipe.Results.ContainsKey(item))
				return 0f;
			return (float)Math.Round(BaseRecipe.Results[item] * actualRate * ProductivityMultiplier(), RoundingDP);
        }

        internal override double outputRateFor(Item item)
        {
			return BaseRecipe.Results[item];
        }

        internal override double inputRateFor(Item item)
        {
			return BaseRecipe.Ingredients[item];
        }

        public override float ProductivityMultiplier()
        {
            var assemblerBonus = GetAssemblers().Keys.Sum(x => x.GetAssemblerProductivity());
            return (float)(1.0 + ProductivityBonus + assemblerBonus);
        }
    }

    public class ErrorNode : ProductionNode
    {
        public override string DisplayName { get { return "ERROR NODE"; } }
        public override IEnumerable<Item> Inputs { get { return new List<Item>(); } }
        public override IEnumerable<Item> Outputs { get { return new List<Item>(); } }
		public override float GetConsumeRate(Item item) { Trace.Fail(String.Format("Error node not consume {0}, nothing should be asking for the rate!", item.FriendlyName)); return 0; }
        public override float GetSupplyRate(Item item) { Trace.Fail(String.Format("Error node not suppy {0}, nothing should be asking for the rate!", item.FriendlyName)); return 0; }
        internal override double inputRateFor(Item item) { throw new ArgumentException("Supply node should not have any inputs!"); }
		internal override double outputRateFor(Item item) { throw new ArgumentException("Supply node should not have any outputs!"); }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
			info.AddValue("NodeType", "ERROR");
		}
		protected ErrorNode(ProductionGraph graph) : base(graph) { }

		public static ErrorNode Create(ProductionGraph graph)
		{
			ErrorNode node = new ErrorNode(graph);
			node.Graph.Nodes.Add(node);
			node.Graph.InvalidateCaches();
			return node;
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

        internal override double outputRateFor(Item item)
        {
            return 1;
        }

        internal override double inputRateFor(Item item)
        {
            throw new ArgumentException("Supply node should not have any inputs!");
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

        internal override double outputRateFor(Item item)
        {
            throw new ArgumentException("Consumer should not have outputs!");
        }

        internal override double inputRateFor(Item item)
        {
            return 1;
        }
    }

	public class PassthroughNode : ProductionNode
	{
        public Item PassedItem;

        protected PassthroughNode(Item item, ProductionGraph graph)
			: base(graph)
		{
			this.PassedItem = item;
		}

		public override IEnumerable<Item> Inputs
		{
            get { return Enumerable.Repeat(PassedItem, 1); }
		}

		public override IEnumerable<Item> Outputs
		{
            get { return Inputs; }
		}

		public static PassthroughNode Create(Item item, ProductionGraph graph)
		{
			PassthroughNode node = new PassthroughNode(item, graph);
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

		public override string DisplayName
		{
			get { return PassedItem.FriendlyName; }
		}

		public override string ToString()
		{
			return String.Format("Pass-through Tree Node: {0}", PassedItem.Name);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", "PassThrough");
			info.AddValue("ItemName", PassedItem.Name);
			info.AddValue("RateType", rateType);
			info.AddValue("ActualRate", actualRate);
            if (rateType == RateType.Manual)
            {
                info.AddValue("DesiredRate", desiredRate);
            }
		}

        public override float GetConsumeRate(Item item)
        {
			return (float)Math.Round(actualRate, RoundingDP);
        }

        public override float GetSupplyRate(Item item)
        {
			return (float)Math.Round(actualRate, RoundingDP);
        }

        internal override double outputRateFor(Item item)
        {
            return 1;
        }

        internal override double inputRateFor(Item item)
        {
            return 1;
        }

        public override float ProductivityMultiplier()
        {
            return 1;
        }
    }
}
