using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using JAIMaker_2.JAIDSP2;

namespace JAIMaker_2
{
    class MidiKeyboard
    {
        InputDevice midiDevice;
        public bool[] keyState = new bool[0xFF];
        public MidiKeyboard()
        {
            var devices = InputDevice.GetAll();
            foreach (InputDevice dev in devices)
            {
                midiDevice = dev;
            }
            if (midiDevice == null)
                return;
            else
                Console.WriteLine($"Got MIDI Device { midiDevice.Name}");
            
            midiDevice.EventReceived += OnEventReceived;
            midiDevice.StartEventsListening();
        }

        private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            if (e.Event.EventType==MidiEventType.NoteOn)
            {
                NoteOnEvent ev = (NoteOnEvent)e.Event;
                keyTrigger(ev.NoteNumber, ev.Velocity > 0);
            }
            else if (e.Event.EventType == MidiEventType.NoteOff)
            {
                NoteOffEvent ev = (NoteOffEvent)e.Event;
                keyTrigger(ev.NoteNumber, false);
            }
        }

        private JAIDSPVoice[] voices = new JAIDSPVoice[0x88];
        private void keyTrigger(int key, bool state)
        {
            //Console.WriteLine($"Piano key pressed {key} - {state}");

            keyState[key] = state;
            var bnk = JAIMAKER.AAF.InstrumentBanks[JAIMAKER.Project.SelectedBank];
            var ins = bnk.instruments[JAIMAKER.Project.SelectedInstrument];
            if (ins == null)
                return;

            var insD = (JAIM.JStandardInstrumentv1)ins;
            var keyR = insD.getKeyRegion(key);
            if (keyR == null)
                return;
            var velR = keyR.getVelocity(127);
            if (velR == null)
                return;


            if (state == false)
            {
                if (voices[key] != null)
                {
                    var v = voices[key];
                    voices[key] = null;
                    v.stop();
                }
            }
            else
            {
                if (voices[key] != null)
                {
                    var v = voices[key];
                    voices[key] = null;
                    v.stop();
                }

                var snd = JAIMAKER.SoundManager.loadSound((byte)velR.WSYSID, (short)velR.WaveID);
                if (snd == null || snd.buffer == null)
                {
                    Console.WriteLine($"PianoBoy::keyTrigger failed to create sound buffer for bnk.{velR.WSYSID} wav.{velR.WaveID}");
                    return;
                }
                var voice = new JAIDSPVoice(snd.buffer, insD);
                JAIDSPVoiceManager.addVoice(voice);
                voice.play();
                voices[key] = voice;
                voice.setPitchMatrix(0, (float)Math.Pow(2, (key - snd.descriptor.BaseKey) / 12f) * insD.Pitch * velR.Pitch);
            }

        }
        ~MidiKeyboard()
        {
            if (midiDevice != null)
                midiDevice.StopEventsListening();
        }
    }
}
