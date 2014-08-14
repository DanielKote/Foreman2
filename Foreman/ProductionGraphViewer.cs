using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Foreman
{
	public partial class ProductionGraphViewer : UserControl
	{
		Dictionary<ProductionNode, ProductionNodeViewer> nodeControls = new Dictionary<ProductionNode, ProductionNodeViewer>();
		public ProductionGraph graph { get; set; }
		private Item parentItem;
		public Item ParentItem
		{
			get
			{
				return parentItem;
			}
			set
			{
				parentItem = value;
				if (value != null)
				{
					graph = new ProductionGraph();
					graph.Nodes.Add(new ConsumerNode(parentItem, 1f, graph));
					while (!graph.Complete)
					{
						graph.IterateNodeDemands();
					}

					Controls.Clear();
					nodeControls.Clear();
					CreateMissingNodeControls();
				}
			}
		}

		private void CreateMissingNodeControls()
		{
			foreach (ProductionNode node in graph.Nodes)
			{
				if (!nodeControls.ContainsKey(node))
				{
					if (node is RecipeNode)
					{
						RecipeNodeViewer recipeControl = new RecipeNodeViewer();
						recipeControl.DisplayedNode = node as RecipeNode;
						recipeControl.parentTreeViewer = this;
						Controls.Add(recipeControl);
						nodeControls.Add(node, recipeControl);
					}
					else if (node is SupplyNode)
					{
						SupplyNodeViewer supplyControl = new SupplyNodeViewer();
						supplyControl.DisplayedNode = node as SupplyNode;
						supplyControl.NameBox.Text = (node as SupplyNode).SuppliedItem.Name;
						supplyControl.parentTreeViewer = this;
						Controls.Add(supplyControl);
						nodeControls.Add(node, supplyControl);
					}
					else if (node is ConsumerNode)
					{
						ConsumerNodeViewer consumerControl = new ConsumerNodeViewer();
						consumerControl.DisplayedNode = node as ConsumerNode;
						consumerControl.NameBox.Text = (node as ConsumerNode).ConsumedItem.Name;
						consumerControl.parentTreeViewer = this;
						Controls.Add(consumerControl);
						nodeControls.Add(node, consumerControl);
					}
				}
			}

			PositionControls();
		}

		private void GenerateRecipeControl(RecipeNode node)
		{
		}

		private void DrawConnections()
		{
			Pen pen = new Pen(Color.DarkRed, 3f);
			Graphics graphics = this.CreateGraphics();
			graphics.Clear(this.BackColor);

			foreach (var n in nodeControls.Keys)
			{
				foreach (var m in nodeControls.Keys)
				{
					if (m.CanTakeFrom(n))
					{
						Point pointN = Point.Add(nodeControls[n].Location, new Size(nodeControls[n].Width / 2, 0));
						Point pointM = Point.Add(nodeControls[m].Location, new Size(nodeControls[m].Width / 2, nodeControls[m].Height));
						graphics.DrawLine(pen, pointN, pointM);
					}
				}
			}

			pen.Dispose();
			graphics.Dispose();
		}

		private void PositionControls()
		{
			if (!nodeControls.Any())
			{
				return;
			}
			var nodeOrder = graph.GetTopologicalSort();
			nodeOrder.Reverse();
			var pathMatrix = graph.PathMatrix;

			List<ProductionNode>[] nodePositions = new List<ProductionNode>[nodeOrder.Count()];
			for (int i = 0; i < nodePositions.Count(); i++)
			{
				nodePositions[i] = new List<ProductionNode>();
			}

			foreach (ProductionNode node in nodeOrder)
			{
				bool PositionFound = false;

				for (int i = nodePositions.Count() - 1; i >= 0 && !PositionFound; i--)
				{
					foreach (ProductionNode listNode in nodePositions[i])
					{
						if (listNode.CanUltimatelyTakeFrom(node))
						{
							nodePositions[i + 1].Add(node);
							PositionFound = true;
							break;
						}
					}
				}

				if (!PositionFound)
				{
					nodePositions.First().Add(node);
				}
			}

			int y = 20;
			foreach (var list in nodePositions)
			{
				int maxHeight = 0;
				int x = 20;

				foreach (var node in list)
				{
					if (!nodeControls.ContainsKey(node))
					{
						continue;
					}
					UserControl control = nodeControls[node];
					control.Location = new Point(x, y);

					x += control.Width + 20;
					maxHeight = Math.Max(control.Height, maxHeight);
				}

				y += maxHeight + 20;
			}
		}

		public ProductionGraphViewer()
		{
			InitializeComponent();
		}

		private void RecipeTreeViewer_Load(object sender, EventArgs e)
		{

		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			DrawConnections();
		}

		public void UpdateNodeControlContents()
		{
			foreach (ProductionNode node in nodeControls.Keys)
			{
				ProductionNodeViewer control = nodeControls[node];

				if (node is RecipeNode)
				{
					if ((control as RecipeNodeViewer).RateTextBox.ReadOnly)
					{
						(control as RecipeNodeViewer).NameBox.Text = (node as RecipeNode).BaseRecipe.Name;
						(control as RecipeNodeViewer).RateTextBox.Text = (node as RecipeNode).Rate.ToString();
					}
				}
				else if (node is SupplyNode)
				{
					if ((control as SupplyNodeViewer).RateTextBox.ReadOnly)
					{
						(control as SupplyNodeViewer).NameBox.Text = (node as SupplyNode).SuppliedItem.Name;
						(control as SupplyNodeViewer).RateTextBox.Text = (node as SupplyNode).SupplyRate.ToString();
					}
				}
				else if (node is ConsumerNode)
				{
					if ((control as ConsumerNodeViewer).RateTextBox.ReadOnly)
					{
						(control as ConsumerNodeViewer).NameBox.Text = (node as ConsumerNode).ConsumedItem.Name;
						(control as ConsumerNodeViewer).RateTextBox.Text = (node as ConsumerNode).ConsumptionRate.ToString();
					}
				}
			}
		}

		private void RecipeTreeViewer_Click(object sender, EventArgs e)
		{
			if (graph != null)
			{
				graph.IterateNodeDemands();
				CreateMissingNodeControls();
			}
		}
	}
}