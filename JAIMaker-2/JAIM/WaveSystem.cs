using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Be.IO;

namespace JAIMaker_2.JAIM
{


    /* STRUCTURE OF A WSYS */
    /* ******************************
     * ** NO POINTERS ARE ABSOLUTE **
     * ** ALL ARE RELATIVE TO WSYS **
     * ******************************
     * 
        0x00 - int32 0x57535953  ('WSYS')
        0x04 - int32 size
        0x08 - int32 global_id
        0x0C - int32 unused 
        0x10 - int32 Offset to WINF
        0x14 - int32 Offset to WBCT

        STRUCTURE OF WINF
        0x00 - int32 ? ('WINF')
        0x04 - int32 count // SHOULD ALWAYS BE THE SAME  COUNT AS THE WBCT.
        0x08 - *int32[count] (WaveGroup)Pointers

        STRUCTURE OF WBCT  
        0x00 - int32 ('WBCT')
        0x04 - int32 unknown
        0x08 - int32 count // SHOULD ALWAYS BE THE SAME COUNT AS THE WINF. 
        0x0C - *int32[count] (SCNE)Pointers

        STRUCTURE OF SCNE
        0x00 - int32 ('SCNE')
        0x04 - long 0;
        0x0C - pointer to C-DF
        0x10 - pointer to C-EX // Goes completely unused?
        0x14 - pointer to C-ST // Goes completely unused? 

        STRUCTURE OF C_DF 
        0x00 - int32 ('C-DF')
        0x04 - int32 count
        0x08 - *int32[count] (waveID)Pointers

        STRUCTURE OF (waveID) 
        0x02 - short awID
        0x04 - short waveID

        r
        STRUCTURE OF 'WaveGroup'  
        0x00 - byte[0x70] ArchiveName
        0x70 - int32 waveCount 
        0x74 - *int32[waveCount] (wave)Pointers

        STRUCTURE OF 'wave'
        0x00 - byte unknown
        0x01 - byte format
        0x02 - byte baseKey  
        0x03 - byte unknown 
        0x04 - float sampleRate 
        0x08 - int32 start
        0x0C - int32 length 
        0x10 - int32 loop >  0  ?  true : false 
        0x14 - int32 loop_start
        0x18 - int32 loop_end 
        0x1C  - int32 sampleCount 


        -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         ~~ IMPORTANT NOTES ABOUT SCNE AND WAVEGROUPS ~~
         The SCNE and WaveGroups are parallel. 
         this means, that SCNE[1] pairs with WaveGroup[1].
         So, when  you load the waves from the WaveGroup, 
         the first entry in the SCNE's C-DF matches
         with the first wave in the WaveGroup. 
         This means: 
         SCNE.C-DF[1] matches with WaveGroup.Waves[1]. 
         Also indicating,  that the first Wave-ID in the 
         C-DF matches with the first wave in the WaveGroup


    */

    class WaveScene
    {
        public WaveCategory DefaultCategory; // C-DF , CATEGORY DeFault
        public WaveCategory ExtendedCategory; // C-EX , CATEGORY EXtended 
        public WaveCategory StoopidCategory; // C-ST, Category SToopid 

        public WaveID[] Waves; // !! Helper. C-DF waves will be unpacked into here because C-EX and C-ST are never used. 


        private void loadFromStream(BeBinaryReader wsysReader, int seekBase)
        {
            var header = wsysReader.ReadUInt32();
            wsysReader.ReadUInt64(); // Padding.

            var cDF = wsysReader.ReadUInt32();
            var cEX = wsysReader.ReadUInt32();
            var cST = wsysReader.ReadUInt32();

            wsysReader.BaseStream.Position = cDF + seekBase;
            DefaultCategory = WaveCategory.CreateFromStream(wsysReader, seekBase);

            wsysReader.BaseStream.Position = cEX + seekBase;
            ExtendedCategory = WaveCategory.CreateFromStream(wsysReader, seekBase);

            wsysReader.BaseStream.Position = cST + seekBase;
            StoopidCategory = WaveCategory.CreateFromStream(wsysReader, seekBase);

            Waves = DefaultCategory.Waves;

        }

        public static WaveScene CreateFromStream(BeBinaryReader wsysReader, int seekBase)
        {
            var newWaveScene = new WaveScene();
            newWaveScene.loadFromStream(wsysReader, seekBase);
            return newWaveScene;
        }
    }

    class WaveCategory
    {
        public WaveID[] Waves;
        private void loadFromStream(BeBinaryReader wsysReader, int seekBase)
        {
            var header = wsysReader.ReadUInt32();
            var count = wsysReader.ReadUInt32();
            var offsets = util.readInt32Array(wsysReader, (int)count);

            Waves = new WaveID[count];
            for (int i = 0; i < count; i++) {
                wsysReader.BaseStream.Position = offsets[i] + seekBase;
                Waves[i] = WaveID.CreateFromStream(wsysReader);
            }
        }

        public static WaveCategory CreateFromStream(BeBinaryReader wsysReader, int seekBase)
        {
            var newWaveCat = new WaveCategory();
            newWaveCat.loadFromStream(wsysReader, seekBase);
            return newWaveCat;
        }
    }

    class WaveID
    {
        public ushort awID;
        public ushort waveID;

        private void loadFromStream(BeBinaryReader wsysReader)
        {
            awID = wsysReader.ReadUInt16();
            waveID = wsysReader.ReadUInt16();
        }

        public static WaveID CreateFromStream(BeBinaryReader wsysReader)
        {
            var newWaveID = new WaveID();
            newWaveID.loadFromStream(wsysReader);
            return newWaveID;
        }
    }

    class WaveGroup
    {
        public string ArchivePath;
        public JWaveDescriptor[] Waves;

        private void loadFromStream(BeBinaryReader wsysReader, int seekBase)
        {
            ArchivePath = readArchiveName(wsysReader);
            var count = wsysReader.ReadUInt32();
            var offsets = util.readInt32Array(wsysReader, (int)count);

            Waves = new JWaveDescriptor[count];    
            for (int i=0; i < count; i++)
            {
                wsysReader.BaseStream.Position = seekBase + offsets[i];
                Waves[i] = JWaveDescriptor.CreateFromStream(wsysReader);
            }                
        }

        public static WaveGroup CreateFromStream(BeBinaryReader wsysReader, int seekBase)
        {
            var newWaveGrp = new WaveGroup();
            newWaveGrp.loadFromStream(wsysReader,seekBase);
            return newWaveGrp;
        }
        private string readArchiveName(BinaryReader aafRead)
        {
            var ofs = aafRead.BaseStream.Position;
            byte nextbyte;
            byte[] name = new byte[0x70];
            int count = 0;
            while ((nextbyte = aafRead.ReadByte()) != 0xFF & nextbyte != 0x00)
            {
                name[count] = nextbyte;
                count++;
            }
            aafRead.BaseStream.Seek(ofs + 0x70, SeekOrigin.Begin);
            return Encoding.ASCII.GetString(name, 0, count);
        }
    }

    public enum JWaveFormat
    {
        ADPCM4 = 0,
        ADPCM2 = 1,
        PCM8 = 2,
        PCM16BE= 3,
    }
    public class JWaveDescriptor
    {
        public JWaveFormat Format;
        public byte BaseKey;
        public float SampleRate;
        public int SampleCount;
        public int WSYS_StartAddress;
        public int WSYS_Length;
        public bool Loop;
        public int LoopPoint_Start;
        public int LoopPoint_End;
        public short LastSample;
        public short PenultimateSample;


        private void loadFromStream(BeBinaryReader wsysReader)
        {
            wsysReader.ReadByte();
            Format = (JWaveFormat)wsysReader.ReadByte();
            BaseKey = wsysReader.ReadByte();
            wsysReader.ReadByte();
            SampleRate = wsysReader.ReadSingle();
            WSYS_StartAddress = wsysReader.ReadInt32();
            WSYS_Length = wsysReader.ReadInt32();
            Loop = wsysReader.ReadUInt32() > 0;
            LoopPoint_Start = wsysReader.ReadInt32();
            LoopPoint_End = wsysReader.ReadInt32();
            LastSample = wsysReader.ReadInt16();
            PenultimateSample = wsysReader.ReadInt16();
        }

        public static JWaveDescriptor CreateFromStream(BeBinaryReader wsysReader)
        {
            var WV = new JWaveDescriptor();
            WV.loadFromStream(wsysReader);
            return WV;
        }
    }


    class WaveSystem
    {
        private const int WSYS = 0x57535953;
        public uint Id;
        public uint Size;
        public uint UppwerWaveID;

        public WaveGroup[] Groups;
        public WaveScene[] Scenes;

        private void loadFromStream(BeBinaryReader wsysReader)
        {
            var seekBase = (int)wsysReader.BaseStream.Position;
            var header = wsysReader.ReadUInt32();
            if (header != WSYS)
                throw new FormatException($"{header:X}!={WSYS:X}");
            Id = wsysReader.ReadUInt32();
            Size = wsysReader.ReadUInt32();
            UppwerWaveID = wsysReader.ReadUInt32(); // PADDING
            var WINFOffset = wsysReader.ReadUInt32();
            var WBCTOffset = wsysReader.ReadUInt32();

            // Loads WaveGroups
            wsysReader.BaseStream.Position = seekBase + WINFOffset;
            var WINFHeader = wsysReader.ReadUInt32();
            var GroupCount = wsysReader.ReadUInt32();
            var GroupOffsets = util.readInt32Array(wsysReader, (int)GroupCount);

            Groups = new WaveGroup[GroupCount];
            for (int i=0; i < GroupCount; i++)
            {
                wsysReader.BaseStream.Position = GroupOffsets[i] + seekBase;
                Groups[i] = WaveGroup.CreateFromStream(wsysReader, seekBase);
            }

            // Loads WaveScenes
            wsysReader.BaseStream.Position = seekBase + WBCTOffset;
            var WBCTHeader = wsysReader.ReadUInt32();
            wsysReader.ReadUInt32(); // Padding.
            var SceneCount = wsysReader.ReadUInt32();
            var SceneOffsets = util.readInt32Array(wsysReader, (int)SceneCount);

            Scenes = new WaveScene[SceneCount];
            for (int i = 0; i < SceneCount; i++)
            {
               wsysReader.BaseStream.Position = SceneOffsets[i] + seekBase;
               Scenes[i] = WaveScene.CreateFromStream(wsysReader, seekBase);
            }

        }

        public static WaveSystem CreateFromStream(BeBinaryReader wsysReader)
        {
            var WV = new WaveSystem();
            WV.loadFromStream(wsysReader);
            return WV;
        }
    }
}
