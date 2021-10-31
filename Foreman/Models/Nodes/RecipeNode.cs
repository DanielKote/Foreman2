using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman
{
	public interface RecipeNode : BaseNode
    {
		Recipe BaseRecipe { get; }

		Assembler SelectedAssembler { get; set; }
		Beacon SelectedBeacon { get; set; }
		float BeaconCount { get; set; }

		List<Module> AssemblerModules { get; }
		List<Module> BeaconModules { get; }

		Item BurnerItem { get; set; }
		Item BurntItem { get; }
	}


	public class RecipeNodePrototype : BaseNodePrototype, RecipeNode
	{
		public Recipe BaseRecipe { get; private set; }

		public Assembler SelectedAssembler { get; set; }
		public Beacon SelectedBeacon { get; set; }
		public float BeaconCount { get; set; }

		public List<Module> AssemblerModules { get; private set; }
		public List<Module> BeaconModules { get; private set; }

		private Item burnerItem;
		private List<Item> burnerItemPriority;
		public Item BurnerItem { get { return burnerItem; } set { burnerItem = value; if (value != null) { burnerItemPriority.Remove(value); burnerItemPriority.Add(value); } } }
		public Item BurntItem { get { if (BurnerItem != null && BurnerItem.BurnResult != null) return BurnerItem.BurnResult; else return null; } }

		public override string DisplayName { get { return BaseRecipe.FriendlyName; } }

		public RecipeNodePrototype(ProductionGraph graph, int nodeID, Recipe baseRecipe) : base(graph, nodeID)
		{
			BaseRecipe = baseRecipe;
			SelectedAssembler = graph.AssemblerSelector.GetAssembler(baseRecipe);
			AssemblerModules = graph.ModuleSelector.GetModules(SelectedAssembler, baseRecipe);
			if (SelectedAssembler != null && SelectedAssembler.IsBurner)
			{
				BurnerItem = burnerItemPriority.LastOrDefault(item => item.FuelsAssemblers.Contains(SelectedAssembler));
				if (BurnerItem == null)
					BurnerItem = SelectedAssembler.ValidFuels.FirstOrDefault();
			}
			else
				BurnerItem = null;

			SelectedBeacon = null;
			BeaconCount = 0;
			BeaconModules = new List<Module>();
			burnerItemPriority = new List<Item>();
		}

		public override IEnumerable<Item> Inputs
		{
			get
			{
				foreach (Item item in BaseRecipe.IngredientList)
					yield return item;
				if (BurnerItem != null && !BaseRecipe.IngredientSet.ContainsKey(BurnerItem)) //provide the burner item if it isnt null or already part of recipe ingredients
					yield return BurnerItem;
			}
		}
		public override IEnumerable<Item> Outputs
		{
			get
			{
				foreach (Item item in BaseRecipe.ProductList)
					yield return item;
				if (BurntItem != null && !BaseRecipe.ProductSet.ContainsKey(BurntItem)) //provide the burnt remains item if it isnt null or already part of recipe products
					yield return BurntItem;
			}
		}

		public override float GetConsumeRate(Item item) { return (float)Math.Round(inputRateFor(item) * ActualRate, RoundingDP); }
		public override float GetSupplyRate(Item item) { return (float)Math.Round(outputRateFor(item) * ActualRate, RoundingDP); }

		internal override double inputRateFor(Item item)
		{
			if(item != BurnerItem)
				return BaseRecipe.IngredientSet[item];
			else
            {
				if (SelectedAssembler == null || !SelectedAssembler.IsBurner)
					Trace.Fail(string.Format("input rate requested for {0} fuel while the assembler was either null or not a burner!", item));

				float recipeRate = BaseRecipe.IngredientSet.ContainsKey(item) ? BaseRecipe.IngredientSet[item] : 0;
				//burner rate = recipe time (modified by speed bonus & assembler) * assembler energy consumption (modified by consumption bonus and assembler) / fuel value of the item
				float burnerRate = (BaseRecipe.Time / (SelectedAssembler.Speed * GetSpeedMultiplier())) * (90000 * GetConsumptionMultiplier() / SelectedAssembler.EnergyEffectivity) / BurnerItem.FuelValue;
				return (float)Math.Round((recipeRate + burnerRate), RoundingDP);
			}
		}
		internal override double outputRateFor(Item item) //Note to Foreman 1.0: YES! this is where all the productivity is taken care of! (not in the solver... why would you multiply the productivity while setting up the constraints and not during the ratios here???)
		{
			if (item != BurntItem)
				return BaseRecipe.ProductSet[item];
			else
			{
				if (SelectedAssembler == null || !SelectedAssembler.IsBurner)
					Trace.Fail(string.Format("input rate requested for {0} fuel while the assembler was either null or not a burner!", item));

				float recipeRate = BaseRecipe.ProductSet.ContainsKey(item) ? BaseRecipe.ProductSet[item] : 0;
				//burner rate is much the same as above, just have to make sure we still use the Burner item!
				float burnerRate = (BaseRecipe.Time / (SelectedAssembler.Speed * GetSpeedMultiplier())) * (SelectedAssembler.EnergyConsumption * GetConsumptionMultiplier() / SelectedAssembler.EnergyEffectivity) / BurnerItem.FuelValue;
				return (float)Math.Round((recipeRate + burnerRate), RoundingDP);
			}
		}

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

        public override float GetConsumptionMultiplier()
        {
			float multiplier = 1.0f;
			foreach (Module module in AssemblerModules)
				multiplier += module.ConsumptionBonus;
			foreach (Module beaconModule in BeaconModules)
				multiplier += beaconModule.ConsumptionBonus * SelectedBeacon.Effectivity * BeaconCount;
			return multiplier > 0.2f ? multiplier : 0.2f;
		}

        public override float GetPollutionMultiplier()
        {
			float multiplier = 1.0f;
			foreach (Module module in AssemblerModules)
				multiplier += module.PollutionBonus;
			foreach (Module beaconModule in BeaconModules)
				multiplier += beaconModule.PollutionBonus * SelectedBeacon.Effectivity * BeaconCount;
			return multiplier;
		}

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", NodeType.Recipe);
			info.AddValue("NodeID", NodeID);
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
