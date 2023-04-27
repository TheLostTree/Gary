using System.Numerics;
using Gary.Interfaces;
using Gary.Widgets;
using Gary.Widgets.DamageTracker;
using SharpPcap.LibPcap;
using Veldrid.Sdl2;

namespace Gary;
using ImGuiNET;
using Veldrid;
using Veldrid.StartupUtilities;

using ui = ImGuiNET.ImGui;

public class App
{
    public Sdl2Window _window;
    private GraphicsDevice _gd;
    private CommandList _cl;
    private Gary.ImGuiRenderer _imguiRenderer;

    private bool alwaysOnTop;
    
    public App()
    {
        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, 960, 540, WindowState.Normal, "Gary Toolkit"),
            out _window,
            out _gd);
        _window.SetCloseRequestedHandler(OnCloseAttempt);

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

    private bool OnCloseAttempt()
    {
        if (parsingstuff is not null)
        {
            parsingstuff.Dispose();
        }

        return false;
    }

    private IEnumerable<PcapInterface> _interfaces = DNToolKit.DNToolKit.GetAllNetworkInterfaces();
    private string[] descs;

    private List<IWidget> _widgets = new();

    private void Initialize()
    {
        parsingstuff = new ParsingStuff();
        var netwidge = new NetTrafficWidget();
        var entitywidge = new EntityListWidget();
        var damagetrack = new DamageTrackerWidget();
        _widgets.Add(netwidge);
        _widgets.Add(entitywidge);
        _widgets.Add(damagetrack);
        parsingstuff.consumers.Add(netwidge);
        parsingstuff.consumers.Add(entitywidge);
        parsingstuff.consumers.Add(damagetrack);
    }

    public void Start()
    {
        Initialize();
        // unfortunately doesnt seem to work :(
        ui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        
        while (_window.Exists)
        {
            var snapshot = _window.PumpEvents();
            _imguiRenderer.Update(1f / 60f, snapshot); // [2]

            
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


    private void ShowSettingsMenu()
    {
        if (ui.MenuItem("New"))
        {
            alwaysOnTop = !alwaysOnTop;
        }
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
            if (ui.BeginMenu("Settings"))
            {
                ShowSettingsMenu();
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
            ui.SetNextWindowSize(ui.GetMainViewport().Size * 0.8f);
            ui.SetWindowFontScale(2);

            ui.OpenPopup("Choose PcapInterface");
        }

        // ui.ShowDemoWindow();
        
        
        if (ui.BeginPopupModal("Choose PcapInterface"))
        {

            ui.Text("Choose a Pcap Interface to listen to:");

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
            ui.SetWindowFontScale(1);

        }
        
        ShowMainMenu();
        
        //fullscreen

        ShowBaseScreen();

        if(!parsingstuff.HasStarted) return;

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
                ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus))
        {
            ui.Text(this.parsingstuff.GetInterfaceName() );
        }
    }
}