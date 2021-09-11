using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using JAIMaker_2.JAIM;
using MidiSharp;

namespace JAIMaker_2
{
    static class JAIMAKER
    {
        public static AudioArchive AAF;
        public static JAIMakerFile Project = new JAIMakerFile();
        public static JAIMakerSettings Settings;
        public static JAIMakerSoundManager SoundManager;
        public static MidiKeyboard MidDevice;
        public static MidiSequence MIDI;
        public static GUI.WindowManager WindowManager;
        public static int DSPTickRate = 1024;
 
        static int currentDSPTicks = 0;
        static DateTime TickSystemStart = DateTime.Now;      

        static void Main()
        {

            Console.WriteLine("__  ____ _ _   _ _ __ __ _  __ _ ");
            Console.WriteLine("\\ \\/ / _` | | | | '__/ _` |/ _` |");
            Console.WriteLine(" >  < (_| | |_| | | | (_| | (_| |");
            Console.WriteLine("/_/\\_\\__,_|\\__, |_|  \\__, |\\__,_|");
            Console.WriteLine("           |___/     |___/       ");

#if DEBUG
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine("!JAIMAKER build in debug mode, do not push into release!");
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine("dump email: bugs@xayr.ga");
            Console.ForegroundColor = ConsoleColor.Gray;
#endif


            JAIDSP2.JAIDSP.Init();
            Settings = JAIMakerSettings.load();
            WindowManager = new GUI.WindowManager();
        
          MidDevice = new MidiKeyboard();
          
          var W = File.OpenRead("jaiinit.aaf");
          var wR = new Be.IO.BeBinaryReader(W);
          var nr = new JAIM.AudioArchive();
          nr.loadFromStream(wR);
          AAF = nr;

          SoundManager = new JAIMakerSoundManager(AAF);
          SoundManager.createAWHandles("banks");
          //*/

            WindowManager.init();
            WindowManager.addWindow("INSTBROWSER", new GUI.InstrumentBrowser());
            WindowManager.addWindow("PIANOBOY", new GUI.PianoBoy());
            WindowManager.addWindow("REMAPPER", new GUI.MidiRemapper());
            WindowManager.addWindow("IMPORTER", new GUI.MidiImportControl());

            //*/            

            while (true)
            {

                var time_elapsed = DateTime.Now - TickSystemStart;
                var target_dsp_ticks = time_elapsed.TotalSeconds * DSPTickRate;

                WindowManager.update();

                // We're 2 seconds behind, probably for a good reason. 
                if (Math.Abs(target_dsp_ticks - currentDSPTicks) > DSPTickRate * 2)
                {
                  Console.WriteLine($"! DSP Out of sync {target_dsp_ticks} - {currentDSPTicks} > {DSPTickRate * 2} forcing sync emulatedTicks = {target_dsp_ticks}.");     
                  currentDSPTicks = (int)target_dsp_ticks;
                              
                }

                while (target_dsp_ticks > currentDSPTicks)
                {
                    JAIDSP2.JAIDSPVoiceManager.updateAll();
                    currentDSPTicks++;
                }
            }
            Console.ReadLine();
        }
    }
}
