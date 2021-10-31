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
	public partial class ItemChooserControl : ChooserControl
	{
		public Item DisplayedItem;

		public ItemChooserControl(Item item, String text, String filterText) : base(text, filterText)
		{
			InitializeComponent();
			this.DoubleBuffered = true;

			DisplayedItem = item;
			TextLabel.Text = text;
		}

		internal override void UpdateIconSize(int iconSize)
		{
			ItemLayoutPanel.ColumnStyles[0].Width = iconSize;
			iconPictureBox.Size = new Size(iconSize, iconSize);
		}

		private void RecipeChooserSupplyNodeOption_Load(object sender, EventArgs e)
		{
			iconPictureBox.Image = DisplayedItem != null ? DisplayedItem.Icon : null;
			iconPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
			MainForm.SetDoubleBuffered(ItemLayoutPanel);

			RegisterMouseEvents(this);
		}
	}
}
