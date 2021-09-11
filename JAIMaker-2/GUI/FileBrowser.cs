using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using System.Numerics;
using JAIMaker_2.JAIDSP2;
using ImGuiNET;

namespace JAIMaker_2.GUI
{
    class FileBrowser : Window
    {
        public string path;
        public string selectedFile = "";
        public bool done = false;

        public string[] drives = new string[0];
        int driveIndex = 0;
        int lastDriveIndex = 0; 

        public string[] files = new string[0];
        int fileIndex = 0;

        public string[] directories = new string[0];
        int directoryIndex = 0;

        bool cantOpen = false;

        string filterPat = "*";

        public FileBrowser(ref string stringData, string pattern)
        {
            path = stringData;
            filterPat = pattern;
            refresh();
        }

        public string getSelectedFileFullPath()
        {
            return $"{path}/{selectedFile}";
        }
        private void refresh()
        {
            try
            {
                var drvs = DriveInfo.GetDrives();
                drives = new string[drvs.Length];
                for (int i = 0; i < drvs.Length; i++)
                    drives[i] = drvs[i].RootDirectory.FullName;

                var dir_temp = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
                files = Directory.GetFiles(path, filterPat, SearchOption.TopDirectoryOnly);

                directories = new string[dir_temp.Length + 2];
                directories[0] = ".";
                directories[1] = "..";
                for (int i = 0; i < dir_temp.Length; i++)
                    directories[i + 2] = Path.GetFileName(dir_temp[i]);

                for (int i = 0; i < files.Length; i++)
                    files[i] = Path.GetFileName(files[i]);
                cantOpen = false;
            } catch
            {
                cantOpen = true;
            }
        }

        public override void init()
        {
            Title = "File Browser";
        }

        public override void draw()
        {
            if (driveIndex!=lastDriveIndex)
            {
                path = drives[driveIndex];
                lastDriveIndex = driveIndex;
                refresh();
            }

            if (cantOpen)
                ImGui.TextColored(new Vector4(0xFF, 0, 0, 0xFF),"Unable to open directory!");
            if (ImGui.InputText("Path", ref path, 256))
                refresh();
   
            ImGui.Combo("##DRIVE", ref driveIndex, drives, drives.Length);
            ImGui.BeginChildFrame(1, new Vector2(0f, 150f));
            ImGui.Columns(2);
            ImGui.PushItemWidth(-1);
            if (ImGui.ListBox("##DIRECTORY", ref directoryIndex, directories, directories.Length))
            {
               path = Path.GetFullPath(Path.Combine(path, directories[directoryIndex]));
               refresh();
            };

            ImGui.NextColumn();
            ImGui.PushItemWidth(-1);
            if (ImGui.ListBox("##FILES", ref fileIndex, files, files.Length))
                selectedFile = files[fileIndex];
            ImGui.EndChildFrame();
            ImGui.Columns(2);
            ImGui.InputText("File", ref selectedFile, 255);
            ImGui.Spacing();
            ImGui.NextColumn();
            if (ImGui.Button("Open"))
                Destroy = true;
            ImGui.SetColumnWidth(0, 600f);
        }
    }
}
