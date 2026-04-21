using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman {
    public class PlantNode : BaseNode {
        [Flags]
        public enum Errors {
            Clean = 0b_0000_0000_0000,
            ItemNotGrowing = 0b_0000_0000_0001,
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

        private readonly BaseNodeController _controller;

        public override BaseNodeController Controller => _controller;

        public ItemQualityPair Seed { get; private set; }
        public PlantProcess BasePlantProcess { get; internal set; }

        public override IEnumerable<ItemQualityPair> Inputs {
            get { yield return Seed; }
        }

        public override IEnumerable<ItemQualityPair> Outputs {
            get { return BasePlantProcess.ProductList.Select(product => new ItemQualityPair(product, product.Owner.DefaultQuality)); }
        }

        // for plant nodes, the SetValue is 'number of plant tiles'
        public override double ActualSetValue => ActualRatePerSec * Seed.Item.PlantResult.GrowTime;

        public override double DesiredSetValue { get; set; }

        public override double MaxDesiredSetValue => ProductionGraph.MaxTiles;

        public override string SetValueDescription => "Number of farming tiles";

        public override double DesiredRatePerSec => DesiredSetValue / Seed.Item.PlantResult.GrowTime;

        public PlantNode(ProductionGraph graph, int nodeId, ItemQualityPair item) : this(graph, nodeId, item.Item.PlantResult, item.Quality) { }

        public PlantNode(ProductionGraph graph, int nodeId, PlantProcess plantProcess, Quality quality) : base(graph, nodeId) {
            BasePlantProcess = plantProcess;
            Seed = new ItemQualityPair(plantProcess.Seed, quality);
            _controller = PlantNodeController.GetController(this);
            ReadOnlyNode = new ReadOnlyPlantNode(this);
        }

        internal override NodeState GetUpdatedState() {
            ErrorSet = Errors.Clean;

            if (Seed.Item.PlantResult == null)
                ErrorSet |= Errors.ItemNotGrowing;
            if (Seed.Item.PlantResult != BasePlantProcess)
                ErrorSet |= Errors.InvalidGrowResult;
            if (Seed.Item.IsMissing)
                ErrorSet |= Errors.InputItemMissing;
            if (BasePlantProcess.IsMissing)
                ErrorSet |= Errors.PlantProcessMissing;
            if (Seed.Quality.IsMissing)
                ErrorSet |= Errors.QualityMissing;
            if (!AllLinksValid)
                ErrorSet |= Errors.InvalidLinks;

            // warnings are NOT processed if error has been found. This makes sense (as an error is something that trumps warnings),
            // plus guarantees we don't accidentally check statuses of missing objects (which rightfully don't exist in regular cache)

            if (ErrorSet != Errors.Clean)
                return NodeState.Error;
            if (AllLinksConnected)
                return NodeState.Clean;
            return NodeState.MissingLink;
        }

        public override double GetConsumeRate(ItemQualityPair item) {
            return ActualRate;
        }

        public override double GetSupplyRate(ItemQualityPair item) {
            return ActualRate * OutputRateFor(item);
        }

        internal override double InputRateFor(ItemQualityPair item) {
            return 1;
        }

        internal override double OutputRateFor(ItemQualityPair item) {
            return BasePlantProcess.ProductSet[item.Item];
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);

            info.AddValue("NodeType", NodeType.Plant);
            info.AddValue("PlantProcessID", BasePlantProcess.PlantID);
            info.AddValue("BaseQuality", Seed.Quality.Name);
        }

        public override string ToString() {
            return $"Plant Growth node for: {Seed.Item.Name} ({Seed.Quality.Name})";
        }
    }

    public class ReadOnlyPlantNode(PlantNode node) : ReadOnlyBaseNode(node) {
        public ItemQualityPair Seed => node.Seed;
        public PlantProcess SeedPlantProcess => node.BasePlantProcess;

        public override List<string> GetErrors() {
            var errorSet = node.ErrorSet;
            var errors = new List<string>();

            if ((errorSet & PlantNode.Errors.InputItemMissing) != 0)
                errors.Add($"> Item \"{Seed.Item.FriendlyName}\" doesn't exist in preset!");
            if ((errorSet & PlantNode.Errors.PlantProcessMissing) != 0)
                errors.Add($"> Growth process for item \"{Seed.Item.FriendlyName}\" doesn't exist in preset!");
            if ((errorSet & PlantNode.Errors.ItemNotGrowing) != 0)
                errors.Add($"> Item \"{Seed.Item.FriendlyName}\" cant be planted!");
            if ((errorSet & PlantNode.Errors.InvalidGrowResult) != 0)
                errors.Add($"> Growth result for item \"{Seed.Item.FriendlyName}\" doesn't match preset!");
            if ((errorSet & PlantNode.Errors.QualityMissing) != 0)
                errors.Add($"> Quality \"{Seed.Quality.FriendlyName}\" doesn't exist in preset!");
            if ((errorSet & PlantNode.Errors.InvalidLinks) != 0)
                errors.Add("> Some links are invalid!");
            return errors;
        }

        public override List<string> GetWarnings() {
            Trace.Fail("Spoil node never has the warning state!");
            return null;
        }
    }

    public class PlantNodeController : BaseNodeController {
        private readonly PlantNode _myNode;

        protected PlantNodeController(PlantNode myNode) : base(myNode) {
            _myNode = myNode;
        }

        public static PlantNodeController GetController(PlantNode node) {
            if (node.Controller != null)
                return (PlantNodeController) node.Controller;
            return new PlantNodeController(node);
        }

        public void UpdatePlantResult() {
            if (_myNode.BasePlantProcess == _myNode.Seed.Item.PlantResult)
                return;

            _myNode.BasePlantProcess = _myNode.Seed.Item.PlantResult;
            foreach (var link in _myNode.OutputLinks.Where(l => !_myNode.BasePlantProcess.ProductList.Contains(l.Item.Item)))
                link.Controller.Delete();
            _myNode.UpdateState();
        }

        public override Dictionary<string, Action> GetErrorResolutions() {
            var resolutions = new Dictionary<string, Action>();
            if ((_myNode.ErrorSet & (PlantNode.Errors.InputItemMissing | PlantNode.Errors.PlantProcessMissing | PlantNode.Errors.ItemNotGrowing)) != 0)
                resolutions.Add("Delete node", Delete);
            if ((_myNode.ErrorSet & PlantNode.Errors.InvalidGrowResult) != 0)
                resolutions.Add("Update plant results", UpdatePlantResult);
            else
                foreach (var kvp in GetInvalidConnectionResolutions())
                    resolutions.Add(kvp.Key, kvp.Value);
            return resolutions;
        }

        public override Dictionary<string, Action> GetWarningResolutions() {
            Trace.Fail("Plant node never has the warning state!");
            return null;
        }
    }
}