using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using ImGuiNET;
using static ImGuiNET.ImGuiNative;
using System.IO;

namespace JAIMaker_2.GUI
{
    class OpenAAFMenu : Window
    {

        string aafPath = "";
        string bankPath = "";
        bool error = false;
        string errorText = "";
        FileBrowser browser;
        DirectoryBrowser browserDir; 

        public override void init()
        {
            Title = "OpenAAF";           
        }

        private void regretDesignDecisions()
        {

        }

        public override void draw()
        {

            if (browser != null)
            {
                aafPath = browser.getSelectedFileFullPath();
                browser.draw();
                if (browser.Destroy)
                {
                    browser = null;
                    if (Directory.Exists(Path.GetDirectoryName(aafPath) + "\\Waves"))
                        bankPath = Path.GetDirectoryName(aafPath) + "\\Waves";
                    if (Directory.Exists(Path.GetDirectoryName(aafPath) + "\\Banks"))
                        bankPath = Path.GetDirectoryName(aafPath) + "\\Banks"; 
                }
                return;
            }

            if (browserDir != null)
            {
                bankPath = browserDir.getSelectedFileFullPath();
                browserDir.draw();
                if (browserDir.Destroy)
                    browserDir = null;
                return;
            }

            if (error)
                ImGui.TextColored(new System.Numerics.Vector4(255, 0, 0, 255), errorText);

            ImGui.InputText("AAF Location", ref aafPath, 0xFF);
            if (ImGui.Button("Browse")) 
                browser = new FileBrowser(ref aafPath, "*.*");

            ImGui.Spacing();
            ImGui.InputText("ADPCM Banks (aw) location.", ref bankPath, 0xFF);
            if (ImGui.Button("Browse##2"))
                browserDir = new DirectoryBrowser(ref bankPath, "*.*");

            ImGui.SetNextItemWidth(-1);
            if (ImGui.Button("Open AAF"))
            {
                try
                {
                    var W = File.OpenRead(aafPath);
                    var wR = new Be.IO.BeBinaryReader(W);
                    var nr = new JAIM.AudioArchive();
                    nr.loadFromStream(wR);
                    JAIMAKER.AAF = nr;                 

                    JAIMAKER.SoundManager = new JAIMakerSoundManager(JAIMAKER.AAF);
                    JAIMAKER.SoundManager.createAWHandles(bankPath);
                    Destroy = true;
                } catch (Exception E)
                {
                    errorText = "! Error Initializing AudioArchive system !\r\n" + E.ToString();
                    error = true;
                }
            }  
        }

    }
}
