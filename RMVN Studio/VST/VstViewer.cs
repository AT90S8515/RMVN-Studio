using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Jacobi.Vst.Core.Host;
using Jacobi.Vst.Interop.Host;
using NAudio.Wave.Asio;
using NAudio.Midi;
using NAudio.Mixer;
using NAudio.Wave;
using System.Runtime.InteropServices;
using Jacobi.Vst.Core;
using System.Xml;
using System.IO;
using Jacobi.Vst.Framework;

namespace RMVN_Studio.VST
{
    public partial class VstViewer : Form
    {
        public static VstViewer vstFormSingleton = null;
        public bool doGUIRefresh = true;
        public VSTController vst;
        public VstViewer(string VSTPath)
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
        private void VSTViewer_Load(object sender, EventArgs e)
        {
			
        }
        private void VSTViewer_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (vst != null && doGUIRefresh && Visible)
            {
                vst.PluginContext.PluginCommandStub.EditorIdle();
            }
        }
        private void VstViewer_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            const byte midiVelocity = 100;
            byte midiNote = KeyEventArgToMidiNote(e);

            if (vst != null && midiNote != 0)
            {
                vst.MIDI_NoteOn(midiNote, midiVelocity);
			}
        }

        private void VstViewer_KeyUp(object sender, KeyEventArgs e)
        {
			e.SuppressKeyPress = true;

            const byte midiVelocity = 0;
            byte midiNote = KeyEventArgToMidiNote(e);

			if (vst != null && midiNote != 0)
            {
                vst.MIDI_NoteOn(midiNote, midiVelocity);
			}
        }

		private byte KeyEventArgToMidiNote(KeyEventArgs e)
		{
			/*
			 * You can use the keyboard of your computer to play notes in a 2-octave range.
			 * It starts from Z at C3 and goes horizontally across the bottom 2 rows of the keyboard to M playing B3
			 * (i.e. Z X C V B N M play the natural notes and S D G H J play the sharps and flats).
			 * It continues with Q playing C4 and uses the top 2 rows of the keyboard to P playing E5
			 * (i.e. Q W E R T Y U I O P play the natural notes and 2 3 5 6 7 9 0 play the sharps and flats).
			 * http://lmms.sourceforge.net/wiki/index.php/File:Keyboard-pianoKeys1.png
			 * http://highlyliquid.com/support/library/midi-note-numbers/
			 * http://vstnet.codeplex.com/discussions/234945
			 */

			byte midiNote = 0;
			try
			{
				switch (e.KeyCode)
				{
					case Keys.Z:
						// C3
						midiNote = 48;
						break;
					case Keys.S:
						// C#3
						midiNote = 49;
						break;
					case Keys.X:
						// D3
						midiNote = 50;
						break;
					case Keys.D:
						// D#3
						midiNote = 51;
						break;
					case Keys.C:
						// E3
						midiNote = 52;
						break;
					case Keys.V:
						// F3
						midiNote = 53;
						break;
					case Keys.G:
						// F#3
						midiNote = 54;
						break;
					case Keys.B:
						// G3
						midiNote = 55;
						break;
					case Keys.H:
						// G#3
						midiNote = 56;
						break;
					case Keys.N:
						// A3
						midiNote = 57;
						break;
					case Keys.J:
						// A#3
						midiNote = 58;
						break;
					case Keys.M:
						// B3
						midiNote = 59;
						break;

					case Keys.Q:
						// C4
						midiNote = 60;
						break;
					case Keys.D2:
						// C#4
						midiNote = 61;
						break;
					case Keys.W:
						// D4
						midiNote = 62;
						break;
					case Keys.D3:
						// D#4
						midiNote = 63;
						break;
					case Keys.E:
						// E4
						midiNote = 64;
						break;
					case Keys.R:
						// F4
						midiNote = 65;
						break;
					case Keys.D5:
						// F#4
						midiNote = 66;
						break;
					case Keys.T:
						// G4
						midiNote = 67;
						break;
					case Keys.D6:
						// G#4
						midiNote = 68;
						break;
					case Keys.Y:
						// A4
						midiNote = 69;
						break;
					case Keys.D7:
						// A#4
						midiNote = 70;
						break;
					case Keys.U:
						// B4
						midiNote = 71;
						break;

					case Keys.I:
						// C4
						midiNote = 72;
						break;
					case Keys.D9:
						// C#4
						midiNote = 73;
						break;
					case Keys.O:
						// D4
						midiNote = 74;
						break;
					case Keys.D0:
						// D#4
						midiNote = 75;
						break;
					case Keys.P:
						// E4
						midiNote = 76;
						break;
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return midiNote;
		}

        private void VstViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
			e.Cancel = true;
			this.Hide();
		}
    }
}
