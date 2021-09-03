using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Be.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace JAIMaker_2
{
    // Big endian settings file?
    // Big endian settings file.

    class JAIMakerSettings
    {
        private const string SAVE_FILE = "_jaimaker/settings.j";

        public string LastProjectDirectory;
        public string LastProject;
        public string SoundDeviceName;
        public string Theme;
        
        public void save()
        {
            if (!File.Exists(SAVE_FILE))
                return;
            Directory.CreateDirectory("_jaimaker");
            var FHnd = File.Open(SAVE_FILE, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var Wrt = new BeBinaryWriter(FHnd);
            var w = new BsonWriter(Wrt);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(w, this);
            w.Close();
        }
        public static JAIMakerSettings load()
        {
            try
            {
                var FHnd = File.Open(SAVE_FILE, FileMode.Open, FileAccess.ReadWrite);
                var Wrt = new BeBinaryReader(FHnd);
                var w = new BsonReader(Wrt);
                JsonSerializer ser = new JsonSerializer();
                var set = ser.Deserialize<JAIMakerSettings>(w);
                return set;
            } catch (Exception E)
            {
                Console.WriteLine("Configuration could not be loaded -- defaulting.");
                return new JAIMakerSettings();
            }
        }
    }
}
