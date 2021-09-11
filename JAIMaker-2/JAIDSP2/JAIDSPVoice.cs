using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JAIMaker_2.JAIM;
using Un4seen.Bass;

namespace JAIMaker_2.JAIDSP2
{
    public static class JAIDSPVoiceManager
    {

        public static int treeDepth = 0;
        private static JAIDSPVoice head;
        private static JAIDSPVoice tail;

        public static void addVoice(JAIDSPVoice voi)
        {
            if (head == null)
                head = voi;
            if (tail == null)
                tail = voi;
            if (tail != voi)
            {
                voi.prev = tail;
                tail.next = voi;
                tail = voi;
            }
        }

        public static void removeVoice(JAIDSPVoice voi)
        {
            if (voi == head)
                head = voi.next;
            if (voi == tail)
                tail = voi.prev;
            if (voi.prev != null)
                voi.prev.next = voi.next;
            if (voi.next != null)
                voi.next.prev = voi.prev;
        }

        public static void updateAll()
        {
            treeDepth = 0;
            var current = head;
            while (current != null)
            {
                current.update();
                if (current.Destroy)
                    removeVoice(current);
                current = current.next;
                treeDepth++;
            }
        }

        public static void destroyAll()
        {
            var current = head;
            while (current!=null) { 
                if (!current.Destroy)
                    current.destroy();

                removeVoice(current);
                current = current.next;
            }
        }
    }
  

    public class JAIDSPVoice
    {
        internal JAIDSPVoice next;
        internal JAIDSPVoice prev;

        private const int PITCHMATRIX_SIZE = 3;
        private const int VOLMATRIX_SIZE = 3;

        private float[] _pitchMatrix = new float[PITCHMATRIX_SIZE];
        private float[] _volMatrix = new float[VOLMATRIX_SIZE];

        public float Pitch;
        public float Volume;
        public bool Destroy;

        JInstrument Instrument;
        JAIDSPOscillator Oscillator;

        private int voiceHandle;
        private int syncHandle;

        private JAIDSPSoundBuffer buffer;

        public JAIDSPVoice(JAIDSPSoundBuffer buff, JInstrument Ins)
        {
            for (int i = 0; i < PITCHMATRIX_SIZE; i++)
                _pitchMatrix[i] = 1f;
            for (int i = 0; i < VOLMATRIX_SIZE; i++)
                _volMatrix[i] = 1f;

            Instrument = Ins;
            buffer = buff;
            
            voiceHandle = Bass.BASS_StreamCreateFile(buff.globalFileBuffer, 0, buff.fileBuffer.Length, BASSFlag.BASS_DEFAULT);
            if (buff.looped)
                syncHandle = Bass.BASS_ChannelSetSync(voiceHandle, BASSSync.BASS_SYNC_POS | BASSSync.BASS_SYNC_MIXTIME, buff.loopEnd, JAIDSP.globalLoopProc, new IntPtr(buff.loopStart));

            if (Ins == null)
                return;

            if (Ins.Percussion)
                return;

            var StdIns = ((JStandardInstrumentv1)Ins);
            if (StdIns.oscillatorA != null)
            {
                Oscillator = new JAIDSPOscillator(StdIns.oscillatorA);
                setVolMatrix(2, 0);
            }
        } 

        public void play()
        {
            if (Oscillator != null)
                Oscillator.attack();
            Bass.BASS_ChannelPlay(voiceHandle, false);
        }

        public void stop()
        {
            if (Oscillator == null)
                destroy();
            else
                Oscillator.release();
        }


        public void destroy()
        {
            Bass.BASS_ChannelStop(voiceHandle);
            Bass.BASS_StreamFree(voiceHandle);
            if (buffer.looped)
                Bass.BASS_ChannelRemoveSync(voiceHandle, syncHandle);
            Destroy = true;
        }

        public void setPitchMatrix(byte slot, float value)
        {
            if (slot > PITCHMATRIX_SIZE)
                return;
            Pitch = 1;
            _pitchMatrix[slot] = value;
            for (int i = 0; i < PITCHMATRIX_SIZE; i++)
                Pitch *= _pitchMatrix[i];
            Bass.BASS_ChannelSetAttribute(voiceHandle, BASSAttribute.BASS_ATTRIB_FREQ, buffer.format.sampleRate * Pitch);
        }

        public void setVolMatrix(byte slot, float value)
        {
            if (slot > VOLMATRIX_SIZE)
                return;
            Volume = 1;
            _volMatrix[slot] = value;
            for (int i = 0; i < VOLMATRIX_SIZE; i++)
                Volume *= _volMatrix[i];
            Bass.BASS_ChannelSetAttribute(voiceHandle, BASSAttribute.BASS_ATTRIB_VOL, Volume);
        }

       
        public void update()
        {
            if (Oscillator != null) {
                Oscillator.update();
                setVolMatrix(2, Oscillator.Value);
                if (Oscillator.Stop)
                    destroy();
            }     
        }

    }
}
