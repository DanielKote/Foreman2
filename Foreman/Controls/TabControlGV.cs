using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Foreman.Controls
{
    public partial class TabControlGV : TabControl
    {
        private Image closeX;

        public MainForm ParentForm;

        public TabControlGV()
        {
            InitializeComponent();

            closeX = Properties.Resources.icon_close;

            this.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.TabControlGV_DrawItem);
            this.SelectedIndexChanged += new System.EventHandler(this.TabControlGV_SelectedIndexChanged);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TabControlGV_MouseClick);

        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
        private void TabControlGV_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.SelectedIndex != -1)
            {                
                ParentForm.GraphViewer = ((Controls.TabPageGV)this.SelectedTab).GraphViewer;
            }
        }

        private void TabControlGV_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                Image img = new Bitmap(closeX);
                Rectangle r = e.Bounds;
                r = GetTabRect(e.Index);
                r.Offset(2, 2);
                Brush TitleBrush = new SolidBrush(Color.Black);
                Font f = this.Font;
                string title = TabPages[e.Index].Text;

                e.Graphics.DrawString(title, f, TitleBrush, new PointF(r.X, r.Y));
                e.Graphics.DrawImage(img, new Point(r.X + GetTabRect(e.Index).Width - 16, r.Y));
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

        }
        private void CloseTab(int i)
        {
            this.SelectedIndex = i;

            TabPageGV pg = (TabPageGV)TabPages[i];
            
            if (pg.TestGraphSavedStatus())
            {
               this.TabPages.RemoveAt(i);
            }
            Invalidate();
        }
        private void TabControlGV_MouseClick(object sender, MouseEventArgs e)
        {
            RectangleF tabTextArea;
            Point pt = new Point(e.X, e.Y);

            for (int i = 0; i < this.TabCount; i++)
            {
                tabTextArea = (RectangleF)this.GetTabRect(i);
                tabTextArea = new RectangleF(tabTextArea.Right - 16, tabTextArea.Y, 16, 16);
                if (tabTextArea.Contains(pt))
                {
                    CloseTab(i);
                    return;
                }
            }
        }
        public void TabControlGV_AddTab()
        {
            Controls.TabPageGV tabPage = new Controls.TabPageGV();
            Controls.Add(tabPage);

            ProductionGraphViewer pgv = tabPage.GraphViewer;
            tabPage.Controls.Add(pgv);
            tabPage.Text = "<empty>   ";

            ParentForm.GraphViewer = pgv;

            if (!Enum.IsDefined(typeof(NodeDirection), Properties.Settings.Default.DefaultNodeDirection))
                Properties.Settings.Default.DefaultNodeDirection = (int)NodeDirection.Up;
            pgv.Graph.DefaultNodeDirection = (NodeDirection)Properties.Settings.Default.DefaultNodeDirection;

            pgv.SmartNodeDirection = Properties.Settings.Default.SmartNodeDirection;

            pgv.Graph.EnableExtraProductivityForNonMiners = Properties.Settings.Default.EnableExtraProductivityForNonMiners;
            pgv.NodeCountForSimpleView = Properties.Settings.Default.NodeCountForSimpleView;
            pgv.FlagOUSuppliedNodes = Properties.Settings.Default.FlagOUSuppliedNodes;

            pgv.ArrowRenderer.ShowErrorArrows = Properties.Settings.Default.ShowErrorArrows;
            pgv.ArrowRenderer.ShowWarningArrows = Properties.Settings.Default.ShowWarningArrows;
            pgv.ArrowRenderer.ShowDisconnectedArrows = Properties.Settings.Default.ShowDisconnectedArrows;
            pgv.ArrowRenderer.ShowOUNodeArrows = Properties.Settings.Default.ShowOUSuppliedArrows;
            pgv.Graph.DefaultToSimplePassthroughNodes = Properties.Settings.Default.SimplePassthroughNodes;

            pgv.IconsOnly = Properties.Settings.Default.IconsOnlyView;
            ParentForm.IconViewCheckBox.Checked = pgv.IconsOnly;
            if (Properties.Settings.Default.IconsSize < 8) Properties.Settings.Default.IconsSize = 8;
            if (Properties.Settings.Default.IconsSize > 256) Properties.Settings.Default.IconsSize = 256;
            pgv.IconsSize = Properties.Settings.Default.IconsSize;

            //scrolling keys
            pgv.KeyDownCode = Properties.Settings.Default.KeyDownCode;
            pgv.KeyUpCode = Properties.Settings.Default.KeyUpCode;
            pgv.KeyRightCode = Properties.Settings.Default.KeyRightCode;
            pgv.KeyLeftCode = Properties.Settings.Default.KeyLeftCode;
            pgv.KeyScrollRatio = Properties.Settings.Default.KeyScrollRatio;

            SelectedTab = tabPage;
        }
        public void LoadGraph()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Foreman files (*.fjson)|*.fjson|Old Foreman files (*.json)|*.json";
            if (!Directory.Exists(Path.Combine(Application.StartupPath, "Saved Graphs")))
                Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Saved Graphs"));
            dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Saved Graphs");
            dialog.CheckFileExists = true;
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            LoadGraph(dialog.FileName, dialog.SafeFileName);
        }

        public async void LoadGraph(string path, string name)
        {
            try
            {
                TabControlGV_AddTab();
                await ParentForm.GraphViewer.LoadFromJson(JObject.Parse(File.ReadAllText(path)), false, true);
                ((TabPageGV)SelectedTab).savefilePath = path;
            }
            catch (Exception exception)
            {
                this.TabPages.RemoveAt(this.SelectedIndex);
                MessageBox.Show("Could not load this file. See log for more details");
                ErrorLogging.LogLine(string.Format("Error loading file '{0}'. Error: '{1}'", path, exception.Message));
                ErrorLogging.LogLine(string.Format("Full error output: {0}", exception.ToString()));
                Invalidate();
                return;
            }

            ParentForm.RateOptionsDropDown.SelectedIndex = (int)ParentForm.GraphViewer.Graph.SelectedRateUnit;
            Properties.Settings.Default.EnableExtraProductivityForNonMiners = ParentForm.GraphViewer.Graph.EnableExtraProductivityForNonMiners;
            Properties.Settings.Default.DefaultRateUnit = (int)ParentForm.GraphViewer.Graph.SelectedRateUnit;
            Properties.Settings.Default.DefaultAssemblerOption = (int)ParentForm.GraphViewer.Graph.AssemblerSelector.DefaultSelectionStyle;
            Properties.Settings.Default.DefaultModuleOption = (int)ParentForm.GraphViewer.Graph.ModuleSelector.DefaultSelectionStyle;
            Properties.Settings.Default.DefaultNodeDirection = (int)ParentForm.GraphViewer.Graph.DefaultNodeDirection;

            Properties.Settings.Default.EnableExtraProductivityForNonMiners = ParentForm.GraphViewer.Graph.EnableExtraProductivityForNonMiners;

            Properties.Settings.Default.Save();
            ParentForm.GraphViewer.Invalidate();

            SelectedTab.Text = name + "  ";
            Invalidate();
        }

        public void NewGraph()
        {
            //if (!TestGraphSavedStatus())
            //	return;

            TabControlGV_AddTab();
            ParentForm.GraphViewer.ClearGraph();
            ParentForm.GraphViewer.Graph.LowPriorityPower = 2f;
            ParentForm.GraphViewer.Graph.PullOutputNodes = false;
            ParentForm.GraphViewer.Graph.PullOutputNodesPower = 1f;

            List<Preset> validPresets = MainForm.GetValidPresetsList();
            if (validPresets != null && validPresets.Count > 0)
            {
                Properties.Settings.Default.CurrentPresetName = validPresets[0].Name;
                ParentForm.GraphViewer.LoadPreset(validPresets[0]);                
            }
            else
            {
                Properties.Settings.Default.CurrentPresetName = "No Preset!";
            }

            Properties.Settings.Default.Save();
        }
    }
}
