using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using System.IO;
using Be.IO;

namespace JAIMaker_2
{


    class JAIProgramRemap
    {
        public string name = "";
        public bool enable;
        public int bank;
        public int program;
    }

    class JAIMakerFile
    {
        public float SaveFileVersion = 1.0f;
        public int SelectedInstrument = 0;
        public int SelectedBank = 0;
        public int SelectedBankID = 0;
        public bool UseMidiOverride = true;
        public bool UseMidiRemap = false;
        public JAIProgramRemap[] MidiRemap = new JAIProgramRemap[128];
        public JAIProgramRemap[] MidiOverrides = new JAIProgramRemap[16];
        public Dictionary<int,Dictionary<int,Dictionary<int,int>>> ProgramRemap = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
        public byte[] midiSequence;
        public static Dictionary<int, Dictionary<int, string>> BankNames = new Dictionary<int, Dictionary<int, string>>();

        public JAIMakerFile()
        {
            for (int i = 0; i < 128; i++)
                MidiRemap[i] = new JAIProgramRemap();
            for (int i = 0; i < 16; i++)
                MidiOverrides[i] = new JAIProgramRemap();
          
        }

        public void save()
        {
            Directory.CreateDirectory("_jaimaker");
            var FHnd = File.Open("_jaimaker/last_project.s", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var Wrt = new BeBinaryWriter(FHnd);
            var w = new BsonWriter(Wrt);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(w, this);
            w.Close();
        }
    }
}
