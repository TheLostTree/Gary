using Common;
using Common.Protobuf;
using DNToolKit.AnimeGame;
using DNToolKit.AnimeGame.Models;
using Gary.Interfaces;

namespace Gary.Widgets;
using ui = ImGuiNET.ImGui;

internal class Actor
{
    public uint entityId;
    public double totalDmg;
    public double critPercent;

    public int critCount;
    public int hitCount;

    public void AddHitEvt()
    {
        
    }
    
    internal Actor()
    {
        
    }
}
public class DamageTrackerWidget: IPacketConsumer, IWidget
{
    private Dictionary<uint,SceneEntityInfo> _entities = new();

    private Dictionary<uint, AvatarInfo> _avatars = new();

    private List<SceneAvatarInfo> _curTeam = new List<SceneAvatarInfo>();

    public void InsertPacket(AnimeGamePacket p)
    {
        if(p.ProtoBuf is null) return;
        
        var msg = p.ProtoBuf!;
        switch (p.PacketType)
        {
            case Opcode.SceneEntityDisappearNotify:
                var sedn = (SceneEntityDisappearNotify)msg;
                foreach (var entityid in sedn.EntityList)
                {
                    _entities.Remove(entityid);
                }
                break;
            case Opcode.SceneEntityAppearNotify:
                var sean = (SceneEntityAppearNotify)msg;
                foreach (var entity in sean.EntityList)
                {
                    if (_entities.ContainsKey(entity.EntityId!))
                    {
                        _entities[entity.EntityId!] = entity;
                    }
                    else
                    {
                        _entities.Add(entity.EntityId!, entity!);
                    }
                }
                break;
            
            case Opcode.UnionCmdNotify:
                var ucn = (UnionCmdNotify)msg;
                foreach (var unionCmd in ucn.CmdList)
                {
                    var pkt = AnimeGamePacket.ParseRaw(unionCmd.Body.ToByteArray(), unionCmd.MessageId, Sender.Client);
                    InsertPacket(pkt);
                }
                break;
            case Opcode.AbilityInvocationsNotify:
                // this is special; in multiplayer the game will send ability invokes to the client; but not in single player
                // therefore: handle everything probably
                break;
            case Opcode.CombatInvocationsNotify:
                if (p.Sender == Sender.Client)
                {
                    var cin = (CombatInvocationsNotify)msg;
                    foreach (var combatInvokeEntry in cin.InvokeList)
                    {
                        HandleCombatInvoke(combatInvokeEntry);
                    }
                }
                break;
            
            
            case Opcode.AvatarDataNotify:
                var adn = (AvatarDataNotify)msg;
                foreach (var avatarInfo in adn.AvatarList)
                {
                    _avatars.Add(avatarInfo.AvatarId, avatarInfo);
                }
                break;
        }
    }

    public void HandleAbilityInvoke(AbilityInvokeEntry entry)
    {
        
    }

    public void HandleCombatInvoke(CombatInvokeEntry entry)
    {
        //todo: missing the proto defs lol
        //i forgor
        //EntityMoveInfo
        //EvtBeingHitInfo
        //EvtBeingHealedNotify
        
        switch (entry.ArgumentType)
        {
            case CombatTypeArgument.EntityMove:
                break;
            case CombatTypeArgument.CombatEvtBeingHit:
                break;
            case CombatTypeArgument.CombatBeingHealedNtf:
                break;
            case CombatTypeArgument.SyncEntityPosition:
                //maybe?
                break;
        }
    }

    public DamageTrackerWidget()
    {
        WidgetName = "Damage Tracker";
    }

    public string WidgetName { get; }
    public bool isShow { get; set; }
    public void DoUI()
    {
        
    }
}