using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using ImGuiNET;
using System.IO;
using static ImGuiNET.ImGuiNative;
using MidiSharp;


namespace JAIMaker_2.GUI
{
    class MidiImportControl : Window
    {

        FileBrowser midiBrowser;
        string error = "";
        string midiPath = "";
        public override void init()
        {
            Title = "MIDI Import";           
        }

        public override void draw()
        {
            if (midiBrowser != null)
            {
                midiBrowser.draw();
                if (midiBrowser.Destroy)
                {
                    try
                    {
                        var midData = File.ReadAllBytes(midiBrowser.getSelectedFileFullPath());
                        JAIMAKER.Project.midiSequence = midData;
                        JAIMAKER.MIDI = MidiSequence.Open(new MemoryStream(midData));
                        JAIMAKER.Project.midiSequence = midData;
                        error = "";
                    }
                    catch (Exception E)
                    {
                        error = E.ToString();
                    }
                    midiBrowser = null;
                }
                return;
            }



            if (error.Length > 1)
                ImGui.TextColored(new System.Numerics.Vector4(255, 0, 0, 255), error);

            if (ImGui.Button("Open MIDI"))
                midiBrowser = new FileBrowser(ref midiPath, "*.mid");


          
            var AAF = JAIMAKER.AAF;
            if (AAF == null)
            {
                ImGui.Text("No Audio Archive loaded.");
                return;
            }


            if (JAIMAKER.MIDI == null)
            {
                ImGui.Text("No MIDI File loaded.");
                return;
            } else
                ImGui.TextColored(new System.Numerics.Vector4(0, 255, 0, 255), "MIDI Loaded");

            ImGui.Checkbox("Channel Program Assignment", ref JAIMAKER.Project.UseMidiOverride);
            if (JAIMAKER.Project.UseMidiOverride && JAIMAKER.Project.UseMidiRemap)
                ImGui.TextColored(new System.Numerics.Vector4(255, 0, 0, 255), "!!! This will interfere with MidiRemap, they cannot be used together."); ;

            ImGui.Columns(4);


            ImGui.Text("Track");
            ImGui.SetColumnWidth(0, 100f);

            for (int i = 0; i < JAIMAKER.MIDI.Tracks.Count; i++)
            {
                ImGui.Text($"Track {i}");
                ImGui.Dummy(new System.Numerics.Vector2(0.0f, 6f));
            }
            ImGui.NextColumn();

            ImGui.Text("Bank");
            ImGui.Dummy(new System.Numerics.Vector2(0.0f, 1f));
            for (int i = 0; i < JAIMAKER.MIDI.Tracks.Count; i++)
            {
                ImGui.InputInt($"Bnk##{i}", ref JAIMAKER.Project.MidiOverrides[i].bank);
                ImGui.Dummy(new System.Numerics.Vector2(0.0f, 0.3f));
            }


            ImGui.NextColumn();
            ImGui.Text("Program");
            ImGui.Separator();
            for (int i = 0; i < JAIMAKER.MIDI.Tracks.Count; i++)
            {
                ImGui.InputInt($"Prg##{i}", ref JAIMAKER.Project.MidiOverrides[i].program);
                ImGui.Dummy(new System.Numerics.Vector2(0.0f, 0.3f));

            }
            ImGui.NextColumn();
            for (int i = 0; i < JAIMAKER.MIDI.Tracks.Count; i++)
            {
                if (ImGui.Button($"Set Selected##{i}"))
                {
                    JAIMAKER.Project.MidiOverrides[i].bank = JAIMAKER.Project.SelectedBankID;
                    JAIMAKER.Project.MidiOverrides[i].program = JAIMAKER.Project.SelectedInstrument;
                }
                ImGui.Separator();
            }

            ImGui.Columns(1);
            ImGui.Separator();
            ImGui.Button("Test BMS Sequence");
            if (ImGui.Button("Export BMS Sequence"))
            {
                generateBMSSequence();
            };
        }


        private void generateBMSSequence()
        {
            var w = File.OpenWrite("test.bms");
            var bw = new Be.IO.BeBinaryWriter(w);

            var Assembler = new Assembler.JV1GenericBMSAssembler();
            Assembler.setOutput(bw);
            var Generator = new MidiToBMSAssembler(JAIMAKER.MIDI, Assembler,JAIMAKER.Project);
            Generator.processSequence();
        }
    }
}
