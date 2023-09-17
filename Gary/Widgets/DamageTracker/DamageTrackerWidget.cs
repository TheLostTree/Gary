using System.Diagnostics;
using Common;
using Common.Protobuf;
using DNToolKit.AnimeGame;
using DNToolKit.AnimeGame.Models;
using Gary.Extentions;
using Gary.Interfaces;
using Gary.Widgets.DamageTracker.Entity;
using Gary.Widgets.DamageTracker.Util;
using ImGuiNET;
using Veldrid.Sdl2;

namespace Gary.Widgets.DamageTracker;
using ui = ImGuiNET.ImGui;

public class DamageTrackerWidget: IPacketConsumer, IWidget
{
    private World world = new World();

    private Thread t;
    private Queue<AnimeGamePacket> packQueue = new();

    private object lockobj = new object();

    public void InsertPacket(AnimeGamePacket p)
    {
        packQueue.Enqueue(p);
    }

    private void ThreadMain()
    {
        while (true)
        {
            while (packQueue.TryDequeue(out var p))
            {
                lock (lockobj)
                {
                    ProcessPacket(p);
                }
            }
            
            Thread.Sleep((1/60) * 1000);
        }
        
    }

    private void ProcessPacket(AnimeGamePacket p)
    {
        
        if(p.ProtoBuf is null) return;

        
        world.now = p.Metadata?.SentMs ?? world.now;
        
        var msg = p.ProtoBuf!;
        bool final = false;
        // try
        // {
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
                    var pkt = AnimeGamePacket.ParseRaw(unionCmd.Body.ToByteArray(), p.MetadataBytes,unionCmd.MessageId,
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
                //trust server invokes more
                var cin = (CombatInvocationsNotify)msg;
                foreach (var combatInvokeEntry in cin.InvokeList)
                {
                    world.OnCombatInvoke(combatInvokeEntry, p.Sender);
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
        // }
        // catch (Exception e)
        // {
        //     // Console.WriteLine(e.ToString());
        //     throw e;
        // }
    }


    private int selectedRow = -1;
    private AvatarEntity? selectedAvatar;
    private void ShowDamageTable()
    {

        if (ui.Button("Reset"))
        {
            world.currentTeam.Reset();
        }
        if (ui.BeginTable("damage view table", 4))
        {
            ui.TableSetupScrollFreeze(0, 1);
            
            
            ui.TableSetupColumn("Name");
            ui.TableSetupColumn("Total Damage");
            ui.TableSetupColumn("Dps");
            ui.TableSetupColumn("% of total damage");
            ui.TableHeadersRow();

            foreach (var avatarEntity in world.currentTeam.avatars)
            {
                ui.TableNextRow();
                int row = ui.TableGetRowIndex();

                int i = 0;
                ui.TableSetColumnIndex(i++);
                if (ui.Selectable(avatarEntity.name, row == selectedRow, ImGuiSelectableFlags.SpanAllColumns))
                {
                    selectedRow = row;
                }
                
                ui.TableSetColumnIndex(i++);
                // Console.WriteLine(avatarEntity.totalDamageDealt);
                if (ui.Selectable($"{NumberFormat.Format(avatarEntity.totalDamageDealt)}", row == selectedRow, ImGuiSelectableFlags.SpanAllColumns))
                {
                    selectedRow = row;
                }
                
                ui.TableSetColumnIndex(i++);
                double dps = avatarEntity.totalDamageDealt*1000/(world.currentTeam.encounterLengthMs);
                // if (dps == 0)
                // {
                //     Console.WriteLine("uh");
                // }
                if (ui.Selectable($"{NumberFormat.Format(dps)}", row == selectedRow, ImGuiSelectableFlags.SpanAllColumns))
                {
                    selectedRow = row;
                }
                
                ui.TableSetColumnIndex(i);
                if (ui.Selectable($"{NumberFormat.Format(avatarEntity.totalDamageDealt*100/world.currentTeam.totalDamage)}%", row == selectedRow, ImGuiSelectableFlags.SpanAllColumns))
                {
                    selectedRow = row;
                }

                if (selectedRow == row)
                {
                    selectedAvatar = avatarEntity;
                }

            }
            ui.EndTable();
        }

        if (selectedAvatar is not null)
        {
            if (ui.BeginTable("attack source view table", 2, ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable))
            {
                ui.TableSetupColumn("Ability Name");
                ui.TableSetupColumn("Damage Instances");
                ui.TableHeadersRow();
                foreach (var keyValuePair in selectedAvatar.attackSource)
                {
                    // Console.WriteLine(keyValuePair.Key);
                    ui.TableNextRow();
                    ui.TableNextColumn();
                    ui.Text(keyValuePair.Key);
                    ui.TableNextColumn();
                    ui.Text(keyValuePair.Value.count.ToString());
                }
                
                ui.EndTable();
            }
            
        }
    }
    

    public DamageTrackerWidget()
    {
        WidgetName = "Damage Tracker";
        t = new Thread(ThreadMain);
    }

    public string WidgetName { get; }
    public bool isShow { get; set; }
    public void DoUI()
    {
        // ui.SetNextWindowSize(ui.GetMainViewport().Size * 0.75f);
        if (ui.Begin("Damage Tracker Widget"))
        {
            ShowDamageTable();
            ui.End();
        }

    }
}