
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using Common;
using DNToolKit.Configuration.Models;
using SharpPcap.LibPcap;
using DNToolKit;
using DNToolKit.AnimeGame;
using DNToolKit.AnimeGame.Models;
using DNToolKit.Configuration;
using Gary.Interfaces;
using DNTK = DNToolKit.DNToolKit;

namespace Gary;

public class ParsingStuff
{



    public Channel<TrafficInstance.TrafficPacket> Channel;

    private Thread t;
    public ParsingStuff()
    {

        Channel = System.Threading.Channels.Channel.CreateUnbounded<TrafficInstance.TrafficPacket>();
        instance = new TrafficInstance(Channel.Writer);
        t = new Thread(() =>
        {
            var r = Channel.Reader;

            while (true)
            {
                if (r.TryRead(out var p))
                {
                    OnPacket(p);
                }
            }
        });

    }

    private static void OnPacket(TrafficInstance.TrafficPacket p)
    {
        
    }
    private TrafficInstance instance;

    public void Start(PcapInterface x)
    {
        

        
    }

    private async void ThreadMain()
    {

    }


    private void SendToAllConsumers(TrafficInstance.TrafficPacket p)
    {
        foreach (var x in consumers)
        {
            x.InsertPacket(p);
            // if (sw.ElapsedMilliseconds > 1)
            // {
            // }
            Console.WriteLine($"consumer {x} >= 1ms {sw.ElapsedTicks}");
        }
    }



    private string? intfname;
    public List<IPacketConsumer> consumers = new();

    public string GetInterfaceName()
    {
        return intfname ?? "None";
    }

}