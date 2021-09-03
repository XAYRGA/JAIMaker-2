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

namespace JAIMaker_2.GUI
{
    class InstrumentBrowser : Window
    {

        public string[] bankList;
        public string[] progList;
        private int remapSelectIndex;
        private int remapCreateIndex;
        private int remapCreateNote;

        public override void init()
        {
            Title = "Instrument Browser";           
        }

        private void rebuildLists()
        {
            if (JAIMAKER.AAF == null)
                return;
            bankList = new string[JAIMAKER.AAF.InstrumentBanks.Length];
            for (int i = 0; i < bankList.Length; i++)
                bankList[i] = $"Instrument Bank {JAIMAKER.AAF.InstrumentBanks[i].globalID}";
        }

        private void rebuildProgramLists()
        {
            if (JAIMAKER.AAF == null)
                return;
            var bnk = JAIMAKER.AAF.InstrumentBanks[JAIMAKER.Project.SelectedBank];
            progList = new string[bnk.instruments.Length];
            for (int i = 0; i < progList.Length; i++)
                progList[i] =  bnk.instruments[i]==null ? "(empty)" : $"Program {i}";
        }

        public override void draw()
        {
            var AAF = JAIMAKER.AAF;
            if (AAF==null)
            {
                ImGui.Text("No Audio Archive loaded.");
                return;
            }
            rebuildLists();
            rebuildProgramLists();
            ImGui.ListBox("Banks", ref JAIMAKER.Project.SelectedBank, bankList, bankList.Length);
            ImGui.ListBox("Programs", ref JAIMAKER.Project.SelectedInstrument, progList, progList.Length);
            JAIMAKER.Project.SelectedBankID = (int)JAIMAKER.AAF.InstrumentBanks[JAIMAKER.Project.SelectedBank].globalID;

            ImGui.Separator();
            Dictionary<int, Dictionary<int, int>> RemapBank;
            Dictionary<int, int> RemapProg;
            if (!JAIMAKER.Project.ProgramRemap.TryGetValue(JAIMAKER.Project.SelectedBank, out RemapBank))
                RemapBank = (JAIMAKER.Project.ProgramRemap[JAIMAKER.Project.SelectedBank] = new Dictionary<int, Dictionary<int, int>>());      

            if (!RemapBank.TryGetValue(JAIMAKER.Project.SelectedInstrument, out RemapProg))
            {
                if (ImGui.Button("Create Note Remapping"))
                    RemapBank[JAIMAKER.Project.SelectedInstrument] = new Dictionary<int, int>();
                return;
            }

            if (ImGui.Button("Delete Note Remapping"))
                RemapBank.Remove(JAIMAKER.Project.SelectedInstrument);

            ImGui.Columns(2);
            ImGui.InputInt("Source Note", ref remapCreateIndex);
            ImGui.NextColumn();
            ImGui.InputInt("Dest Note", ref remapCreateNote);

            if (ImGui.Button("Add Remap"))
            { 
                RemapProg[remapCreateIndex] = remapCreateNote;
            }

            ImGui.Columns(1);

            var keys = RemapProg.Keys.Select(x => x.ToString() + " -> " + RemapProg[x] ).ToArray();

            ImGui.ListBox("Remappings", ref remapSelectIndex, keys, keys.Length);

        }

    }
}
