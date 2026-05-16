using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Foreman {
    [Serializable]
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

        public readonly ItemQualityPair ConsumedItem;

        public override IEnumerable<ItemQualityPair> Inputs { get { yield return ConsumedItem; } }
        public override IEnumerable<ItemQualityPair> Outputs { get { return new ItemQualityPair[0]; } }

        public ConsumerNode(ProductionGraph graph, int nodeID, ItemQualityPair item) : base(graph, nodeID) {
            ConsumedItem = item;
            controller = ConsumerNodeController.GetController(this);
            ReadOnlyNode = new ReadOnlyConsumerNode(this);
        }

        internal override NodeState GetUpdatedState() {
            WarningSet = Warnings.Clean;
            ErrorSet = Errors.Clean;

            if (ConsumedItem.Item?.IsMissing is true)
                ErrorSet |= Errors.ItemMissing;
            if (ConsumedItem.Quality?.IsMissing is true)
                ErrorSet |= Errors.QualityMissing;
            if (!AllLinksValid)
                ErrorSet |= Errors.InvalidLinks;

            if (ErrorSet != Errors.Clean)
                return NodeState.Error;

            if (ConsumedItem.Quality?.Enabled is not true)
                WarningSet |= Warnings.QualityDisabled;
            if (ConsumedItem.Quality?.Available is not true)
                WarningSet |= Warnings.QualityUnavailable;
            if (ConsumedItem.Item?.Available is not true)
                WarningSet |= Warnings.ItemUnavailable;
            if (ConsumedItem.Item?.Enabled is not true)
                WarningSet |= Warnings.ItemDisabled;

            if (WarningSet != Warnings.Clean)
                return NodeState.Warning;
            if (AllLinksConnected)
                return NodeState.Clean;
            return NodeState.MissingLink;
        }

        public override double GetConsumeRate(ItemQualityPair item) { return ActualRate; }
        public override double GetSupplyRate(ItemQualityPair item) { throw new ArgumentException("Consumer does not supply! nothing should be asking for the supply rate"); }

        internal override double inputRateFor(ItemQualityPair item) { return 1; }
        internal override double outputRateFor(ItemQualityPair item) { throw new ArgumentException("Consumer should not have outputs!"); }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);

            info.AddValue("NodeType", NodeType.Consumer);
            info.AddValue("Item", ConsumedItem.Item?.Name ?? "ItemNameError");
            info.AddValue("BaseQuality", ConsumedItem.Quality?.Name ?? "QualityError");
            if (RateType == RateType.Manual)
                info.AddValue("DesiredRate", DesiredRatePerSec);
        }

        public override string ToString() => string.Format("Consumption node for: {0} ({1})", ConsumedItem.Item?.Name, ConsumedItem.Quality?.Name);
    }

    public class ReadOnlyConsumerNode : ReadOnlyBaseNode {
        public ItemQualityPair ConsumedItem => MyNode.ConsumedItem;

        private readonly ConsumerNode MyNode;

        public ReadOnlyConsumerNode(ConsumerNode node) : base(node) { MyNode = node; }

        public override List<string> GetErrors() {
            List<string> errors = new List<string>();
            if (ConsumedItem.Item is null || ConsumedItem.Quality is null)
                return errors;
            if ((MyNode.ErrorSet & ConsumerNode.Errors.ItemMissing) != 0)
                errors.Add(string.Format("> Item \"{0}\" doesnt exist in preset!", ConsumedItem.Item.FriendlyName));
            if ((MyNode.ErrorSet & ConsumerNode.Errors.QualityMissing) != 0)
                errors.Add(string.Format("> Quality \"{0}\" doesnt exist in preset!", ConsumedItem.Quality.FriendlyName));
            if ((MyNode.ErrorSet & ConsumerNode.Errors.InvalidLinks) != 0)
                errors.Add("> Some links are invalid!");
            return errors;
        }

        public override List<string> GetWarnings() {
            List<string> warnings = new List<string>();
            if (ConsumedItem.Quality is null)
                return warnings;
            if ((MyNode.WarningSet & ConsumerNode.Warnings.QualityUnavailable) != 0)
                warnings.Add(string.Format("> Quality \"{0}\" isnt available in regular gameplay.", ConsumedItem.Quality.FriendlyName));
            else if ((MyNode.WarningSet & ConsumerNode.Warnings.QualityDisabled) != 0)
                warnings.Add(string.Format("> Quality \"{0}\" isnt currently enabled.", ConsumedItem.Quality.FriendlyName));
            if ((MyNode.WarningSet & ConsumerNode.Warnings.ItemDisabled) != 0)
                warnings.Add(string.Format("> Item \"{0}\" isnt currently enabled.", ConsumedItem.Quality.FriendlyName));
            if ((MyNode.WarningSet & ConsumerNode.Warnings.ItemUnavailable) != 0)
                warnings.Add(string.Format("> Item \"{0}\" is unavailable in regular play.", ConsumedItem.Quality.FriendlyName));
            return warnings;
        }
    }

    public class ConsumerNodeController : BaseNodeController {
        private readonly ConsumerNode MyNode;

        protected ConsumerNodeController(ConsumerNode myNode) : base(myNode) { MyNode = myNode; }

        public static ConsumerNodeController GetController(ConsumerNode node) {
            if (node.Controller != null)
                return (ConsumerNodeController)node.Controller;
            return new ConsumerNodeController(node);
        }

        public override Dictionary<string, Action> GetErrorResolutions() {
            Dictionary<string, Action> resolutions = new Dictionary<string, Action>();
            if (MyNode.ErrorSet != ConsumerNode.Errors.Clean)
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