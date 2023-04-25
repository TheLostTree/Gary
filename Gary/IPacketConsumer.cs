using DNToolKit.AnimeGame;

namespace Gary;

public interface IPacketConsumer
{
    public void InsertPacket(AnimeGamePacket p);
}