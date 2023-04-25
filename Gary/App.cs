using System.Numerics;
using SharpPcap.LibPcap;
using Veldrid.Sdl2;

namespace Gary;
using ImGuiNET;
using Veldrid;
using Veldrid.StartupUtilities;

using ui = ImGuiNET.ImGui;

public class App
{
    private Sdl2Window _window;
    private GraphicsDevice _gd;
    private CommandList _cl;
    private ImGuiRenderer _imguiRenderer;
    
    
    public App()
    {
        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, 960, 540, WindowState.Normal, "ImGui Test"),
            out _window,
            out _gd);
        

        _cl = _gd.ResourceFactory.CreateCommandList();

        // [1]
        _imguiRenderer = new ImGuiRenderer(
            _gd,
            _gd.MainSwapchain.Framebuffer.OutputDescription,
            _window.Width,
            _window.Height);

        _window.Resized += () =>
        {
            _imguiRenderer.WindowResized(_window.Width, _window.Height);
            _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
        };

        descs = _interfaces.Select(x => $"{x.Name} - {x.Description}").ToArray();
    }

    private IEnumerable<PcapInterface> _interfaces = DNToolKit.DNToolKit.GetAllNetworkInterfaces();
    private string[] descs;

    private List<IWidget> _widgets = new List<IWidget>();

    private void Initialize()
    {
        parsingstuff = new ParsingStuff();
        var netwidge = new NetTrafficWidget();
        _widgets.Add(netwidge);
        parsingstuff.consumers.Add(netwidge);
    }

    public void Start()
    {
        Initialize();
        
        while (_window.Exists)
        {
            var snapshot = _window.PumpEvents();
            _imguiRenderer.Update(1f / 60f, snapshot); // [2]

            // unfortunately doesnt seem to work :(
            // ui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            
            DrawUi();

            _cl.Begin();
            _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(0.13f, 0.18f, 0.31f, 1f));
            _imguiRenderer.Render(_gd, _cl); // [3]
            _cl.End();

            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers(_gd.MainSwapchain);
        }
    }

    private int choice = -1;
    private ParsingStuff parsingstuff;


    private void ShowFileMenu()
    {
        if (ui.MenuItem("New")) {}
        if (ui.MenuItem("Open", "Ctrl+O")) {}
        if (ui.BeginMenu("Open Recent"))
        {
            ui.MenuItem("fish_hat.c");
            ui.MenuItem("fish_hat.inl");
            ui.MenuItem("fish_hat.h");
            if (ui.BeginMenu("More.."))
            {
                ui.MenuItem("Hello");
                ui.MenuItem("Sailor");
    
                ui.EndMenu();
            }
            ui.EndMenu();
        }
        if (ui.MenuItem("Save", "Ctrl+S")) {}
        if (ui.MenuItem("Save As..")) {}
    }

    private void ShowWidgetsMenu()
    {
        foreach (IWidget widg in _widgets)
        {
            if (ui.MenuItem(widg.WidgetName))
            {
                widg.isShow = !widg.isShow;
            }
        }
    }
    private void ShowNetworkMenu()
    {
        if (ui.MenuItem("Reset Pcap interface"))
        {
            //todo: ensure old one is disposed in an actually clean way
            parsingstuff.Dispose();
            parsingstuff = new ParsingStuff();
        }
    }

    private void ShowMainMenu()
    {
        if (ui.BeginMainMenuBar())
        {
            if (ui.BeginMenu("File"))
            {
                ShowFileMenu();
                ui.EndMenu();
            }
            if (ui.BeginMenu("Widgets"))
            {
                ShowWidgetsMenu();
                ui.EndMenu();
            }


            if (ui.BeginMenu("Network"))
            {
                ShowNetworkMenu();
                ui.EndMenu();
            }

            ui.EndMainMenuBar();
        }
    }

    private void DrawUi()
    {

        //only  one main window on da bottom

        if (!parsingstuff.HasStarted)
        {
            Vector2 center = ui.GetMainViewport().GetCenter();
            ui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            ui.OpenPopup("Choose PcapInterface");
        }

        // ui.ShowDemoWindow();
        
        
        if (ui.BeginPopupModal("Choose PcapInterface"))
        {
            ui.Text("Choose a Pcap Interface to listen to:");
            // parsingstuff.Start();
            
            // if (ui.Combo("Choose PcapInterface", ref choice, descs, descs.Length,))
            // {
            //     ui.CloseCurrentPopup();
            //     parsingstuff.Start(_interfaces.ToArray()[choice]);
            // }

            string? cur_item = null;
            if (ui.BeginCombo("###combo", descs[0], ImGuiComboFlags.HeightLarge))
            {
                foreach (var s in descs)
                {
                    bool is_selected = cur_item == s;
                    if (ui.Selectable(s, is_selected))
                        cur_item = s;
                    if (is_selected)
                        ui.SetItemDefaultFocus();   
                }
                ui.EndCombo();
            }

            if (cur_item is not null)
            {
                ui.CloseCurrentPopup();
                var inter = _interfaces.Where(x => cur_item == $"{x.Name} - {x.Description}").FirstOrDefault();
                if (inter is not null)
                {
                    parsingstuff.Start(inter);
                }
            }
            
            
            
            ui.EndPopup();
        }
        
        if(!parsingstuff.HasStarted) return;
        ShowMainMenu();
        
        //fullscreen
        ShowBaseScreen();
        
        foreach(IWidget widge in _widgets)
        {
            if(widge.isShow) widge.DoUI();
        }
    }



    private bool useworkarea = true;
    private void ShowBaseScreen()
    {
        var vp = ui.GetMainViewport();
        ui.SetNextWindowPos(useworkarea ? vp.WorkPos : vp.Pos);
        ui.SetNextWindowSize(useworkarea ? vp.WorkSize : vp.Size);
        if (ui.Begin("window",
                ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings))
        {
            ui.Text(this.parsingstuff.GetInterfaceName());
        }
    }
}