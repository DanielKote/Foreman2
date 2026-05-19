namespace Foreman {
    partial class TextPropertiesForm
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
            this.TextLabel = new System.Windows.Forms.Label();
            this.TextInput = new System.Windows.Forms.TextBox();
            this.FontLabel = new System.Windows.Forms.Label();
            this.FontButton = new System.Windows.Forms.Button();
            this.FontPreviewLabel = new System.Windows.Forms.Label();
            this.AlignLabel = new System.Windows.Forms.Label();
            this.AlignLeftRadio = new System.Windows.Forms.RadioButton();
            this.AlignCenterRadio = new System.Windows.Forms.RadioButton();
            this.AlignRightRadio = new System.Windows.Forms.RadioButton();
            this.TextColorLabel = new System.Windows.Forms.Label();
            this.TextColorButton = new System.Windows.Forms.Button();
            this.BackColorLabel = new System.Windows.Forms.Label();
            this.BackColorButton = new System.Windows.Forms.Button();
            this.TransparentCheckBox = new System.Windows.Forms.CheckBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // TextLabel
            this.TextLabel.AutoSize = true;
            this.TextLabel.Location = new System.Drawing.Point(12, 15);
            this.TextLabel.Text = "Label Text:";

            // TextInput
            this.TextInput.Location = new System.Drawing.Point(120, 12);
            this.TextInput.Size = new System.Drawing.Size(200, 20);
            this.TextInput.TabIndex = 0;
            this.TextInput.TextChanged += new System.EventHandler(this.TextInput_TextChanged);

            // FontLabel
            this.FontLabel.AutoSize = true;
            this.FontLabel.Location = new System.Drawing.Point(12, 48);
            this.FontLabel.Text = "Font:";

            // FontButton
            this.FontButton.Location = new System.Drawing.Point(120, 44);
            this.FontButton.Size = new System.Drawing.Size(100, 26);
            this.FontButton.TabIndex = 1;
            this.FontButton.Text = "Choose Font…";
            this.FontButton.UseVisualStyleBackColor = true;
            this.FontButton.Click += new System.EventHandler(this.FontButton_Click);

            // FontPreviewLabel
            this.FontPreviewLabel.AutoSize = true;
            this.FontPreviewLabel.Location = new System.Drawing.Point(12, 78);
            this.FontPreviewLabel.Size = new System.Drawing.Size(310, 13);
            this.FontPreviewLabel.Name = "FontPreviewLabel";
            this.FontPreviewLabel.Text = "";

            // AlignLabel
            this.AlignLabel.AutoSize = true;
            this.AlignLabel.Location = new System.Drawing.Point(12, 104);
            this.AlignLabel.Text = "Alignment:";

            // AlignLeftRadio
            this.AlignLeftRadio.AutoSize = true;
            this.AlignLeftRadio.Location = new System.Drawing.Point(120, 101);
            this.AlignLeftRadio.Size = new System.Drawing.Size(46, 17);
            this.AlignLeftRadio.TabIndex = 2;
            this.AlignLeftRadio.Text = "Left";
            this.AlignLeftRadio.UseVisualStyleBackColor = true;
            this.AlignLeftRadio.CheckedChanged += new System.EventHandler(this.AlignRadio_CheckedChanged);

            // AlignCenterRadio
            this.AlignCenterRadio.AutoSize = true;
            this.AlignCenterRadio.Location = new System.Drawing.Point(178, 101);
            this.AlignCenterRadio.Size = new System.Drawing.Size(56, 17);
            this.AlignCenterRadio.TabIndex = 3;
            this.AlignCenterRadio.Text = "Center";
            this.AlignCenterRadio.UseVisualStyleBackColor = true;
            this.AlignCenterRadio.CheckedChanged += new System.EventHandler(this.AlignRadio_CheckedChanged);

            // AlignRightRadio
            this.AlignRightRadio.AutoSize = true;
            this.AlignRightRadio.Location = new System.Drawing.Point(246, 101);
            this.AlignRightRadio.Size = new System.Drawing.Size(51, 17);
            this.AlignRightRadio.TabIndex = 4;
            this.AlignRightRadio.Text = "Right";
            this.AlignRightRadio.UseVisualStyleBackColor = true;
            this.AlignRightRadio.CheckedChanged += new System.EventHandler(this.AlignRadio_CheckedChanged);

            // TextColorLabel
            this.TextColorLabel.AutoSize = true;
            this.TextColorLabel.Location = new System.Drawing.Point(12, 132);
            this.TextColorLabel.Text = "Text Colour:";

            // TextColorButton
            this.TextColorButton.Location = new System.Drawing.Point(120, 129);
            this.TextColorButton.Size = new System.Drawing.Size(130, 26);
            this.TextColorButton.TabIndex = 5;
            this.TextColorButton.Text = "Choose…";
            this.TextColorButton.UseVisualStyleBackColor = false;
            this.TextColorButton.Click += new System.EventHandler(this.TextColorButton_Click);

            // BackColorLabel
            this.BackColorLabel.AutoSize = true;
            this.BackColorLabel.Location = new System.Drawing.Point(12, 168);
            this.BackColorLabel.Text = "Background:";

            // BackColorButton
            this.BackColorButton.Location = new System.Drawing.Point(120, 165);
            this.BackColorButton.Size = new System.Drawing.Size(130, 26);
            this.BackColorButton.TabIndex = 6;
            this.BackColorButton.Text = "Choose…";
            this.BackColorButton.UseVisualStyleBackColor = false;
            this.BackColorButton.Click += new System.EventHandler(this.BackColorButton_Click);

            // TransparentCheckBox
            this.TransparentCheckBox.AutoSize = true;
            this.TransparentCheckBox.Location = new System.Drawing.Point(120, 198);
            this.TransparentCheckBox.TabIndex = 7;
            this.TransparentCheckBox.Text = "Transparent background";
            this.TransparentCheckBox.UseVisualStyleBackColor = true;
            this.TransparentCheckBox.CheckedChanged += new System.EventHandler(this.TransparentCheckBox_CheckedChanged);

            // OKButton
            this.OKButton.Location = new System.Drawing.Point(130, 228);
            this.OKButton.Size = new System.Drawing.Size(75, 26);
            this.OKButton.TabIndex = 8;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);

            // CancelBtn
            this.CancelBtn.Location = new System.Drawing.Point(215, 228);
            this.CancelBtn.Size = new System.Drawing.Size(75, 26);
            this.CancelBtn.TabIndex = 9;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelButton_Click);

            // TextPropertiesForm
            this.AcceptButton = this.OKButton;
            this.CancelButton = this.CancelBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 266);
            this.Controls.Add(this.TextLabel);
            this.Controls.Add(this.TextInput);
            this.Controls.Add(this.FontLabel);
            this.Controls.Add(this.FontButton);
            this.Controls.Add(this.FontPreviewLabel);
            this.Controls.Add(this.AlignLabel);
            this.Controls.Add(this.AlignLeftRadio);
            this.Controls.Add(this.AlignCenterRadio);
            this.Controls.Add(this.AlignRightRadio);
            this.Controls.Add(this.TextColorLabel);
            this.Controls.Add(this.TextColorButton);
            this.Controls.Add(this.BackColorLabel);
            this.Controls.Add(this.BackColorButton);
            this.Controls.Add(this.TransparentCheckBox);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.CancelBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TextPropertiesForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Text Properties";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label TextLabel;
        private System.Windows.Forms.TextBox TextInput;
        private System.Windows.Forms.Label FontLabel;
        private System.Windows.Forms.Button FontButton;
        private System.Windows.Forms.Label FontPreviewLabel;
        private System.Windows.Forms.Label AlignLabel;
        private System.Windows.Forms.RadioButton AlignLeftRadio;
        private System.Windows.Forms.RadioButton AlignCenterRadio;
        private System.Windows.Forms.RadioButton AlignRightRadio;
        private System.Windows.Forms.Label TextColorLabel;
        private System.Windows.Forms.Button TextColorButton;
        private System.Windows.Forms.Label BackColorLabel;
        private System.Windows.Forms.Button BackColorButton;
        private System.Windows.Forms.CheckBox TransparentCheckBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelBtn;
    }
}
