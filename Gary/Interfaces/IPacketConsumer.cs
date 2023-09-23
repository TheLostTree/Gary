using DNToolKit;


namespace Gary.Interfaces;

public interface IPacketConsumer
{
    public void InsertPacket(TrafficInstance.TrafficPacket p);
}