using Foreman.DataCaching.DataTypes;
using Foreman.Models;
using Foreman.Models.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Foreman.Graph {
    public sealed class NodeViewModelEventArgs(INodeViewModel viewModel) : EventArgs {
        public INodeViewModel ViewModel { get; } = viewModel;
    }

    public sealed class LinkViewModelEventArgs(INodeLinkViewModel viewModel) : EventArgs {
        public INodeLinkViewModel ViewModel { get; } = viewModel;
    }

    public interface IGraphViewModel {
        IReadOnlyList<INodeViewModel> Nodes { get; }
        IReadOnlyList<INodeLinkViewModel> Links { get; }
        bool TryGetNode(NodeId id, out INodeViewModel? viewModel);
        bool TryGetLink(LinkId id, out INodeLinkViewModel? viewModel);
    }

    public interface INodeViewModel : INotifyPropertyChanged {
        NodeId Id { get; }
        NodeType NodeType { get; }
        Point Location { get; }
        NodeDirection NodeDirection { get; }
        bool KeyNode { get; }
        string KeyNodeTitle { get; }
        RateType RateType { get; }
        NodeState State { get; }
        IEnumerable<ItemQualityPair> Inputs { get; }
        IEnumerable<ItemQualityPair> Outputs { get; }
        IReadOnlyList<INodeLinkViewModel> InputLinks { get; }
        IReadOnlyList<INodeLinkViewModel> OutputLinks { get; }
        double ActualRate { get; }
        double DesiredRate { get; }
        double ActualSetValue { get; }
        double DesiredSetValue { get; }
        double MaxDesiredSetValue { get; }
        string SetValueDescription { get; }
        double GetConsumeRate(ItemQualityPair item);
        double GetSupplyRate(ItemQualityPair item);
        double GetSupplyUsedRate(ItemQualityPair item);
        bool IsOverproducing();
        bool IsOverproducing(ItemQualityPair item);
        bool ManualRateNotMet();
        List<string> GetErrors();
        List<string> GetWarnings();
        event EventHandler? NodeStateChanged;
        event EventHandler? NodeValuesChanged;
    }

    public interface INodeLinkViewModel {
        LinkId Id { get; }
        NodeId SupplierId { get; }
        NodeId ConsumerId { get; }
        ItemQualityPair Item { get; }
        double Throughput { get; }
        NodeDirection SupplierDirection { get; }
        NodeDirection ConsumerDirection { get; }
        bool IsValid { get; }
    }

    public interface ISupplierNodeViewModel : INodeViewModel {
        ItemQualityPair SuppliedItem { get; }
    }

    public interface IConsumerNodeViewModel : INodeViewModel {
        ItemQualityPair ConsumedItem { get; }
    }

    public interface IPassthroughNodeViewModel : INodeViewModel {
        ItemQualityPair PassthroughItem { get; }
        bool SimpleDraw { get; }
    }

    public interface IRecipeNodeViewModel : INodeViewModel {
        bool LowPriority { get; }
        uint MaxQualitySteps { get; }
        RecipeQualityPair BaseRecipe { get; }
        AssemblerQualityPair SelectedAssembler { get; }
        IItem? Fuel { get; }
        IItem? FuelRemains { get; }
        IReadOnlyList<ModuleQualityPair> AssemblerModules { get; }
        BeaconQualityPair SelectedBeacon { get; }
        IReadOnlyList<ModuleQualityPair> BeaconModules { get; }
        double NeighbourCount { get; }
        double ExtraProductivity { get; }
        double BeaconCount { get; }
        double BeaconsPerAssembler { get; }
        double BeaconsConst { get; }
        double GetConsumptionMultiplier();
        double GetSpeedMultiplier();
        double GetProductivityMultiplier();
        double GetPollutionMultiplier();
        double GetQualityMultiplier();
        double GetAssemblerSpeed();
        double GetTotalCrafts();
        double GetTotalAssemblerFuelConsumption();
        double GetAssemblerEnergyConsumption();
        double GetAssemblerPollutionProduction();
        double GetGeneratorMinimumTemperature();
        double GetGeneratorMaximumTemperature();
        double GetGeneratorEffectivity();
        double GetGeneratorElectricalProduction();
        double GetBeaconEnergyConsumption();
        double GetTotalAssemblerElectricalConsumption();
        double GetTotalGeneratorElectricalProduction();
        int GetTotalBeacons();
        double GetTotalBeaconElectricalConsumption();
    }

    public interface ISpoilNodeViewModel : INodeViewModel {
        ItemQualityPair InputItem { get; }
        ItemQualityPair OutputItem { get; }
    }

    public interface IPlantNodeViewModel : INodeViewModel {
        ItemQualityPair Seed { get; }
        IPlantProcess PlantProcess { get; }
    }

    public interface IProductionGraphEditor {
        ProductionGraph Graph { get; }
        NodeId CreateSupplierNode(ItemQualityPair item, Point location);
        NodeId CreateConsumerNode(ItemQualityPair item, Point location);
        NodeId CreatePassthroughNode(ItemQualityPair item, Point location);
        NodeId CreateRecipeNode(RecipeQualityPair recipe, Point location);
        NodeId CreateSpoilNode(ItemQualityPair inputItem, IItem outputItem, Point location);
        NodeId CreatePlantNode(IPlantProcess plantProcess, IQuality quality, Point location);
        LinkId CreateLink(NodeId supplierId, NodeId consumerId, ItemQualityPair item);
        void DeleteNode(NodeId id);
        void DeleteLink(LinkId id);
        void SetLocation(NodeId id, Point location);
        void SetDirection(NodeId id, NodeDirection direction);
        BaseNodeController? RequestNodeController(NodeId id);
    }

    public interface IProductionGraphSession {
        ProductionGraph Graph { get; }
        IGraphViewModel View { get; }
        IProductionGraphEditor Editor { get; }
        bool TryGetDomainNode(NodeId id, out BaseNode? node);
        bool TryGetDomainLink(LinkId id, out NodeLink? link);
        event EventHandler<NodeViewModelEventArgs>? NodeViewModelAdded;
        event EventHandler<NodeViewModelEventArgs>? NodeViewModelRemoved;
        event EventHandler<LinkViewModelEventArgs>? LinkViewModelAdded;
        event EventHandler<LinkViewModelEventArgs>? LinkViewModelRemoved;
        event EventHandler? GraphCleared;
        event EventHandler? NodeValuesUpdated;
        void Attach();
        void Detach();
    }
}
