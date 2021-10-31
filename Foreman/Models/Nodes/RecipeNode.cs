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

		Assembler SelectedAssembler { get;}
		Beacon SelectedBeacon { get;}
		float BeaconCount { get; set; }

		IReadOnlyList<Module> AssemblerModules { get; }
		IReadOnlyList<Module> BeaconModules { get; }

		Item Fuel { get; }
		Item FuelRemains { get; }

		//both of these work with the base time unit (1:1, in this case meaning 1sec) -> so when getting/setting the values they need to be multiplied/divided by the rate conversion factor (ex: by 60 if in minutes)
		float GetBaseNumberOfAssemblers();
		void SetBaseNumberOfAssemblers(float num); //can be used to set the 'rate' of the recipe based on # of assemblers instead of the 'number of recipe results?' that setting the ActualRate would do.

		void SetAssembler(Assembler assembler);
		void AutoSetAssembler();

		void AddAssemblerModule(Module module);
		void RemoveAssemblerModule(int index);
		void SetAssemblerModules(IEnumerable<Module> modules);
		void AutoSetAssemblerModules();

		void SetBeacon(Beacon beacon);
		void AddBeaconModule(Module module);
		void RemoveBeaconModule(int index);
		void SetBeaconModules(IEnumerable<Module> modules);

		void SetFuel(Item fuel);
		void AutoSetFuel();
	}


	public class RecipeNodePrototype : BaseNodePrototype, RecipeNode
	{
		public Recipe BaseRecipe { get; private set; }

		public Assembler SelectedAssembler { get; private set; }
		public Beacon SelectedBeacon { get; private set; }
		public float BeaconCount { get; set; }

		private List<Module> assemblerModules;
		private List<Module> beaconModules;

		public IReadOnlyList<Module> AssemblerModules { get { return assemblerModules; } }
		public IReadOnlyList<Module> BeaconModules { get { return beaconModules; } }

		public Item Fuel { get; private set; }
		public Item FuelRemains { get { return (fuelRemainsOverride != null)? fuelRemainsOverride : (Fuel != null && Fuel.BurnResult != null)? Fuel.BurnResult : null; } }
		internal void SetBurntOverride(Item item){ if(Fuel == null || Fuel.BurnResult != item) fuelRemainsOverride = item; }
		private Item fuelRemainsOverride; //returns as BurntItem if set (error import)

		public override string DisplayName { get { return BaseRecipe.FriendlyName; } }

		public RecipeNodePrototype(ProductionGraph graph, int nodeID, Recipe baseRecipe, bool autoPopulate) : base(graph, nodeID)
		{
			BaseRecipe = baseRecipe;

			SelectedBeacon = null;
			SelectedAssembler = null;
			BeaconCount = 0;
			beaconModules = new List<Module>();
			assemblerModules = new List<Module>();

			if (autoPopulate) //if not then this is an import -> all the values will be set by the import
			{
				AutoSetAssembler();
				AutoSetAssemblerModules();
			}
			else
			{
				SetFuel(null);
			}
		}

		public void SetAssembler(Assembler assembler)
		{
			SelectedAssembler = assembler;

			//check for invalid modules
			for (int i = assemblerModules.Count - 1; i >= 0; i--)
				if (assemblerModules[i].IsMissing || !SelectedAssembler.Modules.Contains(assemblerModules[i]) || !BaseRecipe.Modules.Contains(assemblerModules[i]))
					assemblerModules.RemoveAt(i);
			//check for too many modules
			while (assemblerModules.Count > SelectedAssembler.ModuleSlots)
				assemblerModules.RemoveAt(assemblerModules.Count - 1);
			//check if any modules work (if none work, then turn off beacon)
			if (SelectedAssembler.Modules.Count == 0 || BaseRecipe.Modules.Count == 0)
				SetBeacon(null);
			else //update beacon
				SetBeacon(SelectedBeacon);

			UpdateState();
		}

		public void AutoSetAssembler()
		{
			SetAssembler(MyGraph.AssemblerSelector.GetAssembler(BaseRecipe));
			SetFuel(MyGraph.FuelSelector.GetFuel(SelectedAssembler));
		}

		public void SetBeacon(Beacon beacon)
		{
			SelectedBeacon = beacon;

			if (SelectedBeacon == null)
			{
				beaconModules.Clear();
			}
			else
			{
				//check for invalid modules
				for (int i = beaconModules.Count - 1; i >= 0; i--)
					if (beaconModules[i].IsMissing || !SelectedAssembler.Modules.Contains(beaconModules[i]) || !BaseRecipe.Modules.Contains(beaconModules[i]) || !SelectedBeacon.ValidModules.Contains(beaconModules[i]))
						beaconModules.RemoveAt(i);
				//check for too many modules
				while (beaconModules.Count > SelectedBeacon.ModuleSlots)
					beaconModules.RemoveAt(beaconModules.Count - 1);
			}
			UpdateState();
		}

		public void AddAssemblerModule(Module module) { assemblerModules.Add(module); UpdateState(); }

		public void RemoveAssemblerModule(int index) { if (index >= 0 && index < assemblerModules.Count) assemblerModules.RemoveAt(index); UpdateState(); }

		public void SetAssemblerModules(IEnumerable<Module> modules)
		{
			assemblerModules.Clear();
			if(modules != null)
				foreach (Module module in modules)
					assemblerModules.Add(module);
			UpdateState();
		}

		public void AutoSetAssemblerModules()
		{
			assemblerModules = MyGraph.ModuleSelector.GetModules(SelectedAssembler, BaseRecipe);
			UpdateState();
		}

		public void AddBeaconModule(Module module) { beaconModules.Add(module); UpdateState(); }

		public void RemoveBeaconModule(int index) { if (index >= 0 && index < beaconModules.Count) beaconModules.RemoveAt(index); UpdateState(); }

		public void SetBeaconModules(IEnumerable<Module> modules)
		{
			beaconModules.Clear();
			if(modules != null)
				foreach (Module module in modules)
					beaconModules.Add(module);
			UpdateState();
		}

		public void SetFuel(Item fuel)
		{
			if (Fuel != fuel || fuelRemainsOverride != null)
			{
				//have to remove any links to the burner/burnt item (if they exist) unless the item is also part of the recipe
				if (Fuel != null && !BaseRecipe.IngredientSet.ContainsKey(Fuel))
					foreach (NodeLink link in InputLinks.Where(link => link.Item == Fuel).ToList())
						link.Delete();
				if (FuelRemains != null && !BaseRecipe.ProductSet.ContainsKey(FuelRemains))
					foreach (NodeLink link in OutputLinks.Where(link => link.Item == FuelRemains).ToList())
						link.Delete();

				Fuel = fuel;
				MyGraph.FuelSelector.UseFuel(fuel);
				fuelRemainsOverride = null; //updating the fuel item will naturally remove any override
				UpdateState();
			}
		}

		public void AutoSetFuel()
		{
			SetFuel(MyGraph.FuelSelector.GetFuel(SelectedAssembler));
		}

		public override void UpdateState() { State = GetUpdatedState(); }

		private NodeState GetUpdatedState()
		{
			//error states:
			if (BaseRecipe.IsMissing)
				return NodeState.Error;
			if (SelectedAssembler == null || SelectedAssembler.IsMissing)
				return NodeState.Error;
			if (SelectedAssembler.IsBurner && Fuel == null)
				return NodeState.Error;
			if (SelectedAssembler.IsBurner && Fuel != null && (Fuel.IsMissing || !SelectedAssembler.Fuels.Contains(Fuel) || Fuel.BurnResult != FuelRemains))
				return NodeState.Error;
			if (assemblerModules.FirstOrDefault(m => m.IsMissing) != null)
				return NodeState.Error;
			if (assemblerModules.Count > SelectedAssembler.ModuleSlots)
				return NodeState.Error;
			if (SelectedBeacon != null && SelectedBeacon.IsMissing)
				return NodeState.Error;
			if (SelectedBeacon != null && beaconModules.FirstOrDefault(m => m.IsMissing) != null)
				return NodeState.Error;
			if (SelectedBeacon != null && beaconModules.Count > SelectedBeacon.ModuleSlots)
				return NodeState.Error;
			if (!AllLinksValid)
				return NodeState.Error;

			//warning states (either not enabled or not available both throw up warnings)
			if (!BaseRecipe.Enabled || !BaseRecipe.Available)
				return NodeState.Warning;
			if (!SelectedAssembler.Enabled || !SelectedAssembler.Available)
				return NodeState.Warning;
			if (Fuel != null && (!Fuel.Available || Fuel.ProductionRecipes.FirstOrDefault(r => r.Enabled && r.Assemblers.FirstOrDefault(a => a.Enabled) != null) == null))
				return NodeState.Warning;
			if (assemblerModules.FirstOrDefault(m => !m.Enabled || !m.Available) != null)
				return NodeState.Warning;
			if (SelectedBeacon != null && (!SelectedBeacon.Available || !SelectedBeacon.Enabled || (beaconModules.FirstOrDefault(m => !m.Enabled || !m.Available) != null)))
				return NodeState.Warning;

			return NodeState.Clean;
		}

		public override List<string> GetErrors()
		{
			List<string> output = new List<string>();

			if (BaseRecipe.IsMissing)
			{
				output.Add(string.Format("> Recipe \"{0}\" doesnt exist in preset!", BaseRecipe.FriendlyName));
				return output;
			}

			if (SelectedAssembler == null)
				output.Add("> No assembler exists for this recipe!");
			else
			{
				if (BaseRecipe.Assemblers.Count == 0)
					output.Add("No valid assemblers exist for this recipe!");
				if (SelectedAssembler.IsMissing)
					output.Add(string.Format("> Assembler \"{0}\" doesnt exist in preset!", SelectedAssembler.FriendlyName));
				if (SelectedAssembler.IsBurner)
				{
					if (Fuel == null && SelectedAssembler.Fuels.Count > 0) //if the assembler has no valid fuels then we will just have to give it a pass as a warning :/
						output.Add("> Burner Assembler has no fuel set!");
					else if (!SelectedAssembler.Fuels.Contains(Fuel))
						output.Add("> Burner Assembler has an invalid fuel set!");
					else if (Fuel.IsMissing)
						output.Add("> Burner Assembler's fuel doesnt exist in preset!");

					if (fuelRemainsOverride != null)
						output.Add("> Burning result doesnt match fuel's burn result!");
				}

				if (AssemblerModules.Where(m => m.IsMissing).FirstOrDefault() != null)
					output.Add("> Some of the assembler modules dont exist in preset!");

				if (AssemblerModules.Count > SelectedAssembler.ModuleSlots)
					output.Add(string.Format("> Assembler has too many modules ({0}/{1})!", AssemblerModules.Count, SelectedAssembler.ModuleSlots));
			}

			if(SelectedBeacon != null)
			{
				if (SelectedBeacon.IsMissing)
					output.Add(string.Format("> Beacon \"{0}\" doesnt exist in preset!", SelectedBeacon.FriendlyName));

				if (BeaconModules.Where(m => m.IsMissing).FirstOrDefault() != null)
					output.Add("> Some of the beacon modules dont exist in preset!");

				if (BeaconModules.Count > SelectedBeacon.ModuleSlots)
					output.Add("> Beacon has too many modules!");
			}

			if (!AllLinksValid)
				output.Add("> Some links are invalid!");

			return output;
		}

		public override Dictionary<string, Action> GetErrorResolutions()
		{
			Dictionary<string, Action> resolutions = new Dictionary<string, Action>();
			if (BaseRecipe.IsMissing || BaseRecipe.Assemblers.Count == 0)
				resolutions.Add("Delete node", new Action(() => { this.Delete(); }));
			else
			{
				if (SelectedAssembler == null || SelectedAssembler.IsMissing)
					resolutions.Add("Auto-select assembler", new Action(() => AutoSetAssembler()));
				else if (SelectedAssembler.IsBurner)
				{
					if ((Fuel == null && SelectedAssembler.Fuels.Count > 0) ||
						!SelectedAssembler.Fuels.Contains(Fuel) ||
						(Fuel != null && Fuel.IsMissing))
						resolutions.Add("Auto-select fuel", new Action(() => AutoSetFuel()));
					if (fuelRemainsOverride != null)
						resolutions.Add("Update burn result", new Action(() => SetFuel(Fuel)));
				}

				if (AssemblerModules.Where(m => m.IsMissing).FirstOrDefault() != null || AssemblerModules.Count > SelectedAssembler.ModuleSlots)
					resolutions.Add("Fix assembler modules", new Action(() => {
						for (int i = AssemblerModules.Count - 1; i >= 0; i--) 
							if (AssemblerModules[i].IsMissing || !SelectedAssembler.Modules.Contains(assemblerModules[i]) || !BaseRecipe.Modules.Contains(assemblerModules[i])) 
								RemoveAssemblerModule(i);
						while (AssemblerModules.Count > SelectedAssembler.ModuleSlots)
							RemoveAssemblerModule(AssemblerModules.Count - 1);
					}));

				if (SelectedBeacon != null)
				{
					if (SelectedBeacon.IsMissing)
						resolutions.Add("Remove Beacon", new Action(() => SetBeacon(null)));

					if (BeaconModules.Where(m => m.IsMissing).FirstOrDefault() != null || beaconModules.Count > SelectedBeacon.ModuleSlots)
						resolutions.Add("Fix beacon modules", new Action(() => {
							for (int i = BeaconModules.Count - 1; i >= 0; i--)
								if (BeaconModules[i].IsMissing || !SelectedAssembler.Modules.Contains(beaconModules[i]) || !BaseRecipe.Modules.Contains(beaconModules[i]) || !SelectedBeacon.ValidModules.Contains(beaconModules[i]))
									RemoveBeaconModule(i);
							while (BeaconModules.Count > SelectedBeacon.ModuleSlots)
								RemoveBeaconModule(BeaconModules.Count - 1);
						}));
				}

				foreach (KeyValuePair<string, Action> kvp in GetInvalidConnectionResolutions())
					resolutions.Add(kvp.Key, kvp.Value);
			}

		return resolutions;
		}

		public override List<string> GetWarnings()
		{
			List<string> output = new List<string>();

			//recipe
			if (!BaseRecipe.Enabled)
				output.Add("X> Selected recipe is disabled.");
			if (!BaseRecipe.Available)
				output.Add("X> Selected recipe is uncraftable.");

			//assembler
			if (!SelectedAssembler.Enabled)
			{
				if (BaseRecipe.Assemblers.FirstOrDefault(a => a.Enabled) != null)
					output.Add("> Selected assembler is disabled.");
				else
					output.Add("X> No enabled assemblers for this recipe.");
			}
			if (!SelectedAssembler.Available)
				output.Add("X> Selected assembler is uncraftable.");

			//fuel
			if(Fuel != null)
			{
				if (!Fuel.Available)
					output.Add("X> Selected fuel is uncraftable");
				else if (Fuel.ProductionRecipes.FirstOrDefault(r => r.Enabled && r.Assemblers.FirstOrDefault(a => a.Enabled) != null) == null)
				{
					if (SelectedAssembler.Fuels.FirstOrDefault(fuel => fuel.ProductionRecipes.FirstOrDefault(r => r.Enabled && r.Assemblers.FirstOrDefault(a => a.Enabled) != null) != null) != null)
						output.Add("> Selected fuel cant be produced (recipe/assembler disabled).");
					else
						output.Add("X> All fuels cant be produced (recipes/assemblers disabled).");
				}
			}

			if (assemblerModules.FirstOrDefault(m => !m.Enabled) != null)
				output.Add("> Some selected assembler modules are disabled.");
			if (assemblerModules.FirstOrDefault(m => !m.Available) != null)
				output.Add("> Some selected assembler modules are uncraftable.");

			if (SelectedBeacon != null && beaconModules.FirstOrDefault(m => !m.Enabled) != null)
				output.Add("> Some selected beacon modules are disabled.");
			if (SelectedBeacon != null && beaconModules.FirstOrDefault(m => !m.Available) != null)
				output.Add("> Some selected beacon modules are uncraftable.");

			return output;
		}

		public override Dictionary<string, Action> GetWarningResolutions()
		{
			Dictionary<string, Action> resolutions = new Dictionary<string, Action>();

			if (!SelectedAssembler.Enabled && BaseRecipe.Assemblers.FirstOrDefault(a => a.Enabled) != null)
				resolutions.Add("Switch to enabled assembler", new Action(() => AutoSetAssembler()));
			if (Fuel != null && Fuel.ProductionRecipes.FirstOrDefault(r => r.Enabled && r.Assemblers.FirstOrDefault(a => a.Enabled) != null) == null)
				if (SelectedAssembler.Fuels.FirstOrDefault(fuel => fuel.ProductionRecipes.FirstOrDefault(r => r.Enabled && r.Assemblers.FirstOrDefault(a => a.Enabled) != null) != null) != null)
					resolutions.Add("Switch to valid fuel", new Action(() => AutoSetFuel()));
			if (assemblerModules.FirstOrDefault(m => !m.Enabled) != null)
				resolutions.Add("Remove disabled modules from assembler", new Action(() => {
					for (int i = AssemblerModules.Count - 1; i >= 0; i--)
						if (!AssemblerModules[i].Enabled)
							RemoveAssemblerModule(i);
					UpdateState();
				}));
			if (SelectedBeacon != null && beaconModules.FirstOrDefault(m => !m.Enabled) != null)
				resolutions.Add("Remove disabled modules from beacon", new Action(() => {
					for (int i = BeaconModules.Count - 1; i >= 0; i--)
						if (!BeaconModules[i].Enabled)
							RemoveBeaconModule(i);
					UpdateState();
				}));

			return resolutions;
		}

		public override IEnumerable<Item> Inputs
		{
			get
			{
				foreach (Item item in BaseRecipe.IngredientList)
					yield return item;
				if (Fuel != null && !BaseRecipe.IngredientSet.ContainsKey(Fuel)) //provide the burner item if it isnt null or already part of recipe ingredients
					yield return Fuel;
			}
		}
		public override IEnumerable<Item> Outputs
		{
			get
			{
				foreach (Item item in BaseRecipe.ProductList)
					yield return item;
				if (FuelRemains != null && !BaseRecipe.ProductSet.ContainsKey(FuelRemains)) //provide the burnt remains item if it isnt null or already part of recipe products
					yield return FuelRemains;
			}
		}

		public override float GetConsumeRate(Item item) { return (float)Math.Round(inputRateFor(item) * ActualRate, RoundingDP); }
		public override float GetSupplyRate(Item item) { return (float)Math.Round(outputRateFor(item) * ActualRate, RoundingDP); }

		internal override double inputRateFor(Item item)
		{
			if (item != Fuel)
				return BaseRecipe.IngredientSet[item];
			else
			{
				if (SelectedAssembler == null || (!SelectedAssembler.IsMissing && !SelectedAssembler.IsBurner))
					Trace.Fail(string.Format("input rate requested for {0} fuel while the assembler was either null or not a burner!", item));

				float recipeRate = BaseRecipe.IngredientSet.ContainsKey(item) ? BaseRecipe.IngredientSet[item] : 0;
				//burner rate = recipe time (modified by speed bonus & assembler) * assembler energy consumption (modified by consumption bonus and assembler) / fuel value of the item
				float burnerRate = (BaseRecipe.Time / (SelectedAssembler.Speed * GetSpeedMultiplier())) * (SelectedAssembler.EnergyConsumption * GetConsumptionMultiplier() / SelectedAssembler.EnergyEffectivity) / Fuel.FuelValue;
				return (float)Math.Round((recipeRate + burnerRate), RoundingDP);
			}
		}
		internal override double outputRateFor(Item item) //Note to Foreman 1.0: YES! this is where all the productivity is taken care of! (not in the solver... why would you multiply the productivity while setting up the constraints and not during the ratios here???)
		{
			if (item != FuelRemains)
				return BaseRecipe.ProductSet[item];
			else
			{
				if (SelectedAssembler == null || !SelectedAssembler.IsBurner)
					Trace.Fail(string.Format("input rate requested for {0} fuel while the assembler was either null or not a burner!", item));

				float recipeRate = BaseRecipe.ProductSet.ContainsKey(item) ? BaseRecipe.ProductSet[item] : 0;
				//burner rate is much the same as above, just have to make sure we still use the Burner item!
				float g = GetSpeedMultiplier();
				float f = GetConsumptionMultiplier();
				float burnerRate = (BaseRecipe.Time / (SelectedAssembler.Speed * GetSpeedMultiplier())) * (SelectedAssembler.EnergyConsumption * GetConsumptionMultiplier() / SelectedAssembler.EnergyEffectivity) / Fuel.FuelValue;
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

		public float GetBaseNumberOfAssemblers()
		{
			if (SelectedAssembler == null)
				return 0;
			return (float)Math.Round(ActualRate * (BaseRecipe.Time / (SelectedAssembler.Speed * GetSpeedMultiplier())), RoundingDP);
		}

		public void SetBaseNumberOfAssemblers(float num)
		{
			if (SelectedAssembler == null)
				return;
			DesiredRate = (float)Math.Round(num * SelectedAssembler.Speed * GetSpeedMultiplier() / BaseRecipe.Time, RoundingDP);
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
				if (Fuel != null)
					info.AddValue("Fuel", Fuel.Name);
				if (FuelRemains != null)
					info.AddValue("Burnt", FuelRemains.Name);
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
