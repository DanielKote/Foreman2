using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Foreman
{
	public partial class ConsumerNodeViewer : ProductionNodeViewer
	{
		public ConsumerNodeViewer()
		{
			InitializeComponent();
		}

		private void RateTextBox_TextChanged(object sender, EventArgs e)
		{
			if (!RateTextBox.ReadOnly)
			{
				Regex numberRegex = new Regex(@"^[-+]?[0-9]*[\.\,]?[0-9]*$");

				if (numberRegex.IsMatch(RateTextBox.Text) || RateTextBox.Text == String.Empty)
				{
					RateTextBox.BackColor = Color.Empty;
				}
				else
				{
					RateTextBox.BackColor = Color.Crimson;
				}

				float parsedRate;
				if (float.TryParse(RateTextBox.Text, out parsedRate))
				{
					(displayedNode as ConsumerNode).ConsumptionRate = parsedRate;
				}
				else if (String.IsNullOrWhiteSpace(RateTextBox.Text))
				{
					(displayedNode as ConsumerNode).ConsumptionRate = 0.0f;
				}

				while (!parentTreeViewer.graph.Complete)
				{
					parentTreeViewer.graph.IterateNodeDemands();
				}
				parentTreeViewer.UpdateNodeControlContents();

			}
		}
	}
}
