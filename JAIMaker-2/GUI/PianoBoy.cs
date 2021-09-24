using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using System.Numerics;
using JAIMaker_2.JAIDSP2;
using ImGuiNET;

using static ImGuiNET.ImGuiNative;

namespace JAIMaker_2.GUI
{
    class PianoBoy : Window
    {


        bool[] keyPressedCurrentFrame = new bool[0x88];
        bool[] keyPressedLastFrame = new bool[0x88];
        int lastPressedKey = 0;
        int pressedBlackKey = -1;


        private static uint[] regionColors = new uint[0x7F];
        public override void init()
        {
            var w = new Random();
            
           
            Title = "Piano";           

            for (int i=0; i < 0x7F; i++)
                regionColors[i] = (uint)w.Next(0x00, 0xFFFFFF) | 0xFF000000u;
            regionColors[0] = 0xFFFFFFFF;
        }

        private bool d2DBoxCollideNoTangent(Vector2 Min, Vector2 Max, Vector2 Point)
        {
            if (Point.X > Min.X && Point.Y > Min.Y && Point.X < Max.X && Point.Y < Max.Y)
                return true;
            return false;
        }

        public override void draw()
        {
            if (JAIMAKER.AAF == null)
                return;
            var DrawList = ImGui.GetWindowDrawList();
            var WindowPos = ImGui.GetWindowPos();
            var TotalOffset = 0;
            var KeyPos = 0;
            var currentRegion = 0;


            JAIM.JStandardInstrumentv1 insD = null;
            JAIM.JKeyRegionv1 keyR = null; 

            if (JAIMAKER.AAF!=null)
            {
                var bnk = JAIMAKER.AAF.InstrumentBanks[JAIMAKER.Project.SelectedBank];
                var ins = bnk.instruments[JAIMAKER.Project.SelectedInstrument];
                if (ins == null)
                    return;
                if (ins.Percussion == false)
                {
                    insD = (JAIM.JStandardInstrumentv1)ins;
                    keyR = insD.getKeyRegion(0);
                } else
                {
                    insD = null;
                    keyR = null;
                }
             }

          
            for (int i=0; i< 0x88; i++)
            {
                if (insD!=null && !insD.Percussion)
                {
                    var nextKeyR = insD.getKeyRegion(i);
                    if (keyR!=nextKeyR)
                    {
                        currentRegion++;
                        keyR = nextKeyR;
                    }
                }

                if (KeyPos == 13)
                    KeyPos = 1;
                if (KeyPos == 1 || KeyPos==4 || KeyPos==6 || KeyPos==9 || KeyPos == 11 || KeyPos==13)
                {
                    keyPressedCurrentFrame[i] = false;
                    if (d2DBoxCollideNoTangent(WindowPos + new Vector2(((TotalOffset ) * 15) - 3, 30), WindowPos + new Vector2(((TotalOffset) * 15) + 7, 70), ImGui.GetMousePos()) && ImGui.IsMouseDown(ImGuiMouseButton.Left))
                    {
                        keyPressedCurrentFrame[i] = true;
                        lastPressedKey = i;
                        pressedBlackKey = i;
                    } else if (pressedBlackKey==i) 
                        pressedBlackKey = -1;
                }
                else 
                {
                    keyPressedCurrentFrame[i] = false;
                    var wb = regionColors[currentRegion];

                    if (JAIMAKER.MidDevice!=null && JAIMAKER.MidDevice.keyState[i])
                        wb = 0xFF00FF00;
                    if (d2DBoxCollideNoTangent(WindowPos + new Vector2(TotalOffset * 15, 30), WindowPos + new Vector2(TotalOffset * 15 + 15, 90), ImGui.GetMousePos()) && ImGui.IsMouseDown(ImGuiMouseButton.Left)  && pressedBlackKey==-1)
                    {
                        wb = 0xFF0000FF;
                        keyPressedCurrentFrame[i] = true;
                        lastPressedKey = i;
                    }
                    DrawList.AddRectFilled(WindowPos + new Vector2(TotalOffset * 15, 30), WindowPos + new Vector2(TotalOffset * 15 + 15, 90), wb);
                    DrawList.AddRect(WindowPos + new Vector2(TotalOffset * 15, 30), WindowPos + new Vector2(TotalOffset * 15 + 15, 90), 0x7f010101);
                    TotalOffset++;
                }
                KeyPos++;             
            }

            KeyPos = 0;
            TotalOffset = 0;

            for (int i = 0; i < 0x88; i++)
            {
                if (KeyPos == 13)
                    KeyPos = 1;
                var wb = (keyPressedCurrentFrame[i] || (JAIMAKER.MidDevice==null? false : JAIMAKER.MidDevice.keyState[i])) ? 0xFF0000FF : 0xFF010101;
                if (KeyPos == 1 || KeyPos == 4 || KeyPos == 6 || KeyPos == 9 || KeyPos == 11 || KeyPos == 13)
                    DrawList.AddRectFilled(WindowPos + new Vector2(((TotalOffset) * 15) - 3, 30), WindowPos + new Vector2(((TotalOffset) * 15) + 7, 70), wb);
                else    
                    TotalOffset++;
                KeyPos++;
            }

            for (int i=0; i < 0x88; i++)
            {
                if (keyPressedCurrentFrame[i] != keyPressedLastFrame[i])
                    keyTrigger(i, keyPressedCurrentFrame[i]);
                keyPressedLastFrame[i] = keyPressedCurrentFrame[i];
            }
            ImGui.Dummy(new Vector2(0, 60f));
            ImGui.Text($"Last Key {lastPressedKey}");
            ImGui.Text($"DSP Tree Depth { JAIDSP2.JAIDSPVoiceManager.treeDepth}");
            ImGui.ProgressBar(JAIDSP2.JAIDSPVoiceManager.treeDepth / 128f);
            if (ImGui.Button("Destroy DSP tree"))
                JAIDSPVoiceManager.destroyAll();
            ImGui.SliderInt("DSP Emulation Ticks", ref JAIMAKER.DSPTickRate, 0, 48000);

        }

        private JAIDSPVoice[] voices = new JAIDSPVoice[0x88];
        private void keyTrigger(int key, bool state)
        {
            //Console.WriteLine($"Piano key pressed {key} - {state}");

            var bnk = JAIMAKER.AAF.InstrumentBanks[JAIMAKER.Project.SelectedBank];
            var ins = bnk.instruments[JAIMAKER.Project.SelectedInstrument];
            if (ins == null)
                return;
      

            if (state==false)
            {
                if (voices[key]!=null)
                {
                    var v = voices[key];
                    voices[key] = null;
                    v.stop();
                }
            } else
            {

                if (ins.Percussion == false)
                {
                    var insD = (JAIM.JStandardInstrumentv1)ins;
                    var keyR = insD.getKeyRegion(key);
                    if (keyR == null)
                        return;
                    var velR = keyR.getVelocity(127);
                    if (velR == null)
                        return;
                    var snd = JAIMAKER.SoundManager.loadSound((byte)velR.WSYSID, (short)velR.WaveID);
                    if (snd == null || snd.buffer == null)
                    {
                        Console.WriteLine($"PianoBoy::keyTrigger failed to create sound buffer for bnk.{velR.WSYSID} wav.{velR.WaveID}");
                        return;
                    }
                    var voice = new JAIDSPVoice(snd.buffer, insD);
                    JAIDSPVoiceManager.addVoice(voice);
                    voice.play();
                    voices[key] = voice;
                    voice.setPitchMatrix(0, (float)Math.Pow(2, (key - snd.descriptor.BaseKey) / 12f) * insD.Pitch * velR.Pitch);
                } else
                {
                    if (key >= 100)
                        return;
                    var insD = (JAIM.JPercussion)ins;
                    var prcE = insD.Sounds[key];                   
                    if (prcE == null)
                        return;
                    var velR = prcE.getVelocity(127);
                    if (velR == null)
                        return;
                    var snd = JAIMAKER.SoundManager.loadSound((byte)velR.WSYSID, (short)velR.WaveID);
                    if (snd == null || snd.buffer == null)
                    {
                        Console.WriteLine($"PianoBoy::keyTrigger failed to create sound buffer for bnk.{velR.WSYSID} wav.{velR.WaveID}");
                        return;
                    }
                    var voice = new JAIDSPVoice(snd.buffer, insD);
                    JAIDSPVoiceManager.addVoice(voice);
                    voice.play();
                    voices[key] = voice;
                    voice.setPitchMatrix(0, velR.Pitch);

                }

            }           
        }
    }
}
