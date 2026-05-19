using System;
using System.Collections.Generic;

namespace Foreman.Models.Nodes {
    public class ConsumerNode : BaseNode {
        public enum Errors {
            Clean = 0b_0000_0000_0000,
            ItemMissing = 0b_0000_0000_0001,
            QualityMissing = 0b_0000_0000_0010,
            InvalidLinks = 0b_1000_0000_0000
        }
        public enum Warnings {
            Clean = 0b_0000_0000_0000,
            ItemUnavailable = 0b_0000_0000_0001,
            ItemDisabled = 0b_0000_0000_0010,
            QualityUnavailable = 0b_0000_0000_0100,
            QualityDisabled = 0b_0000_0000_1000
        }
        public Errors ErrorSet { get; private set; }
        public Warnings WarningSet { get; private set; }

        private readonly BaseNodeController controller;
        public override BaseNodeController Controller { get { return controller; } }

        public ItemQualityPair ConsumedItem { get; }
        public override IEnumerable<ItemQualityPair> Inputs { get { yield return ConsumedItem; } }
        public override IEnumerable<ItemQualityPair> Outputs { get { return []; } }

        public ConsumerNode(ProductionGraph graph, int nodeID, ItemQualityPair item) : base(graph, nodeID) {
            ConsumedItem = item;
            controller = new ConsumerNodeController(this);
        }

        internal override NodeState GetUpdatedState() {
            ItemQualityNodeState.Evaluate(ConsumedItem, AllLinksValid, AllLinksConnected, out int errors, out int warnings, out NodeState state);
            ErrorSet = (Errors)errors;
            WarningSet = (Warnings)warnings;
            return state;
        }

        public override double GetConsumeRate(ItemQualityPair item) { return ActualRate; }
        public override double GetSupplyRate(ItemQualityPair item) { throw new ArgumentException("Consumer does not supply! nothing should be asking for the supply rate"); }

        internal override double inputRateFor(ItemQualityPair item) { return 1; }
        internal override double outputRateFor(ItemQualityPair item) { throw new ArgumentException("Consumer should not have outputs!"); }

        public override List<string> GetErrors() => ItemQualityNodeMessages.GetErrors(ConsumedItem, (int)ErrorSet);
        public override List<string> GetWarnings() => ItemQualityNodeMessages.GetWarnings(ConsumedItem, (int)WarningSet);

        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "Consumption node for: {0} ({1})", ConsumedItem.Item?.Name, ConsumedItem.Quality?.Name);
    }

    public class ConsumerNodeController : BaseNodeController {
        private readonly ConsumerNode MyNode;

        internal ConsumerNodeController(ConsumerNode myNode) : base(myNode) { MyNode = myNode; }

        public override Dictionary<string, Action> GetErrorResolutions() =>
            ErrorResolutionsDeleteOrFixLinks(MyNode.ErrorSet != ConsumerNode.Errors.Clean);
    }
}
