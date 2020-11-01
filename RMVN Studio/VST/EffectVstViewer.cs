using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RMVN_Studio.VST
{
    public partial class EffectVstViewer : Form
    {
        VSTController vst;
        public static EffectVstViewer vstFormSingleton = null;
        public bool doGUIRefresh = true;
        EffectVSTController EffectVst;
        public EffectVstViewer(string VSTPath)
        {
            vstFormSingleton = this;
            UtilityAudio.OpenAudio(AudioLibrary.NAudio, RMVN_Studio.Properties.Settings.Default.ASIODriverName);
            InitializeComponent();
            vst = UtilityAudio.LoadVST(VSTPath, this.Handle);
            this.Text = vst.PluginContext.PluginCommandStub.GetEffectName();
            var rect = new Rectangle();
            vst.PluginContext.PluginCommandStub.EditorGetRect(out rect);
            this.SetClientSizeCore(rect.Width, rect.Height);
            UtilityAudio.StartAudio();
        }

        public new void Dispose()
        {
            UtilityAudio.DisposeVST();
            vst = null;
            base.Dispose();

            vstFormSingleton = null;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (vst != null && doGUIRefresh && Visible)
            {
                vst.PluginContext.PluginCommandStub.EditorIdle();
            }
        }

        private void EffectVstViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
