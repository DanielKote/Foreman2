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
	public partial class SupplyNodeChooserControl : UserControl
	{
		private Item displayedItem;

		public SupplyNodeChooserControl(Item item)
		{
			InitializeComponent();

			displayedItem = item;
		}

		private void RecipeChooserSupplyNodeOption_Load(object sender, EventArgs e)
		{
			iconPictureBox.Image = displayedItem.Icon;
			iconPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

			RegisterMouseEvents(this);
		}

		private void RegisterMouseEvents(Control control)
		{
			control.MouseMove += MouseMoved;
			control.MouseClick += MouseClicked;

			foreach (Control subControl in control.Controls)
			{
				RegisterMouseEvents(subControl);
			}
		}

		private void MouseMoved(object sender, MouseEventArgs e)
		{
			BackColor = Color.HotPink;
			(FindForm() as RecipeChooserForm).SelectedControl = this;
		}

		private void MouseClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				(FindForm() as RecipeChooserForm).SelectedControl = this;
				(FindForm() as RecipeChooserForm).DialogResult = DialogResult.OK;
				(FindForm() as RecipeChooserForm).Close();
			}
		}
	}
}
