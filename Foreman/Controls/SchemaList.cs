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

namespace Foreman.Controls
{
    public partial class SchemaList : UserControl
    {
        public MainForm mainForm;
        public SchemaList()
        {
            InitializeComponent();
        }

        public void LoadSchemas()
        {
            DirectoryInfo di = new DirectoryInfo(Application.StartupPath + "//Saved Graphs");
            listSchemas.Items.Clear();
            foreach (FileInfo file in di.GetFiles("*.fjson"))
            { 
                listSchemas.Items.Add(file.Name);
            }
        }

        private void listSchemas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string fn = listSchemas.SelectedItem.ToString();
            mainForm.GraphViewerTabContainer.LoadGraph(Application.StartupPath + "//Saved Graphs//" + fn, fn);
            mainForm.UpdateGridLines();
        }
    }
}
