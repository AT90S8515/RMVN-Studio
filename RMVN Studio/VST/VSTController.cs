using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonUtils.VSTPlugin;
using Jacobi.Vst.Core;
using Jacobi.Vst.Core.Host;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Interop.Host;
using NAudio.Wave;


namespace RMVN_Studio.VST
{
    public class VSTController
    {
        public VstPluginContext PluginContext { get; set; }
        public event EventHandler<VSTStreamEventArgs> StreamCall = null;
		public void MIDI_NoteOn(byte Note, byte Velocity)
		{
			byte Cmd = 0x90;
			MIDI(Cmd, Note, Velocity);
		}
		internal void DisposeVST()
		{
			if (PluginContext != null) PluginContext.Dispose();
		}

		public void MIDI_CC(byte Number, byte Value)
		{
			byte Cmd = 0xB0;
			MIDI(Cmd, Number, Value);
		}

		private void MIDI(byte Cmd, byte Val1, byte Val2)
		{
			var midiData = new byte[4];
			midiData[0] = Cmd;
			midiData[1] = Val1;
			midiData[2] = Val2;
			midiData[3] = 0;    // Reserved, unused

			var vse = new VstMidiEvent(/*DeltaFrames*/ 0,
									   /*NoteLength*/ 0,
									   /*NoteOffset*/  0,
									   midiData,
									   /*Detune*/        0,
									   /*NoteOffVelocity*/ 127);

			var ve = new VstEvent[1];
			ve[0] = vse;

			PluginContext.PluginCommandStub.ProcessEvents(ve);
		}

		internal void Stream_ProcessCalled(object sender, VSTStreamEventArgs e)
		{
			if (StreamCall != null) StreamCall(sender, e);
		}
	}
}
