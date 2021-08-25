using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JAIMaker_2.JAIM;

namespace JAIMaker_2.JAIDSP2
{
  
    class DSPVoice
    {
        private const int PITCHMATRIX_SIZE = 3;
        private const int VOLMATRIX_SIZE = 3;

        private float[] _pitchMatrix = new float[PITCHMATRIX_SIZE];
        private float[] _volMatrix = new float[VOLMATRIX_SIZE];

        public float Pitch;
        public float Volume;

        JInstrument Instrument;
        DSPOscillator Oscillator;

        public DSPVoice(JInstrument Ins)
        {
            for (int i = 0; i < PITCHMATRIX_SIZE; i++)
                _pitchMatrix[i] = 1f;
            Instrument = Ins;

            if (Ins.Percussion)
                return;

            var StdIns = ((JStandardInstrumentv1)Ins);

            if (StdIns.oscillatorA!=null)
                Oscillator = new DSPOscillator(StdIns.oscillatorA);

        } 
        public void setPitchMatrix(byte slot, float value)
        {
            if (slot > PITCHMATRIX_SIZE)
                return;
            Pitch = 1;
            _pitchMatrix[slot] = value;
            for (int i = 0; i < PITCHMATRIX_SIZE; i++)
                Pitch *= _pitchMatrix[i];
        }

        public void setVolMatrix(byte slot, float value)
        {
            if (slot > VOLMATRIX_SIZE)
                return;
            Volume = 1;
            _volMatrix[slot] = value;
            for (int i = 0; i < VOLMATRIX_SIZE; i++)
                Pitch *= _volMatrix[i];
        }

        public void update()
        {
            if (Oscillator != null) {
                Oscillator.update();
                setVolMatrix(2, Oscillator.Value);
            }
                
        }

    }
}
