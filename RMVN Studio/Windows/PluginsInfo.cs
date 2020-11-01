using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RMVN_Studio.VST;

namespace RMVN_Studio.Windows
{
    public partial class PluginsInfo : UserControl
    {
        class WindowCaptureChanged : NativeWindow
        {
            public CaptureChanged OnCaptureChanged;

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 533)    
                    OnCaptureChanged();
                base.WndProc(ref m);
            }
        }
        public delegate void CaptureChanged();
        public CaptureChanged OnCaptureChanged;
        WindowCaptureChanged wcc;

        private void CaptureChangedEventHandler()
        {

            if (!this.Capture)
            {
                if (!this.RectangleToScreen(this.DisplayRectangle).Contains(Cursor.Position))
                {
                    panel1.BackColor = oriColor;
                }
                else
                {
                    this.Capture = true;
                }
            }
        }

        string VSTPath;
        public PluginsInfo(string Path)
        {
            InitializeComponent();
            VSTPath = Path;
            oriColor = panel1.BackColor;
            wcc = new WindowCaptureChanged();
            wcc.AssignHandle(Handle);
            wcc.OnCaptureChanged +=
                      new CaptureChanged(CaptureChangedEventHandler);
        }
        VstViewer vstViewer;
        private void button1_Click(object sender, EventArgs e)
        {
            vstViewer.Show();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (vstViewer != null) vstViewer.Dispose();
            vstViewer = null;
            this.Parent.Controls.Remove(this);
        }

        private void PluginsInfo_Load(object sender, EventArgs e)
        {
            vstViewer = new VstViewer(VSTPath);
        }
        int MixerChannel;
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            MixerChannel = Convert.ToInt32(numericUpDown1.Value);
        }
        Color oriColor;
        private void PluginsInfo_Click(object sender, EventArgs e)
        {
            RMVN_Studio.Properties.Settings.Default.MixerChoose = MixerChannel;
            RMVN_Studio.Properties.Settings.Default.Save();
            panel1.BackColor = Color.Yellow;
        }

        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            panel1.BackColor = oriColor;
        }
    }
}
