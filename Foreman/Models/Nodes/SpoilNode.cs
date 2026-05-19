using Foreman.DataCaching.DataTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Foreman.Models.Nodes {
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

        public ItemQualityPair InputItem { get; }
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
        public SpoilNode(ProductionGraph graph, int nodeID, ItemQualityPair item, IItem? outputItem) : base(graph, nodeID) {
            ArgumentNullException.ThrowIfNull(outputItem);
            if (item.Quality is not IQuality quality)
                throw new ArgumentException("Quality must be populated.", nameof(item));
            InputItem = item;
            OutputItem = new ItemQualityPair(outputItem, quality);
            controller = new SpoilNodeController(this);
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

            return ErrorSet != Errors.Clean ? NodeState.Error : AllLinksConnected ? NodeState.Clean : NodeState.MissingLink;
        }

        public override double GetConsumeRate(ItemQualityPair item) { return ActualRate; }
        public override double GetSupplyRate(ItemQualityPair item) { return ActualRate; }

        internal override double inputRateFor(ItemQualityPair item) { return 1; }
        internal override double outputRateFor(ItemQualityPair item) { return 1; }

        public override List<string> GetErrors() {
            var errors = new List<string>();
            if (InputItem.Item is null && InputItem.Quality is null && OutputItem.Item is null)
                return errors;

            if ((ErrorSet & Errors.InputItemMissing) != 0)
                errors.Add(string.Format(DisplayCulture.Format, "> Item \"{0}\" doesnt exist in preset!", InputItem.Item?.FriendlyName));
            if ((ErrorSet & Errors.OutputItemMissing) != 0)
                errors.Add(string.Format(DisplayCulture.Format, "> Spoilage Item \"{0}\" doesnt exist in preset!", OutputItem.Item?.FriendlyName));
            if ((ErrorSet & Errors.ItemDoesntSpoil) != 0)
                errors.Add(string.Format(DisplayCulture.Format, "> Item \"{0}\" doesnt spoil!", InputItem.Item?.FriendlyName));
            if ((ErrorSet & Errors.InvalidSpoilResult) != 0)
                errors.Add(string.Format(DisplayCulture.Format, "> Spoil result for item \"{0}\" doesnt match preset!", InputItem.Item?.FriendlyName));
            if ((ErrorSet & Errors.QualityMissing) != 0)
                errors.Add(string.Format(DisplayCulture.Format, "> Quality \"{0}\" doesnt exist in preset!", InputItem.Quality?.FriendlyName));
            if ((ErrorSet & Errors.InvalidLinks) != 0)
                errors.Add("> Some links are invalid!");
            return errors;
        }

        public override List<string> GetWarnings() {
            Trace.Fail("Spoil node never has the warning state!");
            return [];
        }

        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "Spoil node for: {0} ({2}) to {1} ({3})", InputItem.Item?.Name, OutputItem.Item?.Name, InputItem.Quality?.Name, OutputItem.Quality?.Name);
    }

    public class SpoilNodeController : BaseNodeController {
        private readonly SpoilNode MyNode;

        internal SpoilNodeController(SpoilNode myNode) : base(myNode) { MyNode = myNode; }

        public void UpdateSpoilResult() {
            if (MyNode.InputItem.Item?.SpoilResult is not IItem spoilResult || MyNode.InputItem.Quality is not IQuality quality)
                return;
            var correctSpoilResult = new ItemQualityPair(spoilResult, quality);
            if (MyNode.OutputItem != correctSpoilResult) {
                foreach (NodeLink link in MyNode.OutputLinks)
                    link.Controller.Delete();
                MyNode.OutputItem = correctSpoilResult;
                MyNode.UpdateState();
            }
        }

        public override Dictionary<string, Action> GetErrorResolutions() =>
            ErrorResolutionsWithFixOrLinks(
                (MyNode.ErrorSet & (SpoilNode.Errors.InputItemMissing | SpoilNode.Errors.OutputItemMissing | SpoilNode.Errors.ItemDoesntSpoil | SpoilNode.Errors.QualityMissing)) != 0,
                (MyNode.ErrorSet & SpoilNode.Errors.InvalidSpoilResult) != 0 ? "Update spoil result" : null,
                (MyNode.ErrorSet & SpoilNode.Errors.InvalidSpoilResult) != 0 ? UpdateSpoilResult : null);

    }
}
