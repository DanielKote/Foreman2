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
	public partial class ChooserFilterTextBox : TextBox
	{
		public ChooserPanel ParentPanel { get; private set; }

		public ChooserFilterTextBox(ChooserPanel parent)
		{
			InitializeComponent();

			ParentPanel = parent;
		}

		public void ChooserFilterTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
			{
				ParentPanel.ChooserPanel_KeyDown(sender, e);
				e.Handled = true;
			}
		}
	}
}
