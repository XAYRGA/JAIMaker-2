using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Be.IO;

namespace JAIMaker_2.JAIM
{

    // Four years later and this code is still bad.

    // HATE.


    class AudioArchive
    {
        public JInstrumentBankv1[] InstrumentBanks;
        public WaveSystem[] WaveSystems;

        private int wsCount = 0;
        private int bankCount = 0;

        public void loadFromStream(BeBinaryReader aafReader)
        {
            var seekBase = aafReader.BaseStream.Position;
            countSections(aafReader);

            InstrumentBanks = new JInstrumentBankv1[bankCount];
            WaveSystems = new WaveSystem[wsCount];

            aafReader.BaseStream.Position = seekBase;

            wsCount = 0;
            bankCount = 0;

            var sectType = 1000u;
            var bankOffset = 1000u;
            while ((sectType = aafReader.ReadUInt32()) != 0)
            {
                if (sectType == 3)
                    while ((bankOffset = aafReader.ReadUInt32()) != 0)
                    {
                        
                        var size = aafReader.ReadUInt32();
                        var flags = aafReader.ReadUInt32();
                        var oldP = aafReader.BaseStream.Position;
                        aafReader.BaseStream.Position = bankOffset;
                        WaveSystems[wsCount] = WaveSystem.CreateFromStream(aafReader);
                        aafReader.BaseStream.Position = oldP;
                        wsCount++;                        
                    }
                else if (sectType == 2)
                    while ((bankOffset = aafReader.ReadUInt32()) != 0)
                    {
                        var size = aafReader.ReadUInt32();
                        var flags = aafReader.ReadUInt32();
                        var oldP = aafReader.BaseStream.Position;
                        aafReader.BaseStream.Position = bankOffset;
                        InstrumentBanks[bankCount] = JInstrumentBankv1.CreateFromStream(aafReader);
                        aafReader.BaseStream.Position = oldP;
                        bankCount++;
                    }
                else
                {
                    var offset = aafReader.ReadUInt32();
                    var size = aafReader.ReadUInt32();
                    var type = aafReader.ReadUInt32();
                }
                bankOffset = 0;
            }
        }

        private void countSections(BeBinaryReader aafReader)
        {

            var sectType = 1000u;
            var bankOffset = 1000u;
            while ((sectType = aafReader.ReadUInt32()) != 0)
            {
                if (sectType == 3)
                    while ((bankOffset = aafReader.ReadUInt32()) != 0)
                    {
                        var size = aafReader.ReadUInt32();
                        var flags = aafReader.ReadUInt32();
                        wsCount++;
                    }
                else if (sectType == 2)
                    while ((bankOffset = aafReader.ReadUInt32()) != 0)
                    {
                        var size = aafReader.ReadUInt32();
                        var flags = aafReader.ReadUInt32();
                        bankCount++;
                    }
                else
                {
                    var offset = aafReader.ReadUInt32();
                    var size = aafReader.ReadUInt32();
                    var type = aafReader.ReadUInt32();
                }
                bankOffset = 0;
            }
        }

    }
}
