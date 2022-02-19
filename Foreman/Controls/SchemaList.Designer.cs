namespace Foreman.Controls
{
    partial class SchemaList
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
            this.listSchemas = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // listSchemas
            // 
            this.listSchemas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listSchemas.FormattingEnabled = true;
            this.listSchemas.Location = new System.Drawing.Point(0, 0);
            this.listSchemas.Name = "listSchemas";
            this.listSchemas.Size = new System.Drawing.Size(150, 150);
            this.listSchemas.TabIndex = 0;
            this.listSchemas.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listSchemas_MouseDoubleClick);
            // 
            // SchemaList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listSchemas);
            this.Name = "SchemaList";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listSchemas;
    }
}
