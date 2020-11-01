using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RMVN_Studio.Windows;

namespace RMVN_Studio
{
    public partial class Mixer : Form
    {
        public Mixer()
        {
            InitializeComponent();
        }

        private void Mixer_Load(object sender, EventArgs e)
        {
            MixerControl mixer = new MixerControl();
            mixer.Dock = DockStyle.Fill;
            this.Controls.Add(mixer);
        }
    }
}
