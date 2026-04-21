using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Foreman {
    public class SupplierNode : BaseNode {
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

        public readonly ItemQualityPair SuppliedItem;

        public override IEnumerable<ItemQualityPair> Inputs => [];

        public override IEnumerable<ItemQualityPair> Outputs {
            get { yield return SuppliedItem; }
        }

        public SupplierNode(ProductionGraph graph, int nodeId, ItemQualityPair item) : base(graph, nodeId) {
            SuppliedItem = item;
            _controller = SupplierNodeController.GetController(this);
            ReadOnlyNode = new ReadOnlySupplierNode(this);
        }

        internal override NodeState GetUpdatedState() {
            WarningSet = Warnings.Clean;
            ErrorSet = Errors.Clean;

            if (SuppliedItem.Item.IsMissing)
                ErrorSet |= Errors.ItemMissing;
            if (!SuppliedItem.Quality.Available)
                ErrorSet |= Errors.QualityMissing;
            if (!AllLinksValid)
                ErrorSet |= Errors.InvalidLinks;

            if (ErrorSet != Errors.Clean)
                return NodeState.Error;

            if (!SuppliedItem.Quality.Enabled)
                WarningSet |= Warnings.QualityDisabled;
            if (!SuppliedItem.Item.Available)
                WarningSet |= Warnings.ItemUnavailable;
            if (!SuppliedItem.Item.Enabled)
                WarningSet |= Warnings.ItemDisabled;

            if (WarningSet != Warnings.Clean)
                return NodeState.Warning;
            if (AllLinksConnected)
                return NodeState.Clean;
            return NodeState.MissingLink;
        }

        public override double GetConsumeRate(ItemQualityPair item) {
            throw new ArgumentException("Supplier does not consume! nothing should be asking for the consume rate");
        }

        public override double GetSupplyRate(ItemQualityPair item) {
            return RateType == RateType.Manual ? DesiredRate : ActualRate;
        }

        internal override double InputRateFor(ItemQualityPair item) {
            throw new ArgumentException("Supplier should not have outputs!");
        }

        internal override double OutputRateFor(ItemQualityPair item) {
            return 1;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);

            info.AddValue("NodeType", NodeType.Supplier);
            info.AddValue("Item", SuppliedItem.Item.Name);
            info.AddValue("BaseQuality", SuppliedItem.Quality.Name);
            if (RateType == RateType.Manual)
                info.AddValue("DesiredRate", DesiredRatePerSec);
        }

        public override string ToString() {
            return $"Supply node for: {SuppliedItem.Item.Name} ({SuppliedItem.Quality.Name})";
        }
    }

    public class ReadOnlySupplierNode(SupplierNode node) : ReadOnlyBaseNode(node) {
        public ItemQualityPair SuppliedItem => node.SuppliedItem;

        public override List<string> GetErrors() {
            var errors = new List<string>();
            if ((node.ErrorSet & SupplierNode.Errors.ItemMissing) != 0)
                errors.Add($"> Item \"{SuppliedItem.Item.FriendlyName}\" doesn't exist in preset!");
            if ((node.ErrorSet & SupplierNode.Errors.QualityMissing) != 0)
                errors.Add($"> Quality \"{SuppliedItem.Quality.FriendlyName}\" doesn't exist in preset!");
            if ((node.ErrorSet & SupplierNode.Errors.InvalidLinks) != 0)
                errors.Add("> Some links are invalid!");
            return errors;
        }

        public override List<string> GetWarnings() {
            var warnings = new List<string>();
            if ((node.WarningSet & SupplierNode.Warnings.QualityUnavailable) != 0)
                warnings.Add($"> Quality \"{SuppliedItem.Quality.FriendlyName}\" isn't available in regular gameplay.");
            else if ((node.WarningSet & SupplierNode.Warnings.QualityDisabled) != 0)
                warnings.Add($"> Quality \"{SuppliedItem.Quality.FriendlyName}\" isn't currently enabled.");
            if ((node.WarningSet & SupplierNode.Warnings.ItemDisabled) != 0)
                warnings.Add($"> Item \"{SuppliedItem.Quality.FriendlyName}\" isn't currently enabled.");
            if ((node.WarningSet & SupplierNode.Warnings.ItemUnavailable) != 0)
                warnings.Add($"> Item \"{SuppliedItem.Quality.FriendlyName}\" is unavailable in regular play.");
            return warnings;
        }
    }

    public class SupplierNodeController : BaseNodeController {
        private readonly SupplierNode _myNode;

        protected SupplierNodeController(SupplierNode myNode) : base(myNode) {
            _myNode = myNode;
        }

        public static SupplierNodeController GetController(SupplierNode node) {
            if (node.Controller != null)
                return (SupplierNodeController) node.Controller;
            return new SupplierNodeController(node);
        }

        public override Dictionary<string, Action> GetErrorResolutions() {
            var resolutions = new Dictionary<string, Action>();
            if (_myNode.ErrorSet != SupplierNode.Errors.Clean)
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