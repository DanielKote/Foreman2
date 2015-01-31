using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Foreman
{
	public class ChooserControl : UserControl
	{
		public String text = "";

		public ChooserControl(String text)
		{
			this.text = text;
		}

		protected void RegisterMouseEvents(Control control)
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
			(FindForm() as ChooserForm).SelectedControl = this;
		}

		private void MouseClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				(FindForm() as ChooserForm).SelectedControl = this;
				(FindForm() as ChooserForm).DialogResult = DialogResult.OK;
				(FindForm() as ChooserForm).Close();
			}
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// ChooserControl
			// 
			this.Name = "ChooserControl";
			this.Size = new System.Drawing.Size(178, 88);
			this.ResumeLayout(false);

		}
	}
}
