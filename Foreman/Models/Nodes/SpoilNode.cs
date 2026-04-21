using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman {
    public class SpoilNode : BaseNode {
        [Flags]
        public enum Errors {
            Clean = 0b_0000_0000_0000,
            ItemNotSpoiling = 0b_0000_0000_0001,
            InvalidSpoilResult = 0b_0000_0000_0010,
            InputItemMissing = 0b_0000_0000_0100,
            OutputItemMissing = 0b_0000_0000_1000,

            QualityMissing = 0b_0000_0001_0000,

            InvalidLinks = 0b_1000_0000_0000
        }

        public Errors ErrorSet { get; private set; }

        private readonly BaseNodeController _controller;

        public override BaseNodeController Controller => _controller;

        public readonly ItemQualityPair InputItem;
        public ItemQualityPair OutputItem { get; internal set; }

        public override IEnumerable<ItemQualityPair> Inputs {
            get { yield return InputItem; }
        }

        public override IEnumerable<ItemQualityPair> Outputs {
            get { yield return OutputItem; }
        }

        // for spoil nodes, the SetValue is 'number of stacks (item slots)'
        public override double ActualSetValue => ActualRatePerSec
            * InputItem.Item.GetItemSpoilageTime(InputItem.Quality)
            / InputItem.Item.StackSize;

        public override double DesiredSetValue { get; set; }

        public override double MaxDesiredSetValue => ProductionGraph.MaxInventorySlots;

        public override string SetValueDescription => "Number of inventory slots";

        public override double DesiredRatePerSec => DesiredSetValue * InputItem.Item.StackSize / InputItem.Item.GetItemSpoilageTime(InputItem.Quality);

        public SpoilNode(ProductionGraph graph, int nodeId, ItemQualityPair item) : this(graph, nodeId, item, item.Item.SpoilResult) { }

        public SpoilNode(ProductionGraph graph, int nodeId, ItemQualityPair item, Item outputItem) : base(graph, nodeId) {
            InputItem = item;
            OutputItem = new ItemQualityPair(outputItem, item.Quality);
            _controller = SpoilNodeController.GetController(this);
            ReadOnlyNode = new ReadOnlySpoilNode(this);
        }

        internal override NodeState GetUpdatedState() {
            ErrorSet = Errors.Clean;

            if (InputItem.Item.SpoilResult == null)
                ErrorSet |= Errors.ItemNotSpoiling;
            if (InputItem.Item.SpoilResult != OutputItem.Item)
                ErrorSet |= Errors.InvalidSpoilResult;
            if (InputItem.Item.IsMissing)
                ErrorSet |= Errors.InputItemMissing;
            if (OutputItem.Item.IsMissing)
                ErrorSet |= Errors.OutputItemMissing;
            if (InputItem.Quality.IsMissing || OutputItem.Quality.IsMissing)
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
            return ActualRate;
        }

        internal override double InputRateFor(ItemQualityPair item) {
            return 1;
        }

        internal override double OutputRateFor(ItemQualityPair item) {
            return 1;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);

            info.AddValue("NodeType", NodeType.Spoil);
            info.AddValue("InputItem", InputItem.Item.Name);
            info.AddValue("OutputItem", OutputItem.Item.Name);
            info.AddValue("BaseQuality", InputItem.Quality.Name);
        }

        public override string ToString() {
            return $"Spoil node for: {InputItem.Item.Name} ({InputItem.Quality.Name}) to {OutputItem.Item.Name} ({InputItem.Quality.Name})";
        }
    }

    public class ReadOnlySpoilNode(SpoilNode node) : ReadOnlyBaseNode(node) {
        public ItemQualityPair InputItem => node.InputItem;
        public ItemQualityPair OutputItem => node.OutputItem;

        public override List<string> GetErrors() {
            var errorSet = node.ErrorSet;
            var errors = new List<string>();

            if ((errorSet & SpoilNode.Errors.InputItemMissing) != 0)
                errors.Add($"> Item \"{InputItem.Item.FriendlyName}\" doesn't exist in preset!");
            if ((errorSet & SpoilNode.Errors.OutputItemMissing) != 0)
                errors.Add($"> Spoilage Item \"{OutputItem.Item.FriendlyName}\" doesn't exist in preset!");
            if ((errorSet & SpoilNode.Errors.ItemNotSpoiling) != 0)
                errors.Add($"> Item \"{InputItem.Item.FriendlyName}\" doesn't spoil!");
            if ((errorSet & SpoilNode.Errors.InvalidSpoilResult) != 0)
                errors.Add($"> Spoilage Item \"{OutputItem.Item.FriendlyName}\" doesn't exist in preset!");
            if ((errorSet & SpoilNode.Errors.QualityMissing) != 0)
                errors.Add($"> Quality \"{InputItem.Quality.FriendlyName}\" doesn't exist in preset!");

            if ((errorSet & SpoilNode.Errors.InvalidLinks) != 0)
                errors.Add("> Some links are invalid!");
            return errors;
        }

        public override List<string> GetWarnings() {
            Trace.Fail("Spoil node never has the warning state!");
            return null;
        }
    }

    public class SpoilNodeController : BaseNodeController {
        private readonly SpoilNode _myNode;

        protected SpoilNodeController(SpoilNode myNode) : base(myNode) {
            _myNode = myNode;
        }

        public static SpoilNodeController GetController(SpoilNode node) {
            if (node.Controller != null)
                return (SpoilNodeController) node.Controller;
            return new SpoilNodeController(node);
        }

        public void UpdateSpoilResult() {
            var correctSpoilResult = new ItemQualityPair(_myNode.InputItem.Item.SpoilResult, _myNode.InputItem.Quality);
            if (_myNode.OutputItem == correctSpoilResult)
                return;

            foreach (var link in _myNode.OutputLinks)
                link.Controller.Delete();
            _myNode.OutputItem = correctSpoilResult;
            _myNode.UpdateState();
        }

        public override Dictionary<string, Action> GetErrorResolutions() {
            var resolutions = new Dictionary<string, Action>();
            if ((_myNode.ErrorSet & (SpoilNode.Errors.InputItemMissing | SpoilNode.Errors.OutputItemMissing | SpoilNode.Errors.ItemNotSpoiling |
                SpoilNode.Errors.QualityMissing)) != 0)
                resolutions.Add("Delete node", Delete);
            if ((_myNode.ErrorSet & SpoilNode.Errors.InvalidSpoilResult) != 0)
                resolutions.Add("Update spoil result", UpdateSpoilResult);
            else
                foreach (var kvp in GetInvalidConnectionResolutions())
                    resolutions.Add(kvp.Key, kvp.Value);
            return resolutions;
        }

        public override Dictionary<string, Action> GetWarningResolutions() {
            Trace.Fail("Spoil node never has the warning state!");
            return null;
        }
    }
}