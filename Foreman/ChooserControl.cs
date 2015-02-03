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
		public String DisplayText = "";
		public String FilterText = "";

		public ChooserPanel ParentPanel = null;

		public ChooserControl(String text, String filterText)
		{
			this.DisplayText = text;
			this.FilterText = filterText;
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
			ParentPanel.SelectedControl = this;
		}

		private void MouseClicked(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ParentPanel.CallbackMethod.Invoke(this);
				ParentPanel.Dispose();
			}
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// ChooserControl
			// 
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Name = "ChooserControl";
			this.Size = new System.Drawing.Size(0, 0);
			this.ResumeLayout(false);

		}
	}
}
