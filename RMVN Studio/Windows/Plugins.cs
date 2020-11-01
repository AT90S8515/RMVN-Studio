using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Jacobi.Vst.Core.Host;
using Jacobi.Vst.Interop.Host;

namespace RMVN_Studio.Windows
{
    public partial class Plugins : UserControl
    {
        public Plugins()
        {
            InitializeComponent();
        }
        PluginsInfo pluginsInfo;
        public void AddPlugins(VstPluginContext pluginContext)
        {
            string path = pluginContext.Find<string>("PluginPath");
            pluginsInfo = new PluginsInfo(path);
            pluginsInfo.label1.Text = pluginContext.PluginCommandStub.GetEffectName();
            pluginsInfo.Dock = DockStyle.Left;
            this.Controls.Add(pluginsInfo);
            this.AutoScroll = false;
            this.VerticalScroll.Enabled = false;
            this.VerticalScroll.Visible = false;
            this.VerticalScroll.Maximum = 0;
            this.AutoScroll = true;
        }
    }
}
