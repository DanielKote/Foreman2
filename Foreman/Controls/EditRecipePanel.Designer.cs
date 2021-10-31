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
			this.autoOption = new System.Windows.Forms.RadioButton();
			this.fixedOption = new System.Windows.Forms.RadioButton();
			this.fixedTextBox = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.PollutionLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.PollutionPercentLabel = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.ProductivityPercentLabel = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.SpeedPercentLabel = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.EnergyPercentLabel = new System.Windows.Forms.Label();
			this.EnergyLabel = new System.Windows.Forms.Label();
			this.SpeedLabel = new System.Windows.Forms.Label();
			this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
			this.button17 = new System.Windows.Forms.Button();
			this.button18 = new System.Windows.Forms.Button();
			this.button19 = new System.Windows.Forms.Button();
			this.button20 = new System.Windows.Forms.Button();
			this.button21 = new System.Windows.Forms.Button();
			this.button22 = new System.Windows.Forms.Button();
			this.button23 = new System.Windows.Forms.Button();
			this.button24 = new System.Windows.Forms.Button();
			this.button25 = new System.Windows.Forms.Button();
			this.button26 = new System.Windows.Forms.Button();
			this.button27 = new System.Windows.Forms.Button();
			this.button29 = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.button6 = new System.Windows.Forms.Button();
			this.button7 = new System.Windows.Forms.Button();
			this.button11 = new System.Windows.Forms.Button();
			this.button12 = new System.Windows.Forms.Button();
			this.button13 = new System.Windows.Forms.Button();
			this.button14 = new System.Windows.Forms.Button();
			this.button15 = new System.Windows.Forms.Button();
			this.button16 = new System.Windows.Forms.Button();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.button8 = new System.Windows.Forms.Button();
			this.button9 = new System.Windows.Forms.Button();
			this.button10 = new System.Windows.Forms.Button();
			this.panel4 = new System.Windows.Forms.Panel();
			this.label7 = new System.Windows.Forms.Label();
			this.panel5 = new System.Windows.Forms.Panel();
			this.label8 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label9 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel3.SuspendLayout();
			this.flowLayoutPanel2.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.panel4.SuspendLayout();
			this.panel5.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// autoOption
			// 
			this.autoOption.AutoSize = true;
			this.autoOption.Checked = true;
			this.autoOption.Location = new System.Drawing.Point(109, 5);
			this.autoOption.Name = "autoOption";
			this.autoOption.Size = new System.Drawing.Size(55, 21);
			this.autoOption.TabIndex = 0;
			this.autoOption.TabStop = true;
			this.autoOption.Text = "Auto";
			this.autoOption.UseVisualStyleBackColor = true;
			this.autoOption.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			// 
			// fixedOption
			// 
			this.fixedOption.AutoSize = true;
			this.fixedOption.Location = new System.Drawing.Point(163, 5);
			this.fixedOption.Name = "fixedOption";
			this.fixedOption.Size = new System.Drawing.Size(59, 21);
			this.fixedOption.TabIndex = 1;
			this.fixedOption.Text = "Fixed";
			this.fixedOption.UseVisualStyleBackColor = true;
			this.fixedOption.CheckedChanged += new System.EventHandler(this.fixedOption_CheckedChanged);
			this.fixedOption.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			// 
			// fixedTextBox
			// 
			this.fixedTextBox.Location = new System.Drawing.Point(215, 4);
			this.fixedTextBox.Name = "fixedTextBox";
			this.fixedTextBox.Size = new System.Drawing.Size(92, 23);
			this.fixedTextBox.TabIndex = 2;
			this.fixedTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			this.fixedTextBox.LostFocus += new System.EventHandler(this.fixedTextBox_LostFocus);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 68F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.PollutionLabel, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.PollutionPercentLabel, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.label4, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.ProductivityPercentLabel, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.label5, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.SpeedPercentLabel, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.label6, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.EnergyPercentLabel, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.EnergyLabel, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.SpeedLabel, 2, 1);
			this.tableLayoutPanel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(129, 18);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(178, 67);
			this.tableLayoutPanel1.TabIndex = 9;
			// 
			// PollutionLabel
			// 
			this.PollutionLabel.AutoSize = true;
			this.PollutionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PollutionLabel.Location = new System.Drawing.Point(120, 48);
			this.PollutionLabel.Margin = new System.Windows.Forms.Padding(0);
			this.PollutionLabel.Name = "PollutionLabel";
			this.PollutionLabel.Size = new System.Drawing.Size(58, 19);
			this.PollutionLabel.TabIndex = 12;
			this.PollutionLabel.Text = "12";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(0, 0);
			this.label2.Margin = new System.Windows.Forms.Padding(0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(68, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "Energy:";
			// 
			// PollutionPercentLabel
			// 
			this.PollutionPercentLabel.AutoSize = true;
			this.PollutionPercentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PollutionPercentLabel.Location = new System.Drawing.Point(68, 48);
			this.PollutionPercentLabel.Margin = new System.Windows.Forms.Padding(0);
			this.PollutionPercentLabel.Name = "PollutionPercentLabel";
			this.PollutionPercentLabel.Size = new System.Drawing.Size(52, 19);
			this.PollutionPercentLabel.TabIndex = 8;
			this.PollutionPercentLabel.Text = "2000%";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(0, 16);
			this.label4.Margin = new System.Windows.Forms.Padding(0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(68, 16);
			this.label4.TabIndex = 2;
			this.label4.Text = "Speed:";
			// 
			// ProductivityPercentLabel
			// 
			this.ProductivityPercentLabel.AutoSize = true;
			this.ProductivityPercentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductivityPercentLabel.Location = new System.Drawing.Point(68, 32);
			this.ProductivityPercentLabel.Margin = new System.Windows.Forms.Padding(0);
			this.ProductivityPercentLabel.Name = "ProductivityPercentLabel";
			this.ProductivityPercentLabel.Size = new System.Drawing.Size(52, 16);
			this.ProductivityPercentLabel.TabIndex = 7;
			this.ProductivityPercentLabel.Text = "100%";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Location = new System.Drawing.Point(0, 32);
			this.label5.Margin = new System.Windows.Forms.Padding(0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(68, 16);
			this.label5.TabIndex = 3;
			this.label5.Text = "Productivity:";
			// 
			// SpeedPercentLabel
			// 
			this.SpeedPercentLabel.AutoSize = true;
			this.SpeedPercentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SpeedPercentLabel.Location = new System.Drawing.Point(68, 16);
			this.SpeedPercentLabel.Margin = new System.Windows.Forms.Padding(0);
			this.SpeedPercentLabel.Name = "SpeedPercentLabel";
			this.SpeedPercentLabel.Size = new System.Drawing.Size(52, 16);
			this.SpeedPercentLabel.TabIndex = 6;
			this.SpeedPercentLabel.Text = "-80%";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label6.Location = new System.Drawing.Point(0, 48);
			this.label6.Margin = new System.Windows.Forms.Padding(0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(68, 19);
			this.label6.TabIndex = 4;
			this.label6.Text = "Pollution:";
			// 
			// EnergyPercentLabel
			// 
			this.EnergyPercentLabel.AutoSize = true;
			this.EnergyPercentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EnergyPercentLabel.Location = new System.Drawing.Point(68, 0);
			this.EnergyPercentLabel.Margin = new System.Windows.Forms.Padding(0);
			this.EnergyPercentLabel.Name = "EnergyPercentLabel";
			this.EnergyPercentLabel.Size = new System.Drawing.Size(52, 16);
			this.EnergyPercentLabel.TabIndex = 5;
			this.EnergyPercentLabel.Text = "10000%";
			// 
			// EnergyLabel
			// 
			this.EnergyLabel.AutoSize = true;
			this.EnergyLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EnergyLabel.Location = new System.Drawing.Point(120, 0);
			this.EnergyLabel.Margin = new System.Windows.Forms.Padding(0);
			this.EnergyLabel.Name = "EnergyLabel";
			this.EnergyLabel.Size = new System.Drawing.Size(58, 16);
			this.EnergyLabel.TabIndex = 9;
			this.EnergyLabel.Text = "5MJ";
			// 
			// SpeedLabel
			// 
			this.SpeedLabel.AutoSize = true;
			this.SpeedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SpeedLabel.Location = new System.Drawing.Point(120, 16);
			this.SpeedLabel.Margin = new System.Windows.Forms.Padding(0);
			this.SpeedLabel.Name = "SpeedLabel";
			this.SpeedLabel.Size = new System.Drawing.Size(58, 16);
			this.SpeedLabel.TabIndex = 10;
			this.SpeedLabel.Text = "128.5";
			// 
			// flowLayoutPanel3
			// 
			this.flowLayoutPanel3.AutoScroll = true;
			this.flowLayoutPanel3.BackColor = System.Drawing.Color.DimGray;
			this.flowLayoutPanel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.flowLayoutPanel3.Controls.Add(this.button17);
			this.flowLayoutPanel3.Controls.Add(this.button18);
			this.flowLayoutPanel3.Controls.Add(this.button19);
			this.flowLayoutPanel3.Controls.Add(this.button20);
			this.flowLayoutPanel3.Controls.Add(this.button21);
			this.flowLayoutPanel3.Controls.Add(this.button22);
			this.flowLayoutPanel3.Controls.Add(this.button23);
			this.flowLayoutPanel3.Controls.Add(this.button24);
			this.flowLayoutPanel3.Controls.Add(this.button25);
			this.flowLayoutPanel3.Controls.Add(this.button26);
			this.flowLayoutPanel3.Controls.Add(this.button27);
			this.flowLayoutPanel3.Controls.Add(this.button29);
			this.flowLayoutPanel3.Location = new System.Drawing.Point(134, 100);
			this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.flowLayoutPanel3.Name = "flowLayoutPanel3";
			this.flowLayoutPanel3.Size = new System.Drawing.Size(173, 59);
			this.flowLayoutPanel3.TabIndex = 14;
			// 
			// button17
			// 
			this.button17.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button17.FlatAppearance.BorderSize = 0;
			this.button17.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button17.Location = new System.Drawing.Point(1, 1);
			this.button17.Margin = new System.Windows.Forms.Padding(1);
			this.button17.Name = "button17";
			this.button17.Size = new System.Drawing.Size(24, 26);
			this.button17.TabIndex = 1;
			this.button17.UseVisualStyleBackColor = false;
			// 
			// button18
			// 
			this.button18.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button18.FlatAppearance.BorderSize = 0;
			this.button18.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button18.Location = new System.Drawing.Point(27, 1);
			this.button18.Margin = new System.Windows.Forms.Padding(1);
			this.button18.Name = "button18";
			this.button18.Size = new System.Drawing.Size(24, 26);
			this.button18.TabIndex = 2;
			this.button18.UseVisualStyleBackColor = false;
			// 
			// button19
			// 
			this.button19.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button19.FlatAppearance.BorderSize = 0;
			this.button19.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button19.Location = new System.Drawing.Point(53, 1);
			this.button19.Margin = new System.Windows.Forms.Padding(1);
			this.button19.Name = "button19";
			this.button19.Size = new System.Drawing.Size(24, 26);
			this.button19.TabIndex = 3;
			this.button19.UseVisualStyleBackColor = false;
			// 
			// button20
			// 
			this.button20.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button20.FlatAppearance.BorderSize = 0;
			this.button20.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button20.Location = new System.Drawing.Point(79, 1);
			this.button20.Margin = new System.Windows.Forms.Padding(1);
			this.button20.Name = "button20";
			this.button20.Size = new System.Drawing.Size(24, 26);
			this.button20.TabIndex = 4;
			this.button20.UseVisualStyleBackColor = false;
			// 
			// button21
			// 
			this.button21.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button21.FlatAppearance.BorderSize = 0;
			this.button21.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button21.Location = new System.Drawing.Point(105, 1);
			this.button21.Margin = new System.Windows.Forms.Padding(1);
			this.button21.Name = "button21";
			this.button21.Size = new System.Drawing.Size(24, 26);
			this.button21.TabIndex = 5;
			this.button21.UseVisualStyleBackColor = false;
			// 
			// button22
			// 
			this.button22.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button22.FlatAppearance.BorderSize = 0;
			this.button22.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button22.Location = new System.Drawing.Point(1, 29);
			this.button22.Margin = new System.Windows.Forms.Padding(1);
			this.button22.Name = "button22";
			this.button22.Size = new System.Drawing.Size(24, 26);
			this.button22.TabIndex = 6;
			this.button22.UseVisualStyleBackColor = false;
			// 
			// button23
			// 
			this.button23.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button23.FlatAppearance.BorderSize = 0;
			this.button23.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button23.Location = new System.Drawing.Point(27, 29);
			this.button23.Margin = new System.Windows.Forms.Padding(1);
			this.button23.Name = "button23";
			this.button23.Size = new System.Drawing.Size(24, 26);
			this.button23.TabIndex = 7;
			this.button23.UseVisualStyleBackColor = false;
			// 
			// button24
			// 
			this.button24.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button24.FlatAppearance.BorderSize = 0;
			this.button24.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button24.Location = new System.Drawing.Point(53, 29);
			this.button24.Margin = new System.Windows.Forms.Padding(1);
			this.button24.Name = "button24";
			this.button24.Size = new System.Drawing.Size(24, 26);
			this.button24.TabIndex = 8;
			this.button24.UseVisualStyleBackColor = false;
			// 
			// button25
			// 
			this.button25.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button25.FlatAppearance.BorderSize = 0;
			this.button25.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button25.Location = new System.Drawing.Point(79, 29);
			this.button25.Margin = new System.Windows.Forms.Padding(1);
			this.button25.Name = "button25";
			this.button25.Size = new System.Drawing.Size(24, 26);
			this.button25.TabIndex = 9;
			this.button25.UseVisualStyleBackColor = false;
			// 
			// button26
			// 
			this.button26.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button26.FlatAppearance.BorderSize = 0;
			this.button26.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button26.Location = new System.Drawing.Point(105, 29);
			this.button26.Margin = new System.Windows.Forms.Padding(1);
			this.button26.Name = "button26";
			this.button26.Size = new System.Drawing.Size(24, 26);
			this.button26.TabIndex = 10;
			this.button26.UseVisualStyleBackColor = false;
			// 
			// button27
			// 
			this.button27.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button27.FlatAppearance.BorderSize = 0;
			this.button27.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button27.Location = new System.Drawing.Point(1, 57);
			this.button27.Margin = new System.Windows.Forms.Padding(1);
			this.button27.Name = "button27";
			this.button27.Size = new System.Drawing.Size(24, 26);
			this.button27.TabIndex = 11;
			this.button27.UseVisualStyleBackColor = false;
			// 
			// button29
			// 
			this.button29.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button29.FlatAppearance.BorderSize = 0;
			this.button29.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button29.Location = new System.Drawing.Point(27, 57);
			this.button29.Margin = new System.Windows.Forms.Padding(1);
			this.button29.Name = "button29";
			this.button29.Size = new System.Drawing.Size(24, 26);
			this.button29.TabIndex = 13;
			this.button29.UseVisualStyleBackColor = false;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(136, 83);
			this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(111, 17);
			this.label3.TabIndex = 13;
			this.label3.Text = "Module Options:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(5, 83);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(107, 17);
			this.label1.TabIndex = 12;
			this.label1.Text = "Active Modules:";
			// 
			// flowLayoutPanel2
			// 
			this.flowLayoutPanel2.AutoScroll = true;
			this.flowLayoutPanel2.BackColor = System.Drawing.Color.DimGray;
			this.flowLayoutPanel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.flowLayoutPanel2.Controls.Add(this.button6);
			this.flowLayoutPanel2.Controls.Add(this.button7);
			this.flowLayoutPanel2.Controls.Add(this.button11);
			this.flowLayoutPanel2.Controls.Add(this.button12);
			this.flowLayoutPanel2.Controls.Add(this.button13);
			this.flowLayoutPanel2.Controls.Add(this.button14);
			this.flowLayoutPanel2.Controls.Add(this.button15);
			this.flowLayoutPanel2.Controls.Add(this.button16);
			this.flowLayoutPanel2.Location = new System.Drawing.Point(4, 100);
			this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.flowLayoutPanel2.Name = "flowLayoutPanel2";
			this.flowLayoutPanel2.Size = new System.Drawing.Size(122, 59);
			this.flowLayoutPanel2.TabIndex = 11;
			// 
			// button6
			// 
			this.button6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button6.FlatAppearance.BorderSize = 0;
			this.button6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button6.Location = new System.Drawing.Point(1, 1);
			this.button6.Margin = new System.Windows.Forms.Padding(1);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(24, 26);
			this.button6.TabIndex = 1;
			this.button6.UseVisualStyleBackColor = false;
			// 
			// button7
			// 
			this.button7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button7.FlatAppearance.BorderSize = 0;
			this.button7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button7.Location = new System.Drawing.Point(27, 1);
			this.button7.Margin = new System.Windows.Forms.Padding(1);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(24, 26);
			this.button7.TabIndex = 2;
			this.button7.UseVisualStyleBackColor = false;
			// 
			// button11
			// 
			this.button11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button11.FlatAppearance.BorderSize = 0;
			this.button11.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button11.Location = new System.Drawing.Point(53, 1);
			this.button11.Margin = new System.Windows.Forms.Padding(1);
			this.button11.Name = "button11";
			this.button11.Size = new System.Drawing.Size(24, 26);
			this.button11.TabIndex = 3;
			this.button11.UseVisualStyleBackColor = false;
			// 
			// button12
			// 
			this.button12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button12.FlatAppearance.BorderSize = 0;
			this.button12.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button12.Location = new System.Drawing.Point(79, 1);
			this.button12.Margin = new System.Windows.Forms.Padding(1);
			this.button12.Name = "button12";
			this.button12.Size = new System.Drawing.Size(24, 26);
			this.button12.TabIndex = 4;
			this.button12.UseVisualStyleBackColor = false;
			// 
			// button13
			// 
			this.button13.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button13.FlatAppearance.BorderSize = 0;
			this.button13.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button13.Location = new System.Drawing.Point(1, 29);
			this.button13.Margin = new System.Windows.Forms.Padding(1);
			this.button13.Name = "button13";
			this.button13.Size = new System.Drawing.Size(24, 26);
			this.button13.TabIndex = 5;
			this.button13.UseVisualStyleBackColor = false;
			// 
			// button14
			// 
			this.button14.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button14.FlatAppearance.BorderSize = 0;
			this.button14.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button14.Location = new System.Drawing.Point(27, 29);
			this.button14.Margin = new System.Windows.Forms.Padding(1);
			this.button14.Name = "button14";
			this.button14.Size = new System.Drawing.Size(24, 26);
			this.button14.TabIndex = 6;
			this.button14.UseVisualStyleBackColor = false;
			// 
			// button15
			// 
			this.button15.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button15.FlatAppearance.BorderSize = 0;
			this.button15.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button15.Location = new System.Drawing.Point(53, 29);
			this.button15.Margin = new System.Windows.Forms.Padding(1);
			this.button15.Name = "button15";
			this.button15.Size = new System.Drawing.Size(24, 26);
			this.button15.TabIndex = 7;
			this.button15.UseVisualStyleBackColor = false;
			// 
			// button16
			// 
			this.button16.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button16.FlatAppearance.BorderSize = 0;
			this.button16.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button16.Location = new System.Drawing.Point(79, 29);
			this.button16.Margin = new System.Windows.Forms.Padding(1);
			this.button16.Name = "button16";
			this.button16.Size = new System.Drawing.Size(24, 26);
			this.button16.TabIndex = 8;
			this.button16.UseVisualStyleBackColor = false;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoScroll = true;
			this.flowLayoutPanel1.BackColor = System.Drawing.Color.DimGray;
			this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.flowLayoutPanel1.Controls.Add(this.button1);
			this.flowLayoutPanel1.Controls.Add(this.button2);
			this.flowLayoutPanel1.Controls.Add(this.button3);
			this.flowLayoutPanel1.Controls.Add(this.button4);
			this.flowLayoutPanel1.Controls.Add(this.button5);
			this.flowLayoutPanel1.Controls.Add(this.button8);
			this.flowLayoutPanel1.Controls.Add(this.button9);
			this.flowLayoutPanel1.Controls.Add(this.button10);
			this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 22);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(122, 59);
			this.flowLayoutPanel1.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button1.FlatAppearance.BorderSize = 0;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Location = new System.Drawing.Point(1, 1);
			this.button1.Margin = new System.Windows.Forms.Padding(1);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(24, 26);
			this.button1.TabIndex = 0;
			this.button1.UseVisualStyleBackColor = false;
			// 
			// button2
			// 
			this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button2.FlatAppearance.BorderSize = 0;
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button2.Location = new System.Drawing.Point(27, 1);
			this.button2.Margin = new System.Windows.Forms.Padding(1);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(24, 26);
			this.button2.TabIndex = 1;
			this.button2.UseVisualStyleBackColor = false;
			// 
			// button3
			// 
			this.button3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button3.FlatAppearance.BorderSize = 0;
			this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button3.Location = new System.Drawing.Point(53, 1);
			this.button3.Margin = new System.Windows.Forms.Padding(1);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(24, 26);
			this.button3.TabIndex = 2;
			this.button3.UseVisualStyleBackColor = false;
			// 
			// button4
			// 
			this.button4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button4.FlatAppearance.BorderSize = 0;
			this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button4.Location = new System.Drawing.Point(79, 1);
			this.button4.Margin = new System.Windows.Forms.Padding(1);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(24, 26);
			this.button4.TabIndex = 3;
			this.button4.UseVisualStyleBackColor = false;
			// 
			// button5
			// 
			this.button5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button5.FlatAppearance.BorderSize = 0;
			this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button5.Location = new System.Drawing.Point(1, 29);
			this.button5.Margin = new System.Windows.Forms.Padding(1);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(24, 26);
			this.button5.TabIndex = 4;
			this.button5.UseVisualStyleBackColor = false;
			// 
			// button8
			// 
			this.button8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button8.FlatAppearance.BorderSize = 0;
			this.button8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button8.Location = new System.Drawing.Point(27, 29);
			this.button8.Margin = new System.Windows.Forms.Padding(1);
			this.button8.Name = "button8";
			this.button8.Size = new System.Drawing.Size(24, 26);
			this.button8.TabIndex = 5;
			this.button8.UseVisualStyleBackColor = false;
			// 
			// button9
			// 
			this.button9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button9.FlatAppearance.BorderSize = 0;
			this.button9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button9.Location = new System.Drawing.Point(53, 29);
			this.button9.Margin = new System.Windows.Forms.Padding(1);
			this.button9.Name = "button9";
			this.button9.Size = new System.Drawing.Size(24, 26);
			this.button9.TabIndex = 6;
			this.button9.UseVisualStyleBackColor = false;
			// 
			// button10
			// 
			this.button10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.button10.FlatAppearance.BorderSize = 0;
			this.button10.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button10.Location = new System.Drawing.Point(79, 29);
			this.button10.Margin = new System.Windows.Forms.Padding(1);
			this.button10.Name = "button10";
			this.button10.Size = new System.Drawing.Size(24, 26);
			this.button10.TabIndex = 7;
			this.button10.UseVisualStyleBackColor = false;
			// 
			// panel4
			// 
			this.panel4.BackColor = System.Drawing.Color.DimGray;
			this.panel4.Controls.Add(this.label7);
			this.panel4.Controls.Add(this.fixedTextBox);
			this.panel4.Controls.Add(this.autoOption);
			this.panel4.Controls.Add(this.fixedOption);
			this.panel4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.panel4.Location = new System.Drawing.Point(2, 2);
			this.panel4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(312, 30);
			this.panel4.TabIndex = 14;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(2, 6);
			this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(113, 17);
			this.label7.TabIndex = 3;
			this.label7.Text = "# of Assemblers:";
			// 
			// panel5
			// 
			this.panel5.BackColor = System.Drawing.Color.DimGray;
			this.panel5.Controls.Add(this.tableLayoutPanel1);
			this.panel5.Controls.Add(this.label8);
			this.panel5.Controls.Add(this.label3);
			this.panel5.Controls.Add(this.flowLayoutPanel3);
			this.panel5.Controls.Add(this.flowLayoutPanel1);
			this.panel5.Controls.Add(this.label1);
			this.panel5.Controls.Add(this.flowLayoutPanel2);
			this.panel5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.panel5.Location = new System.Drawing.Point(2, 37);
			this.panel5.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(312, 163);
			this.panel5.TabIndex = 15;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(5, 2);
			this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(199, 17);
			this.label8.TabIndex = 0;
			this.label8.Text = "Assembler: (Space factory #2)";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.DimGray;
			this.panel1.Controls.Add(this.label9);
			this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.panel1.Location = new System.Drawing.Point(2, 206);
			this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(312, 145);
			this.panel1.TabIndex = 16;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(5, 2);
			this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(199, 17);
			this.label9.TabIndex = 1;
			this.label9.Text = "Assembler: (Space factory #2)";
			// 
			// EditRecipePanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.Black;
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.panel5);
			this.Controls.Add(this.panel4);
			this.ForeColor = System.Drawing.Color.White;
			this.Name = "EditRecipePanel";
			this.Size = new System.Drawing.Size(317, 362);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel3.ResumeLayout(false);
			this.flowLayoutPanel2.ResumeLayout(false);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			this.panel5.ResumeLayout(false);
			this.panel5.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RadioButton autoOption;
        public System.Windows.Forms.RadioButton fixedOption;
        public System.Windows.Forms.TextBox fixedTextBox;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label PollutionLabel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label PollutionPercentLabel;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label ProductivityPercentLabel;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label SpeedPercentLabel;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label EnergyPercentLabel;
		private System.Windows.Forms.Label EnergyLabel;
		private System.Windows.Forms.Label SpeedLabel;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button button17;
		private System.Windows.Forms.Button button18;
		private System.Windows.Forms.Button button19;
		private System.Windows.Forms.Button button20;
		private System.Windows.Forms.Button button21;
		private System.Windows.Forms.Button button22;
		private System.Windows.Forms.Button button23;
		private System.Windows.Forms.Button button24;
		private System.Windows.Forms.Button button25;
		private System.Windows.Forms.Button button26;
		private System.Windows.Forms.Button button27;
		private System.Windows.Forms.Button button29;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.Button button11;
		private System.Windows.Forms.Button button12;
		private System.Windows.Forms.Button button13;
		private System.Windows.Forms.Button button14;
		private System.Windows.Forms.Button button15;
		private System.Windows.Forms.Button button16;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button8;
		private System.Windows.Forms.Button button9;
		private System.Windows.Forms.Button button10;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label9;
	}
}
