using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAIMaker_2
{


    class JAIProgramRemap
    {
        public short bank;
        public short program;
    }

    class JAIMakerFile
    {
        public bool UseBankRemap;
        public JAIProgramRemap[] MIDIProgramRemap;
        public Dictionary<int,Dictionary<int,Dictionary<int,int>>> ProgramRemap;
        public byte[] midiSequence;
        public Dictionary<byte, JAIProgramRemap> InstrumentMap;
    }
}
