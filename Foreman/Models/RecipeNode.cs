using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman
{
	public interface RecipeNode : BaseNode
    {
		Recipe BaseRecipe { get; }
		Assembler SelectedAssembler { get; set; }
		Beacon SelectedBeacon { get; set; }
		List<Module> AssemblerModules { get; }
		List<Module> BeaconModules { get; }
	}


	public class RecipeNodePrototype : BaseNodePrototype, RecipeNode
	{
		public Recipe BaseRecipe { get; private set; }

		public Assembler SelectedAssembler { get; set; }
		public Beacon SelectedBeacon { get; set; }
		public float BeaconCount { get; set; }

		public List<Module> AssemblerModules { get; private set; }
		public List<Module> BeaconModules { get; private set; }

		public override string DisplayName { get { return BaseRecipe.FriendlyName; } }
		public override IEnumerable<Item> Inputs { get { foreach (Item item in BaseRecipe.IngredientList) yield return item; } }
		public override IEnumerable<Item> Outputs { get { foreach (Item item in BaseRecipe.ProductList) yield return item; } }

		public RecipeNodePrototype(ProductionGraph graph, int nodeID, Recipe baseRecipe) : base(graph, nodeID)
		{
			BaseRecipe = baseRecipe;
			SelectedAssembler = null;
			SelectedBeacon = null;
			AssemblerModules = new List<Module>();
			BeaconModules = new List<Module>();
		}

		public override float GetConsumeRate(Item item) { return (float)Math.Round(BaseRecipe.IngredientSet[item] * ActualRate, RoundingDP); }
		public override float GetSupplyRate(Item item) { return (float)Math.Round(BaseRecipe.ProductSet[item] * ActualRate * GetProductivityMultiplier(), RoundingDP); }

		internal override double outputRateFor(Item item) { return BaseRecipe.ProductSet[item]; }
		internal override double inputRateFor(Item item) { return BaseRecipe.IngredientSet[item]; }

		public override float GetSpeedMultiplier()
		{
			float multiplier = 1.0f;
			foreach (Module module in AssemblerModules)
				multiplier += module.SpeedBonus;
			foreach (Module beaconModule in BeaconModules)
				multiplier += beaconModule.SpeedBonus * SelectedBeacon.Effectivity * BeaconCount;
			return multiplier;
		}

		public override float GetProductivityMultiplier()
		{
			float multiplier = 1.0f + (SelectedAssembler == null ? 0 : SelectedAssembler.BaseProductivityBonus);
			foreach (Module module in AssemblerModules)
				multiplier += module.ProductivityBonus;
			foreach (Module beaconModule in BeaconModules)
				multiplier += beaconModule.ProductivityBonus * SelectedBeacon.Effectivity * BeaconCount;
			return multiplier;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", NodeType.Recipe);
			info.AddValue("NodeID:", NodeID);
			info.AddValue("Location", Location);
			info.AddValue("RecipeID", BaseRecipe.RecipeID);
			info.AddValue("RateType", RateType);
			info.AddValue("ActualRate", ActualRate);
			if (RateType == RateType.Manual)
				info.AddValue("DesiredRate", DesiredRate);
			if (SelectedAssembler != null)
			{
				info.AddValue("Assembler", SelectedAssembler.Name);
				info.AddValue("AssemblerModules", AssemblerModules.Select(m => m.Name));
			}
			if (SelectedBeacon != null)
			{
				info.AddValue("Beacon", SelectedBeacon.Name);
				info.AddValue("BeaconCount", BeaconCount);
				info.AddValue("BeaconModules", BeaconModules.Select(m => m.Name));
			}
		}

        public override string ToString() { return string.Format("Recipe node for: {0}", BaseRecipe.Name); }
    }
}
