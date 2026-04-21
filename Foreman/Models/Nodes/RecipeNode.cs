using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman {
    public class RecipeNode : BaseNode {
        [Flags]
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
            BQualityIsMissing = 0b_0010_000_0000_0000,
            AModuleQualityIsMissing = 0b_0100_0000_0000_0000,
            BModuleQualityIsMissing = 0b_1000_0000_0000_0000,

            InvalidLinks = 0b_1000_0000_000_0000_0000
        }

        [Flags]
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

            TemperatureFluidBurnerInvalidLinks = 0b_0100_0000_0000_0000,
        }

        public Errors ErrorSet { get; private set; }
        public Warnings WarningSet { get; private set; }

        private readonly RecipeNodeController _controller;

        public override BaseNodeController Controller => _controller;

        public bool LowPriority { get; set; }

        public readonly RecipeQualityPair BaseRecipe;
        private double _neighbourCount;

        public double NeighbourCount {
            get => _neighbourCount;
            set {
                if (!(Math.Abs(_neighbourCount - value) > double.Epsilon))
                    return;

                _neighbourCount = value;
                _ioUpdateRequired = true;
                UpdateState();
                OnNodeValuesChanged();
            }
        }

        private readonly DataCache _recipeOwner;

        private AssemblerQualityPair _assembler;

        public AssemblerQualityPair SelectedAssembler {
            get => _assembler;
            set {
                if (!value || _assembler == value)
                    return;

                _assembler = value;
                _ioUpdateRequired = true;
                UpdateState();
                OnNodeStateChanged();
            }
        }

        public Item Fuel {
            get => _fuel;
            set {
                if (_fuel == value)
                    return;

                _fuel = value;
                _fuelRemainsOverride = null;
                _ioUpdateRequired = true;
                UpdateState();
                OnNodeStateChanged();
            }
        }

        public Item FuelRemains {
            get {
                if (_fuelRemainsOverride != null) return _fuelRemainsOverride;
                return Fuel is { BurnResult: not null } ? Fuel.BurnResult : null;
            }
        }

        public void SetBurntOverride(Item item) {
            if (Fuel != null && Fuel.BurnResult == item)
                return;

            _fuelRemainsOverride = item;
            _ioUpdateRequired = true;
            UpdateState();
            OnNodeValuesChanged();
        }

        private Item _fuel;
        // returns as BurntItem if set (error import)
        private Item _fuelRemainsOverride;

        private BeaconQualityPair _selectedBeacon;

        public BeaconQualityPair SelectedBeacon {
            get => _selectedBeacon;
            set {
                if (_selectedBeacon == value)
                    return;

                _selectedBeacon = value;
                _ioUpdateRequired = true;
                UpdateState();
                OnNodeValuesChanged();
            }
        }

        private double _beaconCount;

        public double BeaconCount {
            get => _beaconCount;
            set {
                if (!(Math.Abs(_beaconCount - value) > double.Epsilon))
                    return;

                _beaconCount = value;
                _ioUpdateRequired = true;
                UpdateState();
                OnNodeValuesChanged();
            }
        }

        private double _beaconsPerAssembler;

        public double BeaconsPerAssembler {
            get => _beaconsPerAssembler;
            set {
                if (!(Math.Abs(_beaconsPerAssembler - value) > double.Epsilon))
                    return;

                _beaconsPerAssembler = value;
                UpdateState();
                OnNodeValuesChanged();
            }
        }

        private double _beaconsConst;

        public double BeaconsConst {
            get => _beaconsConst;
            set {
                if (!(Math.Abs(_beaconsConst - value) > double.Epsilon))
                    return;

                _beaconsConst = value;
                UpdateState();
                OnNodeValuesChanged();
            }
        }

        public IReadOnlyList<ModuleQualityPair> AssemblerModules => _assemblerModules;

        public IReadOnlyList<ModuleQualityPair> BeaconModules => _beaconModules;

        private List<ModuleQualityPair> _assemblerModules;
        private List<ModuleQualityPair> _beaconModules;

        // for recipe nodes, the SetValue is 'number of assemblers/entities'
        public override double ActualSetValue => ActualRatePerSec
            * BaseRecipe.Recipe.Time
            / (SelectedAssembler.Assembler.GetSpeed(SelectedAssembler.Quality) * GetSpeedMultiplier());

        public override double DesiredSetValue { get; set; }

        public override double MaxDesiredSetValue => ProductionGraph.MaxFactories;

        public override string SetValueDescription => "# of Assemblers:";

        public override double DesiredRatePerSec {
            get => DesiredSetValue * SelectedAssembler.Assembler.GetSpeed(SelectedAssembler.Quality) * GetSpeedMultiplier() / BaseRecipe.Recipe.Time;
            set {
                _neighbourCount = value;
                Trace.Fail("Desired rate set on a recipe node!");
            }
        }

        private double _extraProductivityBonus;

        public double ExtraProductivityBonus {
            get => _extraProductivityBonus;
            set {
                if (!(Math.Abs(_extraProductivityBonus - value) > double.Epsilon))
                    return;

                _extraProductivityBonus = value;
                _ioUpdateRequired = true;
                UpdateState();
                OnNodeValuesChanged();
            }
        }

        // if quality bonus > 0 then we will take this many extra quality steps for products
        public uint MaxQualitySteps {
            get => _maxQualitySteps;
            set {
                if (_maxQualitySteps == value)
                    return;

                _maxQualitySteps = value;
                _ioUpdateRequired = true;
            }
        }

        private uint _maxQualitySteps;

        public override IEnumerable<ItemQualityPair> Inputs {
            get {
                if (_ioUpdateRequired) {
                    UpdateInputsAndOutputs();
                }

                return _inputList;
            }
        }

        private Dictionary<ItemQualityPair, double> _inputSet;
        private List<ItemQualityPair> _inputList;

        public override IEnumerable<ItemQualityPair> Outputs {
            get {
                if (_ioUpdateRequired) {
                    UpdateInputsAndOutputs();
                }

                return _outputList;
            }
        }

        private Dictionary<ItemQualityPair, double> _outputSet;
        private List<ItemQualityPair> _outputList;

        public bool IsFuelPartOfRecipeInputs { get; private set; }
        public bool IsFuelRemainsPartOfRecipeOutputs { get; private set; }

        private bool _ioUpdateRequired;

        public RecipeNode(ProductionGraph graph, int nodeId, RecipeQualityPair recipe, Quality assemblerQuality) : base(graph, nodeId) {
            LowPriority = false;
            _maxQualitySteps = graph.MaxQualitySteps;
            _ioUpdateRequired = false;

            BaseRecipe = recipe;
            _recipeOwner = recipe.Recipe.Owner;

            _controller = RecipeNodeController.GetController(this);
            ReadOnlyNode = new ReadOnlyRecipeNode(this);

            _inputSet = new Dictionary<ItemQualityPair, double>();
            _inputList = [];
            _outputSet = new Dictionary<ItemQualityPair, double>();
            _outputList = [];

            _assemblerModules = [];
            _beaconModules = [];

            // everything here works under the assumption that assembler isn't null.
            SelectedAssembler = new AssemblerQualityPair(recipe.Recipe.Assemblers.First(), assemblerQuality);
            SelectedBeacon = new BeaconQualityPair("no beacon selected");
            NeighbourCount = 0;

            BeaconCount = 0;
            BeaconsPerAssembler = 0;
            BeaconsConst = 0;

            ExtraProductivityBonus = 0;
        }

        internal override NodeState GetUpdatedState() {
            WarningSet = Warnings.Clean;
            ErrorSet = Errors.Clean;

            // error states:

            if (BaseRecipe.Recipe.IsMissing)
                ErrorSet |= Errors.RecipeIsMissing;
            if (BaseRecipe.Quality.IsMissing)
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

            if (SelectedBeacon) {
                if (SelectedBeacon.Beacon.IsMissing)
                    ErrorSet |= Errors.BeaconIsMissing;
                if (SelectedBeacon.Quality.IsMissing)
                    ErrorSet |= Errors.BQualityIsMissing;
                if (BeaconModules.Any(m => m.Module.IsMissing))
                    ErrorSet |= Errors.BModuleIsMissing;
                if (BeaconModules.Count > SelectedBeacon.Beacon.ModuleSlots)
                    ErrorSet |= Errors.BModuleLimitExceeded;
                if (BeaconModules.Any(m => m.Quality.IsMissing))
                    ErrorSet |= Errors.AModuleQualityIsMissing;
            } else if (BeaconModules.Count != 0)
                ErrorSet |= Errors.BModuleLimitExceeded;

            if (!AllLinksValid)
                ErrorSet |= Errors.InvalidLinks;

            // warnings are NOT processed if error has been found.
            // This makes sense (as an error is something that trumps warnings),
            // plus guarantees we don't accidentally check statuses of missing objects (which rightfully don't exist in regular cache)

            if (ErrorSet != Errors.Clean)
                return NodeState.Error;

            // warning states (either not enabled or not available both throw up warnings)

            if (!BaseRecipe.Recipe.Enabled)
                WarningSet |= Warnings.RecipeIsDisabled;
            if (!BaseRecipe.Recipe.Available)
                WarningSet |= Warnings.RecipeIsUnavailable;

            if (!SelectedAssembler.Assembler.Enabled)
                WarningSet |= Warnings.AssemblerIsDisabled;
            if (!SelectedAssembler.Assembler.Available)
                WarningSet |= Warnings.AssemblerIsUnavailable;
            if (!SelectedAssembler.Quality.Enabled)
                WarningSet |= Warnings.AssemblerQualityIsDisabled;
            if (!BaseRecipe.Recipe.Assemblers.Any(a => a.Enabled))
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

            if (SelectedBeacon) {
                if (!SelectedBeacon.Beacon.Enabled)
                    WarningSet |= Warnings.BeaconIsDisabled;
                if (!SelectedBeacon.Beacon.Available)
                    WarningSet |= Warnings.BeaconIsUnavailable;
                if (!SelectedBeacon.Quality.Enabled)
                    WarningSet |= Warnings.BeaconQualityIsDisabled;
            }

            if (BeaconModules.Any(m => !m.Module.Enabled))
                WarningSet |= Warnings.BModuleIsDisabled;
            if (BeaconModules.Any(m => !m.Module.Available))
                WarningSet |= Warnings.BModuleIsUnavailable;
            if (BeaconModules.Any(m => !m.Quality.Enabled))
                WarningSet |= Warnings.BModulesQualityIsDisabled;

            if (SelectedAssembler.Assembler.IsTemperatureFluidBurner &&
                !LinkChecker.GetTemperatureRange(Fuel as Fluid, ReadOnlyNode, LinkType.Output, false).IsPoint())
                WarningSet |= Warnings.TemperatureFluidBurnerInvalidLinks;

            if (WarningSet != Warnings.Clean)
                return NodeState.Warning;
            if (AllLinksConnected)
                return NodeState.Clean;
            return NodeState.MissingLink;
        }

        public void UpdateInputsAndOutputs(bool forceUpdate = false) {
            if (!_ioUpdateRequired && !forceUpdate)
                return;
            _ioUpdateRequired = false;

            // Inputs:

            _inputSet.Clear();
            _inputList.Clear();
            foreach (var item in BaseRecipe.Recipe.IngredientList) {
                var inputItem = new ItemQualityPair(item, item is Fluid ? _recipeOwner.DefaultQuality : BaseRecipe.Quality);
                var inputQuantity = BaseRecipe.Recipe.IngredientSet[item];

                _inputList.Add(inputItem);
                _inputSet.Add(inputItem, inputQuantity);
            }

            // provide the burner item if it isn't null or already part of recipe ingredients

            if (Fuel != null) {
                var fuelIqp = new ItemQualityPair(Fuel, _recipeOwner.DefaultQuality);
                if (!_inputSet.ContainsKey(fuelIqp)) {
                    IsFuelPartOfRecipeInputs = false;
                    _inputList.Add(fuelIqp);
                    _inputSet.Add(fuelIqp, InputRateForFuel());
                } else {
                    IsFuelPartOfRecipeInputs = true;
                    _inputSet[fuelIqp] += InputRateForFuel();
                }
            }

            // Outputs:

            _outputSet.Clear();
            _outputList.Clear();
            foreach (var item in BaseRecipe.Recipe.ProductList) {
                if (SelectedAssembler.Assembler.EntityType == EntityType.Reactor) {
                    var product = new ItemQualityPair(item, _recipeOwner.DefaultQuality);
                    var amount = BaseRecipe.Recipe.ProductSet[item] + 1 * SelectedAssembler.Assembler.NeighbourBonus * NeighbourCount;
                    _outputList.Add(product);
                    _outputSet.Add(product, amount);
                } else {
                    var amount = BaseRecipe.Recipe.ProductSet[item] + BaseRecipe.Recipe.ProductPSet[item] * GetProductivityBonus();

                    if (item is Fluid) {
                        var fluidProduct = new ItemQualityPair(item, _recipeOwner.DefaultQuality);
                        _outputList.Add(fluidProduct);
                        _outputSet.Add(fluidProduct, amount);
                    } else {
                        var currentProduct = new ItemQualityPair(item, BaseRecipe.Quality);
                        uint currentStep = 1;
                        _outputList.Add(currentProduct);
                        _outputSet.Add(currentProduct, amount);
                        var currentMultiplier = GetQualityMultiplier();
                        while (currentStep < MaxQualitySteps && currentProduct.Quality.NextQuality != null) {
                            currentStep++;
                            var lastProduct = currentProduct;
                            currentMultiplier *= currentProduct.Quality.NextProbability;
                            currentProduct = new ItemQualityPair(item, currentProduct.Quality.NextQuality);
                            if (currentMultiplier == 0)
                                break;
                            if (!currentProduct.Quality.Enabled || !currentProduct.Quality.Available)
                                break;

                            _outputList.Add(currentProduct);
                            _outputSet.Add(currentProduct, Math.Min(currentMultiplier, 1.0) * amount);
                            _outputSet[lastProduct] -= _outputSet[currentProduct];

                            if (_outputSet[lastProduct] <= 0) {
                                _outputList.Remove(lastProduct);
                                _outputSet.Remove(lastProduct);
                            }
                        }
                    }
                }
            }

            // provide the burnt item if it isn't null or already part of recipe ingredients

            if (FuelRemains != null) {
                var fuelRemainsIqp = new ItemQualityPair(FuelRemains, _recipeOwner.DefaultQuality);
                if (!_outputSet.ContainsKey(fuelRemainsIqp)) {
                    IsFuelRemainsPartOfRecipeOutputs = false;
                    _outputList.Add(fuelRemainsIqp);
                    _outputSet.Add(fuelRemainsIqp, InputRateForFuel());
                } else {
                    IsFuelRemainsPartOfRecipeOutputs = true;
                    _outputSet[fuelRemainsIqp] += InputRateForFuel();
                }
            }

            // links

            foreach (var link in InputLinks.ToList()) {
                if (!_inputSet.ContainsKey(link.Item))
                    MyGraph.DeleteLink(link.ReadOnlyLink);
            }

            foreach (var link in OutputLinks.ToList()) {
                if (!_outputSet.ContainsKey(link.Item))
                    MyGraph.DeleteLink(link.ReadOnlyLink);
            }

            UpdateState();
        }

        //------------------------------------------------------------------------ assembly/beacon module sets

        public void BeaconModulesAdd(ModuleQualityPair module) {
            _beaconModules.Add(module);
            _ioUpdateRequired = true;
        }

        public void BeaconModulesAddRange(IEnumerable<ModuleQualityPair> modules) {
            _beaconModules.AddRange(modules);
            _ioUpdateRequired = true;
        }

        public void BeaconModulesRemoveAt(int index) {
            _beaconModules.RemoveAt(index);
            _ioUpdateRequired = true;
        }

        public void BeaconModulesRemoveAll(ModuleQualityPair module) {
            _beaconModules.RemoveAll(m => m == module);
            _ioUpdateRequired = true;
        }

        public void BeaconModulesClear() {
            _beaconModules.Clear();
            _ioUpdateRequired = true;
        }

        public void AssemblerModulesAdd(ModuleQualityPair module) {
            _assemblerModules.Add(module);
            _ioUpdateRequired = true;
        }

        public void AssemblerModulesAddRange(IEnumerable<ModuleQualityPair> modules) {
            _assemblerModules.AddRange(modules);
            _ioUpdateRequired = true;
        }

        public void AssemblerModulesRemoveAt(int index) {
            _assemblerModules.RemoveAt(index);
            _ioUpdateRequired = true;
        }

        public void AssemblerModulesRemoveAll(ModuleQualityPair module) {
            _assemblerModules.RemoveAll(m => m == module);
            _ioUpdateRequired = true;
        }

        public void AssemblerModulesClear() {
            _assemblerModules.Clear();
            _ioUpdateRequired = true;
        }


        //------------------------------------------------------------------------ multipliers (speed/productivity/consumption/pollution) & rates

        public double GetSpeedMultiplier() {
            // this is a bit of a hack - by setting the speed multiplier here like
            // so we get the # of buildings to be the # of rockets launched no matter the timescale.

            if (SelectedAssembler.Assembler.EntityType == EntityType.Rocket)
                return 1 / MyGraph.GetRateMultiplier();

            var multiplier = 1.0f
                + AssemblerModules.Sum(module => module.Module.GetSpeedBonus(module.Quality))
                + BeaconModules.Sum(beaconModule => beaconModule.Module.GetSpeedBonus(beaconModule.Quality)
                    * SelectedBeacon.Beacon.GetBeaconEffectivity(SelectedBeacon.Quality, BeaconCount) * BeaconCount);
            return Math.Max(0.2f, multiplier);
        }

        // unlike most of the others, this is the bonus (aka: starts from 0%, not 100%) //also: quality bonus is rounded down to 2 decimal places (1 percent)
        public double GetProductivityBonus() {
            var multiplier = SelectedAssembler.Assembler.BaseProductivityBonus
                + ExtraProductivityBonus
                + AssemblerModules.Sum(module => module.Module.GetProductivityBonus(module.Quality)) + BeaconModules.Sum(beaconModule =>
                    beaconModule.Module.GetProductivityBonus(beaconModule.Quality)
                    * SelectedBeacon.Beacon.GetBeaconEffectivity(SelectedBeacon.Quality, BeaconCount) * BeaconCount);
            return Math.Min(Math.Max(0, multiplier), BaseRecipe.Recipe.MaxProductivityBonus);
        }

        public double GetConsumptionMultiplier() {
            var multiplier = 1.0f
                + AssemblerModules.Sum(module => module.Module.GetConsumptionBonus(module.Quality))
                + BeaconModules.Sum(beaconModule => beaconModule.Module.GetConsumptionBonus(beaconModule.Quality)
                    * SelectedBeacon.Beacon.GetBeaconEffectivity(SelectedBeacon.Quality, BeaconCount) * BeaconCount);
            return Math.Max(0.2f, multiplier);
        }

        public double GetPollutionMultiplier() {
            var multiplier = 1.0f
                + AssemblerModules.Sum(module => module.Module.GetPollutionBonus(module.Quality))
                + BeaconModules.Sum(beaconModule =>
                    beaconModule.Module.GetPollutionBonus(beaconModule.Quality)
                    * SelectedBeacon.Beacon.GetBeaconEffectivity(SelectedBeacon.Quality, BeaconCount) * BeaconCount);
            return Math.Max(0.2f, multiplier);
        }

        // unlike the rest this one starts at 0 and is a multiplier (not bonus)
        // - so without modules that add quality the chance to get better quality items from a recipe is 0%
        public double GetQualityMultiplier() {
            var multiplier = AssemblerModules.Sum(module => module.Module.GetQualityBonus(module.Quality))
                + BeaconModules.Sum(beaconModule => beaconModule.Module.GetQualityBonus(beaconModule.Quality)
                    * SelectedBeacon.Beacon.GetBeaconEffectivity(SelectedBeacon.Quality, BeaconCount)
                    * BeaconCount);
            return Math.Max(0.0f, multiplier);
        }

        //------------------------------------------------------------------------ graph optimization functions

        public override double GetConsumeRate(ItemQualityPair item) {
            return InputRateFor(item) * ActualRate;
        }

        public override double GetSupplyRate(ItemQualityPair item) {
            return OutputRateFor(item) * ActualRate;
        }

        internal override double InputRateFor(ItemQualityPair item) {
            if (_ioUpdateRequired)
                UpdateInputsAndOutputs();
            return _inputSet[item];
        }

        internal override double OutputRateFor(ItemQualityPair item) {
            if (_ioUpdateRequired)
                UpdateInputsAndOutputs();
            return _outputSet[item];
        }

        internal double InputRateForFuel() {
            var temperature = double.NaN;
            if (SelectedAssembler.Assembler.IsTemperatureFluidBurner)
                temperature = LinkChecker.GetTemperatureRange(Fuel as Fluid, ReadOnlyNode, LinkType.Output, false).Min;

            // burner rate = recipe time (modified by speed bonus & assembler)
            // * fuel consumption rate of assembler (modified by fuel, temperature, and consumption modifier)
            return BaseRecipe.Recipe.Time / (SelectedAssembler.Assembler.GetSpeed(SelectedAssembler.Quality) * GetSpeedMultiplier()) *
                SelectedAssembler.Assembler.GetBaseFuelConsumptionRate(Fuel, SelectedAssembler.Quality, temperature) * GetConsumptionMultiplier();
        }

        internal double FactoryRate() {
            return BaseRecipe.Recipe.Time / (SelectedAssembler.Assembler.GetSpeed(SelectedAssembler.Quality) * GetSpeedMultiplier());
        }

        internal double GetMinOutputRatio() {
            return Outputs.Select(OutputRateFor).Prepend(double.MaxValue).Min();
        }

        //------------------------------------------------------------------------object save & string

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);

            info.AddValue("NodeType", NodeType.Recipe);
            info.AddValue("RecipeID", BaseRecipe.Recipe.RecipeID);
            info.AddValue("RecipeQuality", BaseRecipe.Quality.Name);
            info.AddValue("Neighbours", NeighbourCount);
            info.AddValue("ExtraProductivity", ExtraProductivityBonus);

            if (LowPriority)
                info.AddValue("LowPriority", 1);

            // assembler can not be null!

            info.AddValue("Assembler", SelectedAssembler.Assembler.Name);
            info.AddValue("AssemblerQuality", SelectedAssembler.Quality.Name);
            info.AddValue("AssemblerModules", AssemblerModules);

            // fuel is assumed to always be of the 'default quality' - whatever this is for the current datacache

            if (Fuel != null)
                info.AddValue("Fuel", Fuel.Name);
            if (FuelRemains != null)
                info.AddValue("Burnt", FuelRemains.Name);

            if (SelectedBeacon) {
                info.AddValue("Beacon", SelectedBeacon.Beacon.Name);
                info.AddValue("BeaconQuality", SelectedBeacon.Quality.Name);
                info.AddValue("BeaconModules", BeaconModules);
                info.AddValue("BeaconCount", BeaconCount);
                info.AddValue("BeaconsPerAssembler", BeaconsPerAssembler);
                info.AddValue("BeaconsConst", BeaconsConst);
            }
        }

        public override string ToString() {
            return $"Recipe node for: {BaseRecipe.Recipe.Name} ({BaseRecipe.Quality.Name})";
        }
    }

    public class ReadOnlyRecipeNode(RecipeNode node) : ReadOnlyBaseNode(node) {
        public bool LowPriority => node.LowPriority;

        public uint MaxQualitySteps => node.MaxQualitySteps;

        public RecipeQualityPair BaseRecipe => node.BaseRecipe;
        public AssemblerQualityPair SelectedAssembler => node.SelectedAssembler;
        public Item Fuel => node.Fuel;
        public Item FuelRemains => node.FuelRemains;
        public IReadOnlyList<ModuleQualityPair> AssemblerModules => node.AssemblerModules;

        public BeaconQualityPair SelectedBeacon => node.SelectedBeacon;
        public IReadOnlyList<ModuleQualityPair> BeaconModules => node.BeaconModules;

        public double NeighbourCount => node.NeighbourCount;
        public double ExtraProductivity => node.ExtraProductivityBonus;
        public double BeaconCount => node.BeaconCount;
        public double BeaconsPerAssembler => node.BeaconsPerAssembler;
        public double BeaconsConst => node.BeaconsConst;

        public double GetConsumptionMultiplier() => node.GetConsumptionMultiplier();
        public double GetSpeedMultiplier() => node.GetSpeedMultiplier();
        public double GetProductivityMultiplier() => node.GetProductivityBonus() + 1;
        public double GetPollutionMultiplier() => node.GetPollutionMultiplier();
        public double GetQualityMultiplier() => node.GetQualityMultiplier();

        //------------------------------------------------------------------------ warning / errors functions

        public override List<string> GetErrors() {
            var errorSet = node.ErrorSet;

            var output = new List<string>();

            if ((errorSet & RecipeNode.Errors.RecipeIsMissing) != 0) {
                output.Add($"> Recipe \"{node.BaseRecipe.Recipe.FriendlyName}\" doesn't exist in preset!");
                // missing recipe is an automatic end -> we don't care about any other errors, since the only solution is to delete the node.
                return output;
            }

            if ((errorSet & RecipeNode.Errors.RQualityIsMissing) != 0)
                output.Add($"> Recipe's Quality \"{node.BaseRecipe.Quality.FriendlyName}\" doesn't exist in preset!");

            if ((errorSet & RecipeNode.Errors.AssemblerIsMissing) != 0)
                output.Add($"> Assembler \"{node.SelectedAssembler.Assembler.FriendlyName}\" doesn't exist in preset!");
            if ((errorSet & RecipeNode.Errors.AQualityIsMissing) != 0)
                output.Add($"> Assembler's Quality \"{node.SelectedAssembler.Quality.FriendlyName}\" doesn't exist in preset!");

            if ((errorSet & RecipeNode.Errors.BurnerNoFuelSet) != 0)
                output.Add("> Burner Assembler has no fuel set!");
            if ((errorSet & RecipeNode.Errors.FuelIsMissing) != 0)
                output.Add("> Burner Assembler's fuel doesn't exist in preset!");
            if ((errorSet & RecipeNode.Errors.InvalidFuel) != 0)
                output.Add("> Burner Assembler has an invalid fuel set!");
            if ((errorSet & RecipeNode.Errors.InvalidFuelRemains) != 0)
                output.Add("> Burning result doesn't match fuel's burn result!");
            if ((errorSet & RecipeNode.Errors.AModuleIsMissing) != 0)
                output.Add("> Some of the assembler modules don't exist in preset!");
            if ((errorSet & RecipeNode.Errors.AModuleLimitExceeded) != 0)
                output.Add($"> Assembler has too many modules ({node.AssemblerModules.Count}/{node.SelectedAssembler.Assembler.ModuleSlots})!");
            if ((errorSet & RecipeNode.Errors.AModuleQualityIsMissing) != 0)
                output.Add(
                    $"> Assembler's Module's Quality \"{node.AssemblerModules.First(m => m.Quality.IsMissing).Quality.FriendlyName}\" doesn't exist in preset!");

            if ((errorSet & RecipeNode.Errors.BeaconIsMissing) != 0)
                output.Add($"> Beacon \"{node.SelectedBeacon.Beacon.FriendlyName}\" doesn't exist in preset!");
            if ((errorSet & RecipeNode.Errors.BQualityIsMissing) != 0)
                output.Add($"> Beacon's Quality \"{node.SelectedBeacon.Quality.FriendlyName}\" doesn't exist in preset!");

            if ((errorSet & RecipeNode.Errors.BModuleIsMissing) != 0)
                output.Add("> Some of the beacon modules don't exist in preset!");
            if ((errorSet & RecipeNode.Errors.BModuleLimitExceeded) != 0)
                output.Add("> Beacon has too many modules!");
            if ((errorSet & RecipeNode.Errors.BModuleQualityIsMissing) != 0)
                output.Add(
                    $"> Beacon's Module's Quality \"{node.BeaconModules.First(m => m.Quality.IsMissing).Quality.FriendlyName}\" doesn't exist in preset!");

            if ((errorSet & RecipeNode.Errors.InvalidLinks) != 0)
                output.Add("> Some links are invalid!");

            return output;
        }

        public override List<string> GetWarnings() {
            var warningSet = node.WarningSet;

            var output = new List<string>();

            //recipe
            if ((warningSet & RecipeNode.Warnings.RecipeIsDisabled) != 0)
                output.Add("X> Selected recipe is disabled.");
            if ((warningSet & RecipeNode.Warnings.RecipeIsUnavailable) != 0)
                output.Add("X> Selected recipe is unavailable in regular play.");

            if ((warningSet & RecipeNode.Warnings.NoAvailableAssemblers) != 0)
                output.Add("X> No enabled assemblers for this recipe.");
            else {
                if ((warningSet & RecipeNode.Warnings.AssemblerIsDisabled) != 0)
                    output.Add("> Selected assembler is disabled.");
                if ((warningSet & RecipeNode.Warnings.AssemblerIsUnavailable) != 0)
                    output.Add("> Selected assembler is unavailable in regular play.");
            }

            //fuel
            if ((warningSet & RecipeNode.Warnings.NoAvailableFuels) != 0)
                output.Add("X> No fuel can be produced.");
            else {
                if ((warningSet & RecipeNode.Warnings.FuelIsUnavailable) != 0)
                    output.Add("> Selected fuel is unavailable in regular play.");
                if ((warningSet & RecipeNode.Warnings.FuelIsUncraftable) != 0)
                    output.Add("> Selected fuel cant be produced.");
            }

            if ((warningSet & RecipeNode.Warnings.TemperatureFluidBurnerInvalidLinks) != 0)
                output.Add("> Temperature based fuel uses multiple incoming temperatures (fuel use # might be wrong).");

            //modules & beacon modules
            if ((warningSet & RecipeNode.Warnings.AModuleIsDisabled) != 0)
                output.Add("> Some selected assembler modules are disabled.");
            if ((warningSet & RecipeNode.Warnings.AModuleIsUnavailable) != 0)
                output.Add("> Some selected assembler modules are unavailable in regular play.");
            if ((warningSet & RecipeNode.Warnings.BeaconIsDisabled) != 0)
                output.Add("> Selected beacon is disabled.");
            if ((warningSet & RecipeNode.Warnings.BeaconIsUnavailable) != 0)
                output.Add("> Selected beacon is unavailable in regular play.");
            if ((warningSet & RecipeNode.Warnings.BModuleIsDisabled) != 0)
                output.Add("> Some selected beacon modules are disabled.");
            if ((warningSet & RecipeNode.Warnings.BModuleIsUnavailable) != 0)
                output.Add("> Some selected beacon modules are unavailable in regular play.");

            return output;
        }

        //----------------------------------------------------------------------- Get functions (single assembler/beacon info)

        public double GetGeneratorMinimumTemperature() {
            // minimum temperature accepted by generator is the largest of either the default temperature (at which point the power generation is 0,
            // and it actually doesn't consume anything), or the set min temp
            if (SelectedAssembler.Assembler.EntityType == EntityType.Generator) {
                // generators have 1 input & 0 output. only input is the fluid being consumed.
                var fluidBase = (Fluid) BaseRecipe.Recipe.IngredientList[0];
                return Math.Max(fluidBase.DefaultTemperature + 0.1, BaseRecipe.Recipe.IngredientTemperatureMap[fluidBase].Min);
            }

            Trace.Fail("Cant ask for minimum generator temperature for a non-generator!");
            return 0;
        }

        public double GetGeneratorMaximumTemperature() {
            if (SelectedAssembler.Assembler.EntityType == EntityType.Generator)
                return BaseRecipe.Recipe.IngredientTemperatureMap[BaseRecipe.Recipe.IngredientList[0]].Max;
            Trace.Fail("Cant ask for maximum generator temperature for a non-generator!");
            return 0;
        }

        public double GetGeneratorAverageTemperature() {
            if (SelectedAssembler.Assembler.EntityType != EntityType.Generator)
                Trace.Fail("Cant ask for average generator temperature for a non-generator!");

            return GetAverageTemperature(this, node.BaseRecipe.Recipe.IngredientList[0]);

            double GetAverageTemperature(ReadOnlyBaseNode node, Item item) {
                if (node is ReadOnlyPassthroughNode || node == this) {
                    double totalFlow = 0;
                    double totalTemperatureFlow = 0;
                    double totalTemperature = 0;
                    // Throughput node: all same item. Generator node: only input is the fluid item.
                    foreach (var link in node.InputLinks) {
                        totalFlow += link.Throughput;
                        var temperature = GetAverageTemperature(link.Supplier, item);
                        totalTemperatureFlow += temperature * link.Throughput;
                        totalTemperature += temperature;
                    }

                    if (totalFlow != 0)
                        return totalTemperatureFlow / totalFlow;

                    if (!node.InputLinks.Any())
                        return SelectedAssembler.Assembler.OperationTemperature;

                    return totalTemperature / node.InputLinks.Count();
                }

                // assume supplier is optimal temperature (cant exactly set to infinity or something as that would just cause the final result to be infinity)
                if (node is ReadOnlySupplierNode)
                    return SelectedAssembler.Assembler.OperationTemperature;

                if (node is ReadOnlyRecipeNode recipeNode)
                    return recipeNode.BaseRecipe.Recipe.ProductTemperatureMap[item];

                Trace.Fail("Unexpected node type in generator calculation!");
                return 0;
            }
        }

        public double GetGeneratorEffectivity() {
            var fluid = (Fluid) node.BaseRecipe.Recipe.IngredientList[0];
            return Math.Min(
                (GetGeneratorAverageTemperature() - fluid.DefaultTemperature) /
                (node.SelectedAssembler.Assembler.OperationTemperature - fluid.DefaultTemperature), 1);
        }

        // Watts
        public double GetGeneratorElectricalProduction() {
            if (SelectedAssembler.Assembler.EntityType == EntityType.Generator)
                return SelectedAssembler.Assembler.GetEnergyProduction(SelectedAssembler.Quality) * GetGeneratorEffectivity();

            // no consumption multiplier => generators cant have modules / beacon effects
            return SelectedAssembler.Assembler.GetEnergyProduction(SelectedAssembler.Quality);
        }


        public double GetAssemblerSpeed() {
            return SelectedAssembler.Assembler.GetSpeed(SelectedAssembler.Quality) * node.GetSpeedMultiplier();
        }

        // Watts
        public double GetAssemblerEnergyConsumption() {
            return SelectedAssembler.Assembler.GetEnergyDrain() +
                SelectedAssembler.Assembler.GetEnergyConsumption(SelectedAssembler.Quality) * node.GetConsumptionMultiplier();
        }

        // pollution/s
        public double GetAssemblerPollutionProduction() {
            // there are now multiple types of pollution, so not sure how to handle this (at least in terms of displaying it)
            return 0;

            // TODO: POLLUTION UPDATER REQUIRED
            // SelectedAssembler.Pollution * MyNode.GetPollutionMultiplier() * GetAssemblerEnergyConsumption(); //pollution is counted in per energy
        }

        // Watts
        public double GetBeaconEnergyConsumption() {
            if (!SelectedBeacon || SelectedBeacon.Beacon.EnergySource != EnergySource.Electric)
                return 0;
            return SelectedBeacon.Beacon.GetEnergyProduction(SelectedBeacon.Quality) + SelectedBeacon.Beacon.GetEnergyDrain();
        }

        // pollution/s
        public double GetBeaconPollutionProduction() {
            // once again - multiple types of pollution, so not sure how to handle this at this time
            return 0;

            // TODO: POLLUTION UPDATE REQUIRED
            // SelectedBeacon.Pollution * GetBeaconEnergyConsumption();
        }

        //----------------------------------------------------------------------- Get functions (totals)

        public double GetTotalCrafts() {
            return GetAssemblerSpeed() * node.MyGraph.GetRateMultiplier() / node.BaseRecipe.Recipe.Time;
        }

        // fuel items / time unit
        public double GetTotalAssemblerFuelConsumption() {
            if (node.Fuel == null)
                return 0;
            return node.MyGraph.GetRateMultiplier() * node.InputRateForFuel();
        }

        // J/s (W)
        public double GetTotalAssemblerElectricalConsumption() {
            if (node.SelectedAssembler.Assembler.EnergySource != EnergySource.Electric)
                return 0;

            var partialAssembler = node.ActualSetValue % 1;
            var entireAssemblers = node.ActualSetValue - partialAssembler;

            // if there is more than 5% of an extra assembler, assume there is +1 assembler working x% of the time (full drain, x% uptime)

            return (entireAssemblers + (partialAssembler < 0.05 ? 0 : 1))
                * SelectedAssembler.Assembler.GetEnergyDrain()
                + ActualSetValue
                * SelectedAssembler.Assembler.GetEnergyConsumption(SelectedAssembler.Quality)
                * GetConsumptionMultiplier();
        }

        // J/s (W)
        // this is also when the temperature range of incoming fuel is taken into account
        public double GetTotalGeneratorElectricalProduction() {
            return GetGeneratorElectricalProduction() * node.ActualSetValue;
        }

        public int GetTotalBeacons() {
            if (!node.SelectedBeacon)
                return 0;

            // assume 0.2 assemblers (or more) is enough to warrant an extra 'beacons per assembler' row
            return (int) Math.Ceiling((int) (node.ActualSetValue + 0.8) * BeaconsPerAssembler + BeaconsConst);
        }

        // J/s (W)
        public double GetTotalBeaconElectricalConsumption() {
            if (!node.SelectedBeacon)
                return 0;
            return GetTotalBeacons() * GetBeaconEnergyConsumption();
        }
    }

    public class RecipeNodeController : BaseNodeController {
        private readonly RecipeNode _myNode;

        protected RecipeNodeController(RecipeNode myNode) : base(myNode) {
            _myNode = myNode;
        }

        public static RecipeNodeController GetController(RecipeNode node) {
            if (node.Controller != null)
                return (RecipeNodeController) node.Controller;
            return new RecipeNodeController(node);
        }

        //------------------------------------------------------------------------ warning / errors functions

        public override Dictionary<string, Action> GetErrorResolutions() {
            var errorSet = _myNode.ErrorSet;

            var resolutions = new Dictionary<string, Action>();
            if ((errorSet & RecipeNode.Errors.RecipeIsMissing) != 0)
                resolutions.Add("Delete node", Delete);
            else {
                if ((errorSet & (RecipeNode.Errors.AssemblerIsMissing | RecipeNode.Errors.AQualityIsMissing)) != 0)
                    resolutions.Add("Auto-select assembler & quality", AutoSetAssembler);

                if ((errorSet & (RecipeNode.Errors.FuelIsMissing | RecipeNode.Errors.InvalidFuel)) != 0 &&
                    _myNode.SelectedAssembler.Assembler.Fuels.Any(f => !f.IsMissing))
                    resolutions.Add("Auto-select fuel", AutoSetFuel);

                if ((errorSet & RecipeNode.Errors.InvalidFuelRemains) != 0 && _myNode.SelectedAssembler.Assembler.Fuels.Contains(_myNode.Fuel))
                    resolutions.Add("Update burn result", () => SetFuel(_myNode.Fuel));

                if ((errorSet & (RecipeNode.Errors.AModuleIsMissing | RecipeNode.Errors.AModuleLimitExceeded | RecipeNode.Errors.AModuleQualityIsMissing)) != 0)
                    resolutions.Add("Fix assembler modules", () => {
                        for (var i = _myNode.AssemblerModules.Count - 1; i >= 0; i--)
                            if (_myNode.AssemblerModules[i].Module.IsMissing ||
                                !_myNode.SelectedAssembler.Assembler.Modules.Contains(_myNode.AssemblerModules[i].Module) ||
                                !_myNode.BaseRecipe.Recipe.AssemblerModules.Contains(_myNode.AssemblerModules[i].Module) ||
                                _myNode.AssemblerModules[i].Quality.IsMissing)
                                RemoveAssemblerModule(i);
                        while (_myNode.AssemblerModules.Count > _myNode.SelectedAssembler.Assembler.ModuleSlots)
                            RemoveAssemblerModule(_myNode.AssemblerModules.Count - 1);
                    });

                if ((errorSet & (RecipeNode.Errors.BeaconIsMissing | RecipeNode.Errors.BQualityIsMissing)) != 0)
                    resolutions.Add("Remove Beacon", ClearBeacon);

                if ((errorSet & (RecipeNode.Errors.BModuleIsMissing | RecipeNode.Errors.BModuleLimitExceeded | RecipeNode.Errors.BModuleQualityIsMissing)) != 0)
                    resolutions.Add("Fix beacon modules", () => {
                        for (var i = _myNode.BeaconModules.Count - 1; i >= 0; i--)
                            if (_myNode.BeaconModules[i].Module.IsMissing ||
                                !_myNode.SelectedAssembler.Assembler.Modules.Contains(_myNode.BeaconModules[i].Module) ||
                                !_myNode.BaseRecipe.Recipe.AssemblerModules.Contains(_myNode.BeaconModules[i].Module) ||
                                !_myNode.SelectedBeacon.Beacon.Modules.Contains(_myNode.BeaconModules[i].Module) || _myNode.BeaconModules[i].Quality.IsMissing)
                                RemoveBeaconModule(i);
                        while (_myNode.BeaconModules.Count > _myNode.SelectedBeacon.Beacon.ModuleSlots)
                            RemoveBeaconModule(_myNode.BeaconModules.Count - 1);
                    });

                foreach (var kvp in GetInvalidConnectionResolutions())
                    resolutions.Add(kvp.Key, kvp.Value);
            }

            return resolutions;
        }

        public override Dictionary<string, Action> GetWarningResolutions() {
            var warningSet = _myNode.WarningSet;

            var resolutions = new Dictionary<string, Action>();

            if ((warningSet & (RecipeNode.Warnings.AssemblerIsDisabled | RecipeNode.Warnings.AssemblerIsUnavailable |
                    RecipeNode.Warnings.AssemblerQualityIsDisabled)) != 0 && (warningSet & RecipeNode.Warnings.NoAvailableAssemblers) == 0)
                resolutions.Add("Switch to enabled assembler", AutoSetAssembler);

            if ((warningSet & (RecipeNode.Warnings.FuelIsUnavailable | RecipeNode.Warnings.FuelIsUncraftable)) != 0 &&
                (warningSet & RecipeNode.Warnings.NoAvailableFuels) == 0)
                resolutions.Add("Switch to valid fuel", AutoSetFuel);

            if ((warningSet & (RecipeNode.Warnings.AModuleIsDisabled | RecipeNode.Warnings.AModuleIsUnavailable |
                RecipeNode.Warnings.AModulesQualityIsDisabled)) != 0)
                resolutions.Add("Remove error modules from assembler", () => {
                    for (var i = _myNode.AssemblerModules.Count - 1; i >= 0; i--)
                        if (!_myNode.AssemblerModules[i].Module.Enabled || !_myNode.AssemblerModules[i].Module.Available ||
                            !_myNode.AssemblerModules[i].Quality.Enabled)
                            RemoveAssemblerModule(i);
                });

            if ((warningSet & (RecipeNode.Warnings.BeaconIsDisabled | RecipeNode.Warnings.BeaconIsUnavailable)) != 0)
                resolutions.Add("Turn off beacon", ClearBeacon);

            if ((warningSet & (RecipeNode.Warnings.BModuleIsDisabled | RecipeNode.Warnings.BModuleIsUnavailable |
                RecipeNode.Warnings.BModulesQualityIsDisabled)) != 0)
                resolutions.Add("Remove error modules from beacon", () => {
                    for (var i = _myNode.BeaconModules.Count - 1; i >= 0; i--)
                        if (!_myNode.BeaconModules[i].Module.Enabled || !_myNode.BeaconModules[i].Module.Available || !_myNode.BeaconModules[i].Quality.Enabled)
                            RemoveBeaconModule(i);
                });

            if ((warningSet & RecipeNode.Warnings.TemperatureFluidBurnerInvalidLinks) != 0)
                resolutions.Add("Remove fuel links", () => {
                    foreach (var fuelLink in _myNode.InputLinks.Where(l => l.Item == new ItemQualityPair(_myNode.Fuel, _myNode.Fuel.Owner.DefaultQuality))
                        .ToList())
                        _myNode.MyGraph.DeleteLink(fuelLink.ReadOnlyLink);
                });

            return resolutions;
        }

        //-----------------------------------------------------------------------Set functions

        public void SetPriority(bool lowPriority) {
            _myNode.LowPriority = lowPriority;
            _myNode.UpdateState();
        }

        public void SetNeighbourCount(double count) {
            if (Math.Abs(_myNode.NeighbourCount - count) > double.Epsilon)
                _myNode.NeighbourCount = count;
        }

        public void SetExtraProductivityBonus(double bonus) {
            if (Math.Abs(_myNode.ExtraProductivityBonus - bonus) > double.Epsilon)
                _myNode.ExtraProductivityBonus = bonus;
        }

        public void SetBeaconCount(double count) {
            if (Math.Abs(_myNode.BeaconCount - count) > double.Epsilon)
                _myNode.BeaconCount = count;
        }

        public void SetBeaconsPerAssembler(double beacons) {
            if (Math.Abs(_myNode.BeaconsPerAssembler - beacons) > double.Epsilon)
                _myNode.BeaconsPerAssembler = beacons;
        }

        public void SetBeaconsCont(double beacons) {
            if (Math.Abs(_myNode.BeaconsConst - beacons) > double.Epsilon)
                _myNode.BeaconsConst = beacons;
        }

        public void SetAssembler(AssemblerQualityPair assembler) {
            _myNode.SelectedAssembler = assembler;

            // fuel

            if (!assembler.Assembler.IsBurner)
                SetFuel(null);
            else if (_myNode.Fuel != null && assembler.Assembler.Fuels.Contains(_myNode.Fuel))
                SetFuel(_myNode.Fuel);
            else
                AutoSetFuel();

            // check for invalid modules

            for (var i = _myNode.AssemblerModules.Count - 1; i >= 0; i--)
                if (_myNode.AssemblerModules[i].Module.IsMissing ||
                    !_myNode.SelectedAssembler.Assembler.Modules.Contains(_myNode.AssemblerModules[i].Module) ||
                    !_myNode.BaseRecipe.Recipe.AssemblerModules.Contains(_myNode.AssemblerModules[i].Module) ||
                    !_myNode.AssemblerModules[i].Quality.Available ||
                    _myNode.AssemblerModules[i].Quality.IsMissing) {
                    _myNode.AssemblerModulesRemoveAt(i);
                }

            // check for too many modules

            while (_myNode.AssemblerModules.Count > _myNode.SelectedAssembler.Assembler.ModuleSlots)
                _myNode.AssemblerModulesRemoveAt(_myNode.AssemblerModules.Count - 1);

            // check if any modules work (if none work, then turn off beacon)

            if (_myNode.SelectedAssembler.Assembler.Modules.Count == 0 || _myNode.BaseRecipe.Recipe.AssemblerModules.Count == 0)
                ClearBeacon();
            else // update beacon
                SetBeacon(_myNode.SelectedBeacon);

            _myNode.UpdateInputsAndOutputs();
            _myNode.UpdateState();
        }

        public void AutoSetAssembler() {
            var quality = _myNode.SelectedAssembler.Quality.IsMissing || !_myNode.SelectedAssembler.Quality.Enabled
                ? _myNode.SelectedAssembler.Assembler.Owner.DefaultQuality
                : _myNode.SelectedAssembler.Quality;
            var assembler = _myNode.MyGraph.AssemblerSelector.GetAssembler(_myNode.BaseRecipe.Recipe);

            SetAssembler(new AssemblerQualityPair(assembler, quality));
            AutoSetFuel();
        }

        public void AutoSetAssembler(AssemblerSelector.Style style) {
            var quality = _myNode.SelectedAssembler.Quality.IsMissing || !_myNode.SelectedAssembler.Quality.Enabled
                ? _myNode.SelectedAssembler.Assembler.Owner.DefaultQuality
                : _myNode.SelectedAssembler.Quality;
            var assembler = _myNode.MyGraph.AssemblerSelector.GetAssembler(_myNode.BaseRecipe.Recipe, style);

            SetAssembler(new AssemblerQualityPair(assembler, quality));
            AutoSetFuel();
        }

        public void SetFuel(Item fuel) {
            if (_myNode.Fuel == fuel
                && (_myNode.Fuel != null || _myNode.FuelRemains == null)
                && (_myNode.Fuel == null || _myNode.Fuel.BurnResult == _myNode.FuelRemains)) {
                return;
            }

            // have to remove any links to the burner/burnt item (if they exist) unless the item is also part of the recipe

            if (_myNode.Fuel != null && !_myNode.IsFuelPartOfRecipeInputs) {
                var fuelIqp = new ItemQualityPair(_myNode.Fuel, _myNode.Fuel.Owner.DefaultQuality);
                foreach (var link in _myNode.InputLinks.Where(link => link.Item == fuelIqp).ToList())
                    link.Controller.Delete();
            }

            if (_myNode.FuelRemains != null && !_myNode.IsFuelRemainsPartOfRecipeOutputs) {
                var fuelRemainsIqp = new ItemQualityPair(_myNode.FuelRemains, _myNode.FuelRemains.Owner.DefaultQuality);
                foreach (var link in _myNode.OutputLinks.Where(link => link.Item == fuelRemainsIqp).ToList())
                    link.Controller.Delete();
            }

            _myNode.Fuel = fuel;
            _myNode.MyGraph.FuelSelector.UseFuel(fuel);
            _myNode.UpdateState();
        }

        public void AutoSetFuel() {
            SetFuel(_myNode.MyGraph.FuelSelector.GetFuel(_myNode.SelectedAssembler.Assembler));
        }

        public void ClearBeacon() {
            _myNode.SelectedBeacon = new BeaconQualityPair("clearing beacon");
            _myNode.BeaconModulesClear();
            _myNode.BeaconCount = 0;
            _myNode.BeaconsPerAssembler = 0;
            _myNode.BeaconsConst = 0;
            _myNode.UpdateState();
        }

        public void SetBeacon(BeaconQualityPair beacon) {
            // shouldn't be called - but whatever
            if (!beacon) {
                ClearBeacon();
                return;
            }

            _myNode.SelectedBeacon = beacon;

            // check for invalid modules

            for (var i = _myNode.BeaconModules.Count - 1; i >= 0; i--) {
                if (_myNode.BeaconModules[i].Module.IsMissing ||
                    !_myNode.SelectedAssembler.Assembler.Modules.Contains(_myNode.BeaconModules[i].Module) ||
                    !_myNode.BaseRecipe.Recipe.AssemblerModules.Contains(_myNode.BeaconModules[i].Module) ||
                    !_myNode.SelectedBeacon.Beacon.Modules.Contains(_myNode.BeaconModules[i].Module) ||
                    !_myNode.BeaconModules[i].Quality.Available ||
                    _myNode.BeaconModules[i].Quality.IsMissing) {
                    _myNode.BeaconModulesRemoveAt(i);
                }
            }

            // check for too many modules

            while (_myNode.BeaconModules.Count > _myNode.SelectedBeacon.Beacon.ModuleSlots)
                _myNode.BeaconModulesRemoveAt(_myNode.BeaconModules.Count - 1);

            _myNode.UpdateState();
        }

        public void AddAssemblerModule(ModuleQualityPair module) {
            _myNode.AssemblerModulesAdd(module);
            _myNode.UpdateState();
        }

        public void AddAssemblerModules(ModuleQualityPair module) {
            while (_myNode.AssemblerModules.Count < _myNode.SelectedAssembler.Assembler.ModuleSlots)
                _myNode.AssemblerModulesAdd(module);
            _myNode.UpdateState();
        }

        public void RemoveAssemblerModule(int index) {
            if (index >= 0 && index < _myNode.AssemblerModules.Count)
                _myNode.AssemblerModulesRemoveAt(index);
            _myNode.UpdateState();
        }

        public void RemoveAssemblerModules(ModuleQualityPair module) {
            _myNode.AssemblerModulesRemoveAll(module);
            _myNode.UpdateState();
        }

        public void RemoveAssemblerModules() {
            _myNode.AssemblerModulesClear();
            _myNode.UpdateState();
        }

        public void SetAssemblerModules(IEnumerable<ModuleQualityPair> modules, bool filterModules) {
            _myNode.AssemblerModulesClear();
            if (modules != null) {
                if (filterModules) {
                    var acceptableModules =
                        new HashSet<Module>(_myNode.BaseRecipe.Recipe.AssemblerModules.Intersect(_myNode.SelectedAssembler.Assembler.Modules));
                    foreach (var m in modules)
                        if (_myNode.AssemblerModules.Count < _myNode.SelectedAssembler.Assembler.ModuleSlots && acceptableModules.Contains(m.Module))
                            _myNode.AssemblerModulesAdd(m);
                } else
                    _myNode.AssemblerModulesAddRange(modules);
            }

            _myNode.UpdateState();
        }

        public void AutoSetAssemblerModules() {
            _myNode.AssemblerModulesClear();
            _myNode.AssemblerModulesAddRange(_myNode.MyGraph.ModuleSelector.GetModules(_myNode.SelectedAssembler.Assembler, _myNode.BaseRecipe.Recipe)
                .ConvertAll(i => new ModuleQualityPair(i, i.Owner.DefaultQuality)));
            _myNode.UpdateState();
        }

        public void AutoSetAssemblerModules(ModuleSelector.Style style) {
            _myNode.AssemblerModulesClear();
            _myNode.AssemblerModulesAddRange(_myNode.MyGraph.ModuleSelector.GetModules(_myNode.SelectedAssembler.Assembler, _myNode.BaseRecipe.Recipe, style)
                .ConvertAll(i => new ModuleQualityPair(i, i.Owner.DefaultQuality)));
            _myNode.UpdateState();
        }

        public void AddBeaconModule(ModuleQualityPair module) {
            _myNode.BeaconModulesAdd(module);
            _myNode.UpdateState();
        }

        public void AddBeaconModules(ModuleQualityPair module) {
            while (_myNode.BeaconModules.Count < _myNode.SelectedBeacon.Beacon.ModuleSlots)
                _myNode.BeaconModulesAdd(module);
            _myNode.UpdateState();
        }

        public void RemoveBeaconModule(int index) {
            if (index >= 0 && index < _myNode.BeaconModules.Count)
                _myNode.BeaconModulesRemoveAt(index);
            _myNode.UpdateState();
        }

        public void RemoveBeaconModules(ModuleQualityPair module) {
            _myNode.BeaconModulesRemoveAll(module);
            _myNode.UpdateState();
        }

        public void SetBeaconModules(IEnumerable<ModuleQualityPair> modules, bool filterModules) {
            _myNode.BeaconModulesClear();
            if (modules != null) {
                if (filterModules) {
                    var acceptableModules = new HashSet<Module>(_myNode.BaseRecipe.Recipe.AssemblerModules
                        .Intersect(_myNode.SelectedAssembler.Assembler.Modules).Intersect(_myNode.SelectedBeacon.Beacon.Modules));
                    foreach (var m in modules)
                        if (_myNode.BeaconModules.Count < _myNode.SelectedBeacon.Beacon.ModuleSlots && acceptableModules.Contains(m.Module))
                            _myNode.BeaconModulesAdd(m);
                } else
                    _myNode.BeaconModulesAddRange(modules);
            }

            _myNode.UpdateState();
        }
    }
}