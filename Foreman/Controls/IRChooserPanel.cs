using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
    public partial class IRChooserPanel : UserControl
    {
        public enum PanelType { ShowItems, ShowRecipes};

        public PanelType ChooserPanelType { get; private set; }
        public Action<ProductionNode> CallbackRecipeMethod; //returns the created production node (or null if not created)
        public Action<Item> CallbackItemMethod; //returns the selected item

        private static readonly Color SelectedGroupButtonBGColor = Color.FromArgb(255,255,180,100);

        private NFButton[,] IRButtons = new NFButton[10,8];
        private List<NFButton> GroupButtons = new List<NFButton>();
        private Group SelectedGroup;
        private List<DataObjectBase[]> filteredIRSet = new List<DataObjectBase[]>(); //updated on every filter command & group selection. Represents the full set of items/recipes in the IRFlowPanel (the visible ones will come from this set based on scrolling), with each array being size 10 (#buttons/line)

        private ProductionGraphViewer PGViewer;

        //Item list (shown buttons reflect the various items -> usually this means that after you select the item another panel will show up in order to select a recipe)
        public IRChooserPanel(ProductionGraphViewer parent) : this(parent, null, false, false) { }
        //Recipe list (shown buttons reflect the various ways to use the provided item (or all recipes if item is null)
        public IRChooserPanel(ProductionGraphViewer parent, Item item, bool includeSuppliers, bool includeConsumers)
        {
            InitializeComponent();

            ShowHiddenCheckBox.Checked = Properties.Settings.Default.ShowHidden;

            if (includeConsumers || includeSuppliers)
            {
                IgnoreAssemblerCheckBox.Visible = true;
                IgnoreAssemblerCheckBox.Checked = Properties.Settings.Default.IgnoreAssemblerStatus;
                ItemIconPanel.Visible = (item != null);
                if (includeSuppliers && includeConsumers)
                {
                    AsIngredientCheckBox.Visible = true;
                    AsProductCheckBox.Visible = true;
                }
                ChooserPanelType = PanelType.ShowRecipes;
            }
            else
                ChooserPanelType = PanelType.ShowItems;

            InitializeIRButtons();

            PGViewer = parent;
            parent.Controls.Add(this);
            this.Location = new Point(parent.Width / 2 - Width / 2, parent.Height / 2 - Height / 2);
            this.Anchor = AnchorStyles.None;
            this.BringToFront();
            parent.PerformLayout();
        }

        private void InitializeIRButtons()
        {
            //initialize the group buttons
            List<Group> sortedGroups = DataCache.Groups.Values.ToList();
            sortedGroups.Sort();
            foreach(Group group in sortedGroups)
            {
                NFButton button = new NFButton();
                button.BackgroundImageLayout = ImageLayout.Center;
                button.UseVisualStyleBackColor = false;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.TabStop = false;
                button.Click += new EventHandler(GroupButton_Click);
                button.Margin = new Padding(0);
                button.Size = new Size(64, 64);
                button.Image = group.Icon;
                button.Tag = group;
                GroupButtons.Add(button);
            }
            SelectedGroup = sortedGroups[0]; //just going to assume there is at least 1 group :/
            GroupButtons[0].BackColor = SelectedGroupButtonBGColor;

            GroupFlowPanel.SuspendLayout();
            GroupFlowPanel.Controls.AddRange(GroupButtons.ToArray());
            GroupFlowPanel.ResumeLayout();

            //initialize the item/recipe buttons
            for (int x = 0; x < IRButtons.GetLength(0); x++)
            {
                for(int y = 0; y < IRButtons.GetLength(1); y++)
                {
                    NFButton button = new NFButton();
                    button.BackgroundImageLayout = ImageLayout.Center;
                    button.UseVisualStyleBackColor = false;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 2;
                    button.TabStop = false;
                    button.Click += new EventHandler(IRButton_Click);
                    IRButtons[x, y] = button;
                    SetButton(null, x, y);
                }
            }
            IRFlowPanel.SuspendLayout();
            IRFlowPanel.Controls.AddRange(IRButtons.Cast<Button>().ToArray());
            IRFlowPanel.ResumeLayout();

            UpdateIRSet();
        }

        private void SetButton(DataObjectBase irObject, int x, int y)
        {
            NFButton b = IRButtons[x, y];
            if (irObject != null) //full
            {
                b.Margin = new Padding(0);
                b.Size = new Size(36, 36);
                b.Image = irObject.Icon;
                b.Tag = irObject;
            }
            else
            {
                b.Margin = new Padding(8);
                b.Size = new Size(20, 20);
                b.Image = null;
                b.Tag = null;
            }
        }

        private void UpdateIRSet() //called whenever we need to re-populate the item/recipe list (and update the buttons)
        {
            filteredIRSet.Clear();

        }

        bool exitScriptDone = false;
        private void Exit()
        {
            if (!exitScriptDone)
            {
                exitScriptDone = true;

                Properties.Settings.Default.ShowHidden = ShowHiddenCheckBox.Checked;
                Properties.Settings.Default.IgnoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
                Properties.Settings.Default.Save();
                Dispose();
            }
        }


        private void GroupButton_Click(object sender, EventArgs e)
        {

        }

        private void IRChooserPanel_Leave(object sender, EventArgs e)
        {
            Exit();
        }

        public void IRButton_Click(object sender, EventArgs e)
        {
            Exit();
        }
    }

    public class NFButton : Button
    {
        public NFButton() : base() { this.SetStyle(ControlStyles.Selectable, false); }
        protected override bool ShowFocusCues {  get { return false; } }
    }
}
