using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Wave.Asio;

namespace RMVN_Studio
{
    public partial class Setting : Form
    {
        void GetAsioDriverList()
        {
            string[] asioDriverList = AsioDriver.GetAsioDriverNames();
            foreach(string asioName in asioDriverList)
            {
                comboBox1.Items.Add(asioName);
            }

        }
        public Setting()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Add();
        }

        public void Add()
        {
            if (textBox1.Text != string.Empty)
            {
                listPath.Items.Add(new ListViewItem(textBox1.Text));
                Save();
            }
            else
            {
                MessageBox.Show("Path is null or incorrect!", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }
        }

        public void ChooseFolder()
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath) && Path.GetPathRoot(folderBrowserDialog1.SelectedPath) != folderBrowserDialog1.SelectedPath)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
            else
            {
                DialogResult dl = MessageBox.Show("You need choose a correct folder!", "Error!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (dl == System.Windows.Forms.DialogResult.Retry)
                {
                    ChooseFolder();
                }
                else
                {
                    return;
                }
            }
        }

        public void ChooseFolderVST()
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath) && Path.GetPathRoot(folderBrowserDialog1.SelectedPath) != folderBrowserDialog1.SelectedPath)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
            else
            {
                DialogResult dl = MessageBox.Show("You need choose a correct folder!", "Error!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (dl == System.Windows.Forms.DialogResult.Retry)
                {
                    ChooseFolderVST();
                }
                else
                {
                    return;
                }
            }
        }

        public void Save()
        {
            using (var tw = new StreamWriter(@".\Config\listPath.txt"))
            {
                foreach (ListViewItem item in listPath.Items)
                {
                    tw.WriteLine(item.Text);
                }
            }
        }

        public void SavePluginPath()
        {
            using (var tw = new StreamWriter(@".\Config\listPathVST.txt"))
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    tw.WriteLine(item.Text);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Remove();
        }
        public void Remove()
        {
            foreach (ListViewItem eachItem in listPath.SelectedItems)
            {
                if (eachItem.Selected)
                {
                    listPath.Items.Remove(eachItem);
                    Save();
                }
                else
                {
                    MessageBox.Show("No selected item!", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                }
            }
        }
        AsioOut asioOut;
        private void Setting_Load(object sender, EventArgs e)
        {
            timer1.Stop();
            asioOut = new AsioOut(RMVN_Studio.Properties.Settings.Default.ASIODriverName);
            GetAsioDriverList();
            comboBox1.SelectedIndex = RMVN_Studio.Properties.Settings.Default.AsioDriverIndex;
            //asioOut.InitRecordAndPlayback(null, 1, 44100);
            var lines = File.ReadAllLines(@".\Config\listPath.txt");
            if (lines != null)
            {
                foreach (string line in lines)
                {
                    listPath.Items.Add(new ListViewItem(line));
                }
            }

            var lines1 = File.ReadAllLines(@".\Config\listPathVST.txt");
            if (lines1 != null)
            {
                foreach (string line in lines1)
                {
                    listView1.Items.Add(new ListViewItem(line));
                }
            }
            timer1.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ChooseFolder();
        }

        private void OKFile_Click(object sender, EventArgs e)
        {
            RMVN_Studio.Properties.Settings.Default.Save();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ChooseFolderVST();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != string.Empty)
            {
                listView1.Items.Add(new ListViewItem(textBox2.Text));
                SavePluginPath();
            }
            else
            {
                MessageBox.Show("Path is null or incorrect!", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem eachItem in listView1.SelectedItems)
            {
                if (eachItem.Selected)
                {
                    listView1.Items.Remove(eachItem);
                    SavePluginPath();
                }
                else
                {
                    MessageBox.Show("No selected item!", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RMVN_Studio.Properties.Settings.Default.ASIODriverName = comboBox1.GetItemText(comboBox1.SelectedItem);
            RMVN_Studio.Properties.Settings.Default.AsioDriverIndex = comboBox1.SelectedIndex;
            if (asioOut != null)
            asioOut = null;
            asioOut = new AsioOut(RMVN_Studio.Properties.Settings.Default.ASIODriverName);
           //asioOut.InitRecordAndPlayback(null, 1, 44100);
        }
        private void ShowASIOPanel_Click(object sender, EventArgs e)
        {
            ShowASIOCPanel();
        }
        [STAThread]
        public void ShowASIOCPanel()
        {
            asioOut.ShowControlPanel();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }
        public void GetBufferASIO()
        {
            if (asioOut == null)
            {
                asioOut = new AsioOut(RMVN_Studio.Properties.Settings.Default.ASIODriverName);
                //asioOut.InitRecordAndPlayback(null, 1, 44100);
            } else
            {
                asioOut = null;
                asioOut = new AsioOut(RMVN_Studio.Properties.Settings.Default.ASIODriverName);
                //asioOut.InitRecordAndPlayback(null, 1, 44100);
            }
            Buffer.Text = "Buffer: " + asioOut.FramesPerBuffer;
        }
    }
}
