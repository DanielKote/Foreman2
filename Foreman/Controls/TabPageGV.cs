using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Foreman.Controls
{
    public partial class TabPageGV : TabPage
    {
		public string savefilePath = null;

		public  ProductionGraphViewer GraphViewer { get; }
		public TabPageGV()
		{
			InitializeComponent();
			GraphViewer = new ProductionGraphViewer();

			GraphViewer.AllowDrop = true;
			GraphViewer.ArrowsOnLinks = false;
			GraphViewer.AutoSize = true;
			GraphViewer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			GraphViewer.BackColor = System.Drawing.Color.White;
			GraphViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			GraphViewer.DCache = null;
			GraphViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			GraphViewer.IconsOnly = false;
			GraphViewer.IconsSize = 32;
			GraphViewer.LevelOfDetail = Foreman.ProductionGraphViewer.LOD.Medium;
			GraphViewer.Location = new System.Drawing.Point(3, 3);
			GraphViewer.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			GraphViewer.MouseDownElement = null;
			GraphViewer.NodeCountForSimpleView = 200;
			GraphViewer.ShowRecipeToolTip = false;
			GraphViewer.Size = new System.Drawing.Size(914, 587);
			GraphViewer.SmartNodeDirection = false;
			GraphViewer.TabIndex = 12;
			GraphViewer.TooltipsEnabled = true;

			if (!Enum.IsDefined(typeof(ProductionGraph.RateUnit), Properties.Settings.Default.DefaultRateUnit))
				Properties.Settings.Default.DefaultRateUnit = (int)ProductionGraph.RateUnit.Per1Sec;
			GraphViewer.Graph.SelectedRateUnit = (ProductionGraph.RateUnit)Properties.Settings.Default.DefaultRateUnit;

			if (!Enum.IsDefined(typeof(ModuleSelector.Style), Properties.Settings.Default.DefaultModuleOption))
				Properties.Settings.Default.DefaultModuleOption = (int)ModuleSelector.Style.None;
			GraphViewer.Graph.ModuleSelector.DefaultSelectionStyle = (ModuleSelector.Style)Properties.Settings.Default.DefaultModuleOption;

			if (!Enum.IsDefined(typeof(AssemblerSelector.Style), Properties.Settings.Default.DefaultAssemblerOption))
				Properties.Settings.Default.DefaultAssemblerOption = (int)AssemblerSelector.Style.WorstNonBurner;
			GraphViewer.Graph.AssemblerSelector.DefaultSelectionStyle = (AssemblerSelector.Style)Properties.Settings.Default.DefaultAssemblerOption;

			GraphViewer.ArrowsOnLinks = Properties.Settings.Default.ArrowsOnLinks;
			GraphViewer.DynamicLinkWidth = Properties.Settings.Default.DynamicLineWidth;
			GraphViewer.ShowRecipeToolTip = Properties.Settings.Default.ShowRecipeToolTip;
			GraphViewer.LockedRecipeEditPanelPosition = Properties.Settings.Default.LockedRecipeEditorPosition;

			if (!Enum.IsDefined(typeof(ProductionGraphViewer.LOD), Properties.Settings.Default.LevelOfDetail))
				Properties.Settings.Default.LevelOfDetail = (int)ProductionGraphViewer.LOD.Medium;
			GraphViewer.LevelOfDetail = (ProductionGraphViewer.LOD)Properties.Settings.Default.LevelOfDetail;

		}

		protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

		public bool TestGraphSavedStatus()
		{
			if (GraphViewer == null) return true;

			if (savefilePath == null)
			{
				if (GraphViewer.Graph.Nodes.Any())
					return MessageBox.Show("The current graph hasnt been saved!\nIf you continue, you will loose it forever!", "Are you sure?", MessageBoxButtons.OKCancel) == DialogResult.OK;
				else
					return true;
			}

			if (!File.Exists(savefilePath))
				return MessageBox.Show("The current graph's save file has been deleted!\nIf you continue, you will loose it forever!", "Are you sure?", MessageBoxButtons.OKCancel) == DialogResult.OK;

			StringBuilder stringBuilder = new StringBuilder();
			var writer = new JsonTextWriter(new StringWriter(stringBuilder));

			JsonSerializer serialiser = JsonSerializer.Create();
			serialiser.Formatting = Formatting.Indented;
			GraphViewer.Graph.SerializeNodeIdSet = null; //we want to save everything.
			serialiser.Serialize(writer, GraphViewer);

			if (File.ReadAllText(savefilePath) != stringBuilder.ToString())
			{
				DialogResult result = MessageBox.Show("The current graph has been modified!\nDo you wish to save before continuing?", "Are you sure?", MessageBoxButtons.YesNoCancel);
				if (result == DialogResult.Cancel)
					return false;
				if (result == DialogResult.OK)
					SaveGraph(savefilePath);
			}

			return true;
		}
		public void SaveGraphAs()
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.DefaultExt = ".fjson";
			dialog.Filter = "Foreman files (*.fjson)|*.fjson|All files|*.*";
			if (!Directory.Exists(Path.Combine(Application.StartupPath, "Saved Graphs")))
				Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Saved Graphs"));
			dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Saved Graphs");
			dialog.AddExtension = true;
			dialog.OverwritePrompt = true;
			dialog.FileName = "Flowchart.fjson";
			if (dialog.ShowDialog() != DialogResult.OK)
				return;

			SaveGraph(dialog.FileName);
		}


		public bool SaveGraph(string path)
		{
			var serialiser = JsonSerializer.Create();
			serialiser.Formatting = Formatting.Indented;
			var writer = new JsonTextWriter(new StreamWriter(path));
			try
			{
				GraphViewer.Graph.SerializeNodeIdSet = null; //we want to save everything.
				serialiser.Serialize(writer, GraphViewer);
				savefilePath = path;
				this.Text = string.Format("Foreman 2.0 ({0}) - {1}", Properties.Settings.Default.CurrentPresetName, savefilePath ?? "Untitled");
				return true;
			}
			catch (Exception exception)
			{
				MessageBox.Show("Could not save this file. See log for more details");
				ErrorLogging.LogLine(String.Format("Error saving file '{0}'. Error: '{1}'", path, exception.Message));
				ErrorLogging.LogLine(string.Format("Full error output: {0}", exception.ToString()));
				return false;
			}
			finally
			{
				writer.Close();
			}
		}



	}
}
