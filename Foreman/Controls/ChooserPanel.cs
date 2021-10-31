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
	public partial class ChooserPanel : UserControl
	{
		public Action<ProductionNode> CallbackMethod; //returns the created production node (or null if not created)
		private Item Item;
		ProductionGraphViewer PGViewer;

		public ChooserPanel(Item item, bool includeSuppliers, bool includeConsumers, ProductionGraphViewer parent)
			: base()
		{
			InitializeComponent();

			parent.Controls.Add(this);
			this.Location = new Point(parent.Width / 2 - Width / 2, parent.Height / 2 - Height / 2);
			this.Anchor = AnchorStyles.None;
			this.BringToFront();
			parent.PerformLayout();

			this.Item = item;
			this.PGViewer = parent;

			//enable buttons
			if (!includeSuppliers)
				SourceButton.Enabled = false;
			if (!includeConsumers)
				ResultButton.Enabled = false;

			//fill in recipes
			if(includeConsumers)
				foreach (Recipe r in item.ConsumptionRecipes)
					if(Properties.Settings.Default.ShowHidden || (!r.Hidden && r.HasEnabledAssemblers))
						RecipeComboBox.Items.Add(r);
			if (includeSuppliers)
				foreach (Recipe r in item.ProductionRecipes)
					if (Properties.Settings.Default.ShowHidden || (!r.Hidden && r.HasEnabledAssemblers))
						RecipeComboBox.Items.Add(r);

			RecipeComboBox.Focus();
		}

		public void Show(Action<ProductionNode> callback)
		{
			CallbackMethod = callback;
		}

		private void ChooserPanel_Load(object sender, EventArgs e)
		{
			Parent.PerformLayout();
		}

		private void ChooserPanel_Leave(object sender, EventArgs e)
		{
			Dispose();
		}

        private void SourceButton_Click(object sender, EventArgs e)
        {
			CallbackMethod(SupplyNode.Create(Item, PGViewer.Graph));
			Dispose();
        }

        private void PassthroughButton_Click(object sender, EventArgs e)
        {
			CallbackMethod(PassthroughNode.Create(Item, PGViewer.Graph));
			Dispose();
		}

		private void ResultButton_Click(object sender, EventArgs e)
        {
			CallbackMethod(ConsumerNode.Create(Item, PGViewer.Graph));
			Dispose();
		}

		private void RecipeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
			Recipe sRecipe = RecipeComboBox.SelectedItem as Recipe;
			CallbackMethod(RecipeNode.Create(sRecipe, PGViewer.Graph));
			Dispose();
		}
    }
}
