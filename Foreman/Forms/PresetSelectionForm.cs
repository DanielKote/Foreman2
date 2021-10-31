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
    public partial class PresetSelectionForm : Form
    {
        public Preset ChosenPreset;

        private Dictionary<Preset, PresetErrorPackage> PresetErrors;

        public PresetSelectionForm(Dictionary<Preset, PresetErrorPackage> presetErrors)
        {
            PresetErrors = presetErrors;
            InitializeComponent();



        }
    }
}
