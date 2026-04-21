using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Foreman {
    public class ConsumerNode : BaseNode {
        [Flags]
        public enum Errors {
            Clean = 0b_0000_0000_0000,
            ItemMissing = 0b_0000_0000_0001,
            QualityMissing = 0b_0000_0000_0010,
            InvalidLinks = 0b_1000_0000_0000
        }

        [Flags]
        public enum Warnings {
            Clean = 0b_0000_0000_0000,
            ItemUnavailable = 0b_0000_0000_0001,
            ItemDisabled = 0b_0000_0000_0010,
            QualityUnavailable = 0b_0000_0000_0100,
            QualityDisabled = 0b_0000_0000_1000
        }

        public Errors ErrorSet { get; private set; }
        public Warnings WarningSet { get; private set; }

        private readonly BaseNodeController _controller;

        public override BaseNodeController Controller => _controller;

        public readonly ItemQualityPair ConsumedItem;

        public override IEnumerable<ItemQualityPair> Inputs {
            get { yield return ConsumedItem; }
        }

        public override IEnumerable<ItemQualityPair> Outputs => [];

        public ConsumerNode(ProductionGraph graph, int nodeId, ItemQualityPair item) : base(graph, nodeId) {
            ConsumedItem = item;
            _controller = ConsumerNodeController.GetController(this);
            ReadOnlyNode = new ReadOnlyConsumerNode(this);
        }

        internal override NodeState GetUpdatedState() {
            WarningSet = Warnings.Clean;
            ErrorSet = Errors.Clean;

            if (ConsumedItem.Item.IsMissing)
                ErrorSet |= Errors.ItemMissing;
            if (ConsumedItem.Quality.IsMissing)
                ErrorSet |= Errors.QualityMissing;
            if (!AllLinksValid)
                ErrorSet |= Errors.InvalidLinks;

            if (ErrorSet != Errors.Clean)
                return NodeState.Error;

            if (!ConsumedItem.Quality.Enabled)
                WarningSet |= Warnings.QualityDisabled;
            if (!ConsumedItem.Quality.Available)
                WarningSet |= Warnings.QualityUnavailable;
            if (!ConsumedItem.Item.Available)
                WarningSet |= Warnings.ItemUnavailable;
            if (!ConsumedItem.Item.Enabled)
                WarningSet |= Warnings.ItemDisabled;

            if (WarningSet != Warnings.Clean)
                return NodeState.Warning;
            if (AllLinksConnected)
                return NodeState.Clean;
            return NodeState.MissingLink;
        }

        public override double GetConsumeRate(ItemQualityPair item) {
            return ActualRate;
        }

        public override double GetSupplyRate(ItemQualityPair item) {
            throw new ArgumentException("Consumer does not supply! nothing should be asking for the supply rate");
        }

        internal override double InputRateFor(ItemQualityPair item) {
            return 1;
        }

        internal override double OutputRateFor(ItemQualityPair item) {
            throw new ArgumentException("Consumer should not have outputs!");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);

            info.AddValue("NodeType", NodeType.Consumer);
            info.AddValue("Item", ConsumedItem.Item.Name);
            info.AddValue("BaseQuality", ConsumedItem.Quality.Name);
            if (RateType == RateType.Manual)
                info.AddValue("DesiredRate", DesiredRatePerSec);
        }

        public override string ToString() {
            return $"Consumption node for: {ConsumedItem.Item.Name} ({ConsumedItem.Quality.Name})";
        }
    }

    public class ReadOnlyConsumerNode(ConsumerNode node) : ReadOnlyBaseNode(node) {
        public ItemQualityPair ConsumedItem => node.ConsumedItem;

        public override List<string> GetErrors() {
            var errors = new List<string>();
            if ((node.ErrorSet & ConsumerNode.Errors.ItemMissing) != 0)
                errors.Add($"> Item \"{ConsumedItem.Item.FriendlyName}\" doesn't exist in preset!");
            if ((node.ErrorSet & ConsumerNode.Errors.QualityMissing) != 0)
                errors.Add($"> Quality \"{ConsumedItem.Quality.FriendlyName}\" doesn't exist in preset!");
            if ((node.ErrorSet & ConsumerNode.Errors.InvalidLinks) != 0)
                errors.Add("> Some links are invalid!");
            return errors;
        }

        public override List<string> GetWarnings() {
            var warnings = new List<string>();
            if ((node.WarningSet & ConsumerNode.Warnings.QualityUnavailable) != 0)
                warnings.Add($"> Quality \"{ConsumedItem.Quality.FriendlyName}\" isn't available in regular gameplay.");
            else if ((node.WarningSet & ConsumerNode.Warnings.QualityDisabled) != 0)
                warnings.Add($"> Quality \"{ConsumedItem.Quality.FriendlyName}\" isn't currently enabled.");
            if ((node.WarningSet & ConsumerNode.Warnings.ItemDisabled) != 0)
                warnings.Add($"> Item \"{ConsumedItem.Quality.FriendlyName}\" isn't currently enabled.");
            if ((node.WarningSet & ConsumerNode.Warnings.ItemUnavailable) != 0)
                warnings.Add($"> Item \"{ConsumedItem.Quality.FriendlyName}\" is unavailable in regular play.");
            return warnings;
        }
    }

    public class ConsumerNodeController : BaseNodeController {
        private readonly ConsumerNode _myNode;

        protected ConsumerNodeController(ConsumerNode myNode) : base(myNode) {
            _myNode = myNode;
        }

        public static ConsumerNodeController GetController(ConsumerNode node) {
            if (node.Controller != null)
                return (ConsumerNodeController) node.Controller;
            return new ConsumerNodeController(node);
        }

        public override Dictionary<string, Action> GetErrorResolutions() {
            var resolutions = new Dictionary<string, Action>();
            if (_myNode.ErrorSet != ConsumerNode.Errors.Clean)
                resolutions.Add("Delete node", Delete);
            else
                foreach (var kvp in GetInvalidConnectionResolutions())
                    resolutions.Add(kvp.Key, kvp.Value);
            return resolutions;
        }

        public override Dictionary<string, Action> GetWarningResolutions() {
            return new Dictionary<string, Action>();
        }
    }
}