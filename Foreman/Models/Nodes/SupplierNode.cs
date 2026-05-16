using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman {
    [Serializable]
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

        public readonly ItemQualityPair SuppliedItem;

        public override IEnumerable<ItemQualityPair> Inputs { get { return new ItemQualityPair[0]; } }
        public override IEnumerable<ItemQualityPair> Outputs { get { yield return SuppliedItem; } }

        public SupplierNode(ProductionGraph graph, int nodeID, ItemQualityPair item) : base(graph, nodeID) {
            SuppliedItem = item;
            controller = SupplierNodeController.GetController(this);
            ReadOnlyNode = new ReadOnlySupplierNode(this);
        }

        internal override NodeState GetUpdatedState() {
            WarningSet = Warnings.Clean;
            ErrorSet = Errors.Clean;

            if (SuppliedItem.Item?.IsMissing is true)
                ErrorSet |= Errors.ItemMissing;
            if (SuppliedItem.Quality?.Available is not true)
                ErrorSet |= Errors.QualityMissing;
            if (!AllLinksValid)
                ErrorSet |= Errors.InvalidLinks;

            if (ErrorSet != Errors.Clean)
                return NodeState.Error;

            if (SuppliedItem.Quality?.Enabled is not true)
                WarningSet |= Warnings.QualityDisabled;
            if (SuppliedItem.Item?.Available is not true)
                WarningSet |= Warnings.ItemUnavailable;
            if (SuppliedItem.Item?.Enabled is not true)
                WarningSet |= Warnings.ItemDisabled;

            if (WarningSet != Warnings.Clean)
                return NodeState.Warning;
            if (AllLinksConnected)
                return NodeState.Clean;
            return NodeState.MissingLink;
        }

        public override double GetConsumeRate(ItemQualityPair item) { throw new ArgumentException("Supplier does not consume! nothing should be asking for the consume rate"); }
        public override double GetSupplyRate(ItemQualityPair item) { return (RateType == RateType.Manual) ? DesiredRate : ActualRate; }

        internal override double inputRateFor(ItemQualityPair item) { throw new ArgumentException("Supplier should not have outputs!"); }
        internal override double outputRateFor(ItemQualityPair item) { return 1; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);

            info.AddValue("NodeType", NodeType.Supplier);
            info.AddValue("Item", SuppliedItem.Item?.Name ?? "ItemNameError");
            info.AddValue("BaseQuality", SuppliedItem.Quality?.Name ?? "QualityError");
            if (RateType == RateType.Manual)
                info.AddValue("DesiredRate", DesiredRatePerSec);
        }

        public override string ToString() => string.Format("Supply node for: {0} ({1})", SuppliedItem.Item?.Name, SuppliedItem.Quality?.Name);
    }

    public class ReadOnlySupplierNode : ReadOnlyBaseNode {
        public ItemQualityPair SuppliedItem => MyNode.SuppliedItem;

        private readonly SupplierNode MyNode;

        public ReadOnlySupplierNode(SupplierNode node) : base(node) { MyNode = node; }

        public override List<string> GetErrors() {
            List<string> errors = [];
            if (SuppliedItem.Item is null || SuppliedItem.Quality is null)
                return errors;
            if ((MyNode.ErrorSet & SupplierNode.Errors.ItemMissing) != 0)
                errors.Add(string.Format("> Item \"{0}\" doesnt exist in preset!", SuppliedItem.Item.FriendlyName));
            if ((MyNode.ErrorSet & SupplierNode.Errors.QualityMissing) != 0)
                errors.Add(string.Format("> Quality \"{0}\" doesnt exist in preset!", SuppliedItem.Quality.FriendlyName));
            if ((MyNode.ErrorSet & SupplierNode.Errors.InvalidLinks) != 0)
                errors.Add("> Some links are invalid!");
            return errors;
        }

        public override List<string> GetWarnings() {
            List<string> warnings = [];
            if (SuppliedItem.Quality is null)
                return warnings;
            if ((MyNode.WarningSet & SupplierNode.Warnings.QualityUnavailable) != 0)
                warnings.Add(string.Format("> Quality \"{0}\" isnt available in regular gameplay.", SuppliedItem.Quality.FriendlyName));
            else if ((MyNode.WarningSet & SupplierNode.Warnings.QualityDisabled) != 0)
                warnings.Add(string.Format("> Quality \"{0}\" isnt currently enabled.", SuppliedItem.Quality.FriendlyName));
            if ((MyNode.WarningSet & SupplierNode.Warnings.ItemDisabled) != 0)
                warnings.Add(string.Format("> Item \"{0}\" isnt currently enabled.", SuppliedItem.Quality.FriendlyName));
            if ((MyNode.WarningSet & SupplierNode.Warnings.ItemUnavailable) != 0)
                warnings.Add(string.Format("> Item \"{0}\" is unavailable in regular play.", SuppliedItem.Quality.FriendlyName));
            return warnings;
        }
    }

    public class SupplierNodeController : BaseNodeController {
        private readonly SupplierNode MyNode;

        protected SupplierNodeController(SupplierNode myNode) : base(myNode) { MyNode = myNode; }

        public static SupplierNodeController GetController(SupplierNode node) {
            if (node.Controller != null)
                return (SupplierNodeController)node.Controller;
            return new SupplierNodeController(node);
        }

        public override Dictionary<string, Action> GetErrorResolutions() {
            Dictionary<string, Action> resolutions = new Dictionary<string, Action>();
            if (MyNode.ErrorSet != SupplierNode.Errors.Clean)
                resolutions.Add("Delete node", new Action(() => this.Delete()));
            else
                foreach (KeyValuePair<string, Action> kvp in GetInvalidConnectionResolutions())
                    resolutions.Add(kvp.Key, kvp.Value);
            return resolutions;
        }

        public override Dictionary<string, Action> GetWarningResolutions() {
            return new Dictionary<string, Action>();
        }
    }
}