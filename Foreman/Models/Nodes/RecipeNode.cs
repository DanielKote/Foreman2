using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Models;
using Foreman.Models.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Foreman {
    public partial class RecipeNode : BaseNode {
        public enum Errors {
            Clean = 0b_0000_0000_0000,
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

            RQualityIsMissing = 0b_1000_0000_0000,
            AQualityIsMissing = 0b_0001_0000_0000_0000,
            BQualityIsMissing = 0b_0010_0000_0000_0000,
            AModuleQualityIsMissing = 0b_0100_0000_0000_0000,
            BModuleQualityIsMissing = 0b_1000_0000_0000_0000,

            InvalidLinks = 0b_1000_0000_000_0000_0000
        }
        public enum Warnings {
            Clean = 0b_0000_0000_0000_0000,
            RecipeIsDisabled = 0b_0000_0000_0000_0001,
            RecipeIsUnavailable = 0b_0000_0000_0000_0010,
            AssemblerIsDisabled = 0b_0000_0000_0000_0100,
            AssemblerIsUnavailable = 0b_0000_0000_0000_1000,
            NoAvailableAssemblers = 0b_0000_0000_0001_0000,
            FuelIsUnavailable = 0b_0000_0000_0010_0000,
            FuelIsUncraftable = 0b_0000_0000_0100_0000,
            NoAvailableFuels = 0b_0000_0000_1000_0000,
            AModuleIsDisabled = 0b_0000_0001_0000_0000,
            AModuleIsUnavailable = 0b_0000_0010_0000_0000,
            BeaconIsDisabled = 0b_0000_0100_0000_0000,
            BeaconIsUnavailable = 0b_0000_1000_0000_0000,
            BModuleIsDisabled = 0b_0001_0000_0000_0000,
            BModuleIsUnavailable = 0b_0010_0000_0000_0000,

            AssemblerQualityIsDisabled = 0b_1000_0000_0000_0000,
            BeaconQualityIsDisabled = 0b_0001_0000_0000_0000_0000,
            AModulesQualityIsDisabled = 0b_0010_0000_0000_0000_0000,
            BModulesQualityIsDisabled = 0b_0100_0000_0000_0000_0000,

            TemeratureFluidBurnerInvalidLinks = 0b_0100_0000_0000_0000,
        }
        public Errors ErrorSet { get; private set; }
        public Warnings WarningSet { get; private set; }

        private readonly RecipeNodeController controller;
        public override BaseNodeController Controller { get { return controller; } }

        public bool LowPriority { get; set; }

        public RecipeQualityPair BaseRecipe { get; }
        private readonly IRecipe recipeDefinition;
        private readonly IQuality recipeQuality;

        internal IRecipe RecipeDefinition => recipeDefinition;
        internal IQuality RecipeQualityRef => recipeQuality;

        private double neighbourCount;
        public double NeighbourCount { get { return neighbourCount; } set { if (neighbourCount != value) { neighbourCount = value; ioUpdateRequired = true; UpdateState(); OnNodeValuesChanged(); } } }

        private readonly DataCache RecipeOwner;

        public AssemblerQualityPair SelectedAssembler {
            get;
            set { if (value && field != value) { field = value; ioUpdateRequired = true; UpdateState(); OnNodeStateChanged(); } }
        }
        public IItem? Fuel {
            get;
            set { if (field != value) { field = value; fuelRemainsOverride = null; ioUpdateRequired = true; UpdateState(); OnNodeStateChanged(); } }
        }
        public IItem? FuelRemains {
            get {
                return fuelRemainsOverride ?? (Fuel != null && Fuel.BurnResult != null ? Fuel.BurnResult : null);
            }
        }
        public void SetBurntOverride(IItem? item) {
            if (Fuel == null || Fuel.BurnResult != item) {
                fuelRemainsOverride = item;
                ioUpdateRequired = true;
                UpdateState();
                OnNodeValuesChanged();
            }
        }

        private IItem? fuelRemainsOverride; //returns as BurntItem if set (error import)

        private BeaconQualityPair selectedBeacon;
        public BeaconQualityPair SelectedBeacon { get { return selectedBeacon; } set { if (selectedBeacon != value) { selectedBeacon = value; ioUpdateRequired = true; UpdateState(); OnNodeValuesChanged(); } } }
        private double beaconCount;
        public double BeaconCount { get { return beaconCount; } set { if (beaconCount != value) { beaconCount = value; ioUpdateRequired = true; UpdateState(); OnNodeValuesChanged(); } } }
        private double beaconsPerAssembler;
        public double BeaconsPerAssembler { get { return beaconsPerAssembler; } set { if (beaconsPerAssembler != value) { beaconsPerAssembler = value; UpdateState(); OnNodeValuesChanged(); } } }
        private double beaconsConst;
        public double BeaconsConst { get { return beaconsConst; } set { if (beaconsConst != value) { beaconsConst = value; UpdateState(); OnNodeValuesChanged(); } } }

        public IReadOnlyList<ModuleQualityPair> AssemblerModules { get { return assemblerModules; } }
        public IReadOnlyList<ModuleQualityPair> BeaconModules { get { return beaconModules; } }
        private readonly List<ModuleQualityPair> assemblerModules;
        private readonly List<ModuleQualityPair> beaconModules;

        //for recipe nodes, the SetValue is 'number of assemblers/entities'
        public override double ActualSetValue { get { return ActualRatePerSec * recipeDefinition.Time / (SelectedAssembler.Assembler.GetSpeed(SelectedAssembler.Quality) * GetSpeedMultiplier()); } }
        public override double DesiredSetValue { get; set; }
        public override double MaxDesiredSetValue { get { return ProductionGraph.MaxFactories; } }
        public override string SetValueDescription { get { return "# of Assemblers:"; } }

        public override double DesiredRatePerSec { get { return DesiredSetValue * SelectedAssembler.Assembler.GetSpeed(SelectedAssembler.Quality) * GetSpeedMultiplier() / (recipeDefinition.Time); } set { Trace.Fail("Desired rate set on a recipe node!"); } }

        private double extraProductivityBonus;
        public double ExtraProductivityBonus { get { return extraProductivityBonus; } set { if (extraProductivityBonus != value) { extraProductivityBonus = value; ioUpdateRequired = true; UpdateState(); OnNodeValuesChanged(); } } }

        public uint MaxQualitySteps { get { return maxQualitySteps; } set { if (maxQualitySteps != value) { maxQualitySteps = value; ioUpdateRequired = true; } } } //if quality bonus > 0 then we will take this many extra quality steps for products
        private uint maxQualitySteps;

        public override IEnumerable<ItemQualityPair> Inputs { get { if (ioUpdateRequired) { UpdateInputsAndOutputs(); } return inputList; } }
        private readonly Dictionary<ItemQualityPair, double> inputSet;
        private readonly List<ItemQualityPair> inputList;

        public override IEnumerable<ItemQualityPair> Outputs { get { if (ioUpdateRequired) { UpdateInputsAndOutputs(); } return outputList; } }
        private readonly Dictionary<ItemQualityPair, double> outputSet;
        private readonly List<ItemQualityPair> outputList;

        public bool IsFuelPartOfRecipeInputs { get; private set; }
        public bool IsFuelRemainsPartOfRecipeOutputs { get; private set; }

        private bool ioUpdateRequired;

        public RecipeNode(ProductionGraph graph, int nodeID, RecipeQualityPair recipe, IQuality assemblerQuality) : base(graph, nodeID) {
            if (!recipe || recipe.Recipe is null || recipe.Quality is null)
                throw new ArgumentException("Recipe and quality must be populated.", nameof(recipe));
            recipeDefinition = recipe.Recipe;
            recipeQuality = recipe.Quality;

            LowPriority = false;
            maxQualitySteps = graph.MaxQualitySteps;
            ioUpdateRequired = false;

            BaseRecipe = recipe;
            RecipeOwner = recipeDefinition.Owner;

            controller = new RecipeNodeController(this);

            inputSet = [];
            inputList = [];
            outputSet = [];
            outputList = [];

            assemblerModules = [];
            beaconModules = [];

            SelectedAssembler = new AssemblerQualityPair(recipeDefinition.Assemblers.First(), assemblerQuality); //everything here works under the assumption that assember isnt null.
            SelectedBeacon = new BeaconQualityPair();
            NeighbourCount = 0;

            BeaconCount = 0;
            BeaconsPerAssembler = 0;
            BeaconsConst = 0;

            ExtraProductivityBonus = 0;
        }

        internal override NodeState GetUpdatedState() {
            WarningSet = Warnings.Clean;
            ErrorSet = Errors.Clean;

            //error states:
            if (recipeDefinition.IsMissing)
                ErrorSet |= Errors.RecipeIsMissing;
            if (recipeQuality.IsMissing)
                ErrorSet |= Errors.RQualityIsMissing;
            if (SelectedAssembler.Assembler.IsMissing)
                ErrorSet |= Errors.AssemblerIsMissing;
            if (SelectedAssembler.Quality.IsMissing)
                ErrorSet |= Errors.AQualityIsMissing;

            if (SelectedAssembler.Assembler.IsBurner) {
                if (Fuel == null)
                    ErrorSet |= Errors.BurnerNoFuelSet;
                else {
                    if (Fuel.IsMissing)
                        ErrorSet |= Errors.FuelIsMissing;
                    if (!SelectedAssembler.Assembler.Fuels.Contains(Fuel))
                        ErrorSet |= Errors.InvalidFuel;
                    if (Fuel.BurnResult != FuelRemains)
                        ErrorSet |= Errors.InvalidFuelRemains;
                }
            }

            if (AssemblerModules.Any(m => m.Module.IsMissing))
                ErrorSet |= Errors.AModuleIsMissing;
            if (AssemblerModules.Count > SelectedAssembler.Assembler.ModuleSlots)
                ErrorSet |= Errors.AModuleLimitExceeded;
            if (AssemblerModules.Any(m => m.Quality.IsMissing))
                ErrorSet |= Errors.AModuleQualityIsMissing;

            if (SelectedBeacon && SelectedBeacon.Beacon is IBeacon beacon) {
                if (beacon.IsMissing)
                    ErrorSet |= Errors.BeaconIsMissing;
                if (SelectedBeacon.Quality is IQuality beaconQuality && beaconQuality.IsMissing)
                    ErrorSet |= Errors.BQualityIsMissing;
                if (BeaconModules.Any(m => m.Module.IsMissing))
                    ErrorSet |= Errors.BModuleIsMissing;
                if (BeaconModules.Count > beacon.ModuleSlots)
                    ErrorSet |= Errors.BModuleLimitExceeded;
                if (BeaconModules.Any(m => m.Quality.IsMissing))
                    ErrorSet |= Errors.AModuleQualityIsMissing;

            } else if (BeaconModules.Count != 0)
                ErrorSet |= Errors.BModuleLimitExceeded;

            if (!AllLinksValid)
                ErrorSet |= Errors.InvalidLinks;

            if (ErrorSet != Errors.Clean) //warnings are NOT processed if error has been found. This makes sense (as an error is something that trumps warnings), plus guarantees we dont accidentally check statuses of missing objects (which rightfully dont exist in regular cache)
                return NodeState.Error;

            //warning states (either not enabled or not available both throw up warnings)
            if (!recipeDefinition.Enabled)
                WarningSet |= Warnings.RecipeIsDisabled;
            if (!recipeDefinition.Available)
                WarningSet |= Warnings.RecipeIsUnavailable;

            if (!SelectedAssembler.Assembler.Enabled)
                WarningSet |= Warnings.AssemblerIsDisabled;
            if (!SelectedAssembler.Assembler.Available)
                WarningSet |= Warnings.AssemblerIsUnavailable;
            if (!SelectedAssembler.Quality.Enabled)
                WarningSet |= Warnings.AssemblerQualityIsDisabled;
            if (!recipeDefinition.Assemblers.Any(a => a.Enabled))
                WarningSet |= Warnings.NoAvailableAssemblers;

            if (Fuel != null) {
                if (!Fuel.Available)
                    WarningSet |= Warnings.FuelIsUnavailable;
                if (!Fuel.ProductionRecipes.Any(r => r.Enabled && r.Assemblers.Any(a => a.Enabled)))
                    WarningSet |= Warnings.FuelIsUncraftable;
                if (!SelectedAssembler.Assembler.Fuels.Any(f => f.Enabled && f.ProductionRecipes.Any(r => r.Enabled && r.Assemblers.Any(a => a.Enabled))))
                    WarningSet |= Warnings.NoAvailableFuels;
            }

            if (AssemblerModules.Any(m => !m.Module.Enabled))
                WarningSet |= Warnings.AModuleIsDisabled;
            if (AssemblerModules.Any(m => !m.Module.Available))
                WarningSet |= Warnings.AModuleIsUnavailable;
            if (AssemblerModules.Any(m => !m.Quality.Enabled))
                WarningSet |= Warnings.AModulesQualityIsDisabled;

            if (SelectedBeacon && SelectedBeacon.Beacon is IBeacon warningBeacon) {
                if (!warningBeacon.Enabled)
                    WarningSet |= Warnings.BeaconIsDisabled;
                if (!warningBeacon.Available)
                    WarningSet |= Warnings.BeaconIsUnavailable;
                if (SelectedBeacon.Quality is IQuality warningBeaconQuality && !warningBeaconQuality.Enabled)
                    WarningSet |= Warnings.BeaconQualityIsDisabled;
            }
            if (BeaconModules.Any(m => !m.Module.Enabled))
                WarningSet |= Warnings.BModuleIsDisabled;
            if (BeaconModules.Any(m => !m.Module.Available))
                WarningSet |= Warnings.BModuleIsUnavailable;
            if (BeaconModules.Any(m => !m.Quality.Enabled))
                WarningSet |= Warnings.BModulesQualityIsDisabled;

            if (SelectedAssembler.Assembler.IsTemperatureFluidBurner && !LinkChecker.GetTemperatureRange(Fuel as IFluid, this, LinkType.Output, false).IsPoint())
                WarningSet |= Warnings.TemeratureFluidBurnerInvalidLinks;

            return WarningSet != Warnings.Clean ? NodeState.Warning : AllLinksConnected ? NodeState.Clean : NodeState.MissingLink;

        }

        public void UpdateInputsAndOutputs(bool forceUpdate = false) {
            if (!ioUpdateRequired && !forceUpdate)
                return;
            ioUpdateRequired = false;

            //Inputs:
            inputSet.Clear();
            inputList.Clear();
            foreach (IItem item in recipeDefinition.IngredientList) {
                var inputItem = new ItemQualityPair(item, item is IFluid ? RecipeOwner.DefaultQuality ?? recipeQuality : recipeQuality);
                double inputQuantity = recipeDefinition.IngredientSet[item];

                inputList.Add(inputItem);
                inputSet.Add(inputItem, inputQuantity);
            }
            if (Fuel is not null && RecipeOwner.DefaultQuality is not null) //provide the burner item if it isnt null or already part of recipe ingredients
            {
                var fuelIQP = new ItemQualityPair(Fuel, RecipeOwner.DefaultQuality);
                if (!inputSet.ContainsKey(fuelIQP)) {
                    IsFuelPartOfRecipeInputs = false;
                    inputList.Add(fuelIQP);
                    inputSet.Add(fuelIQP, InputRateForFuel());
                } else {
                    IsFuelPartOfRecipeInputs = true;
                    inputSet[fuelIQP] += InputRateForFuel();
                }
            }

            //Outputs:
            outputSet.Clear();
            outputList.Clear();
            foreach (IItem item in recipeDefinition.ProductList) {
                if (SelectedAssembler.Assembler.EntityType == EntityType.Reactor) {
                    var product = new ItemQualityPair(item, RecipeOwner.DefaultQuality ?? recipeQuality);
                    double amount = recipeDefinition.ProductSet[item] + (1 * SelectedAssembler.Assembler.NeighbourBonus * NeighbourCount);
                    outputList.Add(product);
                    outputSet.Add(product, amount);
                } else {
                    double amount = recipeDefinition.ProductSet[item] + (recipeDefinition.ProductPSet[item] * GetProductivityBonus());

                    if (item is IFluid) {
                        var fluidProduct = new ItemQualityPair(item, RecipeOwner.DefaultQuality ?? recipeQuality);
                        outputList.Add(fluidProduct);
                        outputSet.Add(fluidProduct, amount);
                    } else {
                        var currentProduct = new ItemQualityPair(item, recipeQuality);
                        uint currentStep = 1;
                        outputList.Add(currentProduct);
                        outputSet.Add(currentProduct, amount);
                        double currentMultiplier = GetQualityMultiplier();
                        while (currentStep < MaxQualitySteps && currentProduct.Quality is IQuality stepQuality && stepQuality.NextQuality is IQuality nextQuality) {
                            currentStep++;
                            ItemQualityPair lastProduct = currentProduct;
                            currentMultiplier *= stepQuality.NextProbability;
                            currentProduct = new ItemQualityPair(item, nextQuality);
                            if (currentMultiplier == 0)
                                break;
                            if (!nextQuality.Enabled || !nextQuality.Available)
                                break;

                            outputList.Add(currentProduct);
                            outputSet.Add(currentProduct, Math.Min(currentMultiplier, 1.0) * amount);
                            outputSet[lastProduct] -= outputSet[currentProduct];

                            if (outputSet[lastProduct] <= 0) {
                                outputList.Remove(lastProduct);
                                outputSet.Remove(lastProduct);
                            }

                        }
                    }
                }
            }
            if (FuelRemains != null) //provide the burnt item if it isnt null or already part of recipe ingredients
            {
                var fuelRemainsIQP = new ItemQualityPair(FuelRemains, RecipeOwner.DefaultQuality ?? recipeQuality);
                if (!outputSet.ContainsKey(fuelRemainsIQP)) {
                    IsFuelRemainsPartOfRecipeOutputs = false;
                    outputList.Add(fuelRemainsIQP);
                    outputSet.Add(fuelRemainsIQP, InputRateForFuel());
                } else {
                    IsFuelRemainsPartOfRecipeOutputs = true;
                    outputSet[fuelRemainsIQP] += InputRateForFuel();
                }
            }

            //links
            foreach (NodeLink link in InputLinks.ToList()) {
                if (!inputSet.ContainsKey(link.Item))
                    MyGraph.DeleteLink(link);
            }
            foreach (NodeLink link in OutputLinks.ToList()) {
                if (!outputSet.ContainsKey(link.Item))
                    MyGraph.DeleteLink(link);
            }

            UpdateState();
        }

        //------------------------------------------------------------------------ assembly/beacon module sets

        public void BeaconModulesAdd(ModuleQualityPair module) { beaconModules.Add(module); ioUpdateRequired = true; }
        public void BeaconModulesAddRange(IEnumerable<ModuleQualityPair> modules) { beaconModules.AddRange(modules); ioUpdateRequired = true; }
        public void BeaconModulesRemoveAt(int index) { beaconModules.RemoveAt(index); ioUpdateRequired = true; }
        public void BeaconModulesRemoveAll(ModuleQualityPair module) { beaconModules.RemoveAll(m => m == module); ioUpdateRequired = true; }
        public void BeaconModulesClear() { beaconModules.Clear(); ioUpdateRequired = true; }

        public void AssemblerModulesAdd(ModuleQualityPair module) { assemblerModules.Add(module); ioUpdateRequired = true; }
        public void AssemblerModulesAddRange(IEnumerable<ModuleQualityPair> modules) { assemblerModules.AddRange(modules); ioUpdateRequired = true; }
        public void AssemblerModulesRemoveAt(int index) { assemblerModules.RemoveAt(index); ioUpdateRequired = true; }
        public void AssemblerModulesRemoveAll(ModuleQualityPair module) { assemblerModules.RemoveAll(m => m == module); ioUpdateRequired = true; }
        public void AssemblerModulesClear() { assemblerModules.Clear(); ioUpdateRequired = true; }

        // IBeacon effectivity × count for module math; 0 when no beacon/quality.
        private double BeaconTransmissionFromModules() {
            BeaconQualityPair b = SelectedBeacon;
            return b.Beacon is not IBeacon beacon || b.Quality is not IQuality quality
                ? 0
                : beacon.GetBeaconEffectivity(quality, BeaconCount) * BeaconCount;
        }

        //------------------------------------------------------------------------ multipliers (speed/productivity/consumption/pollution) & rates

        public double GetSpeedMultiplier() {
            if (SelectedAssembler.Assembler.EntityType == EntityType.Rocket) //this is a bit of a hack - by setting the speed multiplier here like so we get the # of buildings to be the # of rockets launched no matter the time scale.
                return 1 / MyGraph.GetRateMultipler();

            double multiplier = 1.0f;
            double beaconTransmission = BeaconTransmissionFromModules();
            foreach (ModuleQualityPair module in AssemblerModules)
                multiplier += module.Module.GetSpeedBonus(module.Quality);
            foreach (ModuleQualityPair beaconModule in BeaconModules)
                multiplier += beaconModule.Module.GetSpeedBonus(beaconModule.Quality) * beaconTransmission;
            return Math.Max(0.2f, multiplier);
        }

        public double GetProductivityBonus() //unlike most of the others, this is the bonus (aka: starts from 0%, not 100%) //also: quality bonus is rounded down to 2 decimal places (1 percent)
        {
            double multiplier = SelectedAssembler.Assembler.BaseProductivityBonus + ExtraProductivityBonus;
            double beaconTransmission = BeaconTransmissionFromModules();
            foreach (ModuleQualityPair module in AssemblerModules)
                multiplier += module.Module.GetProductivityBonus(module.Quality);
            foreach (ModuleQualityPair beaconModule in BeaconModules)
                multiplier += beaconModule.Module.GetProductivityBonus(beaconModule.Quality) * beaconTransmission;
            return Math.Min(Math.Max(0, multiplier), recipeDefinition.MaxProductivityBonus);
        }

        public double GetConsumptionMultiplier() {
            double multiplier = 1.0f;
            double beaconTransmission = BeaconTransmissionFromModules();
            foreach (ModuleQualityPair module in AssemblerModules)
                multiplier += module.Module.GetConsumptionBonus(module.Quality);
            foreach (ModuleQualityPair beaconModule in BeaconModules)
                multiplier += beaconModule.Module.GetConsumptionBonus(beaconModule.Quality) * beaconTransmission;
            return Math.Max(0.2f, multiplier);
        }

        public double GetPollutionMultiplier() {
            double multiplier = 1.0f;
            double beaconTransmission = BeaconTransmissionFromModules();
            foreach (ModuleQualityPair module in AssemblerModules)
                multiplier += module.Module.GetPolutionBonus(module.Quality);
            foreach (ModuleQualityPair beaconModule in BeaconModules)
                multiplier += beaconModule.Module.GetPolutionBonus(beaconModule.Quality) * beaconTransmission;
            return Math.Max(0.2f, multiplier);
        }

        public double GetQualityMultiplier() //unlike the rest this one starts at 0 and is a multiplier (not bonus) - so without modules that add quality the chance to get better quality items from a recipe is 0%
        {
            double multiplier = 0.0f;
            double beaconTransmission = BeaconTransmissionFromModules();
            foreach (ModuleQualityPair module in AssemblerModules)
                multiplier += module.Module.GetQualityBonus(module.Quality);
            foreach (ModuleQualityPair beaconModule in BeaconModules)
                multiplier += beaconModule.Module.GetQualityBonus(beaconModule.Quality) * beaconTransmission;

            return Math.Max(0.0f, multiplier);
        }

        //------------------------------------------------------------------------ graph optimization functions

        public override double GetConsumeRate(ItemQualityPair item) { return inputRateFor(item) * ActualRate; }
        public override double GetSupplyRate(ItemQualityPair item) { return outputRateFor(item) * ActualRate; }

        internal override double inputRateFor(ItemQualityPair item) {
            if (ioUpdateRequired)
                UpdateInputsAndOutputs();
            return inputSet[item];
        }
        internal override double outputRateFor(ItemQualityPair item) {
            if (ioUpdateRequired)
                UpdateInputsAndOutputs();
            return outputSet[item];
        }

        internal double InputRateForFuel() {
            double temperature = double.NaN;
            if (SelectedAssembler.Assembler.IsTemperatureFluidBurner)
                temperature = LinkChecker.GetTemperatureRange(Fuel as IFluid, this, LinkType.Output, false).Min;

            //burner rate = recipe time (modified by speed bonus & assembler) * fuel consumption rate of assembler (modified by fuel, temperature, and consumption modifier)
            return (recipeDefinition.Time / (SelectedAssembler.Assembler.GetSpeed(SelectedAssembler.Quality) * GetSpeedMultiplier())) * SelectedAssembler.Assembler.GetBaseFuelConsumptionRate(Fuel, SelectedAssembler.Quality, temperature) * GetConsumptionMultiplier();
        }

        internal double FactoryRate() {
            return recipeDefinition.Time / (SelectedAssembler.Assembler.GetSpeed(SelectedAssembler.Quality) * GetSpeedMultiplier());
        }

        internal double GetMinOutputRatio() {
            double minValue = double.MaxValue;
            foreach (ItemQualityPair item in Outputs)
                minValue = Math.Min(minValue, outputRateFor(item));
            return minValue;
        }

        public override string ToString() { return string.Format(CultureInfo.InvariantCulture, "Recipe node for: {0} ({1})", recipeDefinition.Name, recipeQuality.Name); }
    }

    public class RecipeNodeController : BaseNodeController {
        private readonly RecipeNode MyNode;

        internal RecipeNodeController(RecipeNode myNode) : base(myNode) { MyNode = myNode; }

        //------------------------------------------------------------------------ warning / errors functions

        public override Dictionary<string, Action> GetErrorResolutions() {
            RecipeNode.Errors ErrorSet = MyNode.ErrorSet;

            var resolutions = new Dictionary<string, Action>();
            if ((ErrorSet & RecipeNode.Errors.RecipeIsMissing) != 0)
                resolutions.Add("Delete node", new Action(() => { this.Delete(); }));
            else {
                if ((ErrorSet & (RecipeNode.Errors.AssemblerIsMissing | RecipeNode.Errors.AQualityIsMissing)) != 0)
                    resolutions.Add("Auto-select assembler & quality", new Action(() => AutoSetAssembler()));

                if ((ErrorSet & (RecipeNode.Errors.FuelIsMissing | RecipeNode.Errors.InvalidFuel)) != 0 && MyNode.SelectedAssembler.Assembler.Fuels.Any(f => !f.IsMissing))
                    resolutions.Add("Auto-select fuel", new Action(() => AutoSetFuel()));

                if ((ErrorSet & RecipeNode.Errors.InvalidFuelRemains) != 0 && MyNode.SelectedAssembler.Assembler.Fuels.Contains(MyNode.Fuel))
                    resolutions.Add("Update burn result", new Action(() => SetFuel(MyNode.Fuel)));

                if ((ErrorSet & (RecipeNode.Errors.AModuleIsMissing | RecipeNode.Errors.AModuleLimitExceeded | RecipeNode.Errors.AModuleQualityIsMissing)) != 0)
                    resolutions.Add("Fix assembler modules", new Action(() => {
                        for (int i = MyNode.AssemblerModules.Count - 1; i >= 0; i--)
                            if (MyNode.AssemblerModules[i].Module.IsMissing || !MyNode.SelectedAssembler.Assembler.Modules.Contains(MyNode.AssemblerModules[i].Module) || !MyNode.RecipeDefinition.AssemblerModules.Contains(MyNode.AssemblerModules[i].Module) || MyNode.AssemblerModules[i].Quality.IsMissing)
                                RemoveAssemblerModule(i);
                        while (MyNode.AssemblerModules.Count > MyNode.SelectedAssembler.Assembler.ModuleSlots)
                            RemoveAssemblerModule(MyNode.AssemblerModules.Count - 1);
                    }));

                if ((ErrorSet & (RecipeNode.Errors.BeaconIsMissing | RecipeNode.Errors.BQualityIsMissing)) != 0)
                    resolutions.Add("Remove Beacon", new Action(() => ClearBeacon()));

                if ((ErrorSet & (RecipeNode.Errors.BModuleIsMissing | RecipeNode.Errors.BModuleLimitExceeded | RecipeNode.Errors.BModuleQualityIsMissing)) != 0)
                    resolutions.Add("Fix beacon modules", new Action(() => {
                        if (MyNode.SelectedBeacon.Beacon is not IBeacon beacon)
                            return;
                        for (int i = MyNode.BeaconModules.Count - 1; i >= 0; i--)
                            if (MyNode.BeaconModules[i].Module.IsMissing || !MyNode.SelectedAssembler.Assembler.Modules.Contains(MyNode.BeaconModules[i].Module) || !MyNode.RecipeDefinition.AssemblerModules.Contains(MyNode.BeaconModules[i].Module) || !beacon.Modules.Contains(MyNode.BeaconModules[i].Module) || MyNode.BeaconModules[i].Quality.IsMissing)
                                RemoveBeaconModule(i);
                        while (MyNode.BeaconModules.Count > beacon.ModuleSlots)
                            RemoveBeaconModule(MyNode.BeaconModules.Count - 1);
                    }));

                foreach (KeyValuePair<string, Action> kvp in GetInvalidConnectionResolutions())
                    resolutions.Add(kvp.Key, kvp.Value);
            }

            return resolutions;
        }

        public override Dictionary<string, Action> GetWarningResolutions() {
            RecipeNode.Warnings WarningSet = MyNode.WarningSet;

            var resolutions = new Dictionary<string, Action>();

            if ((WarningSet & (RecipeNode.Warnings.AssemblerIsDisabled | RecipeNode.Warnings.AssemblerIsUnavailable | RecipeNode.Warnings.AssemblerQualityIsDisabled)) != 0 && (WarningSet & RecipeNode.Warnings.NoAvailableAssemblers) == 0)
                resolutions.Add("Switch to enabled assembler", new Action(() => AutoSetAssembler()));

            if ((WarningSet & (RecipeNode.Warnings.FuelIsUnavailable | RecipeNode.Warnings.FuelIsUncraftable)) != 0 && (WarningSet & RecipeNode.Warnings.NoAvailableFuels) == 0)
                resolutions.Add("Switch to valid fuel", new Action(() => AutoSetFuel()));

            if ((WarningSet & (RecipeNode.Warnings.AModuleIsDisabled | RecipeNode.Warnings.AModuleIsUnavailable | RecipeNode.Warnings.AModulesQualityIsDisabled)) != 0)
                resolutions.Add("Remove error modules from assembler", new Action(() => {
                    for (int i = MyNode.AssemblerModules.Count - 1; i >= 0; i--)
                        if (!MyNode.AssemblerModules[i].Module.Enabled || !MyNode.AssemblerModules[i].Module.Available || !MyNode.AssemblerModules[i].Quality.Enabled)
                            RemoveAssemblerModule(i);
                }));

            if ((WarningSet & (RecipeNode.Warnings.BeaconIsDisabled | RecipeNode.Warnings.BeaconIsUnavailable)) != 0)
                resolutions.Add("Turn off beacon", new Action(() => ClearBeacon()));

            if ((WarningSet & (RecipeNode.Warnings.BModuleIsDisabled | RecipeNode.Warnings.BModuleIsUnavailable | RecipeNode.Warnings.BModulesQualityIsDisabled)) != 0)
                resolutions.Add("Remove error modules from beacon", new Action(() => {
                    for (int i = MyNode.BeaconModules.Count - 1; i >= 0; i--)
                        if (!MyNode.BeaconModules[i].Module.Enabled || !MyNode.BeaconModules[i].Module.Available || !MyNode.BeaconModules[i].Quality.Enabled)
                            RemoveBeaconModule(i);
                }));

            if ((WarningSet & RecipeNode.Warnings.TemeratureFluidBurnerInvalidLinks) != 0 && MyNode.Fuel?.Owner.DefaultQuality is not null)
                resolutions.Add("Remove fuel links", new Action(() => {
                    foreach (NodeLink fuelLink in MyNode.InputLinks.Where(l => l.Item == new ItemQualityPair(MyNode.Fuel, MyNode.Fuel.Owner.DefaultQuality)).ToList())
                        MyNode.MyGraph.DeleteLink(fuelLink);
                }));

            return resolutions;
        }

        //-----------------------------------------------------------------------Set functions

        public void SetPriority(bool lowPriority) { MyNode.LowPriority = lowPriority; MyNode.UpdateState(); }

        public void SetNeighbourCount(double count) { if (MyNode.NeighbourCount != count) MyNode.NeighbourCount = count; }
        public void SetExtraProductivityBonus(double bonus) { if (MyNode.ExtraProductivityBonus != bonus) MyNode.ExtraProductivityBonus = bonus; }
        public void SetBeaconCount(double count) { if (MyNode.BeaconCount != count) MyNode.BeaconCount = count; }
        public void SetBeaconsPerAssembler(double beacons) { if (MyNode.BeaconsPerAssembler != beacons) MyNode.BeaconsPerAssembler = beacons; }
        public void SetBeaconsCont(double beacons) { if (MyNode.BeaconsConst != beacons) MyNode.BeaconsConst = beacons; }

        public void SetAssembler(AssemblerQualityPair assembler) {
            MyNode.SelectedAssembler = assembler;

            //fuel
            if (!assembler.Assembler.IsBurner)
                SetFuel(null);
            else if (MyNode.Fuel != null && assembler.Assembler.Fuels.Contains(MyNode.Fuel))
                SetFuel(MyNode.Fuel);
            else
                AutoSetFuel();

            //check for invalid modules
            for (int i = MyNode.AssemblerModules.Count - 1; i >= 0; i--)
                if (MyNode.AssemblerModules[i].Module.IsMissing ||
                    !MyNode.SelectedAssembler.Assembler.Modules.Contains(MyNode.AssemblerModules[i].Module) ||
                    !MyNode.RecipeDefinition.AssemblerModules.Contains(MyNode.AssemblerModules[i].Module) ||
                    !MyNode.AssemblerModules[i].Quality.Available ||
                    MyNode.AssemblerModules[i].Quality.IsMissing) { MyNode.AssemblerModulesRemoveAt(i); }

            //check for too many modules
            while (MyNode.AssemblerModules.Count > MyNode.SelectedAssembler.Assembler.ModuleSlots)
                MyNode.AssemblerModulesRemoveAt(MyNode.AssemblerModules.Count - 1);

            //check if any modules work (if none work, then turn off beacon)
            if (MyNode.SelectedAssembler.Assembler.Modules.Count == 0 || MyNode.RecipeDefinition.AssemblerModules.Count == 0)
                ClearBeacon();
            else //update beacon
                SetBeacon(MyNode.SelectedBeacon);

            MyNode.UpdateInputsAndOutputs();
            MyNode.UpdateState();
        }

        public void AutoSetAssembler() {
            var quality = (MyNode.SelectedAssembler.Quality.IsMissing || !MyNode.SelectedAssembler.Quality.Enabled) ? MyNode.SelectedAssembler.Assembler.Owner.DefaultQuality : MyNode.SelectedAssembler.Quality;
            IAssembler assembler = MyNode.MyGraph.AssemblerSelector.GetAssembler(MyNode.RecipeDefinition);

            if (quality is not null)
                SetAssembler(new AssemblerQualityPair(assembler, quality));
            AutoSetFuel();
        }

        public void AutoSetAssembler(AssemblerSelector.Style style) {
            var quality = (MyNode.SelectedAssembler.Quality.IsMissing || !MyNode.SelectedAssembler.Quality.Enabled) ? MyNode.SelectedAssembler.Assembler.Owner.DefaultQuality : MyNode.SelectedAssembler.Quality;
            IAssembler assembler = AssemblerSelector.GetAssembler(MyNode.RecipeDefinition, style);

            if (quality is not null)
                SetAssembler(new AssemblerQualityPair(assembler, quality));
            AutoSetFuel();
        }

        public void SetFuel(IItem? fuel) {
            if (MyNode.Fuel != fuel || (MyNode.Fuel == null && MyNode.FuelRemains != null) || (MyNode.Fuel != null && MyNode.Fuel.BurnResult != MyNode.FuelRemains)) {
                //have to remove any links to the burner/burnt item (if they exist) unless the item is also part of the recipe
                if (MyNode.Fuel != null && !MyNode.IsFuelPartOfRecipeInputs && MyNode.Fuel.Owner.DefaultQuality is not null) {
                    var fuelIQP = new ItemQualityPair(MyNode.Fuel, MyNode.Fuel.Owner.DefaultQuality);
                    foreach (NodeLink link in MyNode.InputLinks.Where(link => link.Item == fuelIQP).ToList())
                        link.Controller.Delete();
                }
                if (MyNode.FuelRemains != null && !MyNode.IsFuelRemainsPartOfRecipeOutputs && MyNode.FuelRemains.Owner.DefaultQuality is not null) {
                    var fuelRemainsIQP = new ItemQualityPair(MyNode.FuelRemains, MyNode.FuelRemains.Owner.DefaultQuality);
                    foreach (NodeLink link in MyNode.OutputLinks.Where(link => link.Item == fuelRemainsIQP).ToList())
                        link.Controller.Delete();
                }

                MyNode.Fuel = fuel;
                MyNode.MyGraph.FuelSelector.UseFuel(fuel);
                MyNode.UpdateState();
            }
        }

        public void AutoSetFuel() {
            SetFuel(MyNode.MyGraph.FuelSelector.GetFuel(MyNode.SelectedAssembler.Assembler));
        }

        public void ClearBeacon() {
            MyNode.SelectedBeacon = new BeaconQualityPair(/*"clearing beacon"*/);
            MyNode.BeaconModulesClear();
            MyNode.BeaconCount = 0;
            MyNode.BeaconsPerAssembler = 0;
            MyNode.BeaconsConst = 0;
            MyNode.UpdateState();
        }

        public void SetBeacon(BeaconQualityPair beacon) {
            if (!beacon) { ClearBeacon(); return; } //shouldnt be called - but whatever
            if (beacon.Beacon is not IBeacon beaconEntity) {
                ClearBeacon();
                return;
            }

            MyNode.SelectedBeacon = beacon;
            //check for invalid modules
            for (int i = MyNode.BeaconModules.Count - 1; i >= 0; i--) {
                if (MyNode.BeaconModules[i].Module.IsMissing ||
                    !MyNode.SelectedAssembler.Assembler.Modules.Contains(MyNode.BeaconModules[i].Module) ||
                    !MyNode.RecipeDefinition.AssemblerModules.Contains(MyNode.BeaconModules[i].Module) ||
                    !beaconEntity.Modules.Contains(MyNode.BeaconModules[i].Module) ||
                    !MyNode.BeaconModules[i].Quality.Available ||
                    MyNode.BeaconModules[i].Quality.IsMissing) { MyNode.BeaconModulesRemoveAt(i); }
            }
            //check for too many modules
            while (MyNode.BeaconModules.Count > beaconEntity.ModuleSlots)
                MyNode.BeaconModulesRemoveAt(MyNode.BeaconModules.Count - 1);

            MyNode.UpdateState();
        }

        public void AddAssemblerModule(ModuleQualityPair module) {
            MyNode.AssemblerModulesAdd(module);
            MyNode.UpdateState();
        }

        public void AddAssemblerModules(ModuleQualityPair module) {
            while (MyNode.AssemblerModules.Count < MyNode.SelectedAssembler.Assembler.ModuleSlots)
                MyNode.AssemblerModulesAdd(module);
            MyNode.UpdateState();
        }

        public void RemoveAssemblerModule(int index) {
            if (index >= 0 && index < MyNode.AssemblerModules.Count)
                MyNode.AssemblerModulesRemoveAt(index);
            MyNode.UpdateState();
        }

        public void RemoveAssemblerModules(ModuleQualityPair module) {
            MyNode.AssemblerModulesRemoveAll(module);
            MyNode.UpdateState();
        }

        public void RemoveAssemblerModules() {
            MyNode.AssemblerModulesClear();
            MyNode.UpdateState();
        }

        public void SetAssemblerModules(IEnumerable<ModuleQualityPair> modules, bool filterModules) {
            MyNode.AssemblerModulesClear();
            if (modules != null) {
                if (filterModules) {
                    var acceptableModules = new HashSet<IModule>(MyNode.RecipeDefinition.AssemblerModules.Intersect(MyNode.SelectedAssembler.Assembler.Modules));
                    foreach (ModuleQualityPair m in modules)
                        if (MyNode.AssemblerModules.Count < MyNode.SelectedAssembler.Assembler.ModuleSlots && acceptableModules.Contains(m.Module))
                            MyNode.AssemblerModulesAdd(m);
                } else
                    MyNode.AssemblerModulesAddRange(modules);
            }
            MyNode.UpdateState();
        }

        public void AutoSetAssemblerModules() {
            MyNode.AssemblerModulesClear();
            MyNode.AssemblerModulesAddRange(MyNode.MyGraph.ModuleSelector
                .GetModules(MyNode.SelectedAssembler.Assembler, MyNode.RecipeDefinition)
                .Select(i => i.Owner.DefaultQuality is IQuality q ? (i, q) : ((IModule, IQuality)?)null)
                .OfType<(IModule, IQuality)>()
                .Select(i => new ModuleQualityPair(i.Item1, i.Item2)));
            MyNode.UpdateState();
        }

        public void AutoSetAssemblerModules(ModuleSelector.Style style) {
            MyNode.AssemblerModulesClear();
            MyNode.AssemblerModulesAddRange(ModuleSelector
                .GetModules(MyNode.SelectedAssembler.Assembler, MyNode.RecipeDefinition, style)
                .Select(i => i.Owner.DefaultQuality is IQuality q ? (i, q) : ((IModule, IQuality)?)null)
                .OfType<(IModule, IQuality)>()
                .Select(i => new ModuleQualityPair(i.Item1, i.Item2)));
            MyNode.UpdateState();
        }

        public void AddBeaconModule(ModuleQualityPair module) {
            MyNode.BeaconModulesAdd(module);
            MyNode.UpdateState();
        }

        public void AddBeaconModules(ModuleQualityPair module) {
            if (MyNode.SelectedBeacon.Beacon is not IBeacon beacon)
                return;
            while (MyNode.BeaconModules.Count < beacon.ModuleSlots)
                MyNode.BeaconModulesAdd(module);
            MyNode.UpdateState();
        }

        public void RemoveBeaconModule(int index) {
            if (index >= 0 && index < MyNode.BeaconModules.Count)
                MyNode.BeaconModulesRemoveAt(index);
            MyNode.UpdateState();
        }

        public void RemoveBeaconModules(ModuleQualityPair module) {
            MyNode.BeaconModulesRemoveAll(module);
            MyNode.UpdateState();
        }

        public void SetBeaconModules(IEnumerable<ModuleQualityPair> modules, bool filterModules) {
            MyNode.BeaconModulesClear();
            if (modules != null) {
                if (filterModules) {
                    if (MyNode.SelectedBeacon.Beacon is not IBeacon beacon)
                        return;
                    var acceptableModules = new HashSet<IModule>(MyNode.RecipeDefinition.AssemblerModules.Intersect(MyNode.SelectedAssembler.Assembler.Modules).Intersect(beacon.Modules));
                    foreach (ModuleQualityPair m in modules)
                        if (MyNode.BeaconModules.Count < beacon.ModuleSlots && acceptableModules.Contains(m.Module))
                            MyNode.BeaconModulesAdd(m);
                } else
                    MyNode.BeaconModulesAddRange(modules);
            }
            MyNode.UpdateState();
        }
    }
}
