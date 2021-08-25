using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JAIMaker_2.JAIM;

namespace JAIMaker_2.JAIDSP2
{
    class DSPOscillator
    {
        JInstrumentOscillatorv1 Osc;
        JInstrumentEnvelopev1 CurrentEnvelope = null;
        JEnvelopeVector Vector = null;

        float delayMax = 0;
        float delay = 0;
        float prevValue = 0;
        int targetValue = 0;

        int __value;

        public float Value
        {
            get {
                return ((__value / (float)0xFFFF) * Osc.Width) + Osc.Vertex;
            }
        }

        public bool Stop;

        public DSPOscillator(JInstrumentOscillatorv1 Osci)
        {
            Osc = Osci;           
        }
        public void attack()
        {
            if (Osc.Attack != null)
            {
                CurrentEnvelope = Osc.Attack;
                swapVector(Osc.Attack.points[0]);
            }
        }
        public void release()
        {
            if (Osc.Release != null)
            {
                CurrentEnvelope = Osc.Release;
                swapVector(Osc.Release.points[0]);
            }
            else
                Stop = true;
        }

        public void update()
        {
            if (Osc == null || CurrentEnvelope == null || Vector == null || Stop)
                return;
            delay-=Osc.Rate;
            var dist = targetValue - prevValue;
            var perc = (float)(delayMax - delay) / delayMax;
            if (perc > 1)
                perc = 1;
            if (delay == 0)
                if (Vector._next != null)
                    swapVector(Vector._next);
            
            __value = (int)(prevValue + (dist * perc));
        }

        private void swapVector(JEnvelopeVector vector)
        {          
            prevValue = __value;
            targetValue = vector.Value;
            delay = vector.Delay;
            delayMax = delay;
            Vector = vector;
        }

    }
}
