using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RMVN_Studio.Windows
{
    public partial class MixerControl : UserControl
    {
        public MixerControl()
        {
            InitializeComponent();
        }

        private void MixerControl_Load(object sender, EventArgs e)
        {
            Channels channel = new Channels();
            channel.label1.Text = "Master";
            channel.Dock = DockStyle.Left;
            channel.Name = "Master";
            this.Master.Controls.Add(channel);
            for (int i=99; i >= 0; i--)
            {
                Channels channel1 = new Channels();
                channel1.label1.Text = "Channel " + (i + 1).ToString();
                channel1.Dock = DockStyle.Left;
                channel1.Name = "Channel" + (i + 1).ToString();
                this.panel3.Controls.Add(channel1);
            }
            panel3.AutoScroll = false;
            panel3.VerticalScroll.Enabled = false;
            panel3.VerticalScroll.Visible = false;
            panel3.VerticalScroll.Maximum = 0;
            panel3.AutoScroll = true;
        }
    }
}
