
using DNToolKit.Configuration.Models;
using SharpPcap.LibPcap;
using DNToolKit;
using DNToolKit.AnimeGame;
using DNToolKit.AnimeGame.Models;
using DNToolKit.Configuration;
using Gary.Interfaces;
using DNTK = DNToolKit.DNToolKit;

namespace Gary;

public class ParsingStuff : IDisposable
{

    private DNTK dntk;

    public bool HasStarted;

    private Thread t;
    public ParsingStuff()
    {
        
    }

    public void Start(PcapInterface x)
    {
        var cfg = ConfigurationProvider.LoadConfig<SniffConfig>();
        #if true
        cfg.LoadPackets = false;
        cfg.PersistPackets = false;
        
        #endif
        dntk = new DNTK(cfg,x);
        t = new Thread(ThreadMain);
        intfname = $"{x.Name} - {x.Description}";
        // t.IsBackground = true;
        HasStarted = true;
        t.Start();
    }

    private async void ThreadMain()
    {
        dntk.PacketReceived += (_, p) =>
        {
            foreach (var x in consumers)
            {
                x.InsertPacket(AnimeGamePacket.ParseRaw(p.ProtoBufBytes.ToArray(), (uint)p.PacketType, p.Sender));
            }

            // if (p.Sender == Sender.Server)
            // {
            //     Console.WriteLine(p.PacketType.ToString());
            // }
        };
        
        await dntk.RunAsync();
    }

    private string? intfname;
    public List<IPacketConsumer> consumers = new();

    public String GetInterfaceName()
    {
        
        return intfname ?? "None";
    }

    public void Dispose()
    {
        dntk.Close();
        Console.WriteLine("closed dntk");

        t.Join();
        Console.WriteLine("joined thread");

        GC.SuppressFinalize(this);
    }
}