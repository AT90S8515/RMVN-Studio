using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.Devices;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Jacobi.Vst;
using Jacobi.Vst.Core.Host;
using Jacobi.Vst.Framework.Common;
using Jacobi.Vst.Interop.Host;
using RMVN_Studio.VST;
using NAudio.Wave.Asio;
using System.Runtime.InteropServices;
using RMVN_Studio.Windows;

namespace RMVN_Studio
{
    public partial class MainApp : Form
    {
        private List<VstPluginContext> _plugins = new List<VstPluginContext>();

        private void FillPluginList()
        {
            PluginListVw.Items.Clear();

            foreach (VstPluginContext ctx in _plugins)
            {
                ListViewItem lvItem = new ListViewItem(ctx.PluginCommandStub.GetEffectName());
                lvItem.SubItems.Add(ctx.PluginCommandStub.GetProductString());
                lvItem.SubItems.Add(ctx.PluginCommandStub.GetVendorString());
                lvItem.SubItems.Add(ctx.PluginCommandStub.GetVendorVersion().ToString());
                lvItem.SubItems.Add(ctx.Find<string>("PluginPath"));
                lvItem.Tag = ctx;

                PluginListVw.Items.Add(lvItem);
            }
        }
        public MainApp()
        {
            InitializeComponent();
            treeView1.AllowDrop = true;
            this.AllowDrop = true;
        }
        private void CloseBtn_Click(object sender, EventArgs e)
        {
            CloseBtn.BackColor = Color.GhostWhite;
            Application.Exit();
            CloseBtn.BackColor = BackupColor;
        }
        private VstPluginContext OpenPlugin(string pluginPath)
        {
            try
            {
                HostCommandStub hostCmdStub = new HostCommandStub();
                hostCmdStub.PluginCalled += new EventHandler<PluginCalledEventArgs>(HostCmdStub_PluginCalled);

                VstPluginContext ctx = VstPluginContext.Create(pluginPath, hostCmdStub);

                ctx.Set("PluginPath", pluginPath);
                ctx.Set("HostCmdStub", hostCmdStub);

                ctx.PluginCommandStub.Open();

                return ctx;
            }
            catch (Exception){ }

            return null;
        }

        private void HostCmdStub_PluginCalled(object sender, PluginCalledEventArgs e)
        {
            HostCommandStub hostCmdStub = (HostCommandStub)sender;

            if (hostCmdStub.PluginContext.PluginInfo != null)
            {
                Debug.WriteLine("Plugin " + hostCmdStub.PluginContext.PluginInfo.PluginID + " called:" + e.Message);
            }
            else
            {
                Debug.WriteLine("The loading Plugin called:" + e.Message);
            }
        }
        public void SearchPlugin64bit(string dir)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(dir);
            FileInfo[] directory = directoryInfo.GetFiles("*.dll", SearchOption.AllDirectories);
            foreach (var file in directory)
            {
                AddPlugin(file.FullName.ToString(), true);
            }
        }
        void AddPlugin(string dir, bool is64)
        {
            VstPluginContext ctx = OpenPlugin(dir);

            if (ctx != null)
            {
                _plugins.Add(ctx);
                FillPluginList();
            }
        }

        private VstPluginContext PluginContext;

        Image minImg;
        Image maxImg;
        private void MinMaxBtn_Click(object sender, EventArgs e)
        {
            MinMaxBtn.BackColor = Color.GhostWhite;
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                MinMaxBtn.Image = maxImg;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                MinMaxBtn.Image = minImg;
            }
            MinMaxBtn.BackColor = BackupColor;
        }

        private void MinBtn_Click(object sender, EventArgs e)
        {
            MinBtn.BackColor = Color.GhostWhite;
            this.WindowState = FormWindowState.Minimized;
            MinBtn.BackColor = BackupColor;
        }
        Color BackupColor;
        private void CloseBtn_MouseHover(object sender, EventArgs e)
        {
            CloseBtn.BackColor = Color.WhiteSmoke;
        }

        private void CloseBtn_MouseLeave(object sender, EventArgs e)
        {
            CloseBtn.BackColor = BackupColor;
        }
        int mov;
        int movX;
        int movY;
        Setting settingForm;
        byte[] fileBefore;
        byte[] fileAfter;
        LoadApp loadApp;
        Image OnImg;
        Image OffImg;
        Plugins plugins;
        Mixer mixer;
        private void MainApp_Load(object sender, EventArgs e)
        {
            timer2.Stop();
            timer1.Stop();
            this.Hide();
            loadApp = new LoadApp();
            loadApp.Show();
            mixer = new Mixer();
            plugins = new Plugins();
            PluginsList.Controls.Add(plugins);
            BPM = 128;
            plugins.Dock = DockStyle.Fill;
            if (RMVN_Studio.Properties.Settings.Default.ASIODriverName == string.Empty)
            {
                string[] asioList = AsioDriver.GetAsioDriverNames();
                RMVN_Studio.Properties.Settings.Default.ASIODriverName = asioList[0];
                RMVN_Studio.Properties.Settings.Default.AsioDriverIndex = 0;
            }
            BackupColor = CloseBtn.BackColor;
            minImg = new Bitmap(RMVN_Studio.Properties.Resources.Min2);
            maxImg = new Bitmap(RMVN_Studio.Properties.Resources.max);
            OnImg = new Bitmap(RMVN_Studio.Properties.Resources.On);
            OffImg = new Bitmap(RMVN_Studio.Properties.Resources.off);
            this.Location = Screen.AllScreens[0].WorkingArea.Location;
            Process thisApp = Process.GetCurrentProcess();
            cpuCounter = new PerformanceCounter("Process", "% Processor Time", thisApp.ProcessName, true);
            settingForm = new Setting();
            var lines = File.ReadAllLines(@".\Config\listPath.txt");
            if (lines != null)
            {
                foreach (string line in lines)
                {
                    LoadDirectory(line);
                }
            }

            var lines1 = File.ReadAllLines(@".\Config\listPathVST.txt");
            if (lines1 != null)
            {
                foreach (string line in lines1)
                {
                    SearchPlugin64bit(line);
                }
            }
            fileBefore = this.FileToBytes(@".\Config\listPath.txt");
            fileBefore1 = this.FileToBytes(@".\Config\listPathVST.txt");

            loadApp.Hide();
            this.Show();
            timer1.Start();
            timer2.Start();
        }

        private void AsioOut_AudioAvailable(object sender, AsioAudioAvailableEventArgs e)
        {
            throw new NotImplementedException();
        }

        private byte[] FileToBytes(string filePath)
        {
            using (FileStream fsSource = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fsSource.Length];
                int numBytesToRead = (int)fsSource.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);
                    if (n == 0)
                        break;

                    numBytesRead += n;
                    numBytesToRead -= n;
                }

                return bytes;
            }
        }

        private bool FilesTheSame(byte[] b1, byte[] b2)
        {
            if (b1.LongLength == b2.LongLength)
            {
                for (long l = 0; l < b1.LongLength; l++)
                {
                    if (b1[l] != b2[l])
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void MinMaxBtn_MouseHover(object sender, EventArgs e)
        {
            MinMaxBtn.BackColor = Color.WhiteSmoke;
        }

        private void MinMaxBtn_MouseLeave(object sender, EventArgs e)
        {
            MinMaxBtn.BackColor = BackupColor;
        }

        private void MinBtn_MouseHover(object sender, EventArgs e)
        {
            MinBtn.BackColor = Color.WhiteSmoke;
        }

        private void MinBtn_MouseLeave(object sender, EventArgs e)
        {
            MinBtn.BackColor = BackupColor;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mov = 1;
            movX = e.X;
            movY = e.Y;
        }

        private void BorderRight_MouseDown(object sender, MouseEventArgs e)
        {
            mov = 1;
            movX = e.X;
            movY = e.Y;
        }

        private void BorderLeft_MouseDown(object sender, MouseEventArgs e)
        {
            mov = 1;
            movX = e.X;
            movY = e.Y;
        }

        private void BorderBottom_MouseDown(object sender, MouseEventArgs e)
        {
            mov = 1;
            movX = e.X;
            movY = e.Y;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mov == 1)
            {
                this.SetDesktopLocation(MousePosition.X - movX, MousePosition.Y - movY);
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            mov = 0;
        }

        private void BorderRight_MouseMove(object sender, MouseEventArgs e)
        {
            if (mov == 1)
            {
                this.SetDesktopLocation(MousePosition.X - movX - this.Width, MousePosition.Y - movY);
            }
        }

        private void BorderRight_MouseUp(object sender, MouseEventArgs e)
        {
            mov = 0;
        }

        private void BorderBottom_MouseMove(object sender, MouseEventArgs e)
        {
            if (mov == 1)
            {
                this.SetDesktopLocation(MousePosition.X - movX, MousePosition.Y - movY - this.Height);
            }
        }

        private void BorderBottom_MouseUp(object sender, MouseEventArgs e)
        {
            mov = 0;
        }

        private void BorderLeft_MouseMove(object sender, MouseEventArgs e)
        {
            if (mov == 1)
            {
                this.SetDesktopLocation(MousePosition.X - movX, MousePosition.Y - movY);
            }
        }
        private void BorderLeft_MouseUp(object sender, MouseEventArgs e)
        {
            mov = 0;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        PerformanceCounter cpuCounter;
        private void timer1_Tick(object sender, EventArgs e)
        {
            CPUUsage.Value = (int)Math.Round(cpuCounter.NextValue());
            PercentCPUUsage.Text = CPUUsage.Value + "%";
            ComputerInfo info = new ComputerInfo();
            Process thisApp = Process.GetCurrentProcess();
            long UsageRamByApp = thisApp.PrivateMemorySize64;
            long UsageRaminMB = UsageRamByApp / 1048576;
            int ramPercent = (int)Math.Round(((decimal)UsageRaminMB / (info.TotalPhysicalMemory / 1024 / 1024)) * 100);
            RamUsageBar.Value = ramPercent;
            RamUsage.Text = UsageRaminMB + "MB";
        }

        public void LoadDirectory(string path)
        {
            if (path != "" && Directory.Exists(path))
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    TreeNode tds = treeView1.Nodes.Add(di.FullName);
                    tds.Tag = di.FullName;
                    tds.StateImageIndex = 0;
                    LoadFiles(path, tds);
                    LoadSubDirectories(path, tds);
                } catch (Exception ex)
                {
                    MessageBox.Show("Access is denied to folder " + path + "\n You can try open this DAW in administrator!\n Full Error:" + ex, "Fatal Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadSubDirectories(string dir, TreeNode td)
        {
            string[] subdirectoryEntries = Directory.GetDirectories(dir);
            foreach (string subdirectory in subdirectoryEntries)
            {
                DirectoryInfo di = new DirectoryInfo(subdirectory);
                TreeNode tds = td.Nodes.Add(di.Name);
                tds.StateImageIndex = 0;
                tds.ImageIndex = 0;
                tds.Tag = di.FullName;
                LoadFiles(subdirectory, tds);
                LoadSubDirectories(subdirectory, tds);
            }
        }

        public void Reload()
        {
            var lines = File.ReadAllLines(@".\Config\listPath.txt");
            if (lines != null)
            {
                treeView1.Nodes.Clear();
                foreach (string line in lines)
                {
                    LoadDirectory(line);
                }
            }
        }

        private static int lastTick;
        private static int lastFrameRate;
        private static int frameRate;

        public int CalculateFrameRate()
        {
            if (System.Environment.TickCount - lastTick >= 1000)
            {
                lastFrameRate = frameRate;
                frameRate = 0;
                lastTick = System.Environment.TickCount;
            }
            frameRate++;
            return lastFrameRate;
        }

        public float DeltaTime()
        {
            return 2 / (CalculateFrameRate() / 60);
        }
        private void LoadFiles(string dir, TreeNode td)
        {
            string[] WavFiles = Directory.GetFiles(dir, "*.wav");
            string[] MP3Files = Directory.GetFiles(dir, "*.mp3");
            string[] MidiFiles = Directory.GetFiles(dir, "*.mid");
            string[] FlacFiles = Directory.GetFiles(dir, "*.flac");
            string[] ProjectFiles = Directory.GetFiles(dir, "*.rmsp");
            string[] VSTPresetFiles = Directory.GetFiles(dir, "*.fxp");
            string[] VSTPresetFiles1 = Directory.GetFiles(dir, "*.fxb");
            string[] Files = WavFiles.Concat(MP3Files).ToArray().Concat(MidiFiles).ToArray().Concat(MidiFiles).ToArray().Concat(FlacFiles).ToArray();
            string[] FilesNone = ProjectFiles.Concat(VSTPresetFiles).Concat(VSTPresetFiles1).ToArray();
            foreach (string file in Files)
            {
                FileInfo fi = new FileInfo(file);
                TreeNode tds = td.Nodes.Add(fi.Name);
                tds.Tag = fi.FullName;
                tds.ImageIndex = 1;
                tds.StateImageIndex = 1;
            }
            foreach (string file in FilesNone)
            {
                FileInfo fi = new FileInfo(file);
                TreeNode tds = td.Nodes.Add(fi.Name);
                tds.Tag = fi.FullName;
                tds.ImageIndex = 2;
                tds.StateImageIndex = 2;
            }
        }

        private void generalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settingForm.ShowDialog();
            settingForm.tabControl1.SelectedIndex = 0;
        }

        private void audioSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settingForm.ShowDialog();
            settingForm.tabControl1.SelectedIndex = 1;
        }

        private void fileSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settingForm.ShowDialog();
            settingForm.tabControl1.SelectedIndex = 2;
        }

        private void midiSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settingForm.ShowDialog();
            settingForm.tabControl1.SelectedIndex = 3;
        }

        private void pluginSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settingForm.ShowDialog();
            settingForm.tabControl1.SelectedIndex = 4;
        }

        private void projectSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settingForm.ShowDialog();
            settingForm.tabControl1.SelectedIndex = 5;
        }
        byte[] fileAfter1;
        byte[] fileBefore1;
        private void timer2_Tick(object sender, EventArgs e)
        {
            fileAfter = this.FileToBytes(@".\Config\listPath.txt");
            if (!this.FilesTheSame(fileBefore, fileAfter))
            {
                Reload();
                fileBefore = this.FileToBytes(@".\Config\listPath.txt");
            }
            fileAfter1 = this.FileToBytes(@".\Config\listPathVST.txt");
            if (!this.FilesTheSame(fileBefore1, fileAfter1))
            {
                ReloadVST();
                fileBefore1 = this.FileToBytes(@".\Config\listPathVST.txt");
            }

        }
        private void ReloadVST()
        {
            var lines = File.ReadAllLines(@".\Config\listPathVST.txt");
            if (lines != null)
            {
                PluginListVw.Clear();
                foreach (string line in lines)
                {
                    SearchPlugin64bit(line);
                }
            }
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            
        }
        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            
        }
        private void treeView1_DragOver(object sender, DragEventArgs e)
        {

        }
        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {

        }
        string pathAudioFile;
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (timer3.Enabled)
            {
                timer3.Stop();
            }
            pathAudioFile = e.Node.FullPath;
            try
            {
                if (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Stop();
                }
                if (pathAudioFile.ToLower().Contains(".wav"))
                {
                    waveViewer1.WaveStream = new WaveFileReader(pathAudioFile);
                    waveViewer1.SamplesPerPixel = 400;
                    waveOut = new WaveOutEvent();
                    WaveFileReader wavReader = new WaveFileReader(pathAudioFile);
                    waveOut.Init(wavReader);
                    waveOut.Play();
                    timer3.Start();
                }
                else if (pathAudioFile.ToLower().Contains(".mp3"))
                {
                    waveViewer1.WaveStream = new Mp3FileReader(pathAudioFile);
                    waveOut = new WaveOutEvent();
                    Mp3FileReader mp3Reader = new Mp3FileReader(pathAudioFile);
                    waveOut.Init(mp3Reader);
                    waveOut.Play();
                    timer3.Start();
                }
            }
            catch (Exception) { }
        }
        WaveOutEvent waveOut;
        private void timer3_Tick(object sender, EventArgs e)
        {
            waveOut.Stop();
            timer3.Stop();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        void InitTrackList(string trackName)
        {
            
        }

        private void MuteUnmuteBtn_Click(object sender, EventArgs e)
        {
            MuteUnmute(1, (PictureBox)sender);
        }

        void MuteUnmute(int trackInt, PictureBox Btn)
        {
            if (Btn.Image == OnImg)
            {
                Btn.Image = OffImg;
                //mute track int
            }
            else
            {
                Btn.Image = OnImg;
                //unmute track int
            }
        }
        bool isEffect;
        private void PluginListVw_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (PluginListVw.SelectedItems.Count > 0)
                { 
                    PluginContext = (VstPluginContext)PluginListVw.SelectedItems[0].Tag;
                }
                else
                {
                    PluginContext = null;
                }
                string Category = PluginContext.PluginCommandStub.GetCategory().ToString();

                if (Category != "Effect") isEffect = false;
                else isEffect = true;
                if (isEffect == false) plugins.AddPlugins(PluginContext);
                else
                {

                }
            }
            catch (Exception) {}
        }
        public double BPM;
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            BPM = Convert.ToDouble(numericUpDown1.Value);
            RMVN_Studio.Properties.Settings.Default.BPM = BPM;
            RMVN_Studio.Properties.Settings.Default.Save();
        }


        private void treeView1_MouseUp(object sender, MouseEventArgs e)
        {
            
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
           
        }

        private void MainApp_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void AllTrack_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            mixer.Show();
        }
    }
}
