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
    class InstrumentBrowser
    {

        public string[] bankList;
        public string[] progList;


        int BankSelection = 0;
        int ProgSelection = 0;
        public void init()
        {

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
            var bnk = JAIMAKER.AAF.InstrumentBanks[BankSelection];
            progList = new string[bnk.instruments.Length];
            for (int i = 0; i < progList.Length; i++)
                progList[i] =  bnk.instruments[i]==null ? "(empty)" : $"Program {i}";
        }

        public void update()
        {
            var AAF = JAIMAKER.AAF;
            if (AAF==null)
            {
                ImGui.Text("No Audio Archive loaded.");
                return;
            }

            rebuildLists();
            rebuildProgramLists();

            ImGui.Columns(2);
            ImGui.ListBox("Instruments", ref BankSelection, bankList, bankList.Length);
            ImGui.NextColumn();
            ImGui.ListBox("Instruments2", ref ProgSelection, progList, progList.Length);
        }
    }
}
