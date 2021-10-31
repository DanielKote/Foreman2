using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman
{
	public class RecipeNode : BaseNode
	{
		public enum Errors
		{
			Clean = 0b_000_000_000,
			RecipeIsMissing = 0b_0000_0000_0001,
			AssemblerIsMissing = 0b_0000_0000_0010,
			BurnerNoFuelSet = 0b_0000_0000_0100,
			FuelIsMissing = 0b_0000_0000_1000,
			InvalidFuel = 0b_0000_0001_0000,
			InvalidFuelRemains = 0b_0000_0010_0000,
			AModuleIsMissing = 0b_0000_0100_0000,
			AModuleLimitExceeded = 0b_0000_1000_0000,
			BeaconIsMissing = 0b_0001_0000_0000,
			BModuleIsMissing = 0b_0010_0000_0000,
			BModuleLimitExceeded = 0b_0100_0000_0000,
			InvalidLinks = 0b_100_000_000
		}
		public enum Warnings
		{
			Clean = 0b_0000_0000_0000_0000,
			RecipeIsDisabled = 0b_0000_0000_0000_0001,
			RecipeIsUnavailable = 0b_0000_0000_0000_0010,
			AssemblerIsDisabled = 0b_0000_0000_0000_0100,
			AssemblerIsUnavailable = 0b_0000_0000_0000_1000,
			NoAvailableAssemblers = 0b_0000_0000_0001_0000,
			FuelIsUnavailable = 0b_0000_0000_0010_0000,
			FuelIsUncraftable = 0b_0000_0000_0100_0000,
			NoAvailableFuels = 0b_0000_1000_0000_0000,
			AModuleIsDisabled = 0b_0000_0001_0000_0000,
			AModuleIsUnavailable = 0b_0000_0010_0000_0000,
			BeaconIsDisabled = 0b_0000_0100_0000_0000,
			BeaconIsUnavailable = 0b_0000_1000_0000_0000,
			BModuleIsDisabled = 0b_0001_0000_0000_0000,
			BModuleIsUnavailable = 0b_0010_0000_0000_0000,
		}
		public Errors ErrorSet { get; private set; }
		public Warnings WarningSet { get; private set; }

		private readonly RecipeNodeController controller;
		public override BaseNodeController Controller { get { return controller; } }

		public readonly Recipe BaseRecipe;
		public double NeighbourCount { get; set; }

		private Assembler assembler;
		public Assembler SelectedAssembler { get { return assembler; }set { if (value != null) assembler = value; } }
		public Item Fuel { get { return fuel; } set { fuel = value; fuelRemainsOverride = null; } }
		public Item FuelRemains { get { return fuelRemainsOverride ?? ((Fuel != null && Fuel.BurnResult != null) ? Fuel.BurnResult : null); } }
		public void SetBurntOverride(Item item) { if (Fuel == null || Fuel.BurnResult != item) fuelRemainsOverride = item; }
		private Item fuel;
		private Item fuelRemainsOverride; //returns as BurntItem if set (error import)

		public Beacon SelectedBeacon { get; set; }
		public double BeaconCount { get; set; }
		public double BeaconsPerAssembler { get; set; }
		public double BeaconsConst { get; set; }

		public List<Module> AssemblerModules { get; private set; }
		public List<Module> BeaconModules { get; private set; }

		public double DesiredAssemblerCount { get; set; }
		public double ActualAssemblerCount { get { return ActualRatePerSec * BaseRecipe.Time / (SelectedAssembler.Speed * GetSpeedMultiplier() * MyGraph.GetRateMultipler()); } }
		public override double DesiredRatePerSec { get { return DesiredAssemblerCount * SelectedAssembler.Speed * GetSpeedMultiplier() * MyGraph.GetRateMultipler() / BaseRecipe.Time; } set { Trace.Fail("Desired rate set on a recipe node!"); } }

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

		public RecipeNode(ProductionGraph graph, int nodeID, Recipe recipe) : base(graph, nodeID)
		{
			BaseRecipe = recipe;
			controller = RecipeNodeController.GetController(this);
			ReadOnlyNode = new ReadOnlyRecipeNode(this);

			SelectedAssembler = recipe.Assemblers.First(); //everything here works under the assumption that assember isnt null.
			SelectedBeacon = null;
			NeighbourCount = 0;

			BeaconCount = 0;
			BeaconsPerAssembler = 0;
			BeaconsConst = 0;

			BeaconModules = new List<Module>();
			AssemblerModules = new List<Module>();
		}

		public override bool UpdateState()
		{
			NodeState oldState = State;
			State = GetUpdatedState();
			if (oldState != State)
			{
				OnNodeStateChanged();
				return true;
			}
			return false;
		}

		private NodeState GetUpdatedState()
		{
			WarningSet = Warnings.Clean;
			ErrorSet = Errors.Clean;

			//error states:
			if (BaseRecipe.IsMissing)
				ErrorSet |= Errors.RecipeIsMissing;
			if (SelectedAssembler == null || SelectedAssembler.IsMissing)
				ErrorSet |= Errors.AssemblerIsMissing;

			if (SelectedAssembler.IsBurner && Fuel == null)
				ErrorSet |= Errors.BurnerNoFuelSet;
			else if (SelectedAssembler.IsBurner && Fuel != null)
			{
				if (Fuel.IsMissing)
					ErrorSet |= Errors.FuelIsMissing;
				if (!SelectedAssembler.Fuels.Contains(Fuel))
					ErrorSet |= Errors.InvalidFuel;
				if (Fuel.BurnResult != FuelRemains)
					ErrorSet |= Errors.InvalidFuelRemains;
			}

			if (AssemblerModules.FirstOrDefault(m => m.IsMissing) != null)
				ErrorSet |= Errors.AModuleIsMissing;
			if (AssemblerModules.Count > SelectedAssembler.ModuleSlots)
				ErrorSet |= Errors.AModuleLimitExceeded;

			if (SelectedBeacon != null)
			{
				if (SelectedBeacon.IsMissing)
					ErrorSet |= Errors.BeaconIsMissing;
				if (BeaconModules.FirstOrDefault(m => m.IsMissing) != null)
					ErrorSet |= Errors.BModuleIsMissing;
				if (BeaconModules.Count > SelectedBeacon.ModuleSlots)
					ErrorSet |= Errors.BModuleLimitExceeded;
			}
			else if (BeaconModules.Count != 0)
				ErrorSet |= Errors.BModuleLimitExceeded;

			if (!AllLinksValid)
				ErrorSet |= Errors.InvalidLinks;

			//warning states (either not enabled or not available both throw up warnings)
			if (!BaseRecipe.Enabled)
				WarningSet |= Warnings.RecipeIsDisabled;
			if (!BaseRecipe.Available)
				WarningSet |= Warnings.RecipeIsUnavailable;
			if (!SelectedAssembler.Enabled)
				WarningSet |= Warnings.AssemblerIsDisabled;
			if (!SelectedAssembler.Available)
				WarningSet |= Warnings.AssemblerIsUnavailable;
			if (BaseRecipe.Assemblers.FirstOrDefault(a => a.Enabled) == null)
				WarningSet |= Warnings.NoAvailableAssemblers;
			if (Fuel != null)
			{
				if (!Fuel.Available)
					WarningSet |= Warnings.FuelIsUnavailable;
				if (Fuel.ProductionRecipes.FirstOrDefault(r => r.Enabled && r.Assemblers.FirstOrDefault(a => a.Enabled) != null) == null)
					WarningSet |= Warnings.FuelIsUncraftable;
				if (SelectedAssembler.Fuels.FirstOrDefault(f => f.Enabled && f.ProductionRecipes.FirstOrDefault(r => r.Enabled && r.Assemblers.FirstOrDefault(a => a.Enabled) != null) != null) != null)
					WarningSet |= Warnings.NoAvailableFuels;
			}
			if (AssemblerModules.FirstOrDefault(m => !m.Enabled) != null)
				WarningSet |= Warnings.AModuleIsDisabled;
			if (AssemblerModules.FirstOrDefault(m => !m.Available) != null)
				WarningSet |= Warnings.AModuleIsUnavailable;
			if (SelectedBeacon != null)
			{
				if (!SelectedBeacon.Enabled)
					WarningSet |= Warnings.BeaconIsDisabled;
				if (!SelectedBeacon.Available)
					WarningSet |= Warnings.BeaconIsUnavailable;
			}
			if (BeaconModules.FirstOrDefault(m => !m.Enabled) != null)
				WarningSet |= Warnings.BModuleIsDisabled;
			if (BeaconModules.FirstOrDefault(m => !m.Available) != null)
				WarningSet |= Warnings.BModuleIsUnavailable;

			if (ErrorSet != Errors.Clean)
				return NodeState.Error;
			if (WarningSet != Warnings.Clean)
				return NodeState.Warning;
			return NodeState.Clean;
		}

		//------------------------------------------------------------------------ multipliers (speed/productivity/consumption/pollution) & rates

		public double GetSpeedMultiplier()
		{
			double multiplier = 1.0f;
			foreach (Module module in AssemblerModules)
				multiplier += module.SpeedBonus;
			foreach (Module beaconModule in BeaconModules)
				multiplier += beaconModule.SpeedBonus * SelectedBeacon.BeaconEffectivity * BeaconCount;
			return multiplier;
		}

		public double GetProductivityMultiplier()
		{
			double multiplier = 1.0f + (SelectedAssembler == null ? 0 : SelectedAssembler.BaseProductivityBonus);
			foreach (Module module in AssemblerModules)
				multiplier += module.ProductivityBonus;
			foreach (Module beaconModule in BeaconModules)
				multiplier += beaconModule.ProductivityBonus * SelectedBeacon.BeaconEffectivity * BeaconCount;
			return multiplier;
		}

		public double GetConsumptionMultiplier()
		{
			double multiplier = 1.0f;
			foreach (Module module in AssemblerModules)
				multiplier += module.ConsumptionBonus;
			foreach (Module beaconModule in BeaconModules)
				multiplier += beaconModule.ConsumptionBonus * SelectedBeacon.BeaconEffectivity * BeaconCount;
			return multiplier > 0.2f ? multiplier : 0.2f;
		}

		public double GetPollutionMultiplier()
		{
			double multiplier = 1.0f;
			foreach (Module module in AssemblerModules)
				multiplier += module.PollutionBonus;
			foreach (Module beaconModule in BeaconModules)
				multiplier += beaconModule.PollutionBonus * SelectedBeacon.BeaconEffectivity * BeaconCount;
			return multiplier > 0.2f ? multiplier : 0.2f;
		}

		public override double GetConsumeRate(Item item) { return inputRateFor(item) * ActualRate; }
		public override double GetSupplyRate(Item item) { return outputRateFor(item) * ActualRate; }

		//------------------------------------------------------------------------ graph optimization functions

		internal override double inputRateFor(Item item)
		{
			if (item != Fuel)
				return BaseRecipe.IngredientSet[item];
			else
			{
				if (!SelectedAssembler.IsMissing && !SelectedAssembler.IsBurner)
					Trace.Fail(string.Format("input rate requested for {0} fuel while the assembler was not a burner!", item));

				double recipeRate = BaseRecipe.IngredientSet.ContainsKey(item) ? BaseRecipe.IngredientSet[item] : 0;
				//burner rate = recipe time (modified by speed bonus & assembler) * assembler energy consumption (modified by consumption bonus and assembler) / fuel value of the item
				double burnerRate = (BaseRecipe.Time / (SelectedAssembler.Speed * GetSpeedMultiplier())) * (SelectedAssembler.EnergyConsumption * GetConsumptionMultiplier() / SelectedAssembler.ConsumptionEffectivity) / Fuel.FuelValue;
				return recipeRate + burnerRate;
			}
		}
		internal override double outputRateFor(Item item) //Note to Foreman 1.0: YES! this is where all the productivity is taken care of! (not in the solver... why would you multiply the productivity while setting up the constraints and not during the ratios here???)
		{
			if (item != FuelRemains)
			{
				if (SelectedAssembler.EntityType == EntityType.Reactor)
					return BaseRecipe.ProductSet[item] * (1 + SelectedAssembler.NeighbourBonus * NeighbourCount) * GetProductivityMultiplier();
				else
					return BaseRecipe.ProductSet[item] * GetProductivityMultiplier();
			}
			else
			{
				if (SelectedAssembler == null || !SelectedAssembler.IsBurner)
					Trace.Fail(string.Format("input rate requested for {0} fuel while the assembler was either null or not a burner!", item));

				double recipeRate = BaseRecipe.ProductSet.ContainsKey(item) ? BaseRecipe.ProductSet[item] * GetProductivityMultiplier() : 0;
				//burner rate is much the same as above (without the productivity), just have to make sure we still use the Burner item!
				double g = GetSpeedMultiplier();
				double f = GetConsumptionMultiplier();
				double burnerRate = (BaseRecipe.Time / (SelectedAssembler.Speed * GetSpeedMultiplier())) * (SelectedAssembler.EnergyConsumption * GetConsumptionMultiplier() / SelectedAssembler.ConsumptionEffectivity) / Fuel.FuelValue;
				return recipeRate + burnerRate;
			}
		}
		internal double GetMaxIORatio()
		{
			double maxValue = 0;
			double minValue = double.MaxValue;
			foreach (Item item in Inputs)
			{
				maxValue = Math.Max(maxValue, inputRateFor(item));
				minValue = Math.Min(minValue, inputRateFor(item));
			}
			foreach (Item item in Outputs)
			{
				maxValue = Math.Max(maxValue, outputRateFor(item));
				minValue = Math.Min(minValue, outputRateFor(item));
			}
			return maxValue / minValue;
		}

		//------------------------------------------------------------------------object save & string

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", NodeType.Recipe);
			info.AddValue("NodeID", NodeID);
			info.AddValue("Location", Location);
			info.AddValue("RecipeID", BaseRecipe.RecipeID);
			info.AddValue("RateType", RateType);
			info.AddValue("ActualRate", ActualRatePerSec);
			info.AddValue("Neighbours", NeighbourCount);
			if (RateType == RateType.Manual)
				info.AddValue("DesiredAssemblers", DesiredAssemblerCount);

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
				info.AddValue("BeaconModules", BeaconModules.Select(m => m.Name));
				info.AddValue("BeaconCount", BeaconCount);
				info.AddValue("BeaconsPerAssembler", BeaconsPerAssembler);
				info.AddValue("BeaconsConst", BeaconsConst);
			}
		}

		public override string ToString() { return string.Format("Recipe node for: {0}", BaseRecipe.Name); }
	}

	public class ReadOnlyRecipeNode : ReadOnlyBaseNode
	{
		public Recipe BaseRecipe { get { return MyNode.BaseRecipe; } }
		public double NeighbourCount { get { return MyNode.NeighbourCount; } }
		public double BeaconCount { get { return MyNode.BeaconCount; } }
		public double BeaconsPerAssembler { get { return MyNode.BeaconsPerAssembler; } }
		public double BeaconsConst { get { return MyNode.BeaconsConst; } }

		//----------------------------------------------------------------------- Get functions (not dependent on rate)

		public double GetAssemblerEnergyConsumption() //Watts
		{
			return MyNode.SelectedAssembler.EnergyDrain + (MyNode.SelectedAssembler.EnergyConsumption * MyNode.GetConsumptionMultiplier());
		}

		public double GetAssemblerElectricalProduction() //Watts
		{
			return MyNode.SelectedAssembler.EnergyProduction; //no consumption multiplier => generators cant have modules / beacon effects
		}

		public double GetAssemblerPollutionProduction() //pollution/sec
		{
			return MyNode.SelectedAssembler.Pollution * MyNode.GetPollutionMultiplier() * GetAssemblerEnergyConsumption(); //pollution is counted in per energy
		}

		public double GetBeaconPollutionProduction() //pollution/sec
		{
			if (MyNode.SelectedBeacon == null)
				return 0;
			return GetTotalBeacons() * MyNode.SelectedBeacon.Pollution * (MyNode.SelectedBeacon.EnergyConsumption + MyNode.SelectedBeacon.EnergyDrain);
		}

		public double GetTotalBeacons()
		{
			if (MyNode.SelectedBeacon == null)
				return 0;
			return Math.Ceiling(((int)(MyNode.ActualAssemblerCount + 0.8) * BeaconsPerAssembler) + BeaconsConst); //assume 0.2 assemblers (or more) is enough to warrant an extra 'beacons per assembler' row
		}

		//----------------------------------------------------------------------- Get functions (rate dependent)

		public double GetTotalAssemblerFuelConsumption() //fuel items / time unit
		{
			if (MyNode.Fuel == null)
				return 0;
			return (MyNode.MyGraph.GetRateMultipler() * BaseRecipe.Time * MyNode.SelectedAssembler.EnergyConsumption * MyNode.GetConsumptionMultiplier() * MyNode.ActualRatePerSec) /
				(MyNode.SelectedAssembler.Speed * MyNode.GetSpeedMultiplier() * MyNode.SelectedAssembler.ConsumptionEffectivity * MyNode.Fuel.FuelValue);
		}

		public double GetTotalAssemblerElectricalConsumption() // J/time unit
		{
			if (MyNode.SelectedAssembler.EnergySource != EnergySource.Electric)
				return 0;

			double partialAssembler = MyNode.ActualAssemblerCount % 1;
			double entireAssemblers = MyNode.ActualAssemblerCount - partialAssembler;

			return MyNode.MyGraph.GetRateMultipler() * (((entireAssemblers + (partialAssembler < 0.05 ? 0 : 1)) * MyNode.SelectedAssembler.EnergyDrain) + (MyNode.ActualAssemblerCount * MyNode.SelectedAssembler.EnergyConsumption * MyNode.GetConsumptionMultiplier())); //if there is more than 5% of an extra assembler, assume there is +1 assembler working x% of the time (full drain, x% uptime)
		}

		public double GetTotalAssemblerElectricalProduction() // J/time unit
		{
			return MyNode.MyGraph.GetRateMultipler() * GetAssemblerElectricalProduction() * MyNode.ActualAssemblerCount;
		}

		public double GetTotalBeaconElectricalConsumption() // J/time unit
		{
			if (MyNode.SelectedBeacon == null || MyNode.SelectedBeacon.EnergySource != EnergySource.Electric)
				return 0;
			return MyNode.MyGraph.GetRateMultipler() * GetTotalBeacons() * (MyNode.SelectedBeacon.EnergyConsumption + MyNode.SelectedBeacon.EnergyDrain);
		}

		private readonly RecipeNode MyNode;

		public ReadOnlyRecipeNode(RecipeNode node) : base(node) { }
	}

	public class RecipeNodeController : BaseNodeController
	{
		public Recipe BaseRecipe { get { return MyNode.BaseRecipe; } }

		public double NeighbourCount => MyNode.NeighbourCount;
		public double BeaconCount => MyNode.BeaconCount;
		public double BeaconsPerAssembler => MyNode.BeaconsPerAssembler;
		public double BeaconsConst => MyNode.BeaconCount;

		private readonly RecipeNode MyNode;

		protected RecipeNodeController(RecipeNode myNode) : base(myNode) { MyNode = myNode; }

		public static RecipeNodeController GetController(RecipeNode node)
		{
			if (node.Controller != null)
				return (RecipeNodeController)node.Controller;
			return new RecipeNodeController(node);
		}

		//------------------------------------------------------------------------ warning / errors functions

		public override List<string> GetErrors()
		{
			RecipeNode.Errors ErrorSet = MyNode.ErrorSet;

			List<string> output = new List<string>();

			if ((ErrorSet & RecipeNode.Errors.RecipeIsMissing) != 0)
			{
				output.Add(string.Format("> Recipe \"{0}\" doesnt exist in preset!", MyNode.BaseRecipe.FriendlyName));
				return output; //missing recipe is an automatic end -> we dont care about any other errors, since the only solution is to delete the node.
			}

			if ((ErrorSet & RecipeNode.Errors.AssemblerIsMissing) != 0)
				output.Add(string.Format("> Assembler \"{0}\" doesnt exist in preset!", MyNode.SelectedAssembler.FriendlyName));
			if ((ErrorSet & RecipeNode.Errors.BurnerNoFuelSet) != 0)
				output.Add("> Burner Assembler has no fuel set!");
			if ((ErrorSet & RecipeNode.Errors.FuelIsMissing) != 0)
				output.Add("> Burner Assembler's fuel doesnt exist in preset!");
			if ((ErrorSet & RecipeNode.Errors.InvalidFuel) != 0)
				output.Add("> Burner Assembler has an invalid fuel set!");
			if ((ErrorSet & RecipeNode.Errors.InvalidFuelRemains) != 0)
				output.Add("> Burning result doesnt match fuel's burn result!");
			if ((ErrorSet & RecipeNode.Errors.AModuleIsMissing) != 0)
				output.Add("> Some of the assembler modules dont exist in preset!");
			if ((ErrorSet & RecipeNode.Errors.AModuleLimitExceeded) != 0)
				output.Add(string.Format("> Assembler has too many modules ({0}/{1})!", MyNode.AssemblerModules.Count, MyNode.SelectedAssembler.ModuleSlots));
			if ((ErrorSet & RecipeNode.Errors.BeaconIsMissing) != 0)
				output.Add(string.Format("> Beacon \"{0}\" doesnt exist in preset!", MyNode.SelectedBeacon.FriendlyName));
			if ((ErrorSet & RecipeNode.Errors.BModuleIsMissing) != 0)
				output.Add("> Some of the beacon modules dont exist in preset!");
			if ((ErrorSet & RecipeNode.Errors.BModuleLimitExceeded) != 0)
				output.Add("> Beacon has too many modules!");
			if ((ErrorSet & RecipeNode.Errors.InvalidLinks) != 0)
				output.Add("> Some links are invalid!");

			return output;
		}

		public override Dictionary<string, Action> GetErrorResolutions()
		{
			RecipeNode.Errors ErrorSet = MyNode.ErrorSet;

			Dictionary<string, Action> resolutions = new Dictionary<string, Action>();
			if ((ErrorSet & RecipeNode.Errors.RecipeIsMissing) != 0)
				resolutions.Add("Delete node", new Action(() => { this.Delete(); }));
			else
			{
				if ((ErrorSet & RecipeNode.Errors.AssemblerIsMissing) != 0)
					resolutions.Add("Auto-select assembler", new Action(() => AutoSetAssembler()));

				if ((ErrorSet & (RecipeNode.Errors.FuelIsMissing | RecipeNode.Errors.InvalidFuel)) != 0 && MyNode.SelectedAssembler.Fuels.FirstOrDefault(f => !f.IsMissing) != null)
					resolutions.Add("Auto-select fuel", new Action(() => AutoSetFuel()));

				if ((ErrorSet & RecipeNode.Errors.InvalidFuelRemains) != 0 && MyNode.SelectedAssembler.Fuels.Contains(MyNode.Fuel))
					resolutions.Add("Update burn result", new Action(() => SetFuel(MyNode.Fuel)));

				if ((ErrorSet & (RecipeNode.Errors.AModuleIsMissing | RecipeNode.Errors.AModuleLimitExceeded)) != 0)
					resolutions.Add("Fix assembler modules", new Action(() =>
					{
						for (int i = MyNode.AssemblerModules.Count - 1; i >= 0; i--)
							if (MyNode.AssemblerModules[i].IsMissing || !MyNode.SelectedAssembler.Modules.Contains(MyNode.AssemblerModules[i]) || !MyNode.BaseRecipe.Modules.Contains(MyNode.AssemblerModules[i]))
								RemoveAssemblerModule(i);
						while (MyNode.AssemblerModules.Count > MyNode.SelectedAssembler.ModuleSlots)
							RemoveAssemblerModule(MyNode.AssemblerModules.Count - 1);
					}));

				if ((ErrorSet & RecipeNode.Errors.BeaconIsMissing) != 0)
					resolutions.Add("Remove Beacon", new Action(() => SetBeacon(null)));

				if ((ErrorSet & (RecipeNode.Errors.BModuleIsMissing | RecipeNode.Errors.BModuleLimitExceeded)) != 0)
					resolutions.Add("Fix beacon modules", new Action(() =>
					{
						for (int i = MyNode.BeaconModules.Count - 1; i >= 0; i--)
							if (MyNode.BeaconModules[i].IsMissing || !MyNode.SelectedAssembler.Modules.Contains(MyNode.BeaconModules[i]) || !MyNode.BaseRecipe.Modules.Contains(MyNode.BeaconModules[i]) || !MyNode.SelectedBeacon.Modules.Contains(MyNode.BeaconModules[i]))
								RemoveBeaconModule(i);
						while (MyNode.BeaconModules.Count > MyNode.SelectedBeacon.ModuleSlots)
							RemoveBeaconModule(MyNode.BeaconModules.Count - 1);
					}));

				foreach (KeyValuePair<string, Action> kvp in GetInvalidConnectionResolutions())
					resolutions.Add(kvp.Key, kvp.Value);
			}

			return resolutions;
		}

		public override List<string> GetWarnings()
		{
			RecipeNode.Warnings WarningSet = MyNode.WarningSet;

			List<string> output = new List<string>();

			//recipe
			if ((WarningSet & RecipeNode.Warnings.RecipeIsDisabled) != 0)
				output.Add("X> Selected recipe is disabled.");
			if ((WarningSet & RecipeNode.Warnings.RecipeIsUnavailable) != 0)
				output.Add("X> Selected recipe is unavailable in regular play.");

			if ((WarningSet & RecipeNode.Warnings.AssemblerIsDisabled) != 0)
				output.Add("> Selected assembler is disabled.");
			if ((WarningSet & RecipeNode.Warnings.AssemblerIsUnavailable) != 0)
				output.Add("X> Selected assembler is unavailable in regular play.");
			if ((WarningSet & RecipeNode.Warnings.NoAvailableAssemblers) != 0)
				output.Add("X> No enabled assemblers for this recipe.");

			//fuel
			if ((WarningSet & RecipeNode.Warnings.FuelIsUnavailable) != 0)
				output.Add("> Selected fuel is unavailable in regular play.");
			if ((WarningSet & RecipeNode.Warnings.FuelIsUncraftable) != 0)
				output.Add("X> Selected fuel cant be produced.");
			if ((WarningSet & RecipeNode.Warnings.NoAvailableFuels) != 0)
				output.Add("X> No fuel can be produced.");

			//modules & beacon modules
			if ((WarningSet & RecipeNode.Warnings.AModuleIsDisabled) != 0)
				output.Add("> Some selected assembler modules are disabled.");
			if ((WarningSet & RecipeNode.Warnings.AModuleIsUnavailable) != 0)
				output.Add("> Some selected assembler modules are unavailable in regular play.");
			if ((WarningSet & RecipeNode.Warnings.BeaconIsDisabled) != 0)
				output.Add("> Selected beacon is disabled.");
			if ((WarningSet & RecipeNode.Warnings.BeaconIsUnavailable) != 0)
				output.Add("> Selected beacon is unavailable in regular play.");
			if ((WarningSet & RecipeNode.Warnings.BModuleIsDisabled) != 0)
				output.Add("> Some selected beacon modules are disabled.");
			if ((WarningSet & RecipeNode.Warnings.BModuleIsUnavailable) != 0)
				output.Add("> Some selected beacon modules are unavailable in regular play.");

			return output;
		}

		public override Dictionary<string, Action> GetWarningResolutions()
		{
			RecipeNode.Warnings WarningSet = MyNode.WarningSet;

			Dictionary<string, Action> resolutions = new Dictionary<string, Action>();

			if((WarningSet & (RecipeNode.Warnings.AssemblerIsDisabled | RecipeNode.Warnings.AssemblerIsUnavailable)) != 0 && (WarningSet & RecipeNode.Warnings.NoAvailableAssemblers) == 0)
				resolutions.Add("Switch to enabled assembler", new Action(() => AutoSetAssembler()));

			if((WarningSet & (RecipeNode.Warnings.FuelIsUnavailable | RecipeNode.Warnings.FuelIsUncraftable)) != 0 && (WarningSet & RecipeNode.Warnings.NoAvailableFuels) == 0)
				resolutions.Add("Switch to valid fuel", new Action(() => AutoSetFuel()));

			if ((WarningSet & (RecipeNode.Warnings.AModuleIsDisabled | RecipeNode.Warnings.AModuleIsUnavailable)) != 0)
				resolutions.Add("Remove error modules from assembler", new Action(() =>
				{
					for (int i = MyNode.AssemblerModules.Count - 1; i >= 0; i--)
						if (!MyNode.AssemblerModules[i].Enabled || !MyNode.AssemblerModules[i].Available)
							RemoveAssemblerModule(i);
					MyNode.UpdateState();
				}));

			if ((WarningSet & (RecipeNode.Warnings.BeaconIsDisabled | RecipeNode.Warnings.BeaconIsUnavailable)) != 0)
				resolutions.Add("Turn off beacon", new Action(() => SetBeacon(null)));

			if ((WarningSet & (RecipeNode.Warnings.BModuleIsDisabled | RecipeNode.Warnings.BModuleIsUnavailable)) != 0)
			resolutions.Add("Remove error modules from beacon", new Action(() =>
			{
				for (int i = MyNode.BeaconModules.Count - 1; i >= 0; i--)
					if (!MyNode.BeaconModules[i].Enabled || !MyNode.BeaconModules[i].Available)
						RemoveBeaconModule(i);
				MyNode.UpdateState();
			}));

			return resolutions;
		}

		//-----------------------------------------------------------------------Set functions

		public void SetAssembler(Assembler assembler)
		{
			if (assembler == null)
				Trace.Fail("Cant set a null assembler!");
			MyNode.SelectedAssembler = assembler;

			//fuel
			if (!assembler.IsBurner)
				SetFuel(null);
			else if (MyNode.Fuel != null && assembler.Fuels.Contains(MyNode.Fuel))
				SetFuel(MyNode.Fuel);
			else
				AutoSetFuel();

			//check for invalid modules
			for (int i = MyNode.AssemblerModules.Count - 1; i >= 0; i--)
				if (MyNode.AssemblerModules[i].IsMissing || !MyNode.SelectedAssembler.Modules.Contains(MyNode.AssemblerModules[i]) || !MyNode.BaseRecipe.Modules.Contains(MyNode.AssemblerModules[i]))
					MyNode.AssemblerModules.RemoveAt(i);
			//check for too many modules
			while (MyNode.AssemblerModules.Count > MyNode.SelectedAssembler.ModuleSlots)
				MyNode.AssemblerModules.RemoveAt(MyNode.AssemblerModules.Count - 1);
			//check if any modules work (if none work, then turn off beacon)
			if (MyNode.SelectedAssembler.Modules.Count == 0 || MyNode.BaseRecipe.Modules.Count == 0)
				SetBeacon(null);
			else //update beacon
				SetBeacon(MyNode.SelectedBeacon);

			MyNode.UpdateState();
		}

		public void AutoSetAssembler()
		{
			SetAssembler(MyNode.MyGraph.AssemblerSelector.GetAssembler(MyNode.BaseRecipe));
			AutoSetFuel();
		}

		public void AutoSetAssembler(AssemblerSelector.Style style)
		{
			SetAssembler(MyNode.MyGraph.AssemblerSelector.GetAssembler(MyNode.BaseRecipe, style));
			SetFuel(MyNode.MyGraph.FuelSelector.GetFuel(MyNode.SelectedAssembler));
		}

		public void SetFuel(Item fuel)
		{
			if (MyNode.Fuel != fuel || (MyNode.Fuel == null && MyNode.FuelRemains != null) || (MyNode.Fuel != null && MyNode.Fuel.BurnResult != MyNode.FuelRemains))
			{
				//have to remove any links to the burner/burnt item (if they exist) unless the item is also part of the recipe
				if (MyNode.Fuel != null && !MyNode.BaseRecipe.IngredientSet.ContainsKey(MyNode.Fuel))
					foreach (NodeLink link in MyNode.InputLinks.Where(link => link.Item == MyNode.Fuel).ToList())
						link.Controller.Delete();
				if (MyNode.FuelRemains != null && !MyNode.BaseRecipe.ProductSet.ContainsKey(MyNode.FuelRemains))
					foreach (NodeLink link in MyNode.OutputLinks.Where(link => link.Item == MyNode.FuelRemains).ToList())
						link.Controller.Delete();

				MyNode.Fuel = fuel;
				MyNode.MyGraph.FuelSelector.UseFuel(fuel);
			}
			MyNode.UpdateState();
		}

		public void AutoSetFuel()
		{
			SetFuel(MyNode.MyGraph.FuelSelector.GetFuel(MyNode.SelectedAssembler));
		}

		public void SetBeacon(Beacon beacon)
		{
			MyNode.SelectedBeacon = beacon;

			if (MyNode.SelectedBeacon == null)
			{
				MyNode.BeaconModules.Clear();
				MyNode.BeaconCount = 0;
				MyNode.BeaconsPerAssembler = 0;
				MyNode.BeaconsConst = 0;
			}
			else
			{
				//check for invalid modules
				for (int i = MyNode.BeaconModules.Count - 1; i >= 0; i--)
					if (MyNode.BeaconModules[i].IsMissing || !MyNode.SelectedAssembler.Modules.Contains(MyNode.BeaconModules[i]) || !MyNode.BaseRecipe.Modules.Contains(MyNode.BeaconModules[i]) || !MyNode.SelectedBeacon.Modules.Contains(MyNode.BeaconModules[i]))
						MyNode.BeaconModules.RemoveAt(i);
				//check for too many modules
				while (MyNode.BeaconModules.Count > MyNode.SelectedBeacon.ModuleSlots)
					MyNode.BeaconModules.RemoveAt(MyNode.BeaconModules.Count - 1);
			}
			MyNode.UpdateState();
		}

		public void AddAssemblerModule(Module module)
		{
			MyNode.AssemblerModules.Add(module);
			MyNode.UpdateState();
		}

		public void RemoveAssemblerModule(int index)
		{
			if (index >= 0 && index < MyNode.AssemblerModules.Count)
				MyNode.AssemblerModules.RemoveAt(index);
			MyNode.UpdateState();
		}

		public void SetAssemblerModules(IEnumerable<Module> modules)
		{
			MyNode.AssemblerModules.Clear();
			if (modules != null)
				foreach (Module module in modules)
					MyNode.AssemblerModules.Add(module);
			MyNode.UpdateState();
		}

		public void AutoSetAssemblerModules()
		{
			MyNode.AssemblerModules.Clear();
			MyNode.AssemblerModules.AddRange(MyNode.MyGraph.ModuleSelector.GetModules(MyNode.SelectedAssembler, BaseRecipe));
			MyNode.UpdateState();
		}

		public void AutoSetAssemblerModules(ModuleSelector.Style style)
		{
			MyNode.AssemblerModules.Clear();
			MyNode.AssemblerModules.AddRange(MyNode.MyGraph.ModuleSelector.GetModules(MyNode.SelectedAssembler, BaseRecipe, style));
			MyNode.UpdateState();
		}

		public void AddBeaconModule(Module module)
		{
			MyNode.BeaconModules.Add(module);
			MyNode.UpdateState();
		}

		public void RemoveBeaconModule(int index)
		{
			if (index >= 0 && index < MyNode.BeaconModules.Count)
				MyNode.BeaconModules.RemoveAt(index);
			MyNode.UpdateState();
		}

		public void SetBeaconModules(IEnumerable<Module> modules)
		{
			MyNode.BeaconModules.Clear();
			if (modules != null)
				foreach (Module module in modules)
					MyNode.BeaconModules.Add(module);
			MyNode.UpdateState();
		}
	}
}
