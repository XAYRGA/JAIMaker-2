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
    class MidiImportControl : Window
    {

        public override void init()
        {
            Title = "MIDI Import";           
        }

        public override void draw()
        {
            var AAF = JAIMAKER.AAF;
            if (AAF==null)
            {
                ImGui.Text("No Audio Archive loaded.");
                return;
            }

            ImGui.Checkbox("Channel Program Assignment", ref JAIMAKER.Project.UseMidiOverride);
            if (JAIMAKER.Project.UseMidiOverride && JAIMAKER.Project.UseMidiRemap)
                ImGui.TextColored(new System.Numerics.Vector4(255, 0, 0, 255), "!!! This will interfere with MidiRemap, they cannot be used together."); ;

            ImGui.Columns(4);


            ImGui.Text("Track");
            ImGui.SetColumnWidth(0, 100f);
       
            for (int i = 0; i < 16; i++)
            {
                ImGui.Text($"Track {i}");
                ImGui.Dummy(new System.Numerics.Vector2(0.0f, 6f));
            }
            ImGui.NextColumn();

            ImGui.Text("Bank");
            ImGui.Dummy(new System.Numerics.Vector2(0.0f, 1f));
            for (int i = 0; i < 16; i++)
            {
                ImGui.InputInt($"Bnk##{i}", ref JAIMAKER.Project.MidiOverrides[i].bank);
                ImGui.Dummy(new System.Numerics.Vector2(0.0f, 0.3f));
            }
         

            ImGui.NextColumn();
            ImGui.Text("Program");
            ImGui.Separator();
            for (int i = 0; i < 16; i++)
            {
                ImGui.InputInt($"Prg##{i}", ref JAIMAKER.Project.MidiOverrides[i].program);
                ImGui.Dummy(new System.Numerics.Vector2(0.0f, 0.3f));

            }
            ImGui.NextColumn();
            for (int i=0; i < 16; i++)
            {
                if (ImGui.Button($"Set Selected##{i}"))
                {
                    JAIMAKER.Project.MidiOverrides[i].bank = JAIMAKER.Project.SelectedBankID;
                    JAIMAKER.Project.MidiOverrides[i].program = JAIMAKER.Project.SelectedInstrument;
                }
                ImGui.Separator();
            }


           
        }

    }
}
