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

		public ItemChooserControl(Item item, String text) : base(text)
		{
			InitializeComponent();

			DisplayedItem = item;
			TextLabel.Text = text;
		}

		private void RecipeChooserSupplyNodeOption_Load(object sender, EventArgs e)
		{
			iconPictureBox.Image = DisplayedItem.Icon;
			iconPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

			RegisterMouseEvents(this);
		}
	}
}
