using DNToolKit.AnimeGame;

namespace Gary.Interfaces;

public interface IPacketConsumer
{
    public void InsertPacket(AnimeGamePacket p);
}