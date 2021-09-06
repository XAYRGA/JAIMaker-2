using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using System.Numerics;
using static ImGuiNET.ImGuiNative;


namespace JAIMaker_2.GUI
{
    public abstract class Window
    {
        public string Title;
        public bool Visible = true;

        public abstract void init();
        public abstract void draw();
    }

    public class WindowManager
    {

        private Sdl2Window _window;
        private GraphicsDevice _gd;
        private CommandList _cl;
        private Texture JAIMLogo; 

        private ImGuiController _controller;
        private Vector3 _clearColor = new Vector3(0.0f, 0.0f, 0.0f);

        private Dictionary<string, Window> windows = new Dictionary<string, Window>() {
            
        };


        public void addWindow(string name, Window win)
        {
            windows[name] = win;
            win.init();
            Console.WriteLine($"Added window {name}");
        }
        public void init()
        {
            VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, 1024, 1024, WindowState.Normal, "JAIMaker 2"),
            new GraphicsDeviceOptions(true, null, true),
            out _window,
            out _gd);
            _window.Resized += () =>
            {
                _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                _controller.WindowResized(_window.Width, _window.Height);
            };


            _cl = _gd.ResourceFactory.CreateCommandList();
            _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);


            var style = ImGui.GetStyle();
            style.FrameRounding = 4.0f;
            style.WindowBorderSize = 0.0f;
            style.PopupBorderSize = 0.0f;
            style.GrabRounding = 4.0f;

           
            var colors = style.Colors;
            colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.73f, 0.75f, 0.74f, 1.00f);
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.09f, 0.09f, 0.09f, 0.94f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);
            colors[(int)ImGuiCol.Border] = new Vector4(0.20f, 0.20f, 0.20f, 0.50f);
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.40f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.67f);
            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.47f, 0.22f, 0.22f, 0.67f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.47f, 0.22f, 0.22f, 1.00f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.47f, 0.22f, 0.22f, 0.67f);
            colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.34f, 0.16f, 0.16f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.02f, 0.02f, 0.02f, 0.53f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
            colors[(int)ImGuiCol.CheckMark] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
            colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.71f, 0.39f, 0.39f, 1.00f);
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.84f, 0.66f, 0.66f, 1.00f);
            colors[(int)ImGuiCol.Button] = new Vector4(0.47f, 0.22f, 0.22f, 0.65f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.71f, 0.39f, 0.39f, 0.65f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.20f, 0.20f, 0.20f, 0.50f);
            colors[(int)ImGuiCol.Header] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.65f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.00f);
            colors[(int)ImGuiCol.Separator] = new Vector4(0.43f, 0.43f, 0.50f, 0.50f);
            colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f);
            colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f);
            colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f);
            colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f);
            colors[(int)ImGuiCol.Tab] = new Vector4(0.71f, 0.39f, 0.39f, 0.54f);
            colors[(int)ImGuiCol.TabHovered] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f);
            colors[(int)ImGuiCol.TabActive] = new Vector4(0.84f, 0.66f, 0.66f, 0.66f);
            colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.07f, 0.10f, 0.15f, 0.97f);
            colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.14f, 0.26f, 0.42f, 1.00f);
            colors[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
            colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
            colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
            colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
            colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
            colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);          

        }

        public void update()
        {
            if (_window == null)
                return;
            InputSnapshot snapshot = _window.PumpEvents();
            if (!_window.Exists) { return; }
            _controller.Update(1f / 60f, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

            ImGui.BeginMainMenuBar();

            if (ImGui.BeginMenu("JAIMaker 2        "))
            {
                if (ImGui.MenuItem("Save"))
                    JAIMAKER.Project.save();
                ImGui.EndMenu();
            }
           

            if (ImGui.BeginMenu("Windows"))
            {
                foreach (KeyValuePair<string, Window> winDict in windows)
                    if (ImGui.MenuItem(winDict.Value.Title))
                        winDict.Value.Visible = !winDict.Value.Visible;
                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();



            ImGui.DockSpaceOverViewport();
            //for (int i = 0; i < windows.Count; i++)
            foreach (KeyValuePair<string, Window> winDict in windows)            
                if (winDict.Value.Visible)
                {
                    ImGui.Begin(winDict.Value.Title);
                    winDict.Value.draw();
                    ImGui.End();
                }
            

            _cl.Begin();
            _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
            _controller.Render(_gd, _cl);
            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers(_gd.MainSwapchain);
        }

    }
}
