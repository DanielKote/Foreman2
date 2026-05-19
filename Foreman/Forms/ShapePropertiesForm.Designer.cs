namespace Foreman
{
    partial class ShapePropertiesForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.ShapeTypeLabel = new System.Windows.Forms.Label();
            this.ShapeTypeCombo = new System.Windows.Forms.ComboBox();
            this.FillColorLabel = new System.Windows.Forms.Label();
            this.FillColorButton = new System.Windows.Forms.Button();
            this.FillAlphaLabel = new System.Windows.Forms.Label();
            this.FillAlphaTrack = new System.Windows.Forms.TrackBar();
            this.NoFillCheckBox = new System.Windows.Forms.CheckBox();
            this.BorderColorLabel = new System.Windows.Forms.Label();
            this.BorderColorButton = new System.Windows.Forms.Button();
            this.BorderWidthLabel = new System.Windows.Forms.Label();
            this.BorderWidthInput = new System.Windows.Forms.NumericUpDown();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.FillAlphaTrack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BorderWidthInput)).BeginInit();
            this.SuspendLayout();

            // ShapeTypeLabel
            this.ShapeTypeLabel.AutoSize = true;
            this.ShapeTypeLabel.Location = new System.Drawing.Point(12, 15);
            this.ShapeTypeLabel.Text = "Shape Type:";

            // ShapeTypeCombo
            this.ShapeTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ShapeTypeCombo.Items.AddRange(new object[] { "Rectangle", "Ellipse" });
            this.ShapeTypeCombo.Location = new System.Drawing.Point(120, 12);
            this.ShapeTypeCombo.Size = new System.Drawing.Size(130, 21);
            this.ShapeTypeCombo.TabIndex = 0;
            this.ShapeTypeCombo.SelectedIndexChanged += new System.EventHandler(this.ShapeTypeCombo_SelectedIndexChanged);

            // FillColorLabel
            this.FillColorLabel.AutoSize = true;
            this.FillColorLabel.Location = new System.Drawing.Point(12, 50);
            this.FillColorLabel.Text = "Fill Colour:";

            // FillColorButton
            this.FillColorButton.Location = new System.Drawing.Point(120, 47);
            this.FillColorButton.Size = new System.Drawing.Size(130, 26);
            this.FillColorButton.TabIndex = 1;
            this.FillColorButton.Text = "Choose…";
            this.FillColorButton.UseVisualStyleBackColor = false;
            this.FillColorButton.Click += new System.EventHandler(this.FillColorButton_Click);

            // NoFillCheckBox
            this.NoFillCheckBox.AutoSize = true;
            this.NoFillCheckBox.Location = new System.Drawing.Point(12, 82);
            this.NoFillCheckBox.TabIndex = 2;
            this.NoFillCheckBox.Text = "No fill";
            this.NoFillCheckBox.UseVisualStyleBackColor = true;
            this.NoFillCheckBox.CheckedChanged += new System.EventHandler(this.NoFillCheckBox_CheckedChanged);

            // FillAlphaLabel
            this.FillAlphaLabel.AutoSize = true;
            this.FillAlphaLabel.Location = new System.Drawing.Point(12, 108);
            this.FillAlphaLabel.Text = "Opacity: 255";

            // FillAlphaTrack
            this.FillAlphaTrack.Location = new System.Drawing.Point(120, 104);
            this.FillAlphaTrack.Maximum = 255;
            this.FillAlphaTrack.Minimum = 0;
            this.FillAlphaTrack.Size = new System.Drawing.Size(130, 30);
            this.FillAlphaTrack.TabIndex = 3;
            this.FillAlphaTrack.TickFrequency = 32;
            this.FillAlphaTrack.Value = 255;
            this.FillAlphaTrack.Scroll += new System.EventHandler(this.FillAlphaTrack_Scroll);

            // BorderColorLabel
            this.BorderColorLabel.AutoSize = true;
            this.BorderColorLabel.Location = new System.Drawing.Point(12, 150);
            this.BorderColorLabel.Text = "Border Colour:";

            // BorderColorButton
            this.BorderColorButton.Location = new System.Drawing.Point(120, 147);
            this.BorderColorButton.Size = new System.Drawing.Size(130, 26);
            this.BorderColorButton.TabIndex = 4;
            this.BorderColorButton.Text = "Choose…";
            this.BorderColorButton.UseVisualStyleBackColor = false;
            this.BorderColorButton.Click += new System.EventHandler(this.BorderColorButton_Click);

            // BorderWidthLabel
            this.BorderWidthLabel.AutoSize = true;
            this.BorderWidthLabel.Location = new System.Drawing.Point(12, 185);
            this.BorderWidthLabel.Text = "Border Width:";

            // BorderWidthInput
            this.BorderWidthInput.Location = new System.Drawing.Point(120, 183);
            this.BorderWidthInput.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            this.BorderWidthInput.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            this.BorderWidthInput.Size = new System.Drawing.Size(60, 20);
            this.BorderWidthInput.TabIndex = 5;
            this.BorderWidthInput.Value = new decimal(new int[] { 2, 0, 0, 0 });
            this.BorderWidthInput.ValueChanged += new System.EventHandler(this.BorderWidthInput_ValueChanged);

            // OKButton
            this.OKButton.Location = new System.Drawing.Point(90, 220);
            this.OKButton.Size = new System.Drawing.Size(75, 26);
            this.OKButton.TabIndex = 6;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);

            // CancelBtn
            this.CancelBtn.Location = new System.Drawing.Point(175, 220);
            this.CancelBtn.Size = new System.Drawing.Size(75, 26);
            this.CancelBtn.TabIndex = 7;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelButton_Click);

            // ShapePropertiesForm
            this.AcceptButton = this.OKButton;
            this.CancelButton = this.CancelBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(264, 258);
            this.Controls.Add(this.ShapeTypeLabel);
            this.Controls.Add(this.ShapeTypeCombo);
            this.Controls.Add(this.FillColorLabel);
            this.Controls.Add(this.FillColorButton);
            this.Controls.Add(this.NoFillCheckBox);
            this.Controls.Add(this.FillAlphaLabel);
            this.Controls.Add(this.FillAlphaTrack);
            this.Controls.Add(this.BorderColorLabel);
            this.Controls.Add(this.BorderColorButton);
            this.Controls.Add(this.BorderWidthLabel);
            this.Controls.Add(this.BorderWidthInput);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.CancelBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShapePropertiesForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Shape Properties";
            ((System.ComponentModel.ISupportInitialize)(this.FillAlphaTrack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BorderWidthInput)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label ShapeTypeLabel;
        private System.Windows.Forms.ComboBox ShapeTypeCombo;
        private System.Windows.Forms.Label FillColorLabel;
        private System.Windows.Forms.Button FillColorButton;
        private System.Windows.Forms.Label FillAlphaLabel;
        private System.Windows.Forms.TrackBar FillAlphaTrack;
        private System.Windows.Forms.CheckBox NoFillCheckBox;
        private System.Windows.Forms.Label BorderColorLabel;
        private System.Windows.Forms.Button BorderColorButton;
        private System.Windows.Forms.Label BorderWidthLabel;
        private System.Windows.Forms.NumericUpDown BorderWidthInput;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelBtn;
    }
}
