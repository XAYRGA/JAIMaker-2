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
    class MidiRemapper : Window
    {

        private string[] mapNames;
        private int editingRemap;

        public override void init()
        {
            Title = "MIDI Program Remapper";
            mapNames = new string[128];
        }

        private void rebuildLists()
        {
            for (int i = 0; i < JAIMAKER.Project.MidiRemap.Length; i++)
                mapNames[i] = (JAIMAKER.Project.MidiRemap[i].enable? "*" : "") + (JAIMAKER.Project.MidiRemap[i].name == null || JAIMAKER.Project.MidiRemap[i].name == "" ? $"{i} MIDI Program" : JAIMAKER.Project.MidiRemap[i].name);
        }

        public override void draw()
        {
            var AAF = JAIMAKER.AAF;
            if (AAF==null)
            {
                ImGui.Text("No Audio Archive loaded.");
                return;
            }
           
            ImGui.Columns(2);
            rebuildLists();
            ImGui.Checkbox("Use MIDI remap for export?", ref JAIMAKER.Project.UseMidiRemap);
            if (JAIMAKER.Project.UseMidiRemap)
            {

                ImGui.ListBox("Programs", ref editingRemap, mapNames, mapNames.Length);
                ImGui.NextColumn();
                if (JAIMAKER.Project.MidiRemap[editingRemap]!=null)
                {
                    var em = JAIMAKER.Project.MidiRemap[editingRemap];
                    ImGui.Checkbox("Enable Remap?", ref em.enable);
                    ImGui.InputText("Remap Name", ref em.name, 256);
                    ImGui.InputInt("Remap Bank", ref em.bank);
                    ImGui.InputInt("Remap Prog", ref em.program);
                    if (ImGui.Button("Set Selected"))
                    {
                        em.bank = JAIMAKER.Project.SelectedBankID;
                        em.program = JAIMAKER.Project.SelectedInstrument;
                    }
                }

            }
        }

    }
}
