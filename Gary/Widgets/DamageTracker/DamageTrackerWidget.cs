using Common;
using Common.Protobuf;
using DNToolKit.AnimeGame;
using DNToolKit.AnimeGame.Models;
using Gary.Interfaces;

namespace Gary.Widgets.DamageTracker;
using ui = ImGuiNET.ImGui;

public class DamageTrackerWidget: IPacketConsumer, IWidget
{
    private World world = new World();

    public void InsertPacket(AnimeGamePacket p)
    {
        if(p.ProtoBuf is null) return;
        
        var msg = p.ProtoBuf!;
        bool final = false;
        try
        {
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
                        var pkt = AnimeGamePacket.ParseRaw(unionCmd.Body.ToByteArray(), unionCmd.MessageId,
                            Sender.Client);
                        InsertPacket(pkt);
                    }

                    break;
                case Opcode.ClientAbilityInitFinishNotify:
                    world.OnClientAbilityInitFinish((ClientAbilityInitFinishNotify)msg);
                    break;
                case Opcode.ClientAbilityChangeNotify:
                    world.OnClientAbilityChange((ClientAbilityChangeNotify)msg);
                    break;
                case Opcode.AbilityChangeNotify:
                    world.OnAbilityChange((AbilityChangeNotify)msg);
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
                case Opcode.EvtCreateGadgetNotify:
                    world.OnGadgetCreate((EvtCreateGadgetNotify)msg);
                    break;
                case Opcode.EvtDestroyGadgetNotify:
                    world.OnGadgetDestroy((EvtDestroyGadgetNotify)msg);
                    break;
                case Opcode.EntityFightPropNotify:
                    world.OnEntityFightProp((EntityFightPropNotify)msg);
                    break;
                case Opcode.EntityFightPropUpdateNotify:
                    world.OnEntityFightPropUpdate((EntityFightPropUpdateNotify)msg);
                    break;
                case Opcode.AvatarFightPropUpdateNotify:
                case Opcode.AvatarFightPropNotify:
                    break;
                default:
                    final = true;
                    break;
            }

            if (!final)
            {
                // Console.WriteLine(p.PacketType.ToString());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
        ui.SetNextWindowSize(ui.GetMainViewport().Size * 0.75f);
        if (ui.Begin("Damage Tracker Widget"))
        {
            ui.End();
        }

    }
}