using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman {
    public class PassthroughNode : BaseNode {
        [Flags]
        public enum Errors {
            Clean = 0b_0000_0000_0000,
            InvalidLinks = 0b_1000_0000_0000
        }

        public Errors ErrorSet { get; private set; }

        private readonly BaseNodeController _controller;

        public override BaseNodeController Controller => _controller;

        public readonly ItemQualityPair PassthroughItem;

        public override IEnumerable<ItemQualityPair> Inputs {
            get { yield return PassthroughItem; }
        }

        public override IEnumerable<ItemQualityPair> Outputs {
            get { yield return PassthroughItem; }
        }

        public bool SimpleDraw;

        public PassthroughNode(ProductionGraph graph, int nodeId, ItemQualityPair item) : base(graph, nodeId) {
            PassthroughItem = item;
            SimpleDraw = true;
            _controller = PassthroughNodeController.GetController(this);
            ReadOnlyNode = new ReadOnlyPassthroughNode(this);
        }

        internal override NodeState GetUpdatedState() {
            ErrorSet = Errors.Clean;

            if (!AllLinksValid)
                ErrorSet |= Errors.InvalidLinks;

            if (ErrorSet != Errors.Clean)
                return NodeState.Error;

            return AllLinksConnected ? NodeState.Clean : NodeState.MissingLink;
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

            info.AddValue("NodeType", NodeType.Passthrough);
            info.AddValue("Item", PassthroughItem.Item.Name);
            info.AddValue("BaseQuality", PassthroughItem.Quality.Name);
            if (RateType == RateType.Manual)
                info.AddValue("DesiredRate", DesiredRatePerSec);
            info.AddValue("SDraw", SimpleDraw);
        }

        public override string ToString() {
            return $"Supply node for: {PassthroughItem.Item.Name} ({PassthroughItem.Quality.Name})";
        }
    }

    public class ReadOnlyPassthroughNode(PassthroughNode node) : ReadOnlyBaseNode(node) {
        public ItemQualityPair PassthroughItem => node.PassthroughItem;

        public bool SimpleDraw => node.SimpleDraw;

        public override List<string> GetErrors() {
            var errors = new List<string>();
            if ((node.ErrorSet & PassthroughNode.Errors.InvalidLinks) != 0)
                errors.Add("> Some links are invalid!");
            return errors;
        }

        public override List<string> GetWarnings() {
            Trace.Fail("Passthrough node never has the warning state!");
            return null;
        }
    }

    public class PassthroughNodeController : BaseNodeController {
        private readonly PassthroughNode _myNode;

        protected PassthroughNodeController(PassthroughNode myNode) : base(myNode) {
            _myNode = myNode;
        }

        public static PassthroughNodeController GetController(PassthroughNode node) {
            if (node.Controller != null)
                return (PassthroughNodeController) node.Controller;
            return new PassthroughNodeController(node);
        }

        public void SetSimpleDraw(bool alwaysRegularDraw) {
            _myNode.SimpleDraw = alwaysRegularDraw;
        }

        public override Dictionary<string, Action> GetErrorResolutions() {
            var resolutions = new Dictionary<string, Action>();
            if (_myNode.ErrorSet != PassthroughNode.Errors.Clean)
                resolutions.Add("Delete node", Delete);
            else
                foreach (var kvp in GetInvalidConnectionResolutions())
                    resolutions.Add(kvp.Key, kvp.Value);
            return resolutions;
        }

        public override Dictionary<string, Action> GetWarningResolutions() {
            Trace.Fail("Passthrough node never has the warning state!");
            return null;
        }
    }
}