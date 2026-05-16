using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman {
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
            controller = new PassthroughNodeController(this);
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

        public override List<string> GetErrors() =>
            (ErrorSet & Errors.InvalidLinks) != 0 ? ["> Some links are invalid!"] : [];

        public override List<string> GetWarnings() {
            Trace.Fail("Passthrough node never has the warning state!");
            return [];
        }

        public override string ToString() => string.Format("Passthrough node for: {0} ({1})", PassthroughItem.Item?.Name, PassthroughItem.Quality?.Name);
    }

    public class PassthroughNodeController : BaseNodeController {
        private readonly PassthroughNode MyNode;

        internal PassthroughNodeController(PassthroughNode myNode) : base(myNode) { MyNode = myNode; }

        public void SetSimpleDraw(bool alwaysRegularDraw) { MyNode.SimpleDraw = alwaysRegularDraw; }

        public override Dictionary<string, Action> GetErrorResolutions() =>
            ErrorResolutionsDeleteOrFixLinks(MyNode.ErrorSet != PassthroughNode.Errors.Clean);

    }
}