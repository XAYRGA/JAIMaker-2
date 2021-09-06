using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JAIMaker_2.JAIM;

namespace JAIMaker_2.JAIDSP2
{
    class JAIDSPOscillator
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

        public JAIDSPOscillator(JInstrumentOscillatorv1 Osci)
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
            if (delay < 0)
                if (Vector._next != null)
                    swapVector(Vector._next);

            if (delay < 0 && Vector.Mode > 0xE)
                Stop = true;

            if (delay < 0 && Vector.Mode < 0xE && Vector._next == null)
                panic();
                
            //Console.WriteLine($"Vector Mode {Vector.Mode} {delay}");
            if (Vector.Mode != 0x0E)
            {
                
                var preVal = (int)(prevValue + (dist * perc));
                __value = preVal < 0 ? 0 : preVal;
                Console.WriteLine(__value);
            }
        }

        private void panic()
        {
            Console.WriteLine($"Oscillator Panic!! Vector reached termination delay, mode is not stop, and next vector is null!");
            Stop = true;
        }

        private void swapVector(JEnvelopeVector vector)
        {
            if (vector.Mode != 0x0E)
            {
                Console.WriteLine($"Swapping vector with value {__value}, prev {(Vector==null ? 0 : Vector.Value)}, next {vector.Value}");
                prevValue = __value;
                targetValue = vector.Value;
            }
            delay = vector.Delay;
            delayMax = delay;
            Vector = vector;
        }
    }
}
