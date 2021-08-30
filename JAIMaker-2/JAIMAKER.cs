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
        public static JAIMakerSettings Settings;
        public static JAIMakerSoundManager SoundManager;
        public static GUI.WindowManager WindowManager;

        static void Main()
        {
            Settings = JAIMakerSettings.load();
            // JAIDSP2.JAIDSP.Init(Settings.SoundDeviceName);
            WindowManager = new GUI.WindowManager();

            var W = File.OpenRead("Jaiinit.aaf");
            var wR = new Be.IO.BeBinaryReader(W);
            var nr = new JAIM.AudioArchive();
            nr.loadFromStream(wR);
            AAF = nr;

            WindowManager.init();
            while (true)
            {
                WindowManager.update();
            }

            Console.ReadLine();
        }
    }
}
