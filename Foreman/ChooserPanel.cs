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
		public Action<ChooserControl> CallbackMethod;

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

		public ChooserPanel(IEnumerable<ChooserControl> controls, ProductionGraphViewer parent)
			: base()
		{
			InitializeComponent();
			this.controls = controls.ToList();

			parent.Controls.Add(this);
			this.Location = new Point(parent.Width / 2 - Width / 2, parent.Height / 2 - Height / 2);
			this.Anchor = AnchorStyles.None;
			parent.PerformLayout();

			FilterTextBox.Focus();
		}

		private void ChooserPanel_Load(object sender, EventArgs e)
		{
			foreach (ChooserControl control in controls)
			{
				flowLayoutPanel1.Controls.Add(control);
				control.ParentPanel = this;
				control.Dock = DockStyle.Top;
				control.Width = this.Width;
				RegisterKeyEvents(control);
			}

			Parent.PerformLayout();

			UpdateControlWidth();
		}

		private void UpdateControlWidth()
		{
			if (flowLayoutPanel1.Controls[flowLayoutPanel1.Controls.Count - 1].Bottom > 600)
			{
				flowLayoutPanel1.Padding = new Padding(Padding.Left, Padding.Top, SystemInformation.VerticalScrollBarWidth, Padding.Bottom);
			}
			else
			{
				flowLayoutPanel1.Padding = new Padding(Padding.Left, Padding.Top, Padding.Left, Padding.Bottom);
			}
		}

		public void Show(Action<ChooserControl> callback)
		{
			CallbackMethod = callback;
		}

		private void RegisterKeyEvents(Control control)
		{
			control.KeyDown += ChooserPanel_KeyDown;

			foreach (Control subControl in control.Controls)
			{
				RegisterKeyEvents(subControl);
			}
		}

		public void ChooserPanel_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Escape:
					CallbackMethod(null);
					Dispose();
					break;
				case Keys.Down:
					SelectedControl = controls[Math.Min(controls.IndexOf(selectedControl) + 1, controls.Count - 1)];
					break;
				case Keys.Up:
					SelectedControl = controls[Math.Max(controls.IndexOf(selectedControl) - 1, 0)];
					break;
				case Keys.Enter:
					CallbackMethod(SelectedControl);
					Dispose();
					break;
				default:
					FilterTextBox.Focus();
					SendKeys.Send(e.KeyCode.ToString());
					break;
			}
		}

		private void ChooserPanel_MouseMove(object sender, MouseEventArgs e)
		{
			SelectedControl = null;
		}

		private void ChooserPanel_MouseLeave(object sender, EventArgs e)
		{
			SelectedControl = null;
		}

		private void FilterTextBox_TextChanged(object sender, EventArgs e)
		{
			SuspendLayout();
			foreach (ChooserControl control in flowLayoutPanel1.Controls)
			{
				if (control.FilterText.ToLower().Contains(FilterTextBox.Text.ToLower()))
				{
					control.Visible = true;
				}
				else
				{
					control.Visible = false;
				}
			}
			ResumeLayout(false);
		}

		private void ChooserPanel_Leave(object sender, EventArgs e)
		{
			Dispose();
		}

		private void FilterTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Enter)
			{
				ChooserPanel_KeyDown(sender, e);
				e.Handled = true;
			}
		}
	}
}
