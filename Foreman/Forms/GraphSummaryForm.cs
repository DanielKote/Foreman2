using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public partial class GraphSummaryForm : Form
	{

		public GraphSummaryForm(IEnumerable<ReadOnlyBaseNode> nodes, IEnumerable<ReadOnlyNodeLink> links, string rateString)
		{

			InitializeComponent();
			GS.InitGraphSummary(nodes,links,rateString,null);	
		}

		//-------------------------------------------------------------------------------------------------------Initial list initialization

	}
}
