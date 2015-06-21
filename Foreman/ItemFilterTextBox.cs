using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Foreman
{
	class ItemFilterTextBox: TextBox
	{
		public ListView List { get; private set; }
		public Button EnterButton { get; private set; }

		public ItemFilterTextBox(ListView list, Button enterButton) : base()
		{
			List = list;
			EnterButton = enterButton;

			KeyDown += new KeyEventHandler(ItemFilterTextBox_KeyDown);
		}

		void ItemFilterTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			int currentSelection;
			if (List.SelectedIndices.Count == 0)
			{
				currentSelection = -1;
			}
			else
			{
				currentSelection = List.SelectedIndices[0];
			}
			if (e.KeyCode == Keys.Down)
			{
				int newSelection = currentSelection + 1;
				if (newSelection >= List.Items.Count) newSelection = List.Items.Count - 1;
				List.SelectedIndices.Clear();
				List.SelectedIndices.Add(newSelection);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Up)
			{
				int newSelection = currentSelection - 1;
				if (newSelection == -1) newSelection = 0;
				List.SelectedIndices.Clear();
				List.SelectedIndices.Add(newSelection);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Enter)
			{
				EnterButton.PerformClick();
			}
		}
	}
}
