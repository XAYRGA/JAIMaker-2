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
using Melanchall.DryWetMidi.Devices;


namespace JAIMaker_2.GUI
{
    class MidiInputControl : Window
    {

        string error = "";
        string midiPath = "";
        string[] deviceNames = new string[0];
        int selectedDevice = 0;
       
        public override void init()
        {
            Title = "MIDI Input";
            refreshDevices();
        }


        private void refreshDevices()
        {
            var devs = InputDevice.GetAll();
            deviceNames = new string[devs.Count<InputDevice>()];
            var i = 0;
            foreach (InputDevice dev in devs)
            {
                deviceNames[i] = dev.Name;
                i++;
            }
        }
        public override void draw()
        {

            if (error.Length > 1)
                ImGui.TextColored(new System.Numerics.Vector4(255, 0, 0, 255), error);

            var AAF = JAIMAKER.AAF;
            if (AAF == null)
            {
                ImGui.Text("No Audio Archive loaded.");
                return;
            }

            ImGui.Combo("MIDI Device", ref selectedDevice, deviceNames, deviceNames.Length);
            if (ImGui.Button("Refresh MIDI Devices"))
                refreshDevices();
     
            if (ImGui.Button("Open MIDI Device"))
            {

                var devs = InputDevice.GetAll();
                foreach (InputDevice dev in devs)
                {
                    if (dev.Name==deviceNames[selectedDevice])
                    {
                        JAIMAKER.MidDevice = new MidiInput(dev);
                        break;
                    }        
                }
            }


            if (JAIMAKER.MidDevice == null)
            {
                ImGui.Text("No MIDI device connected.");
                return;
            } else
                ImGui.TextColored(new System.Numerics.Vector4(0, 255, 0, 255), "MIDI device connected!");


            ImGui.Columns(5);


            ImGui.Text("Track");
            ImGui.SetColumnWidth(0, 100f);

            for (int i = 0; i < JAIMAKER.MidDevice.Channels.Length; i++)
            {
                ImGui.Text($"Track {i+1}");
                ImGui.Dummy(new System.Numerics.Vector2(0.0f, 6f));
            }
            ImGui.NextColumn();

            ImGui.Text("Bank");
            ImGui.Dummy(new System.Numerics.Vector2(0.0f, 1f));
            for (int i = 0; i < JAIMAKER.MidDevice.Channels.Length; i++)
            {
                ImGui.InputInt($"Bnk##{i}", ref JAIMAKER.MidDevice.Channels[i].bank,1,0);
                ImGui.Dummy(new System.Numerics.Vector2(0.0f, 0.3f));
            }


            ImGui.NextColumn();
            ImGui.Text("Program");
            ImGui.Separator();
            for (int i = 0; i < JAIMAKER.MidDevice.Channels.Length; i++)
            {
                ImGui.InputInt($"Prg##{i}", ref JAIMAKER.MidDevice.Channels[i].program);
                ImGui.Dummy(new System.Numerics.Vector2(0.0f, 0.3f));

            }
            ImGui.NextColumn();
            for (int i = 0; i < JAIMAKER.MidDevice.Channels.Length; i++)
            {
                if (ImGui.Button($"Set Selected##{i}"))
                {
                    JAIMAKER.MidDevice.Channels[i].bank = JAIMAKER.Project.SelectedBankID;
                    JAIMAKER.MidDevice.Channels[i].program = JAIMAKER.Project.SelectedInstrument;
                }
                ImGui.Dummy(new System.Numerics.Vector2(0.0f, 0.3f));
            }

            ImGui.NextColumn();
            for (int i = 0; i < JAIMAKER.MidDevice.Channels.Length; i++)
            {
                ImGui.Checkbox($"Sync Selected##{i}", ref JAIMAKER.MidDevice.Channels[i].enable);
                if (JAIMAKER.MidDevice.Channels[i].enable)
                {
                    JAIMAKER.MidDevice.Channels[i].bank = JAIMAKER.Project.SelectedBankID;
                    JAIMAKER.MidDevice.Channels[i].program = JAIMAKER.Project.SelectedInstrument;
                }
                ImGui.Separator();
            }

            ImGui.Columns(1);
            ImGui.Separator();
            ImGui.Button("MIDI Panic");
            ImGui.Dummy(new System.Numerics.Vector2(0.0f, 5f));
            ImGui.Separator();
            if (ImGui.Button("Move to override presets"))
            {
                for (int i = 0; i < JAIMAKER.MidDevice.Channels.Length; i++)
                {
                    if (i < JAIMAKER.Project.MidiOverrides.Length)
                    {
                        // Have to do by-value or else it will pass the object reference.
                        JAIMAKER.Project.MidiOverrides[i].bank = JAIMAKER.MidDevice.Channels[i].bank;
                        JAIMAKER.Project.MidiOverrides[i].program = JAIMAKER.MidDevice.Channels[i].program;
                    }
                }
            }

        }


    }
}
