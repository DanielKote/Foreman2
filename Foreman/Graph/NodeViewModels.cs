using Foreman.DataCaching.DataTypes;
using Foreman.Models;
using Foreman.Models.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Foreman.Graph {
    internal abstract class NodeViewModelBase : INodeViewModel {
        private readonly ProductionGraphSession _session;
        protected BaseNode Node { get; }
        private IReadOnlyList<INodeLinkViewModel> _inputLinks = [];
        private IReadOnlyList<INodeLinkViewModel> _outputLinks = [];

        protected NodeViewModelBase(NodeId id, BaseNode node, ProductionGraphSession session) {
            Id = id;
            Node = node;
            _session = session;
            Node.NodeStateChanged += OnDomainNodeStateChanged;
            Node.NodeValuesChanged += OnDomainNodeValuesChanged;
            RefreshLinkViewModels();
        }

        public NodeId Id { get; }
        public abstract NodeType NodeType { get; }

        public Point Location => Node.Location;
        public NodeDirection NodeDirection => Node.NodeDirection;
        public bool KeyNode => Node.KeyNode;
        public string KeyNodeTitle => Node.KeyNodeTitle;
        public RateType RateType => Node.RateType;
        public NodeState State => Node.State;
        public IEnumerable<ItemQualityPair> Inputs => Node.Inputs;
        public IEnumerable<ItemQualityPair> Outputs => Node.Outputs;
        public IReadOnlyList<INodeLinkViewModel> InputLinks => _inputLinks;
        public IReadOnlyList<INodeLinkViewModel> OutputLinks => _outputLinks;
        public double ActualRate => Node.ActualRate;
        public double DesiredRate => Node.DesiredRate;
        public double ActualSetValue => Node.ActualSetValue;
        public double DesiredSetValue => Node.DesiredSetValue;
        public double MaxDesiredSetValue => Node.MaxDesiredSetValue;
        public string SetValueDescription => Node.SetValueDescription;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? NodeStateChanged;
        public event EventHandler? NodeValuesChanged;

        public double GetConsumeRate(ItemQualityPair item) => Node.GetConsumeRate(item);
        public double GetSupplyRate(ItemQualityPair item) => Node.GetSupplyRate(item);
        public double GetSupplyUsedRate(ItemQualityPair item) => Node.GetSupplyUsedRate(item);
        public bool IsOverproducing() => Node.IsOverproducing();
        public bool IsOverproducing(ItemQualityPair item) => Node.IsOverproducing(item);
        public bool ManualRateNotMet() => Node.ManualRateNotMet();

        public virtual List<string> GetErrors() => Node.GetErrors();
        public virtual List<string> GetWarnings() => Node.GetWarnings();

        internal void RefreshLinkViewModels() {
            _inputLinks = [.. Node.InputLinks.Select(link => _session.GetOrCreateLinkViewModel(link))];
            _outputLinks = [.. Node.OutputLinks.Select(link => _session.GetOrCreateLinkViewModel(link))];
            RaisePropertyChanged(nameof(InputLinks));
            RaisePropertyChanged(nameof(OutputLinks));
        }

        internal void NotifyValuesChanged() {
            RaisePropertyChanged(nameof(ActualRate));
            RaisePropertyChanged(nameof(DesiredRate));
            RaisePropertyChanged(nameof(ActualSetValue));
            RaisePropertyChanged(nameof(DesiredSetValue));
            RaisePropertyChanged(nameof(State));
            NodeValuesChanged?.Invoke(this, EventArgs.Empty);
        }

        internal void NotifyStateChanged() {
            RaisePropertyChanged(nameof(State));
            RaisePropertyChanged(nameof(NodeDirection));
            RaisePropertyChanged(nameof(Inputs));
            RaisePropertyChanged(nameof(Outputs));
            RaisePropertyChanged(nameof(RateType));
            RaisePropertyChanged(nameof(KeyNode));
            RaisePropertyChanged(nameof(KeyNodeTitle));
            RefreshLinkViewModels();
            NodeStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnDomainNodeStateChanged(object? sender, EventArgs e) => NotifyStateChanged();
        private void OnDomainNodeValuesChanged(object? sender, EventArgs e) => NotifyValuesChanged();

        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal sealed class SupplierNodeViewModel(NodeId id, SupplierNode node, ProductionGraphSession session) : NodeViewModelBase(id, node, session), ISupplierNodeViewModel {
        private readonly SupplierNode _supplierNode = node;

        public override NodeType NodeType => NodeType.Supplier;
        public ItemQualityPair SuppliedItem => _supplierNode.SuppliedItem;
    }

    internal sealed class ConsumerNodeViewModel(NodeId id, ConsumerNode node, ProductionGraphSession session) : NodeViewModelBase(id, node, session), IConsumerNodeViewModel {
        private readonly ConsumerNode _consumerNode = node;

        public override NodeType NodeType => NodeType.Consumer;
        public ItemQualityPair ConsumedItem => _consumerNode.ConsumedItem;
    }

    internal sealed class PassthroughNodeViewModel(NodeId id, PassthroughNode node, ProductionGraphSession session) : NodeViewModelBase(id, node, session), IPassthroughNodeViewModel {
        private readonly PassthroughNode _passthroughNode = node;

        public override NodeType NodeType => NodeType.Passthrough;
        public ItemQualityPair PassthroughItem => _passthroughNode.PassthroughItem;
        public bool SimpleDraw => _passthroughNode.SimpleDraw;
    }

    internal sealed class RecipeNodeViewModel(NodeId id, RecipeNode node, ProductionGraphSession session) : NodeViewModelBase(id, node, session), IRecipeNodeViewModel {
        private readonly RecipeNode _recipeNode = node;

        public override NodeType NodeType => NodeType.Recipe;
        public bool LowPriority => _recipeNode.LowPriority;
        public uint MaxQualitySteps => _recipeNode.MaxQualitySteps;
        public RecipeQualityPair BaseRecipe => _recipeNode.BaseRecipe;
        public AssemblerQualityPair SelectedAssembler => _recipeNode.SelectedAssembler;
        public IItem? Fuel => _recipeNode.Fuel;
        public IItem? FuelRemains => _recipeNode.FuelRemains;
        public IReadOnlyList<ModuleQualityPair> AssemblerModules => _recipeNode.AssemblerModules;
        public BeaconQualityPair SelectedBeacon => _recipeNode.SelectedBeacon;
        public IReadOnlyList<ModuleQualityPair> BeaconModules => _recipeNode.BeaconModules;
        public double NeighbourCount => _recipeNode.NeighbourCount;
        public double ExtraProductivity => _recipeNode.ExtraProductivityBonus;
        public double BeaconCount => _recipeNode.BeaconCount;
        public double BeaconsPerAssembler => _recipeNode.BeaconsPerAssembler;
        public double BeaconsConst => _recipeNode.BeaconsConst;
        public double GetConsumptionMultiplier() => _recipeNode.GetConsumptionMultiplier();
        public double GetSpeedMultiplier() => _recipeNode.GetSpeedMultiplier();
        public double GetProductivityMultiplier() => _recipeNode.GetProductivityBonus() + 1;
        public double GetPollutionMultiplier() => _recipeNode.GetPollutionMultiplier();
        public double GetQualityMultiplier() => _recipeNode.GetQualityMultiplier();
        public double GetAssemblerSpeed() => _recipeNode.GetAssemblerSpeed();
        public double GetTotalCrafts() => _recipeNode.GetTotalCrafts();
        public double GetTotalAssemblerFuelConsumption() => _recipeNode.GetTotalAssemblerFuelConsumption();
        public double GetAssemblerEnergyConsumption() => _recipeNode.GetAssemblerEnergyConsumption();
        public double GetAssemblerPollutionProduction() => _recipeNode.GetAssemblerPollutionProduction();
        public double GetGeneratorMinimumTemperature() => _recipeNode.GetGeneratorMinimumTemperature();
        public double GetGeneratorMaximumTemperature() => _recipeNode.GetGeneratorMaximumTemperature();
        public double GetGeneratorEffectivity() => _recipeNode.GetGeneratorEffectivity();
        public double GetGeneratorElectricalProduction() => _recipeNode.GetGeneratorElectricalProduction();
        public double GetBeaconEnergyConsumption() => _recipeNode.GetBeaconEnergyConsumption();
        public double GetTotalAssemblerElectricalConsumption() => _recipeNode.GetTotalAssemblerElectricalConsumption();
        public double GetTotalGeneratorElectricalProduction() => _recipeNode.GetTotalGeneratorElectricalProduction();
        public int GetTotalBeacons() => _recipeNode.GetTotalBeacons();
        public double GetTotalBeaconElectricalConsumption() => _recipeNode.GetTotalBeaconElectricalConsumption();
    }

    internal sealed class SpoilNodeViewModel(NodeId id, SpoilNode node, ProductionGraphSession session) : NodeViewModelBase(id, node, session), ISpoilNodeViewModel {
        private readonly SpoilNode _spoilNode = node;

        public override NodeType NodeType => NodeType.Spoil;
        public ItemQualityPair InputItem => _spoilNode.InputItem;
        public ItemQualityPair OutputItem => _spoilNode.OutputItem;
    }

    internal sealed class PlantNodeViewModel(NodeId id, PlantNode node, ProductionGraphSession session) : NodeViewModelBase(id, node, session), IPlantNodeViewModel {
        private readonly PlantNode _plantNode = node;

        public override NodeType NodeType => NodeType.Plant;
        public ItemQualityPair Seed => _plantNode.Seed;
        public IPlantProcess PlantProcess => _plantNode.BasePlantProcess;
    }

    internal static class NodeViewModelFactory {
        public static INodeViewModel Create(NodeId id, BaseNode node, ProductionGraphSession session) =>
            node switch {
                SupplierNode supplier => new SupplierNodeViewModel(id, supplier, session),
                ConsumerNode consumer => new ConsumerNodeViewModel(id, consumer, session),
                PassthroughNode passthrough => new PassthroughNodeViewModel(id, passthrough, session),
                RecipeNode recipe => new RecipeNodeViewModel(id, recipe, session),
                SpoilNode spoil => new SpoilNodeViewModel(id, spoil, session),
                PlantNode plant => new PlantNodeViewModel(id, plant, session),
                _ => throw new ArgumentException($"Unsupported node type: {node.GetType().Name}", nameof(node)),
            };
    }
}
