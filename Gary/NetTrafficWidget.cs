using System.Net.Mime;
using DNToolKit.AnimeGame;

namespace Gary;

using ui = ImGuiNET.ImGui;

public class NetTrafficWidget : IPacketConsumer, IWidget
{
    private List<AnimeGamePacket> _gamePackets = new List<AnimeGamePacket>();

    private List<string> processed = new List<string>();

    private int count = 0;

    public NetTrafficWidget()
    {
        WidgetName = "Packet Viewer";
    }
    public void DoUI()
    {
        if(ui.Begin("Packet Viewer"))
        {
            for (int i = 0; i < processed.Count; i++)
            {
                // ui.Text($"{i}: {_gamePackets[i].PacketType.ToString()}");
                ui.Text(processed[i]);
            }
            // ui.SetScrollHereY();
        }
    }

    public void InsertPacket(AnimeGamePacket p)
    {
        _gamePackets.Add(p);
        processed.Add($"{++count}: {p.PacketType.ToString()}");
    }

    public string WidgetName { get; }
    public bool isShow { get; set; }
}