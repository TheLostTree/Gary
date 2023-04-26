using Common;
using Common.Protobuf;
using DNToolKit.AnimeGame;
using DNToolKit.AnimeGame.Models;
using Gary.Interfaces;
using Gary.Widgets.DamageTracker;

namespace Gary.Widgets;
using ui = ImGuiNET.ImGui;

public class DamageTrackerWidget: IPacketConsumer, IWidget
{
    private World world = new World();

    public void InsertPacket(AnimeGamePacket p)
    {
        if(p.ProtoBuf is null) return;
        
        var msg = p.ProtoBuf!;
        switch (p.PacketType)
        {
            case Opcode.SceneEntityDisappearNotify:
                var sedn = (SceneEntityDisappearNotify)msg;
                world.OnSceneDisappearNotify(sedn);
                break;
            case Opcode.SceneEntityAppearNotify:
                var sean = (SceneEntityAppearNotify)msg;
                world.OnSceneEntityAppear(sean);
                break;
            case Opcode.SceneTeamUpdateNotify:
                var stun = (SceneTeamUpdateNotify)msg;
                world.OnSceneTeamUpdate(stun);
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
                var ain = (AbilityInvocationsNotify)msg;
                foreach (var abilityInvokeEntry in ain.Invokes)
                {
                    world.OnAbilityInvoke(abilityInvokeEntry);
                }
                break;
            case Opcode.CombatInvocationsNotify:
                if (p.Sender == Sender.Client)
                {
                    var cin = (CombatInvocationsNotify)msg;
                    foreach (var combatInvokeEntry in cin.InvokeList)
                    {
                        world.OnCombatInvoke(combatInvokeEntry);
                    }
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
        //
        // switch (entry.ArgumentType)
        // {
        //     case CombatTypeArgument.EntityMove:
        //     {
        //         var mv = EntityMoveInfo.Parser.ParseFrom(entry.CombatData);
        //         if (_entities.ContainsKey(mv.EntityId))
        //         {
        //             var et = _entities[mv.EntityId];
        //             
        //             //maybe its a merge? maybe its a replace idk
        //             et.MotionInfo.MergeFrom(mv.MotionInfo);
        //         }
        //
        //     }
        //         break;
        //     case CombatTypeArgument.CombatEvtBeingHit:
        //     {
        //         var hitinfo = EvtBeingHitInfo.Parser.ParseFrom(entry.CombatData);
        //         if (_entities.ContainsKey(hitinfo.AttackResult.DefenseId) &&
        //             _entities.ContainsKey(hitinfo.AttackResult.AttackerId))
        //         {
        //             var target = _entities[hitinfo.AttackResult.DefenseId];
        //             var attacker = _entities[hitinfo.AttackResult.AttackerId];
        //
        //             var damage = hitinfo.AttackResult.Damage;
        //         }
        //     }
        //     
        //         break;
        //     case CombatTypeArgument.CombatBeingHealedNtf:
        //         break;
        //     case CombatTypeArgument.SyncEntityPosition:
        //         //maybe?
        //         break;
        // }
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