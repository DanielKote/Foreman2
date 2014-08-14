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
	public partial class ProductionNodeViewer : UserControl
	{
		protected ProductionNode displayedNode;
		public ProductionNode DisplayedNode
		{
			get
			{
				return displayedNode;
			}
			set
			{
				displayedNode = value;
			}
		}

		public ProductionGraphViewer parentTreeViewer;
		public ProductionNodeViewer()
		{
			InitializeComponent();
		}

	}
}