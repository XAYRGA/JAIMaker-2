using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace JAIMaker_2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        public static float[] crushFFTWindow(float[] specDat, int size)
        {
            // Find how many blocks we're going to split into
            var chunkSize = (specDat.Length / size); // fuck you you're an int. 
                                                     // Create an array that fits that many blocks
            var crushedSpectrum = new float[chunkSize];
            for (int i = 0; i < (chunkSize); i++) // Calculate each "block" / number
            {
                var spectrumOffset = chunkSize * i; // How far we are into the original data
                var crushTotal = 0f; // total number, gets reset every iteration then has the sample added to it with each loop below
                for (int crushIndex = 0; crushIndex < chunkSize; crushIndex++)
                {
                    Console.Write($"{specDat[spectrumOffset + crushIndex]},");
                    crushTotal += specDat[spectrumOffset + crushIndex]; // Add the fft sample to the total
                }

                crushedSpectrum[i] = (crushTotal / chunkSize); // Perform an average, store it
                Console.WriteLine();
                Console.ReadKey();
            }
            return crushedSpectrum; // Spit it out 
        }
        static void Main()
        {
            var W = File.OpenRead("Jaiinit.aaf");
            var wR = new Be.IO.BeBinaryReader(W);
            var nr = new JAIM.AudioArchive();
            nr.loadFromStream(wR);
        }
    }
}
