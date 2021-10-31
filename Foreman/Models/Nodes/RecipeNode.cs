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
		private Item burntOverrideItem; //returns as BurntItem if set (error import)
		internal void SetBurntOverride(Item item) { burntOverrideItem = item; }
		public Item BurntItem { get { return (burntOverrideItem != null)? burntOverrideItem : (BurnerItem != null && BurnerItem.BurnResult != null)? BurnerItem.BurnResult : null; } }
		public Item BurnerItem
		{
			get
			{
				return burnerItem;
			}
			set
			{
				if (burnerItem != value)
				{
					//have to remove any links to the burner/burnt item (if they exist) unless the item is also part of the recipe
					if (BurnerItem != null && !BaseRecipe.IngredientSet.ContainsKey(BurnerItem))
						foreach (NodeLink link in InputLinks.Where(link => link.Item == BurnerItem).ToList())
							link.Delete();
					if (BurntItem != null && !BaseRecipe.ProductSet.ContainsKey(BurntItem))
						foreach (NodeLink link in OutputLinks.Where(link => link.Item == BurntItem).ToList())
							link.Delete();

					burnerItem = value;
					MyGraph.FuelSelector.UseFuel(value);
					burntOverrideItem = null; //updating the burner item will naturally remove any override
				}
			}
		}

		public override string DisplayName { get { return BaseRecipe.FriendlyName; } }

		public RecipeNodePrototype(ProductionGraph graph, int nodeID, Recipe baseRecipe, bool autoPopulate) : base(graph, nodeID)
		{
			BaseRecipe = baseRecipe;

			SelectedBeacon = null;
			BeaconCount = 0;
			BeaconModules = new List<Module>();

			if (autoPopulate) //if not then this is an import -> all the values will be set by the import
			{
				SelectedAssembler = graph.AssemblerSelector.GetAssembler(baseRecipe);
				AssemblerModules = graph.ModuleSelector.GetModules(SelectedAssembler, baseRecipe);
				if (SelectedAssembler != null && SelectedAssembler.IsBurner)
					burnerItem = MyGraph.FuelSelector.GetFuel(SelectedAssembler);
				else
					burnerItem = null;
			}
			else
			{
				SelectedAssembler = null;
				AssemblerModules = new List<Module>();
				burnerItem = null;
				burntOverrideItem = null;
			}
		}

		public override bool IsValid { get { return GetErrors().Count == 0; } }
		public override List<string> GetErrors()
		{
			List<string> output = new List<string>();
			HashSet<Module> missingModules = new HashSet<Module>();

			if (BaseRecipe.IsMissing)
				output.Add(string.Format("Recipe \"{0}\" doesnt exist in preset!", BaseRecipe.FriendlyName));

			if (SelectedAssembler == null)
				output.Add("No assembler exists for this recipe!");
			else
			{
				if (SelectedAssembler.IsMissing)
					output.Add(string.Format("Assembler \"{0}\" doesnt exist in preset!", SelectedAssembler.FriendlyName));

				if (SelectedAssembler.IsBurner && BurnerItem == null)
					output.Add("Burner Assembler has no fuel set!");
				else if(SelectedAssembler.IsBurner && !SelectedAssembler.ValidFuels.Contains(BurnerItem))

				foreach (Module module in AssemblerModules)
					if (module.IsMissing)
						missingModules.Add(module);

				if (AssemblerModules.Count > SelectedAssembler.ModuleSlots)
					output.Add("Assembler has too many modules!");
			}

			if(SelectedBeacon != null)
			{
				if(SelectedBeacon.IsMissing)
					output.Add(string.Format("Beacon \"{0}\" doesnt exist in preset!", SelectedBeacon.FriendlyName));

				foreach (Module module in BeaconModules)
					if (module.IsMissing)
						missingModules.Add(module);

				if (BeaconModules.Count > SelectedBeacon.ModuleSlots)
					output.Add("Beacon has too many modules!");

			}

			return output;
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
			if (item != BurnerItem)
				return BaseRecipe.IngredientSet[item];
			else
			{
				if (SelectedAssembler == null || !SelectedAssembler.IsBurner)
					Trace.Fail(string.Format("input rate requested for {0} fuel while the assembler was either null or not a burner!", item));

				float recipeRate = BaseRecipe.IngredientSet.ContainsKey(item) ? BaseRecipe.IngredientSet[item] : 0;
				//burner rate = recipe time (modified by speed bonus & assembler) * assembler energy consumption (modified by consumption bonus and assembler) / fuel value of the item
				float burnerRate = (BaseRecipe.Time / (SelectedAssembler.Speed * GetSpeedMultiplier())) * (SelectedAssembler.EnergyConsumption * GetConsumptionMultiplier() / SelectedAssembler.EnergyEffectivity) / BurnerItem.FuelValue;
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
				float g = GetSpeedMultiplier();
				float f = GetConsumptionMultiplier();
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
				if (BurnerItem != null)
					info.AddValue("Fuel", BurnerItem.Name);
				if (BurntItem != null)
					info.AddValue("Burnt", BurntItem.Name);
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
