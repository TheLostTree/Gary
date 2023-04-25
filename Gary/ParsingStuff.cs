
using DNToolKit.Configuration.Models;
using SharpPcap.LibPcap;
using DNToolKit;
using DNToolKit.Configuration;
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
        dntk = new DNTK(ConfigurationProvider.LoadConfig<SniffConfig>(),x);
        t = new Thread(ThreadMain);
        intfname = $"{x.Name} - {x.Description}";
        t.IsBackground = true;
        HasStarted = true;
        t.Start();
    }

    private async void ThreadMain()
    {
        dntk.PacketReceived += (_, p) =>
        {
            consumers.ForEach(x=>{x.InsertPacket(p);});
        };
        
        await dntk.RunAsync();
    }

    private string intfname;
    public List<IPacketConsumer> consumers = new List<IPacketConsumer>();

    public String GetInterfaceName()
    {

        return intfname;
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