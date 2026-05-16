using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman {
    [Serializable]
    public class SpoilNode : BaseNode {
        public enum Errors {
            Clean = 0b_0000_0000_0000,
            ItemDoesntSpoil = 0b_0000_0000_0001,
            InvalidSpoilResult = 0b_0000_0000_0010,
            InputItemMissing = 0b_0000_0000_0100,
            OutputItemMissing = 0b_0000_0000_1000,

            QualityMissing = 0b_0000_0001_0000,

            InvalidLinks = 0b_1000_0000_0000
        }
        public Errors ErrorSet { get; private set; }

        private readonly BaseNodeController controller;
        public override BaseNodeController Controller { get { return controller; } }

        public readonly ItemQualityPair InputItem;
        public ItemQualityPair OutputItem { get; internal set; }

        public override IEnumerable<ItemQualityPair> Inputs { get { yield return InputItem; } }
        public override IEnumerable<ItemQualityPair> Outputs { get { yield return OutputItem; } }

        //for spoil nodes, the SetValue is 'number of stacks (item slots)'
        public override double ActualSetValue => ActualRatePerSec * (InputItem.Quality is not null ? InputItem.Item?.GetItemSpoilageTime(InputItem.Quality) ?? 1 : 1) / (InputItem.Item?.StackSize ?? 1);
        public override double DesiredSetValue { get; set; }
        public override double MaxDesiredSetValue => ProductionGraph.MaxInventorySlots;
        public override string SetValueDescription => "Number of inventory slots";

        public override double DesiredRatePerSec => DesiredSetValue * (InputItem.Item?.StackSize ?? 1) / (InputItem.Quality is not null ? InputItem.Item?.GetItemSpoilageTime(InputItem.Quality) ?? 1 : 1);

        public SpoilNode(ProductionGraph graph, int nodeID, ItemQualityPair item) : this(graph, nodeID, item, item.Item?.SpoilResult) { }
        public SpoilNode(ProductionGraph graph, int nodeID, ItemQualityPair item, Item? outputItem) : base(graph, nodeID) {
            if (outputItem is null)
                throw new ArgumentNullException(nameof(outputItem));
            if (item.Quality is not Quality quality)
                throw new ArgumentException("Quality must be populated.", nameof(item));
            InputItem = item;
            OutputItem = new ItemQualityPair(outputItem, quality);
            controller = SpoilNodeController.GetController(this);
            ReadOnlyNode = new ReadOnlySpoilNode(this);
        }

        internal override NodeState GetUpdatedState() {
            ErrorSet = Errors.Clean;

            if (InputItem.Item?.SpoilResult is null)
                ErrorSet |= Errors.ItemDoesntSpoil;
            if (InputItem.Item?.SpoilResult != OutputItem.Item)
                ErrorSet |= Errors.InvalidSpoilResult;
            if (InputItem.Item?.IsMissing is true)
                ErrorSet |= Errors.InputItemMissing;
            if (OutputItem.Item?.IsMissing is true)
                ErrorSet |= Errors.OutputItemMissing;
            if (InputItem.Quality?.IsMissing is true || OutputItem.Quality?.IsMissing is true)
                ErrorSet |= Errors.QualityMissing;
            if (!AllLinksValid)
                ErrorSet |= Errors.InvalidLinks;

            if (ErrorSet != Errors.Clean) //warnings are NOT processed if error has been found. This makes sense (as an error is something that trumps warnings), plus guarantees we dont accidentally check statuses of missing objects (which rightfully dont exist in regular cache)
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

            info.AddValue("NodeType", NodeType.Spoil);
            info.AddValue("InputItem", InputItem.Item?.Name ?? "ItemNameError");
            info.AddValue("OutputItem", OutputItem.Item?.Name ?? "ItemNameError");
            info.AddValue("BaseQuality", InputItem.Quality?.Name ?? "QualityError");
        }

        public override string ToString() => string.Format("Spoil node for: {0} ({2}) to {1} ({2})", InputItem.Item?.Name, OutputItem.Item?.Name, InputItem.Quality?.Name);
    }

    public class ReadOnlySpoilNode : ReadOnlyBaseNode {
        public ItemQualityPair InputItem => MyNode.InputItem;
        public ItemQualityPair OutputItem => MyNode.OutputItem;

        private readonly SpoilNode MyNode;

        public ReadOnlySpoilNode(SpoilNode node) : base(node) { MyNode = node; }

        public override List<string> GetErrors() {
            SpoilNode.Errors ErrorSet = MyNode.ErrorSet;
            List<string> errors = new List<string>();
            if (InputItem.Item is null && InputItem.Quality is null && OutputItem.Item is null)
                return errors;

            if ((ErrorSet & SpoilNode.Errors.InputItemMissing) != 0)
                errors.Add(string.Format("> Item \"{0}\" doesnt exist in preset!", InputItem.Item?.FriendlyName));
            if ((ErrorSet & SpoilNode.Errors.OutputItemMissing) != 0)
                errors.Add(string.Format("> Spoilage Item \"{0}\" doesnt exist in preset!", OutputItem.Item?.FriendlyName));
            if ((ErrorSet & SpoilNode.Errors.ItemDoesntSpoil) != 0)
                errors.Add(string.Format("> Item \"{0}\" doesnt spoil!", InputItem.Item?.FriendlyName));
            if ((ErrorSet & SpoilNode.Errors.InvalidSpoilResult) != 0)
                errors.Add(string.Format("> Spoilage Item \"{0}\" doesnt exist in preset!", OutputItem.Item?.FriendlyName));
            if ((ErrorSet & SpoilNode.Errors.QualityMissing) != 0)
                errors.Add(string.Format("> Quality \"{0}\" doesnt exist in preset!", InputItem.Quality?.FriendlyName));

            if ((ErrorSet & SpoilNode.Errors.InvalidLinks) != 0)
                errors.Add("> Some links are invalid!");
            return errors;
        }

        public override List<string> GetWarnings() { Trace.Fail("Spoil node never has the warning state!"); return null; }
    }

    public class SpoilNodeController : BaseNodeController {
        private readonly SpoilNode MyNode;

        protected SpoilNodeController(SpoilNode myNode) : base(myNode) { MyNode = myNode; }

        public static SpoilNodeController GetController(SpoilNode node) {
            if (node.Controller != null)
                return (SpoilNodeController)node.Controller;
            return new SpoilNodeController(node);
        }

        public void UpdateSpoilResult() {
            if (MyNode.InputItem.Item?.SpoilResult is not Item spoilResult || MyNode.InputItem.Quality is not Quality quality)
                return;
            var correctSpoilResult = new ItemQualityPair(spoilResult, quality);
            if (MyNode.OutputItem != correctSpoilResult) {
                foreach (NodeLink link in MyNode.OutputLinks)
                    link.Controller.Delete();
                MyNode.OutputItem = correctSpoilResult;
                MyNode.UpdateState();
            }
        }

        public override Dictionary<string, Action> GetErrorResolutions() {
            Dictionary<string, Action> resolutions = new Dictionary<string, Action>();
            if ((MyNode.ErrorSet & (SpoilNode.Errors.InputItemMissing | SpoilNode.Errors.OutputItemMissing | SpoilNode.Errors.ItemDoesntSpoil | SpoilNode.Errors.QualityMissing)) != 0)
                resolutions.Add("Delete node", new Action(() => this.Delete()));
            if ((MyNode.ErrorSet & SpoilNode.Errors.InvalidSpoilResult) != 0)
                resolutions.Add("Update spoil result", new Action(() => UpdateSpoilResult()));
            else
                foreach (KeyValuePair<string, Action> kvp in GetInvalidConnectionResolutions())
                    resolutions.Add(kvp.Key, kvp.Value);
            return resolutions;
        }

        public override Dictionary<string, Action> GetWarningResolutions() { Trace.Fail("Spoil node never has the warning state!"); return null; }
    }
}