using Foreman.DataCaching.DataTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Foreman.Models.Nodes {
    public class PlantNode : BaseNode {
        public enum Errors {
            Clean = 0b_0000_0000_0000,
            ItemDoesntGrow = 0b_0000_0000_0001,
            InvalidGrowResult = 0b_0000_0000_0010,
            InputItemMissing = 0b_0000_0000_0100,
            PlantProcessMissing = 0b_0000_0000_1000,

            QualityMissing = 0b_0000_0001_0000,

            InvalidLinks = 0b_1000_0000_0000
        }
        public enum Warnings {
            Clean = 0b_0000_0000_0000,
            QualityIsDisabled = 0b_1000_0000_0000_0000,
        }
        public Errors ErrorSet { get; private set; }
        public Warnings WarningSet { get; private set; }

        private readonly BaseNodeController controller;
        public override BaseNodeController Controller { get { return controller; } }

        public ItemQualityPair Seed { get; private set; }
        public IPlantProcess BasePlantProcess { get; internal set; }

        public override IEnumerable<ItemQualityPair> Inputs { get { yield return Seed; } }
        public override IEnumerable<ItemQualityPair> Outputs {
            get {
                foreach (IItem product in BasePlantProcess.ProductList)
                    if (product.Owner.DefaultQuality is not null)
                        yield return new ItemQualityPair(product, product.Owner.DefaultQuality);
            }
        }

        //for plant nodes, the SetValue is 'number of plant tiles'
        public override double ActualSetValue { get { return ActualRatePerSec * (Seed.Item?.PlantResult?.GrowTime ?? 1); } }
        public override double DesiredSetValue { get; set; }
        public override double MaxDesiredSetValue { get { return ProductionGraph.MaxTiles; } }
        public override string SetValueDescription { get { return "Number of farming tiles"; } }

        public override double DesiredRatePerSec { get { return DesiredSetValue / (Seed.Item?.PlantResult?.GrowTime ?? 1); } }

        public PlantNode(ProductionGraph graph, int nodeID, ItemQualityPair item) : this(graph, nodeID, item.Item?.PlantResult, item.Quality ?? throw new ArgumentException("Quality must be populated.", nameof(item))) { }
        public PlantNode(ProductionGraph graph, int nodeID, IPlantProcess? plantProcess, IQuality quality) : base(graph, nodeID) {
            if (plantProcess is null || plantProcess.Seed is null)
                throw new InvalidOperationException(nameof(plantProcess) + "/" + nameof(plantProcess.Seed) + " is null when it must not be.");
            BasePlantProcess = plantProcess;
            Seed = new ItemQualityPair(plantProcess.Seed, quality);
            controller = new PlantNodeController(this);
        }

        internal override NodeState GetUpdatedState() {
            ErrorSet = Errors.Clean;

            if (Seed.Item?.PlantResult is null)
                ErrorSet |= Errors.ItemDoesntGrow;
            if (Seed.Item?.PlantResult != BasePlantProcess)
                ErrorSet |= Errors.InvalidGrowResult;
            if (Seed.Item?.IsMissing is true)
                ErrorSet |= Errors.InputItemMissing;
            if (BasePlantProcess.IsMissing)
                ErrorSet |= Errors.PlantProcessMissing;
            if (Seed.Quality?.IsMissing is true)
                ErrorSet |= Errors.QualityMissing;
            if (!AllLinksValid)
                ErrorSet |= Errors.InvalidLinks;

            return ErrorSet != Errors.Clean ? NodeState.Error : AllLinksConnected ? NodeState.Clean : NodeState.MissingLink;
        }

        public override double GetConsumeRate(ItemQualityPair item) { return ActualRate; }
        public override double GetSupplyRate(ItemQualityPair item) { return ActualRate * outputRateFor(item); }

        internal override double inputRateFor(ItemQualityPair item) { return 1; }
        internal override double outputRateFor(ItemQualityPair item) {
            return item.Item is not IItem outputItem
                ? throw new ArgumentException("Item must be populated.", nameof(item))
                : BasePlantProcess.ProductSet[outputItem];
        }

        public override List<string> GetErrors() {
            var errors = new List<string>();
            if (Seed.Item is null || Seed.Quality is null)
                return errors;

            if ((ErrorSet & Errors.InputItemMissing) != 0)
                errors.Add(string.Format(DisplayCulture.Format, "> Item \"{0}\" doesnt exist in preset!", Seed.Item.FriendlyName));
            if ((ErrorSet & Errors.PlantProcessMissing) != 0)
                errors.Add(string.Format(DisplayCulture.Format, "> Growth process for item \"{0}\" doesnt exist in preset!", Seed.Item.FriendlyName));
            if ((ErrorSet & Errors.ItemDoesntGrow) != 0)
                errors.Add(string.Format(DisplayCulture.Format, "> Item \"{0}\" cant be planted!", Seed.Item.FriendlyName));
            if ((ErrorSet & Errors.InvalidGrowResult) != 0)
                errors.Add(string.Format(DisplayCulture.Format, "> Growth result for item \"{0}\" doesnt match preset!", Seed.Item.FriendlyName));
            if ((ErrorSet & Errors.QualityMissing) != 0)
                errors.Add(string.Format(DisplayCulture.Format, "> Quality \"{0}\" doesnt exist in preset!", Seed.Quality.FriendlyName));
            if ((ErrorSet & Errors.InvalidLinks) != 0)
                errors.Add("> Some links are invalid!");
            return errors;
        }

        public override List<string> GetWarnings() {
            Trace.Fail("Plant node never has the warning state!");
            return [];
        }

        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "Plant Growth node for: {0} ({1})", Seed.Item?.Name, Seed.Quality?.Name);
    }

    public class PlantNodeController : BaseNodeController {
        private readonly PlantNode MyNode;

        internal PlantNodeController(PlantNode myNode) : base(myNode) { MyNode = myNode; }

        public void UpdatePlantResult() {
            if (MyNode.Seed.Item?.PlantResult is IPlantProcess plantResult && MyNode.BasePlantProcess != plantResult) {
                MyNode.BasePlantProcess = plantResult;
                foreach (NodeLink link in MyNode.OutputLinks.Where(l => l.Item.Item is IItem linkItem && !MyNode.BasePlantProcess.ProductList.Contains(linkItem)))
                    link.Controller.Delete();
                MyNode.UpdateState();
            }
        }

        public override Dictionary<string, Action> GetErrorResolutions() =>
            ErrorResolutionsWithFixOrLinks(
                (MyNode.ErrorSet & (PlantNode.Errors.InputItemMissing | PlantNode.Errors.PlantProcessMissing | PlantNode.Errors.ItemDoesntGrow)) != 0,
                (MyNode.ErrorSet & PlantNode.Errors.InvalidGrowResult) != 0 ? "Update plant results" : null,
                (MyNode.ErrorSet & PlantNode.Errors.InvalidGrowResult) != 0 ? UpdatePlantResult : null);

    }
}
