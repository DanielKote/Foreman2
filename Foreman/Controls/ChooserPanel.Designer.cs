namespace Foreman
{
	partial class ChooserPanel
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
            this.RecipeComboBox = new System.Windows.Forms.ComboBox();
            this.SourceButton = new System.Windows.Forms.Button();
            this.PassthroughButton = new System.Windows.Forms.Button();
            this.ResultButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // RecipeComboBox
            // 
            this.RecipeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RecipeComboBox.FormattingEnabled = true;
            this.RecipeComboBox.Location = new System.Drawing.Point(3, 41);
            this.RecipeComboBox.Name = "RecipeComboBox";
            this.RecipeComboBox.Size = new System.Drawing.Size(287, 24);
            this.RecipeComboBox.TabIndex = 0;
            this.RecipeComboBox.SelectedIndexChanged += new System.EventHandler(this.RecipeComboBox_SelectedIndexChanged);
            // 
            // SourceButton
            // 
            this.SourceButton.Location = new System.Drawing.Point(3, 3);
            this.SourceButton.Name = "SourceButton";
            this.SourceButton.Size = new System.Drawing.Size(75, 32);
            this.SourceButton.TabIndex = 1;
            this.SourceButton.Text = "Source";
            this.SourceButton.UseVisualStyleBackColor = true;
            this.SourceButton.Click += new System.EventHandler(this.SourceButton_Click);
            // 
            // PassthroughButton
            // 
            this.PassthroughButton.Location = new System.Drawing.Point(84, 3);
            this.PassthroughButton.Name = "PassthroughButton";
            this.PassthroughButton.Size = new System.Drawing.Size(125, 32);
            this.PassthroughButton.TabIndex = 2;
            this.PassthroughButton.Text = "Passthrough";
            this.PassthroughButton.UseVisualStyleBackColor = true;
            this.PassthroughButton.Click += new System.EventHandler(this.PassthroughButton_Click);
            // 
            // ResultButton
            // 
            this.ResultButton.Location = new System.Drawing.Point(215, 3);
            this.ResultButton.Name = "ResultButton";
            this.ResultButton.Size = new System.Drawing.Size(75, 32);
            this.ResultButton.TabIndex = 3;
            this.ResultButton.Text = "Result";
            this.ResultButton.UseVisualStyleBackColor = true;
            this.ResultButton.Click += new System.EventHandler(this.ResultButton_Click);
            // 
            // ChooserPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.ResultButton);
            this.Controls.Add(this.PassthroughButton);
            this.Controls.Add(this.SourceButton);
            this.Controls.Add(this.RecipeComboBox);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximumSize = new System.Drawing.Size(1333331, 738);
            this.Name = "ChooserPanel";
            this.Size = new System.Drawing.Size(293, 70);
            this.Load += new System.EventHandler(this.ChooserPanel_Load);
            this.Leave += new System.EventHandler(this.ChooserPanel_Leave);
            this.ResumeLayout(false);

		}

        #endregion

        private System.Windows.Forms.ComboBox RecipeComboBox;
        private System.Windows.Forms.Button SourceButton;
        private System.Windows.Forms.Button PassthroughButton;
        private System.Windows.Forms.Button ResultButton;
    }
}
