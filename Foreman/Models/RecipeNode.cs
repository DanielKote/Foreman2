using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman
{
	public partial class RecipeNode : ProductionNode
	{
		public Recipe BaseRecipe { get; private set; }
		// If null, best available assembler should be used, using speed (not number of module slots)
		// as a proxy since "best" can have varying definitions.
		public Assembler Assembler { get; set; }

		public ModuleSelector NodeModules { get; set; }


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
				foreach (Item item in BaseRecipe.IngredientSet.Keys)
					yield return item;
			}
		}

		public override IEnumerable<Item> Outputs
		{
			get
			{
				foreach (Item item in BaseRecipe.ProductSet.Keys)
					yield return item;
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
			info.AddValue("RecipeID", BaseRecipe.RecipeID);
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
			if (BaseRecipe.IsMissingRecipe || !BaseRecipe.IngredientSet.ContainsKey(item))
				return 0f;
			return (float)Math.Round(BaseRecipe.IngredientSet[item] * actualRate, RoundingDP);
		}

		public override float GetSupplyRate(Item item)
		{
			if (BaseRecipe.IsMissingRecipe || !BaseRecipe.ProductSet.ContainsKey(item))
				return 0f;
			return (float)Math.Round(BaseRecipe.ProductSet[item] * actualRate * ProductivityMultiplier(), RoundingDP);
		}

		internal override double outputRateFor(Item item)
		{
			return BaseRecipe.ProductSet[item];
		}

		internal override double inputRateFor(Item item)
		{
			return BaseRecipe.IngredientSet[item];
		}

		public override float ProductivityMultiplier()
		{
			var assemblerBonus = 0; // = GetAssemblers().Keys.Sum(x => x.GetAssemblerProductivity());
			return (float)(1.0 + ProductivityBonus + assemblerBonus);
		}
	}
}
