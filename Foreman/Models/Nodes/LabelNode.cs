using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman
{
    public class LabelNode : BaseNode
    {
        private readonly BaseNodeController controller;
        public override BaseNodeController Controller { get { return controller; } }
        public override IEnumerable<Item> Inputs { get { return new Item[0]; } }
        public override IEnumerable<Item> Outputs { get { return new Item[0]; } }
        public override double GetConsumeRate(Item item) { throw new ArgumentException("Label does not consume! nothing should be asking for the consume rate"); }
        public override double GetSupplyRate(Item item) { throw new ArgumentException("Label does not supply! nothing should be asking for the supply rate"); }
        internal override double inputRateFor(Item item) { throw new ArgumentException("Label should not have outputs!"); }
        internal override double outputRateFor(Item item) { throw new ArgumentException("Label should not have outputs!"); }
		
		public string LabelText;
		public int LabelSize;
		public LabelNode(ProductionGraph graph, int nodeID, string text, int size) : base(graph, nodeID)
		{
			LabelText = text;
			LabelSize = size;
			controller = LabelNodeController.GetController(this);
			ReadOnlyNode = new ReadOnlyLabelNode(this);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("NodeType", NodeType.Label);
			info.AddValue("LabelText", LabelText);
			info.AddValue("LabelSize", LabelSize);
		}
	}
	public class ReadOnlyLabelNode : ReadOnlyBaseNode
	{
		public Item LabelItem => null;

		public readonly LabelNode MyNode;

		public ReadOnlyLabelNode(LabelNode node) : base(node) { MyNode = node; }

		public override List<string> GetErrors()
		{
			List<string> errors = new List<string>();
			// no errors for labels
			return errors;
		}

		public override List<string> GetWarnings() { Trace.Fail("Consumer node never has the warning state!"); return null; }
	}

	public class LabelNodeController : BaseNodeController
	{
		private readonly LabelNode MyNode;

		protected LabelNodeController(LabelNode myNode) : base(myNode) { MyNode = myNode; }

		public static LabelNodeController GetController(LabelNode node)
		{
			if (node.Controller != null)
				return (LabelNodeController)node.Controller;
			return new LabelNodeController(node);
		}

		public override Dictionary<string, Action> GetErrorResolutions()
		{
			Dictionary<string, Action> resolutions = new Dictionary<string, Action>();
			// no resolutions - as there should be no errors
			return resolutions;
		}

		public override Dictionary<string, Action> GetWarningResolutions() { Trace.Fail("Label node never has the warning state!"); return null; }

		public void SetLabelText(string newText)
		{
			MyNode.LabelText = newText;
		}

		public void SetLabelSize(int size)
		{
			MyNode.LabelSize = size;
		}

	}
}
