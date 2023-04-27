// See https://aka.ms/new-console-template for more information


using Gary.Widgets.DamageTracker.Util;
using Serilog;


namespace Gary;

public class Program
{

    public static App app;
    static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger();
        Log.Information("DNToolKit for v{Major}.{Minor}", 3, 6);
        Console.WriteLine(EmbryoUtil.Embryos.Count);
        Console.WriteLine(FriendlyNameUtil.FriendlyNames.Count);
        app = new App();
        app.Start();
    }

    
}