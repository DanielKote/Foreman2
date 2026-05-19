using System;
using System.Collections.Generic;

namespace Foreman.Models.Nodes {
    public class SupplierNode : BaseNode {
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

        public ItemQualityPair SuppliedItem { get; }
        public override IEnumerable<ItemQualityPair> Inputs { get { return []; } }
        public override IEnumerable<ItemQualityPair> Outputs { get { yield return SuppliedItem; } }

        public SupplierNode(ProductionGraph graph, int nodeID, ItemQualityPair item) : base(graph, nodeID) {
            SuppliedItem = item;
            controller = new SupplierNodeController(this);
        }

        internal override NodeState GetUpdatedState() {
            ItemQualityNodeState.Evaluate(SuppliedItem, AllLinksValid, AllLinksConnected, out int errors, out int warnings, out NodeState state);
            ErrorSet = (Errors)errors;
            WarningSet = (Warnings)warnings;
            return state;
        }

        public override double GetConsumeRate(ItemQualityPair item) { throw new ArgumentException("Supplier does not consume! nothing should be asking for the consume rate"); }
        public override double GetSupplyRate(ItemQualityPair item) { return (RateType == RateType.Manual) ? DesiredRate : ActualRate; }

        internal override double inputRateFor(ItemQualityPair item) { throw new ArgumentException("Supplier does not consume!"); }
        internal override double outputRateFor(ItemQualityPair item) { return 1; }

        public override List<string> GetErrors() => ItemQualityNodeMessages.GetErrors(SuppliedItem, (int)ErrorSet);
        public override List<string> GetWarnings() => ItemQualityNodeMessages.GetWarnings(SuppliedItem, (int)WarningSet);

        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "Supply node for: {0} ({1})", SuppliedItem.Item?.Name, SuppliedItem.Quality?.Name);
    }

    public class SupplierNodeController : BaseNodeController {
        private readonly SupplierNode MyNode;

        internal SupplierNodeController(SupplierNode myNode) : base(myNode) { MyNode = myNode; }

        public override Dictionary<string, Action> GetErrorResolutions() =>
            ErrorResolutionsDeleteOrFixLinks(MyNode.ErrorSet != SupplierNode.Errors.Clean);
    }
}
