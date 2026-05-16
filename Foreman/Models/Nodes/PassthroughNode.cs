using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman {
    [Serializable]
    public class PassthroughNode : BaseNode {
        public enum Errors {
            Clean = 0b_0000_0000_0000,
            InvalidLinks = 0b_1000_0000_0000
        }

        public Errors ErrorSet { get; private set; }

        private readonly BaseNodeController controller;
        public override BaseNodeController Controller { get { return controller; } }

        public readonly ItemQualityPair PassthroughItem;

        public override IEnumerable<ItemQualityPair> Inputs { get { yield return PassthroughItem; } }
        public override IEnumerable<ItemQualityPair> Outputs { get { yield return PassthroughItem; } }

        public bool SimpleDraw;

        public PassthroughNode(ProductionGraph graph, int nodeID, ItemQualityPair item) : base(graph, nodeID) {
            PassthroughItem = item;
            SimpleDraw = true;
            controller = PassthroughNodeController.GetController(this);
            ReadOnlyNode = new ReadOnlyPassthroughNode(this);
        }

        internal override NodeState GetUpdatedState() {
            ErrorSet = Errors.Clean;

            if (!AllLinksValid)
                ErrorSet |= Errors.InvalidLinks;

            if (ErrorSet != Errors.Clean)
                return NodeState.Error;

            if (AllLinksConnected)
                return NodeState.Clean;
            return NodeState.MissingLink;
        }

        public override double GetConsumeRate(ItemQualityPair item) { return ActualRate; }
        public override double GetSupplyRate(ItemQualityPair item) { return ActualRate; }

        internal override double inputRateFor(ItemQualityPair item) { return 1; }
        internal override double outputRateFor(ItemQualityPair item) { return 1; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);

            info.AddValue("NodeType", NodeType.Passthrough);
            info.AddValue("Item", PassthroughItem.Item?.Name ?? "ItemNameError");
            info.AddValue("BaseQuality", PassthroughItem.Quality?.Name ?? "QualityError");
            if (RateType == RateType.Manual)
                info.AddValue("DesiredRate", DesiredRatePerSec);
            info.AddValue("SDraw", SimpleDraw);
        }

        public override string ToString() => string.Format("Supply node for: {0} ({1})", PassthroughItem.Item?.Name, PassthroughItem.Quality?.Name);
    }

    public class ReadOnlyPassthroughNode : ReadOnlyBaseNode {
        public ItemQualityPair PassthroughItem => MyNode.PassthroughItem;

        private readonly PassthroughNode MyNode;

        public ReadOnlyPassthroughNode(PassthroughNode node) : base(node) { MyNode = node; }

        public bool SimpleDraw => MyNode.SimpleDraw;

        public override List<string> GetErrors() {
            List<string> errors = new List<string>();
            if ((MyNode.ErrorSet & PassthroughNode.Errors.InvalidLinks) != 0)
                errors.Add("> Some links are invalid!");
            return errors;
        }

        public override List<string> GetWarnings() { Trace.Fail("Passthrough node never has the warning state!"); return null; }
    }

    public class PassthroughNodeController : BaseNodeController {
        private readonly PassthroughNode MyNode;

        protected PassthroughNodeController(PassthroughNode myNode) : base(myNode) { MyNode = myNode; }

        public static PassthroughNodeController GetController(PassthroughNode node) {
            if (node.Controller != null)
                return (PassthroughNodeController)node.Controller;
            return new PassthroughNodeController(node);
        }

        public void SetSimpleDraw(bool alwaysRegularDraw) { MyNode.SimpleDraw = alwaysRegularDraw; }

        public override Dictionary<string, Action> GetErrorResolutions() {
            Dictionary<string, Action> resolutions = new Dictionary<string, Action>();
            if (MyNode.ErrorSet != PassthroughNode.Errors.Clean)
                resolutions.Add("Delete node", new Action(() => this.Delete()));
            else
                foreach (KeyValuePair<string, Action> kvp in GetInvalidConnectionResolutions())
                    resolutions.Add(kvp.Key, kvp.Value);
            return resolutions;
        }

        public override Dictionary<string, Action> GetWarningResolutions() { Trace.Fail("Passthrough node never has the warning state!"); return null; }
    }
}