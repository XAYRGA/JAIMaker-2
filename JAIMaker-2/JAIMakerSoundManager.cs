using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using JAIMaker_2.JAIM;
using JAIMaker_2.JAIDSP2;

namespace JAIMaker_2
{
    class JAIWaveContainer
    {
        public JWaveDescriptor descriptor;
        public WaveID id;
        public WaveGroup parent;
        public JAIDSPSoundBuffer buffer;
    }
    class JAIMakerSoundManager
    {
        private Dictionary<string, BinaryReader> awHandles = new Dictionary<string, BinaryReader>();
        private Dictionary<int, Dictionary<int, JAIWaveContainer>> bankMap = new Dictionary<int, Dictionary<int, JAIWaveContainer>>();
        private AudioArchive AAF;


        public JAIMakerSoundManager(AudioArchive AAFData)
        {
            AAF = AAFData;
            reinitializemap();
            buildMap();
        }
        public void createAWHandles(string relativePath)
        {
            for (int i = 0; i < AAF.WaveSystems.Length; i++)
            {
                var cWS = AAF.WaveSystems[i];
                for (int gI = 0; gI < cWS.Groups.Length; gI++)
                {
                    var grp = cWS.Groups[gI];
                    if (!awHandles.ContainsKey(grp.ArchivePath))
                        if (File.Exists($"{relativePath}/{grp.ArchivePath}"))
                            awHandles[grp.ArchivePath] = new BinaryReader(File.OpenRead($"{relativePath}/{grp.ArchivePath}"));
                        else
                            Console.WriteLine($"JAIMakerSoundManager::createAWHandles cannot create handle for {relativePath}/{grp.ArchivePath}");
                }
            }
        }

        private void buildMap()
        {
            Console.WriteLine("JAIMakerSoundManager::buildMap start WSYS wave tablization...");
            for (int i = 0; i < AAF.WaveSystems.Length; i++)
            {
                var cWS = AAF.WaveSystems[i];
                var currentMap = new Dictionary<int, JAIWaveContainer>();
                bankMap[(int)cWS.Id] = currentMap;
                for (int gI = 0; gI < cWS.Groups.Length; gI++)
                {
                    var grp = cWS.Groups[gI];
                    var scn = cWS.Scenes[gI];
                    for (int wI=0; wI  < grp.Waves.Length; wI++)
                    {
                        var cWI = grp.Waves[wI];
                        var cwS = scn.Waves[wI];
                        currentMap[cwS.waveID] = new JAIWaveContainer
                        {
                            descriptor = cWI,
                            id = cwS,
                            parent = grp
                        };
                     
                    }
                }
            }
            Console.WriteLine("JAIMakerSoundManager::buildMap end WSYS wave tablization...");
        }


        public JAIWaveContainer loadSound(byte wsysID, short WaveID)
        {
            Dictionary<int, JAIWaveContainer> wsysTree;
            if (!bankMap.TryGetValue(wsysID, out wsysTree))
            {
                Console.WriteLine($"Failed to lookup WSYS {wsysID}");
                return null;
            }
            JAIWaveContainer wave;
            if (!wsysTree.TryGetValue(WaveID, out wave))
            {
                Console.WriteLine($"Failed to lookup WaveID {wsysID}, {WaveID}");
                return null;
            }
            if (wave.buffer != null)
                return wave;
            var group = wave.parent.ArchivePath;
            var info = wave.descriptor;
            BinaryReader br;
            if (!awHandles.TryGetValue(group, out br))
                return null;
            br.BaseStream.Position = info.WSYS_StartAddress;
            var adpcm_data = br.ReadBytes(info.WSYS_Length);
            var pcm_data = bananapeel.ADPCMToPCM16(adpcm_data,bananapeel.ADPCMFormat.FOUR_BIT);
            Console.WriteLine("JAIWaveContainer::loadsound -> New JAIDSPSoundBuffer ");
            JAIDSPSoundBuffer buff = null;
            if (!info.Loop)
                buff = JAIDSP.SetupSoundBuffer(pcm_data, 1, (int)info.SampleRate, 0);
            else
                buff = JAIDSP.SetupSoundBuffer(pcm_data, 1, (int)info.SampleRate, 0, info.LoopPoint_Start, info.LoopPoint_End);
            wave.buffer = buff;

            return wave;
        }

        private void reinitializemap()
        {
            bankMap = new Dictionary<int, Dictionary<int, JAIWaveContainer>>();
        }
    }
}
