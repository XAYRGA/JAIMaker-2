using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using JAIMaker_2.JAIM;

namespace JAIMaker_2
{
    static class JAIMAKER
    {
        public static AudioArchive AAF;
        public static JAIMakerFile Project = new JAIMakerFile();
        public static JAIMakerSettings Settings;
        public static JAIMakerSoundManager SoundManager;
        public static GUI.WindowManager WindowManager;

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



            var W = File.OpenRead("Jaiinit.aaf");
            var wR = new Be.IO.BeBinaryReader(W);
            var nr = new JAIM.AudioArchive();
            nr.loadFromStream(wR);
            AAF = nr;

            SoundManager = new JAIMakerSoundManager(AAF);
            SoundManager.createAWHandles("banks");
           


            WindowManager.init();
            WindowManager.addWindow("INSTBROWSER", new GUI.InstrumentBrowser());
            WindowManager.addWindow("PIANOBOY", new GUI.PianoBoy());
            WindowManager.addWindow("REMAPPER", new GUI.MidiRemapper());
            WindowManager.addWindow("IMPORTER", new GUI.MidiImportControl());

            while (true)
            {
                WindowManager.update();
            }

            Console.ReadLine();
        }
    }
}
