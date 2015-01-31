using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Foreman
{
	public partial class ChooserForm : Form
	{
		private List<ChooserControl> controls;

		private ChooserControl selectedControl = null;
		public ChooserControl SelectedControl
		{
			get
			{
				return selectedControl;
			}
			set
			{
				if (selectedControl != null)
				{
					selectedControl.BackColor = Color.White;
				}
				selectedControl = value;
				if (value != null)
				{
					selectedControl.BackColor = Color.FromArgb(0xFF, 0xAE, 0xC6, 0xCF);
				}
			}
		}

		public ChooserForm(IEnumerable<ChooserControl> controls)
		{
			InitializeComponent();

			this.controls = controls.ToList();
		}

		private void RecipeChooserForm_Load(object sender, EventArgs e)
		{
			foreach (ChooserControl control in controls)
			{
				listPanel.Controls.Add(control);
			}

			MaximumSize = new Size(Int32.MaxValue, 500);
		}

		private void RegisterKeyEvents(Control control)
		{
			control.KeyDown += RecipeChooserForm_KeyDown;

			foreach (Control subControl in control.Controls)
			{
				RegisterKeyEvents(subControl);
			}
		}

		private void RecipeChooserForm_MouseMove(object sender, MouseEventArgs e)
		{
			SelectedControl = null;
		}

		private void RecipeChooserForm_MouseLeave(object sender, EventArgs e)
		{
			SelectedControl = null;
		}

		private void RecipeChooserForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				DialogResult = DialogResult.Cancel;
				Close();
			}
		}
	}
}
