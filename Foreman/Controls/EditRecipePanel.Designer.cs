using System.Drawing;

namespace Foreman
{
    partial class EditRecipePanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.AutoAssemblersOption = new System.Windows.Forms.RadioButton();
			this.FixedAssemblersOption = new System.Windows.Forms.RadioButton();
			this.FixedAssemblerCountInput = new System.Windows.Forms.TextBox();
			this.AssemblerInfoTable = new System.Windows.Forms.TableLayoutPanel();
			this.AssemblerPollutionLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.AssemblerPollutionPercentLabel = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.AssemblerProductivityPercentLabel = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.AssemblerSpeedPercentLabel = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.AssemblerEnergyPercentLabel = new System.Windows.Forms.Label();
			this.AssemblerEnergyLabel = new System.Windows.Forms.Label();
			this.AssemblerSpeedLabel = new System.Windows.Forms.Label();
			this.AModuleOptionsLabel = new System.Windows.Forms.Label();
			this.AModulesLabel = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.AssemblerTitle = new System.Windows.Forms.Label();
			this.MainTable = new System.Windows.Forms.TableLayoutPanel();
			this.BeaconTable = new System.Windows.Forms.TableLayoutPanel();
			this.BModulesChoicePanel = new System.Windows.Forms.Panel();
			this.BModulesChoiceTable = new System.Windows.Forms.TableLayoutPanel();
			this.SelectedBModulesPanel = new System.Windows.Forms.Panel();
			this.SelectedBModulesTable = new System.Windows.Forms.TableLayoutPanel();
			this.BModulesLabel = new System.Windows.Forms.Label();
			this.BeaconTitle = new System.Windows.Forms.Label();
			this.BModuleOptionsLabel = new System.Windows.Forms.Label();
			this.BeaconChoicePanel = new System.Windows.Forms.Panel();
			this.BeaconChoiceTable = new System.Windows.Forms.TableLayoutPanel();
			this.BeaconInfoTable = new System.Windows.Forms.TableLayoutPanel();
			this.label14 = new System.Windows.Forms.Label();
			this.TotalBeaconsLabel = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.BeaconEfficiencyLabel = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.BeaconModuleCountLabel = new System.Windows.Forms.Label();
			this.label20 = new System.Windows.Forms.Label();
			this.BeaconEnergyLabel = new System.Windows.Forms.Label();
			this.BeaconValuesTable = new System.Windows.Forms.TableLayoutPanel();
			this.ExtraBeaconCountInput = new System.Windows.Forms.TextBox();
			this.BeaconsPerAssemblerCountInput = new System.Windows.Forms.TextBox();
			this.BeaconCountInput = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.AssemblerTable = new System.Windows.Forms.TableLayoutPanel();
			this.FuelOptionsPanel = new System.Windows.Forms.Panel();
			this.FuelOptionsTable = new System.Windows.Forms.TableLayoutPanel();
			this.FuelTitle = new System.Windows.Forms.Label();
			this.AModulesChoicePanel = new System.Windows.Forms.Panel();
			this.AModulesChoiceTable = new System.Windows.Forms.TableLayoutPanel();
			this.AssemblerChoicePanel = new System.Windows.Forms.Panel();
			this.AssemblerChoiceTable = new System.Windows.Forms.TableLayoutPanel();
			this.SelectedAModulesPanel = new System.Windows.Forms.Panel();
			this.SelectedAModulesTable = new System.Windows.Forms.TableLayoutPanel();
			this.RateOptionsTable = new System.Windows.Forms.TableLayoutPanel();
			this.SelectedAssemblerIcon = new System.Windows.Forms.PictureBox();
			this.SelectedFuelIcon = new System.Windows.Forms.PictureBox();
			this.SelectedBeaconIcon = new System.Windows.Forms.PictureBox();
			this.ToolTip = new Foreman.CustomToolTip();
			this.AssemblerInfoTable.SuspendLayout();
			this.MainTable.SuspendLayout();
			this.BeaconTable.SuspendLayout();
			this.BModulesChoicePanel.SuspendLayout();
			this.SelectedBModulesPanel.SuspendLayout();
			this.BeaconChoicePanel.SuspendLayout();
			this.BeaconInfoTable.SuspendLayout();
			this.BeaconValuesTable.SuspendLayout();
			this.AssemblerTable.SuspendLayout();
			this.FuelOptionsPanel.SuspendLayout();
			this.AModulesChoicePanel.SuspendLayout();
			this.AssemblerChoicePanel.SuspendLayout();
			this.SelectedAModulesPanel.SuspendLayout();
			this.RateOptionsTable.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectedAssemblerIcon)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SelectedFuelIcon)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SelectedBeaconIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// AutoAssemblersOption
			// 
			this.AutoAssemblersOption.AutoSize = true;
			this.AutoAssemblersOption.Checked = true;
			this.AutoAssemblersOption.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutoAssemblersOption.Location = new System.Drawing.Point(227, 3);
			this.AutoAssemblersOption.Name = "AutoAssemblersOption";
			this.AutoAssemblersOption.Size = new System.Drawing.Size(47, 20);
			this.AutoAssemblersOption.TabIndex = 0;
			this.AutoAssemblersOption.TabStop = true;
			this.AutoAssemblersOption.Text = "Auto";
			this.AutoAssemblersOption.UseVisualStyleBackColor = true;
			this.AutoAssemblersOption.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			// 
			// FixedAssemblersOption
			// 
			this.FixedAssemblersOption.AutoSize = true;
			this.FixedAssemblersOption.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FixedAssemblersOption.Location = new System.Drawing.Point(280, 3);
			this.FixedAssemblersOption.Name = "FixedAssemblersOption";
			this.FixedAssemblersOption.Size = new System.Drawing.Size(50, 20);
			this.FixedAssemblersOption.TabIndex = 1;
			this.FixedAssemblersOption.Text = "Fixed";
			this.FixedAssemblersOption.UseVisualStyleBackColor = true;
			this.FixedAssemblersOption.CheckedChanged += new System.EventHandler(this.FixedAssemblerOption_CheckedChanged);
			this.FixedAssemblersOption.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			// 
			// FixedAssemblerCountInput
			// 
			this.FixedAssemblerCountInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FixedAssemblerCountInput.Location = new System.Drawing.Point(336, 3);
			this.FixedAssemblerCountInput.Name = "FixedAssemblerCountInput";
			this.FixedAssemblerCountInput.Size = new System.Drawing.Size(94, 20);
			this.FixedAssemblerCountInput.TabIndex = 2;
			this.FixedAssemblerCountInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			this.FixedAssemblerCountInput.LostFocus += new System.EventHandler(this.FixedAssemblerCountInput_LostFocus);
			// 
			// AssemblerInfoTable
			// 
			this.AssemblerInfoTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AssemblerInfoTable.ColumnCount = 3;
			this.AssemblerTable.SetColumnSpan(this.AssemblerInfoTable, 2);
			this.AssemblerInfoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
			this.AssemblerInfoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.AssemblerInfoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 178F));
			this.AssemblerInfoTable.Controls.Add(this.AssemblerPollutionLabel, 2, 3);
			this.AssemblerInfoTable.Controls.Add(this.label2, 0, 0);
			this.AssemblerInfoTable.Controls.Add(this.AssemblerPollutionPercentLabel, 1, 3);
			this.AssemblerInfoTable.Controls.Add(this.label4, 0, 1);
			this.AssemblerInfoTable.Controls.Add(this.AssemblerProductivityPercentLabel, 1, 2);
			this.AssemblerInfoTable.Controls.Add(this.label5, 0, 2);
			this.AssemblerInfoTable.Controls.Add(this.AssemblerSpeedPercentLabel, 1, 1);
			this.AssemblerInfoTable.Controls.Add(this.label6, 0, 3);
			this.AssemblerInfoTable.Controls.Add(this.AssemblerEnergyPercentLabel, 1, 0);
			this.AssemblerInfoTable.Controls.Add(this.AssemblerEnergyLabel, 2, 0);
			this.AssemblerInfoTable.Controls.Add(this.AssemblerSpeedLabel, 2, 1);
			this.AssemblerInfoTable.Dock = System.Windows.Forms.DockStyle.Top;
			this.AssemblerInfoTable.Location = new System.Drawing.Point(154, 39);
			this.AssemblerInfoTable.Margin = new System.Windows.Forms.Padding(2, 7, 2, 2);
			this.AssemblerInfoTable.Name = "AssemblerInfoTable";
			this.AssemblerInfoTable.RowCount = 4;
			this.AssemblerInfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.AssemblerInfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.AssemblerInfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.AssemblerInfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.AssemblerInfoTable.Size = new System.Drawing.Size(277, 60);
			this.AssemblerInfoTable.TabIndex = 9;
			// 
			// AssemblerPollutionLabel
			// 
			this.AssemblerPollutionLabel.AutoSize = true;
			this.AssemblerPollutionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerPollutionLabel.Location = new System.Drawing.Point(126, 46);
			this.AssemblerPollutionLabel.Margin = new System.Windows.Forms.Padding(1);
			this.AssemblerPollutionLabel.Name = "AssemblerPollutionLabel";
			this.AssemblerPollutionLabel.Size = new System.Drawing.Size(176, 13);
			this.AssemblerPollutionLabel.TabIndex = 12;
			this.AssemblerPollutionLabel.Text = "12/min";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(1, 1);
			this.label2.Margin = new System.Windows.Forms.Padding(1);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(73, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Energy:";
			// 
			// AssemblerPollutionPercentLabel
			// 
			this.AssemblerPollutionPercentLabel.AutoSize = true;
			this.AssemblerPollutionPercentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerPollutionPercentLabel.Location = new System.Drawing.Point(76, 46);
			this.AssemblerPollutionPercentLabel.Margin = new System.Windows.Forms.Padding(1);
			this.AssemblerPollutionPercentLabel.Name = "AssemblerPollutionPercentLabel";
			this.AssemblerPollutionPercentLabel.Size = new System.Drawing.Size(48, 13);
			this.AssemblerPollutionPercentLabel.TabIndex = 8;
			this.AssemblerPollutionPercentLabel.Text = "2000%";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(1, 16);
			this.label4.Margin = new System.Windows.Forms.Padding(1);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(73, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Speed:";
			// 
			// AssemblerProductivityPercentLabel
			// 
			this.AssemblerProductivityPercentLabel.AutoSize = true;
			this.AssemblerProductivityPercentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerProductivityPercentLabel.Location = new System.Drawing.Point(76, 31);
			this.AssemblerProductivityPercentLabel.Margin = new System.Windows.Forms.Padding(1);
			this.AssemblerProductivityPercentLabel.Name = "AssemblerProductivityPercentLabel";
			this.AssemblerProductivityPercentLabel.Size = new System.Drawing.Size(48, 13);
			this.AssemblerProductivityPercentLabel.TabIndex = 7;
			this.AssemblerProductivityPercentLabel.Text = "100%";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Location = new System.Drawing.Point(1, 31);
			this.label5.Margin = new System.Windows.Forms.Padding(1);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(73, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Productivity:";
			// 
			// AssemblerSpeedPercentLabel
			// 
			this.AssemblerSpeedPercentLabel.AutoSize = true;
			this.AssemblerSpeedPercentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerSpeedPercentLabel.Location = new System.Drawing.Point(76, 16);
			this.AssemblerSpeedPercentLabel.Margin = new System.Windows.Forms.Padding(1);
			this.AssemblerSpeedPercentLabel.Name = "AssemblerSpeedPercentLabel";
			this.AssemblerSpeedPercentLabel.Size = new System.Drawing.Size(48, 13);
			this.AssemblerSpeedPercentLabel.TabIndex = 6;
			this.AssemblerSpeedPercentLabel.Text = "-80%";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label6.Location = new System.Drawing.Point(1, 46);
			this.label6.Margin = new System.Windows.Forms.Padding(1);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(73, 13);
			this.label6.TabIndex = 4;
			this.label6.Text = "Pollution:";
			// 
			// AssemblerEnergyPercentLabel
			// 
			this.AssemblerEnergyPercentLabel.AutoSize = true;
			this.AssemblerEnergyPercentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerEnergyPercentLabel.Location = new System.Drawing.Point(76, 1);
			this.AssemblerEnergyPercentLabel.Margin = new System.Windows.Forms.Padding(1);
			this.AssemblerEnergyPercentLabel.Name = "AssemblerEnergyPercentLabel";
			this.AssemblerEnergyPercentLabel.Size = new System.Drawing.Size(48, 13);
			this.AssemblerEnergyPercentLabel.TabIndex = 5;
			this.AssemblerEnergyPercentLabel.Text = "10000%";
			// 
			// AssemblerEnergyLabel
			// 
			this.AssemblerEnergyLabel.AutoSize = true;
			this.AssemblerEnergyLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerEnergyLabel.Location = new System.Drawing.Point(126, 1);
			this.AssemblerEnergyLabel.Margin = new System.Windows.Forms.Padding(1);
			this.AssemblerEnergyLabel.Name = "AssemblerEnergyLabel";
			this.AssemblerEnergyLabel.Size = new System.Drawing.Size(176, 13);
			this.AssemblerEnergyLabel.TabIndex = 9;
			this.AssemblerEnergyLabel.Text = "5MJ";
			// 
			// AssemblerSpeedLabel
			// 
			this.AssemblerSpeedLabel.AutoSize = true;
			this.AssemblerSpeedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerSpeedLabel.Location = new System.Drawing.Point(126, 16);
			this.AssemblerSpeedLabel.Margin = new System.Windows.Forms.Padding(1);
			this.AssemblerSpeedLabel.Name = "AssemblerSpeedLabel";
			this.AssemblerSpeedLabel.Size = new System.Drawing.Size(176, 13);
			this.AssemblerSpeedLabel.TabIndex = 10;
			this.AssemblerSpeedLabel.Text = "128.5";
			// 
			// AModuleOptionsLabel
			// 
			this.AModuleOptionsLabel.AutoSize = true;
			this.AModuleOptionsLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
			this.AssemblerTable.SetColumnSpan(this.AModuleOptionsLabel, 2);
			this.AModuleOptionsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AModuleOptionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.AModuleOptionsLabel.Location = new System.Drawing.Point(152, 190);
			this.AModuleOptionsLabel.Margin = new System.Windows.Forms.Padding(0);
			this.AModuleOptionsLabel.Name = "AModuleOptionsLabel";
			this.AModuleOptionsLabel.Padding = new System.Windows.Forms.Padding(3);
			this.AModuleOptionsLabel.Size = new System.Drawing.Size(281, 23);
			this.AModuleOptionsLabel.TabIndex = 13;
			this.AModuleOptionsLabel.Text = "Module Options:";
			// 
			// AModulesLabel
			// 
			this.AModulesLabel.AutoSize = true;
			this.AModulesLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
			this.AModulesLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AModulesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.AModulesLabel.Location = new System.Drawing.Point(0, 190);
			this.AModulesLabel.Margin = new System.Windows.Forms.Padding(0);
			this.AModulesLabel.Name = "AModulesLabel";
			this.AModulesLabel.Padding = new System.Windows.Forms.Padding(3);
			this.AModulesLabel.Size = new System.Drawing.Size(152, 23);
			this.AModulesLabel.TabIndex = 12;
			this.AModulesLabel.Text = "Modules:";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
			this.label7.Location = new System.Drawing.Point(3, 1);
			this.label7.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(218, 22);
			this.label7.TabIndex = 3;
			this.label7.Text = "# of Assemblers:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// AssemblerTitle
			// 
			this.AssemblerTitle.AutoSize = true;
			this.AssemblerTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
			this.AssemblerTable.SetColumnSpan(this.AssemblerTitle, 2);
			this.AssemblerTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
			this.AssemblerTitle.Location = new System.Drawing.Point(0, 0);
			this.AssemblerTitle.Margin = new System.Windows.Forms.Padding(0);
			this.AssemblerTitle.Name = "AssemblerTitle";
			this.AssemblerTitle.Padding = new System.Windows.Forms.Padding(3, 6, 3, 9);
			this.AssemblerTitle.Size = new System.Drawing.Size(401, 32);
			this.AssemblerTitle.TabIndex = 0;
			this.AssemblerTitle.Text = "Assembler: (Space factory #2)";
			// 
			// MainTable
			// 
			this.MainTable.AutoSize = true;
			this.MainTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainTable.ColumnCount = 1;
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MainTable.Controls.Add(this.BeaconTable, 0, 2);
			this.MainTable.Controls.Add(this.AssemblerTable, 0, 1);
			this.MainTable.Controls.Add(this.RateOptionsTable, 0, 0);
			this.MainTable.Location = new System.Drawing.Point(0, 0);
			this.MainTable.Margin = new System.Windows.Forms.Padding(0);
			this.MainTable.Name = "MainTable";
			this.MainTable.RowCount = 3;
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.Size = new System.Drawing.Size(439, 552);
			this.MainTable.TabIndex = 17;
			// 
			// BeaconTable
			// 
			this.BeaconTable.AutoSize = true;
			this.BeaconTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BeaconTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(65)))), ((int)(((byte)(65)))));
			this.BeaconTable.ColumnCount = 4;
			this.BeaconTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.BeaconTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.BeaconTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.BeaconTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.BeaconTable.Controls.Add(this.BModulesChoicePanel, 1, 3);
			this.BeaconTable.Controls.Add(this.SelectedBModulesPanel, 0, 3);
			this.BeaconTable.Controls.Add(this.BModulesLabel, 0, 2);
			this.BeaconTable.Controls.Add(this.BeaconTitle, 0, 0);
			this.BeaconTable.Controls.Add(this.BModuleOptionsLabel, 1, 2);
			this.BeaconTable.Controls.Add(this.BeaconChoicePanel, 0, 1);
			this.BeaconTable.Controls.Add(this.BeaconInfoTable, 1, 1);
			this.BeaconTable.Controls.Add(this.BeaconValuesTable, 2, 1);
			this.BeaconTable.Controls.Add(this.SelectedBeaconIcon, 3, 0);
			this.BeaconTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BeaconTable.Location = new System.Drawing.Point(3, 332);
			this.BeaconTable.Name = "BeaconTable";
			this.BeaconTable.RowCount = 4;
			this.BeaconTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.BeaconTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.BeaconTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.BeaconTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.BeaconTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.BeaconTable.Size = new System.Drawing.Size(433, 217);
			this.BeaconTable.TabIndex = 21;
			// 
			// BModulesChoicePanel
			// 
			this.BModulesChoicePanel.AutoScroll = true;
			this.BModulesChoicePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.BeaconTable.SetColumnSpan(this.BModulesChoicePanel, 3);
			this.BModulesChoicePanel.Controls.Add(this.BModulesChoiceTable);
			this.BModulesChoicePanel.Location = new System.Drawing.Point(155, 142);
			this.BModulesChoicePanel.Name = "BModulesChoicePanel";
			this.BModulesChoicePanel.Size = new System.Drawing.Size(272, 72);
			this.BModulesChoicePanel.TabIndex = 20;
			// 
			// BModulesChoiceTable
			// 
			this.BModulesChoiceTable.AutoSize = true;
			this.BModulesChoiceTable.ColumnCount = 9;
			this.BModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.BModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.BModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.BModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.BModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.BModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.BModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.BModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.BModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.BModulesChoiceTable.Dock = System.Windows.Forms.DockStyle.Top;
			this.BModulesChoiceTable.Location = new System.Drawing.Point(0, 0);
			this.BModulesChoiceTable.Name = "BModulesChoiceTable";
			this.BModulesChoiceTable.RowCount = 1;
			this.BModulesChoiceTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.BModulesChoiceTable.Size = new System.Drawing.Size(268, 32);
			this.BModulesChoiceTable.TabIndex = 0;
			// 
			// SelectedBModulesPanel
			// 
			this.SelectedBModulesPanel.AutoScroll = true;
			this.SelectedBModulesPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.SelectedBModulesPanel.Controls.Add(this.SelectedBModulesTable);
			this.SelectedBModulesPanel.Location = new System.Drawing.Point(3, 142);
			this.SelectedBModulesPanel.Margin = new System.Windows.Forms.Padding(3, 3, 13, 3);
			this.SelectedBModulesPanel.Name = "SelectedBModulesPanel";
			this.SelectedBModulesPanel.Size = new System.Drawing.Size(136, 72);
			this.SelectedBModulesPanel.TabIndex = 19;
			// 
			// SelectedBModulesTable
			// 
			this.SelectedBModulesTable.AutoSize = true;
			this.SelectedBModulesTable.ColumnCount = 5;
			this.SelectedBModulesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.SelectedBModulesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.SelectedBModulesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.SelectedBModulesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.SelectedBModulesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.SelectedBModulesTable.Dock = System.Windows.Forms.DockStyle.Top;
			this.SelectedBModulesTable.Location = new System.Drawing.Point(0, 0);
			this.SelectedBModulesTable.Name = "SelectedBModulesTable";
			this.SelectedBModulesTable.RowCount = 1;
			this.SelectedBModulesTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.SelectedBModulesTable.Size = new System.Drawing.Size(132, 32);
			this.SelectedBModulesTable.TabIndex = 0;
			// 
			// BModulesLabel
			// 
			this.BModulesLabel.AutoSize = true;
			this.BModulesLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
			this.BModulesLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BModulesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.BModulesLabel.Location = new System.Drawing.Point(0, 116);
			this.BModulesLabel.Margin = new System.Windows.Forms.Padding(0);
			this.BModulesLabel.Name = "BModulesLabel";
			this.BModulesLabel.Padding = new System.Windows.Forms.Padding(3);
			this.BModulesLabel.Size = new System.Drawing.Size(152, 23);
			this.BModulesLabel.TabIndex = 12;
			this.BModulesLabel.Text = "Modules:";
			// 
			// BeaconTitle
			// 
			this.BeaconTitle.AutoSize = true;
			this.BeaconTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
			this.BeaconTable.SetColumnSpan(this.BeaconTitle, 3);
			this.BeaconTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BeaconTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
			this.BeaconTitle.Location = new System.Drawing.Point(0, 0);
			this.BeaconTitle.Margin = new System.Windows.Forms.Padding(0);
			this.BeaconTitle.Name = "BeaconTitle";
			this.BeaconTitle.Padding = new System.Windows.Forms.Padding(3, 6, 3, 9);
			this.BeaconTitle.Size = new System.Drawing.Size(401, 32);
			this.BeaconTitle.TabIndex = 0;
			this.BeaconTitle.Text = "Beacon: (Beacon 2)";
			// 
			// BModuleOptionsLabel
			// 
			this.BModuleOptionsLabel.AutoSize = true;
			this.BModuleOptionsLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
			this.BeaconTable.SetColumnSpan(this.BModuleOptionsLabel, 3);
			this.BModuleOptionsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BModuleOptionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.BModuleOptionsLabel.Location = new System.Drawing.Point(152, 116);
			this.BModuleOptionsLabel.Margin = new System.Windows.Forms.Padding(0);
			this.BModuleOptionsLabel.Name = "BModuleOptionsLabel";
			this.BModuleOptionsLabel.Padding = new System.Windows.Forms.Padding(3);
			this.BModuleOptionsLabel.Size = new System.Drawing.Size(281, 23);
			this.BModuleOptionsLabel.TabIndex = 13;
			this.BModuleOptionsLabel.Text = "Module Options:";
			// 
			// BeaconChoicePanel
			// 
			this.BeaconChoicePanel.AutoScroll = true;
			this.BeaconChoicePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.BeaconChoicePanel.Controls.Add(this.BeaconChoiceTable);
			this.BeaconChoicePanel.Location = new System.Drawing.Point(3, 35);
			this.BeaconChoicePanel.Margin = new System.Windows.Forms.Padding(3, 3, 13, 3);
			this.BeaconChoicePanel.Name = "BeaconChoicePanel";
			this.BeaconChoicePanel.Size = new System.Drawing.Size(136, 72);
			this.BeaconChoicePanel.TabIndex = 18;
			// 
			// BeaconChoiceTable
			// 
			this.BeaconChoiceTable.AutoSize = true;
			this.BeaconChoiceTable.ColumnCount = 5;
			this.BeaconChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.BeaconChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.BeaconChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.BeaconChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.BeaconChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.BeaconChoiceTable.Dock = System.Windows.Forms.DockStyle.Top;
			this.BeaconChoiceTable.Location = new System.Drawing.Point(0, 0);
			this.BeaconChoiceTable.Name = "BeaconChoiceTable";
			this.BeaconChoiceTable.RowCount = 1;
			this.BeaconChoiceTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.BeaconChoiceTable.Size = new System.Drawing.Size(132, 32);
			this.BeaconChoiceTable.TabIndex = 0;
			// 
			// BeaconInfoTable
			// 
			this.BeaconInfoTable.AutoSize = true;
			this.BeaconInfoTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BeaconInfoTable.ColumnCount = 2;
			this.BeaconInfoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 68F));
			this.BeaconInfoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52F));
			this.BeaconInfoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.BeaconInfoTable.Controls.Add(this.label14, 0, 0);
			this.BeaconInfoTable.Controls.Add(this.TotalBeaconsLabel, 1, 3);
			this.BeaconInfoTable.Controls.Add(this.label16, 0, 1);
			this.BeaconInfoTable.Controls.Add(this.BeaconEfficiencyLabel, 1, 2);
			this.BeaconInfoTable.Controls.Add(this.label18, 0, 2);
			this.BeaconInfoTable.Controls.Add(this.BeaconModuleCountLabel, 1, 1);
			this.BeaconInfoTable.Controls.Add(this.label20, 0, 3);
			this.BeaconInfoTable.Controls.Add(this.BeaconEnergyLabel, 1, 0);
			this.BeaconInfoTable.Location = new System.Drawing.Point(154, 34);
			this.BeaconInfoTable.Margin = new System.Windows.Forms.Padding(2);
			this.BeaconInfoTable.Name = "BeaconInfoTable";
			this.BeaconInfoTable.RowCount = 4;
			this.BeaconInfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.BeaconInfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.BeaconInfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.BeaconInfoTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.BeaconInfoTable.Size = new System.Drawing.Size(120, 60);
			this.BeaconInfoTable.TabIndex = 9;
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label14.Location = new System.Drawing.Point(1, 1);
			this.label14.Margin = new System.Windows.Forms.Padding(1);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(66, 13);
			this.label14.TabIndex = 1;
			this.label14.Text = "Energy:";
			// 
			// TotalBeaconsLabel
			// 
			this.TotalBeaconsLabel.AutoSize = true;
			this.TotalBeaconsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TotalBeaconsLabel.Location = new System.Drawing.Point(69, 46);
			this.TotalBeaconsLabel.Margin = new System.Windows.Forms.Padding(1);
			this.TotalBeaconsLabel.Name = "TotalBeaconsLabel";
			this.TotalBeaconsLabel.Size = new System.Drawing.Size(50, 13);
			this.TotalBeaconsLabel.TabIndex = 8;
			this.TotalBeaconsLabel.Text = "250";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label16.Location = new System.Drawing.Point(1, 16);
			this.label16.Margin = new System.Windows.Forms.Padding(1);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(66, 13);
			this.label16.TabIndex = 2;
			this.label16.Text = "Modules:";
			// 
			// BeaconEfficiencyLabel
			// 
			this.BeaconEfficiencyLabel.AutoSize = true;
			this.BeaconEfficiencyLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BeaconEfficiencyLabel.Location = new System.Drawing.Point(69, 31);
			this.BeaconEfficiencyLabel.Margin = new System.Windows.Forms.Padding(1);
			this.BeaconEfficiencyLabel.Name = "BeaconEfficiencyLabel";
			this.BeaconEfficiencyLabel.Size = new System.Drawing.Size(50, 13);
			this.BeaconEfficiencyLabel.TabIndex = 7;
			this.BeaconEfficiencyLabel.Text = "50%";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label18.Location = new System.Drawing.Point(1, 31);
			this.label18.Margin = new System.Windows.Forms.Padding(1);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(66, 13);
			this.label18.TabIndex = 3;
			this.label18.Text = "Efficiency:";
			// 
			// BeaconModuleCountLabel
			// 
			this.BeaconModuleCountLabel.AutoSize = true;
			this.BeaconModuleCountLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BeaconModuleCountLabel.Location = new System.Drawing.Point(69, 16);
			this.BeaconModuleCountLabel.Margin = new System.Windows.Forms.Padding(1);
			this.BeaconModuleCountLabel.Name = "BeaconModuleCountLabel";
			this.BeaconModuleCountLabel.Size = new System.Drawing.Size(50, 13);
			this.BeaconModuleCountLabel.TabIndex = 6;
			this.BeaconModuleCountLabel.Text = "4";
			// 
			// label20
			// 
			this.label20.AutoSize = true;
			this.label20.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label20.Location = new System.Drawing.Point(1, 46);
			this.label20.Margin = new System.Windows.Forms.Padding(1);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(66, 13);
			this.label20.TabIndex = 4;
			this.label20.Text = "#Beacons:";
			// 
			// BeaconEnergyLabel
			// 
			this.BeaconEnergyLabel.AutoSize = true;
			this.BeaconEnergyLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BeaconEnergyLabel.Location = new System.Drawing.Point(69, 1);
			this.BeaconEnergyLabel.Margin = new System.Windows.Forms.Padding(1);
			this.BeaconEnergyLabel.Name = "BeaconEnergyLabel";
			this.BeaconEnergyLabel.Size = new System.Drawing.Size(50, 13);
			this.BeaconEnergyLabel.TabIndex = 5;
			this.BeaconEnergyLabel.Text = "10MJ";
			// 
			// BeaconValuesTable
			// 
			this.BeaconValuesTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BeaconValuesTable.ColumnCount = 2;
			this.BeaconTable.SetColumnSpan(this.BeaconValuesTable, 2);
			this.BeaconValuesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.BeaconValuesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.BeaconValuesTable.Controls.Add(this.ExtraBeaconCountInput, 1, 2);
			this.BeaconValuesTable.Controls.Add(this.BeaconsPerAssemblerCountInput, 1, 1);
			this.BeaconValuesTable.Controls.Add(this.BeaconCountInput, 1, 0);
			this.BeaconValuesTable.Controls.Add(this.label8, 0, 0);
			this.BeaconValuesTable.Controls.Add(this.label9, 0, 1);
			this.BeaconValuesTable.Controls.Add(this.label10, 0, 2);
			this.BeaconValuesTable.Dock = System.Windows.Forms.DockStyle.Left;
			this.BeaconValuesTable.Location = new System.Drawing.Point(286, 35);
			this.BeaconValuesTable.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.BeaconValuesTable.Name = "BeaconValuesTable";
			this.BeaconValuesTable.RowCount = 3;
			this.BeaconValuesTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33332F));
			this.BeaconValuesTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.BeaconValuesTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
			this.BeaconValuesTable.Size = new System.Drawing.Size(144, 78);
			this.BeaconValuesTable.TabIndex = 21;
			// 
			// ExtraBeaconCountInput
			// 
			this.ExtraBeaconCountInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExtraBeaconCountInput.Location = new System.Drawing.Point(70, 54);
			this.ExtraBeaconCountInput.Name = "ExtraBeaconCountInput";
			this.ExtraBeaconCountInput.Size = new System.Drawing.Size(71, 20);
			this.ExtraBeaconCountInput.TabIndex = 5;
			// 
			// BeaconsPerAssemblerCountInput
			// 
			this.BeaconsPerAssemblerCountInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BeaconsPerAssemblerCountInput.Location = new System.Drawing.Point(70, 28);
			this.BeaconsPerAssemblerCountInput.Name = "BeaconsPerAssemblerCountInput";
			this.BeaconsPerAssemblerCountInput.Size = new System.Drawing.Size(71, 20);
			this.BeaconsPerAssemblerCountInput.TabIndex = 4;
			// 
			// BeaconCountInput
			// 
			this.BeaconCountInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BeaconCountInput.Location = new System.Drawing.Point(70, 3);
			this.BeaconCountInput.Name = "BeaconCountInput";
			this.BeaconCountInput.Size = new System.Drawing.Size(71, 20);
			this.BeaconCountInput.TabIndex = 3;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.Location = new System.Drawing.Point(2, 2);
			this.label8.Margin = new System.Windows.Forms.Padding(2);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(63, 21);
			this.label8.TabIndex = 0;
			this.label8.Text = "# Beacons:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label9.Location = new System.Drawing.Point(2, 27);
			this.label9.Margin = new System.Windows.Forms.Padding(2);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(63, 22);
			this.label9.TabIndex = 1;
			this.label9.Text = "/Assembler:";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label10.Location = new System.Drawing.Point(2, 53);
			this.label10.Margin = new System.Windows.Forms.Padding(2);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(63, 23);
			this.label10.TabIndex = 2;
			this.label10.Text = "Additional:";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// AssemblerTable
			// 
			this.AssemblerTable.AutoSize = true;
			this.AssemblerTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AssemblerTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(65)))), ((int)(((byte)(65)))));
			this.AssemblerTable.ColumnCount = 3;
			this.AssemblerTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.AssemblerTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.AssemblerTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.AssemblerTable.Controls.Add(this.FuelOptionsPanel, 0, 3);
			this.AssemblerTable.Controls.Add(this.FuelTitle, 0, 2);
			this.AssemblerTable.Controls.Add(this.AModulesChoicePanel, 1, 5);
			this.AssemblerTable.Controls.Add(this.AssemblerTitle, 0, 0);
			this.AssemblerTable.Controls.Add(this.AssemblerChoicePanel, 0, 1);
			this.AssemblerTable.Controls.Add(this.AssemblerInfoTable, 1, 1);
			this.AssemblerTable.Controls.Add(this.SelectedAModulesPanel, 0, 5);
			this.AssemblerTable.Controls.Add(this.AModulesLabel, 0, 4);
			this.AssemblerTable.Controls.Add(this.AModuleOptionsLabel, 1, 4);
			this.AssemblerTable.Controls.Add(this.SelectedAssemblerIcon, 2, 0);
			this.AssemblerTable.Controls.Add(this.SelectedFuelIcon, 2, 2);
			this.AssemblerTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerTable.Location = new System.Drawing.Point(3, 35);
			this.AssemblerTable.Name = "AssemblerTable";
			this.AssemblerTable.RowCount = 6;
			this.AssemblerTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.AssemblerTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.AssemblerTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.AssemblerTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.AssemblerTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.AssemblerTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.AssemblerTable.Size = new System.Drawing.Size(433, 291);
			this.AssemblerTable.TabIndex = 20;
			// 
			// FuelOptionsPanel
			// 
			this.FuelOptionsPanel.AutoScroll = true;
			this.FuelOptionsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.AssemblerTable.SetColumnSpan(this.FuelOptionsPanel, 3);
			this.FuelOptionsPanel.Controls.Add(this.FuelOptionsTable);
			this.FuelOptionsPanel.Location = new System.Drawing.Point(3, 149);
			this.FuelOptionsPanel.Name = "FuelOptionsPanel";
			this.FuelOptionsPanel.Size = new System.Drawing.Size(424, 38);
			this.FuelOptionsPanel.TabIndex = 22;
			// 
			// FuelOptionsTable
			// 
			this.FuelOptionsTable.AutoSize = true;
			this.FuelOptionsTable.ColumnCount = 13;
			this.FuelOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333332F));
			this.FuelOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333332F));
			this.FuelOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333332F));
			this.FuelOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333332F));
			this.FuelOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333332F));
			this.FuelOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333332F));
			this.FuelOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333332F));
			this.FuelOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333332F));
			this.FuelOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333332F));
			this.FuelOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333332F));
			this.FuelOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333332F));
			this.FuelOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333332F));
			this.FuelOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.FuelOptionsTable.Dock = System.Windows.Forms.DockStyle.Top;
			this.FuelOptionsTable.Location = new System.Drawing.Point(0, 0);
			this.FuelOptionsTable.Name = "FuelOptionsTable";
			this.FuelOptionsTable.RowCount = 1;
			this.FuelOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.FuelOptionsTable.Size = new System.Drawing.Size(420, 32);
			this.FuelOptionsTable.TabIndex = 0;
			// 
			// FuelTitle
			// 
			this.FuelTitle.AutoSize = true;
			this.FuelTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
			this.AssemblerTable.SetColumnSpan(this.FuelTitle, 2);
			this.FuelTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FuelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
			this.FuelTitle.Location = new System.Drawing.Point(0, 114);
			this.FuelTitle.Margin = new System.Windows.Forms.Padding(0);
			this.FuelTitle.Name = "FuelTitle";
			this.FuelTitle.Padding = new System.Windows.Forms.Padding(3, 6, 3, 9);
			this.FuelTitle.Size = new System.Drawing.Size(401, 32);
			this.FuelTitle.TabIndex = 21;
			this.FuelTitle.Text = "Fuel: (Rockets)";
			// 
			// AModulesChoicePanel
			// 
			this.AModulesChoicePanel.AutoScroll = true;
			this.AModulesChoicePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.AssemblerTable.SetColumnSpan(this.AModulesChoicePanel, 2);
			this.AModulesChoicePanel.Controls.Add(this.AModulesChoiceTable);
			this.AModulesChoicePanel.Location = new System.Drawing.Point(155, 216);
			this.AModulesChoicePanel.Name = "AModulesChoicePanel";
			this.AModulesChoicePanel.Size = new System.Drawing.Size(272, 72);
			this.AModulesChoicePanel.TabIndex = 20;
			// 
			// AModulesChoiceTable
			// 
			this.AModulesChoiceTable.AutoSize = true;
			this.AModulesChoiceTable.ColumnCount = 9;
			this.AModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.AModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.AModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.AModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.AModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.AModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.AModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.AModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
			this.AModulesChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.AModulesChoiceTable.Dock = System.Windows.Forms.DockStyle.Top;
			this.AModulesChoiceTable.Location = new System.Drawing.Point(0, 0);
			this.AModulesChoiceTable.Name = "AModulesChoiceTable";
			this.AModulesChoiceTable.RowCount = 1;
			this.AModulesChoiceTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.AModulesChoiceTable.Size = new System.Drawing.Size(268, 32);
			this.AModulesChoiceTable.TabIndex = 0;
			// 
			// AssemblerChoicePanel
			// 
			this.AssemblerChoicePanel.AutoScroll = true;
			this.AssemblerChoicePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.AssemblerChoicePanel.Controls.Add(this.AssemblerChoiceTable);
			this.AssemblerChoicePanel.Location = new System.Drawing.Point(3, 39);
			this.AssemblerChoicePanel.Margin = new System.Windows.Forms.Padding(3, 7, 13, 3);
			this.AssemblerChoicePanel.Name = "AssemblerChoicePanel";
			this.AssemblerChoicePanel.Size = new System.Drawing.Size(136, 72);
			this.AssemblerChoicePanel.TabIndex = 18;
			// 
			// AssemblerChoiceTable
			// 
			this.AssemblerChoiceTable.AutoSize = true;
			this.AssemblerChoiceTable.ColumnCount = 5;
			this.AssemblerChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.AssemblerChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.AssemblerChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.AssemblerChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.AssemblerChoiceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.AssemblerChoiceTable.Dock = System.Windows.Forms.DockStyle.Top;
			this.AssemblerChoiceTable.Location = new System.Drawing.Point(0, 0);
			this.AssemblerChoiceTable.Name = "AssemblerChoiceTable";
			this.AssemblerChoiceTable.RowCount = 1;
			this.AssemblerChoiceTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.AssemblerChoiceTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.AssemblerChoiceTable.Size = new System.Drawing.Size(132, 32);
			this.AssemblerChoiceTable.TabIndex = 0;
			// 
			// SelectedAModulesPanel
			// 
			this.SelectedAModulesPanel.AutoScroll = true;
			this.SelectedAModulesPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.SelectedAModulesPanel.Controls.Add(this.SelectedAModulesTable);
			this.SelectedAModulesPanel.Location = new System.Drawing.Point(3, 216);
			this.SelectedAModulesPanel.Margin = new System.Windows.Forms.Padding(3, 3, 13, 3);
			this.SelectedAModulesPanel.Name = "SelectedAModulesPanel";
			this.SelectedAModulesPanel.Size = new System.Drawing.Size(136, 72);
			this.SelectedAModulesPanel.TabIndex = 19;
			// 
			// SelectedAModulesTable
			// 
			this.SelectedAModulesTable.AutoSize = true;
			this.SelectedAModulesTable.ColumnCount = 5;
			this.SelectedAModulesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.SelectedAModulesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.SelectedAModulesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.SelectedAModulesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.SelectedAModulesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.SelectedAModulesTable.Dock = System.Windows.Forms.DockStyle.Top;
			this.SelectedAModulesTable.Location = new System.Drawing.Point(0, 0);
			this.SelectedAModulesTable.Name = "SelectedAModulesTable";
			this.SelectedAModulesTable.RowCount = 1;
			this.SelectedAModulesTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.SelectedAModulesTable.Size = new System.Drawing.Size(132, 32);
			this.SelectedAModulesTable.TabIndex = 0;
			// 
			// RateOptionsTable
			// 
			this.RateOptionsTable.AutoSize = true;
			this.RateOptionsTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.RateOptionsTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
			this.RateOptionsTable.ColumnCount = 4;
			this.RateOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.RateOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.RateOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.RateOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.RateOptionsTable.Controls.Add(this.FixedAssemblerCountInput, 3, 0);
			this.RateOptionsTable.Controls.Add(this.label7, 0, 0);
			this.RateOptionsTable.Controls.Add(this.FixedAssemblersOption, 2, 0);
			this.RateOptionsTable.Controls.Add(this.AutoAssemblersOption, 1, 0);
			this.RateOptionsTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RateOptionsTable.Location = new System.Drawing.Point(3, 3);
			this.RateOptionsTable.Name = "RateOptionsTable";
			this.RateOptionsTable.RowCount = 1;
			this.RateOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.RateOptionsTable.Size = new System.Drawing.Size(433, 26);
			this.RateOptionsTable.TabIndex = 18;
			// 
			// SelectedAssemblerIcon
			// 
			this.SelectedAssemblerIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
			this.SelectedAssemblerIcon.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectedAssemblerIcon.Location = new System.Drawing.Point(401, 0);
			this.SelectedAssemblerIcon.Margin = new System.Windows.Forms.Padding(0);
			this.SelectedAssemblerIcon.Name = "SelectedAssemblerIcon";
			this.SelectedAssemblerIcon.Size = new System.Drawing.Size(32, 32);
			this.SelectedAssemblerIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.SelectedAssemblerIcon.TabIndex = 23;
			this.SelectedAssemblerIcon.TabStop = false;
			// 
			// SelectedFuelIcon
			// 
			this.SelectedFuelIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
			this.SelectedFuelIcon.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectedFuelIcon.Location = new System.Drawing.Point(401, 114);
			this.SelectedFuelIcon.Margin = new System.Windows.Forms.Padding(0);
			this.SelectedFuelIcon.Name = "SelectedFuelIcon";
			this.SelectedFuelIcon.Size = new System.Drawing.Size(32, 32);
			this.SelectedFuelIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.SelectedFuelIcon.TabIndex = 24;
			this.SelectedFuelIcon.TabStop = false;
			// 
			// SelectedBeaconIcon
			// 
			this.SelectedBeaconIcon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
			this.SelectedBeaconIcon.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectedBeaconIcon.Location = new System.Drawing.Point(401, 0);
			this.SelectedBeaconIcon.Margin = new System.Windows.Forms.Padding(0);
			this.SelectedBeaconIcon.Name = "SelectedBeaconIcon";
			this.SelectedBeaconIcon.Size = new System.Drawing.Size(32, 32);
			this.SelectedBeaconIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.SelectedBeaconIcon.TabIndex = 22;
			this.SelectedBeaconIcon.TabStop = false;
			// 
			// ToolTip
			// 
			this.ToolTip.AutoPopDelay = 100000;
			this.ToolTip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(65)))), ((int)(((byte)(65)))));
			this.ToolTip.ForeColor = System.Drawing.Color.White;
			this.ToolTip.InitialDelay = 200;
			this.ToolTip.OwnerDraw = true;
			this.ToolTip.ReshowDelay = 100;
			this.ToolTip.TextFont = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			// 
			// EditRecipePanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.Black;
			this.Controls.Add(this.MainTable);
			this.DoubleBuffered = true;
			this.ForeColor = System.Drawing.Color.White;
			this.Name = "EditRecipePanel";
			this.Size = new System.Drawing.Size(439, 552);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			this.AssemblerInfoTable.ResumeLayout(false);
			this.AssemblerInfoTable.PerformLayout();
			this.MainTable.ResumeLayout(false);
			this.MainTable.PerformLayout();
			this.BeaconTable.ResumeLayout(false);
			this.BeaconTable.PerformLayout();
			this.BModulesChoicePanel.ResumeLayout(false);
			this.BModulesChoicePanel.PerformLayout();
			this.SelectedBModulesPanel.ResumeLayout(false);
			this.SelectedBModulesPanel.PerformLayout();
			this.BeaconChoicePanel.ResumeLayout(false);
			this.BeaconChoicePanel.PerformLayout();
			this.BeaconInfoTable.ResumeLayout(false);
			this.BeaconInfoTable.PerformLayout();
			this.BeaconValuesTable.ResumeLayout(false);
			this.BeaconValuesTable.PerformLayout();
			this.AssemblerTable.ResumeLayout(false);
			this.AssemblerTable.PerformLayout();
			this.FuelOptionsPanel.ResumeLayout(false);
			this.FuelOptionsPanel.PerformLayout();
			this.AModulesChoicePanel.ResumeLayout(false);
			this.AModulesChoicePanel.PerformLayout();
			this.AssemblerChoicePanel.ResumeLayout(false);
			this.AssemblerChoicePanel.PerformLayout();
			this.SelectedAModulesPanel.ResumeLayout(false);
			this.SelectedAModulesPanel.PerformLayout();
			this.RateOptionsTable.ResumeLayout(false);
			this.RateOptionsTable.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectedAssemblerIcon)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SelectedFuelIcon)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SelectedBeaconIcon)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.RadioButton AutoAssemblersOption;
        public System.Windows.Forms.RadioButton FixedAssemblersOption;
        public System.Windows.Forms.TextBox FixedAssemblerCountInput;
		private System.Windows.Forms.TableLayoutPanel AssemblerInfoTable;
		private System.Windows.Forms.Label AssemblerPollutionLabel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label AssemblerPollutionPercentLabel;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label AssemblerProductivityPercentLabel;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label AssemblerSpeedPercentLabel;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label AssemblerEnergyPercentLabel;
		private System.Windows.Forms.Label AssemblerEnergyLabel;
		private System.Windows.Forms.Label AssemblerSpeedLabel;
		private System.Windows.Forms.Label AModuleOptionsLabel;
		private System.Windows.Forms.Label AModulesLabel;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label AssemblerTitle;
		private System.Windows.Forms.TableLayoutPanel AssemblerTable;
		private System.Windows.Forms.Panel AModulesChoicePanel;
		private System.Windows.Forms.TableLayoutPanel AModulesChoiceTable;
		private System.Windows.Forms.Panel AssemblerChoicePanel;
		private System.Windows.Forms.TableLayoutPanel AssemblerChoiceTable;
		private System.Windows.Forms.TableLayoutPanel MainTable;
		private System.Windows.Forms.TableLayoutPanel BeaconTable;
		private System.Windows.Forms.Panel BModulesChoicePanel;
		private System.Windows.Forms.TableLayoutPanel BModulesChoiceTable;
		private System.Windows.Forms.Panel SelectedBModulesPanel;
		private System.Windows.Forms.TableLayoutPanel SelectedBModulesTable;
		private System.Windows.Forms.Label BeaconTitle;
		private System.Windows.Forms.Panel BeaconChoicePanel;
		private System.Windows.Forms.TableLayoutPanel BeaconChoiceTable;
		private System.Windows.Forms.Label BModulesLabel;
		private System.Windows.Forms.Label BModuleOptionsLabel;
		private System.Windows.Forms.TableLayoutPanel BeaconInfoTable;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label TotalBeaconsLabel;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label BeaconEfficiencyLabel;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label BeaconModuleCountLabel;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.Label BeaconEnergyLabel;
		private System.Windows.Forms.TableLayoutPanel RateOptionsTable;
		private System.Windows.Forms.TableLayoutPanel BeaconValuesTable;
		public System.Windows.Forms.TextBox ExtraBeaconCountInput;
		public System.Windows.Forms.TextBox BeaconsPerAssemblerCountInput;
		public System.Windows.Forms.TextBox BeaconCountInput;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private CustomToolTip ToolTip;
		private System.Windows.Forms.Panel FuelOptionsPanel;
		private System.Windows.Forms.TableLayoutPanel FuelOptionsTable;
		private System.Windows.Forms.Label FuelTitle;
		private System.Windows.Forms.Panel SelectedAModulesPanel;
		private System.Windows.Forms.TableLayoutPanel SelectedAModulesTable;
		private System.Windows.Forms.PictureBox SelectedAssemblerIcon;
		private System.Windows.Forms.PictureBox SelectedFuelIcon;
		private System.Windows.Forms.PictureBox SelectedBeaconIcon;
	}
}
