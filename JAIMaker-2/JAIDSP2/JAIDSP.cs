using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Un4seen.Bass.AddOn.Fx;

namespace JAIMaker_2.JAIDSP2
{
    public static class JAIDSP
    {

        public static SYNCPROC globalLoopProc;
        public static string Device;

        public static bool Init(string DeviceInfo = "none")
        {

            Console.WriteLine("===JAIDSP INIT START===");

            Console.Write("Regi....");
            #region dumb obfuscation for email and registration key, just to prevent bots.
            byte obfu = 0xDA;
            byte[] eml = new byte[] {0xBE, 0xBB, 0xB4,0xBF,0x9A,0xA2,0xBB,0xA3,0xA8,0xF4,0xBD,0xBB};
            byte[] rkey = new byte[] {0xE8,0x82,0xE3,0xE9,0xE8,0xE9,0xEB,0xE8,0xEE,0xE9,0xE9};
            for (int i = 0; i < eml.Length; i++)
                eml[i] ^= (obfu);

            for (int i = 0; i < rkey.Length; i++) 
                rkey[i] ^= (obfu);
            
            #endregion
            Un4seen.Bass.BassNet.Registration(Encoding.ASCII.GetString(eml), Encoding.ASCII.GetString(rkey));
            Console.WriteLine("OK!");
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero); // Initialize audio engine
            BassFx.LoadMe();
            globalLoopProc = new SYNCPROC(DoLoop);
            changeDevice(DeviceInfo);
            Console.WriteLine("===JAIDSP INIT END===");
            return true;
        }

        public static bool changeDevice(string nameMatch)
        {
            var oldDevice = Device;
            var sL = GetDeviceList();
            for (int i = 0; i < sL.Length; i++)
                if (sL[i] == nameMatch)
                {
                    Bass.BASS_SetDevice(i);
                    Device = sL[i];
                    Console.WriteLine($"JAIDSP: Changing device to '{sL[i]}'");
                    return true;
                }
            return false;     
        }

        public static string[] GetDeviceList()
        {
            BASS_DEVICEINFO info = new BASS_DEVICEINFO(); // Print device info. 
            int n = 0;
            for (n = 0; Bass.BASS_GetDeviceInfo(n, info); n++) ;
            string[] devices = new string[n];
            for (int i = 0; Bass.BASS_GetDeviceInfo(i, info); i++)
                devices[i] = info.name;
            return devices;
        }
        public static bool Deinit() // free's engine thread
        {
            Bass.BASS_Free();
            BassFx.FreeMe();
            return true;
        }

        private static void DoLoop(int syncHandle, int channel, int data, IntPtr user)
        {
            Bass.BASS_ChannelSetPosition(channel, user.ToInt64(),BASSMode.BASS_POS_BYTE);
        }
        private static int v1 = 0;
        public static JAIDSPSoundBuffer SetupSoundBuffer(byte[] pcm, int cn, int sr, int bs, int ls, int le)
        {
            v1++;
            var rt = new JAIDSPSoundBuffer()
            {
                format = new JAIDSPFormat()
                {
                    channels = cn,
                    sampleRate = sr,
                },
                buffer = pcm,
                loopStart = (int)Math.Floor((ls / 8f) * 16f), // 16 samples = 8 bytes
                loopEnd = (int)Math.Floor((le / 8f) * 16f),
                looped = true,
            };
            rt.generateFileBuffer();
            return rt;
        }

        public static JAIDSPSoundBuffer SetupSoundBuffer(byte[] pcm, int cn, int sr, int bs)
        {
            v1++;
            var rt = new JAIDSPSoundBuffer()
            {
                format = new JAIDSPFormat()
                {
                    channels = cn,
                    sampleRate = sr,
                },
                buffer = pcm,
                looped = false,
            };
            rt.generateFileBuffer();
            return rt;
        }
    }
}
