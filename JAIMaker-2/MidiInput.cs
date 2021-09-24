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
    class MidiInput
    {
        InputDevice midiDevice;
        public bool[] keyState = new bool[0xFF];
        public JAIProgramRemap[] Channels = new JAIProgramRemap[17]; // 0-16 
        public Dictionary<int, JAIDSPVoice>[] Voices = new Dictionary<int, JAIDSPVoice>[17];
        public bool Ready = false;

        public MidiInput(InputDevice MidiDevice)
        {
            midiDevice = MidiDevice;
            if (midiDevice == null)
                return;
            else
                Console.WriteLine($"Got MIDI Device { midiDevice.Name}");
            
            midiDevice.EventReceived += OnEventReceived;
            midiDevice.StartEventsListening();

            for (int i = 0; i < Channels.Length; i++) {
                Channels[i] = new JAIProgramRemap();
                Voices[i] = new Dictionary<int, JAIDSPVoice>();

            }

            Ready = true;
        }

        private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            if (e.Event.EventType==MidiEventType.NoteOn)
            {
                NoteOnEvent ev = (NoteOnEvent)e.Event;
                keyTrigger(ev.NoteNumber, ev.Velocity > 0, ev.Velocity, ev.Channel);
            }
            else if (e.Event.EventType == MidiEventType.NoteOff)
            {
                NoteOffEvent ev = (NoteOffEvent)e.Event;
                keyTrigger(ev.NoteNumber, false, ev.Velocity, ev.Channel);
            }
        }

        private void keyTrigger(int key, bool state, int velocity, int channel)
        {
            if (JAIMAKER.AAF == null) // Don't do a thing until we have data to deal with.
                return; 
            var voiceContainer = Voices[channel];
            if (voiceContainer == null)
                return;
            JAIDSPVoice currentVoice;
            if (voiceContainer.TryGetValue(key, out currentVoice))
            {
                currentVoice.stop();
                voiceContainer.Remove(key);
            }
            keyState[key] = state;
            if (state == false) // We're done here.
                return;

            var bankProgConfig = Channels[channel];

            var bank = JAIMAKER.AAF.getBankID(bankProgConfig.bank);
            if (bank == null)
                return;
            var ins = bank.instruments[bankProgConfig.program];

            if (ins == null)
                return;
           
            if (ins.Percussion==false)
            {
                var insD = (JAIM.JStandardInstrumentv1)ins;
                var keyR = insD.getKeyRegion(key);
                if (keyR == null)
                    return;
                var velR = keyR.getVelocity(velocity);
                if (velR == null)
                    return;

                var snd = JAIMAKER.SoundManager.loadSound((byte)velR.WSYSID, (short)velR.WaveID);
                if (snd == null || snd.buffer == null)
                {
                    Console.WriteLine($"PianoBoy::keyTrigger failed to create sound buffer for bnk.{velR.WSYSID} wav.{velR.WaveID}");
                    return;
                }
                var voice = new JAIDSPVoice(snd.buffer, insD);
                JAIDSPVoiceManager.addVoice(voice);
                voice.play();
                voiceContainer[key] = voice;
                voice.setPitchMatrix(0, (float)Math.Pow(2, (key - snd.descriptor.BaseKey) / 12f) * insD.Pitch * velR.Pitch);
                voice.setVolMatrix(0, velocity / 128f);
            } else
            {
                var insD = (JAIM.JPercussion)ins;
                if (key >= 100)
                    return; // no
                var percE = insD.Sounds[key];
                if (percE == null)
                    return;
                var velR = percE.getVelocity(velocity);
                if (velR == null)
                    return;
                var snd = JAIMAKER.SoundManager.loadSound((byte)velR.WSYSID, (short)velR.WaveID);
                if (snd == null || snd.buffer == null)
                {
                    Console.WriteLine($"PianoBoy::keyTrigger failed to create sound buffer for bnk.{velR.WSYSID} wav.{velR.WaveID}");
                    return;
                }
                var voice = new JAIDSPVoice(snd.buffer, insD);
                JAIDSPVoiceManager.addVoice(voice);
                voice.play();
                voiceContainer[key] = voice;
                voice.setPitchMatrix(0, velR.Pitch);
                voice.setVolMatrix(0, velR.Volume * (velocity / 128f));
            }
        }
        ~MidiInput()
        {
            if (midiDevice != null)
                midiDevice.StopEventsListening();
        }
    }
}
