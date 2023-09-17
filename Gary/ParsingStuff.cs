
using System.Collections.Concurrent;
using System.Diagnostics;
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

    private ConcurrentQueue<AnimeGamePacket> packets = new ConcurrentQueue<AnimeGamePacket>();

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
            packets.Enqueue(p);
            // if (p.Sender == Sender.Server)
            // {
            //     Console.WriteLine(p.PacketType.ToString());
            // }
        };
        
        await dntk.RunAsync();
    }

    private Stopwatch sw = new Stopwatch();

    public int ProcessAllQueuedPackets()
    {
        int incr = 0;
        if (packets.Count == 0) return 0;
        Console.WriteLine("----start");


        while (packets.TryDequeue(out var p) )
        {
            if (incr > 2) break;
            Console.WriteLine($"yay packet {p.PacketType}");
            
            foreach (var x in consumers)
            {
                sw.Restart();
                x.InsertPacket(p);
                sw.Stop();
                // if (sw.ElapsedMilliseconds > 1)
                // {
                // }
                Console.WriteLine($"consumer {x} >= 1ms {sw.ElapsedMilliseconds}");

            }

            incr++;
        }
        Console.WriteLine("----end");

        return incr;
    }

    private string? intfname;
    public List<IPacketConsumer> consumers = new();

    public String GetInterfaceName()
    {
        
        return intfname ?? "None";
    }

    public void Dispose()
    {
        dntk?.Close();
        Console.WriteLine("closed dntk");

        t?.Join();
        Console.WriteLine("joined thread");

        GC.SuppressFinalize(this);
    }
}