using Common.Protobuf;
using Gary.Extentions;
using Gary.Widgets.DamageTracker.Entity;
using Gary.Widgets.DamageTracker.Util;
using Newtonsoft.Json;
using JsonFormatter = Google.Protobuf.JsonFormatter;

namespace Gary.Widgets.DamageTracker;

public class World
{
    private Dictionary<uint, BaseEntity> _entities = new();

    public World()
    {
        
    }


    public void RegisterEntity(BaseEntity entity)
    {
        if (entity is GadgetEntity gadgetEntity)
        {
            //
            // Console.WriteLine("starting search....");
            // Console.WriteLine(JsonConvert.SerializeObject(gadgetEntity));
            // foreach (var vec in abilityinvokecreategadget)
            // {
            //     var xdiff = entity.CurrentPos.X - vec.X;
            //     var ydiff = entity.CurrentPos.Y - vec.Y;
            //     var zdiff = entity.CurrentPos.Z - vec.Z;
            //
            //     if (zdiff < 10 && ydiff < 10 && xdiff < 10)
            //     {
            //         Console.WriteLine("Possible match: ");
            //         Console.WriteLine(JsonFormatter.Default.Format(vec));
            //         
            //     }
            // } 
            // Console.WriteLine("ended search....");

        }
        _entities[entity.EntityId] = entity;
    }
    
    public void DeregisterEntity(BaseEntity entity)
    {
        DeregisterEntity(entity.EntityId);
    }
    public void DeregisterEntity(uint entityId)
    {
        if (_entities.ContainsKey(entityId))
        {
            foreach (var keyValuePair in _entities.Where(x=>x.Value.Owner?.EntityId == entityId))
            {
                keyValuePair.Value.Owner = null;
            }
        }

        Task.Run(() =>
        {
            //don't remove it *immediately*
            Task.Delay(5000);
            _entities.Remove(entityId);
        });
    }
    public BaseEntity? GetEntity(uint id)
    {
        return _entities.TryGetValue(id, out BaseEntity? value) ? value : null;
    }

    public BaseEntity? FromSceneEntityInfo(SceneEntityInfo info)
    {
        switch (info.EntityType)
        {
            case ProtEntityType.ProtEntityAvatar:
                return new AvatarEntity(info);
            case ProtEntityType.ProtEntityMonster:
                var monst = new MonsterEntity(info);
                monst.Owner = GetEntity(info.Monster.OwnerEntityId);
                return monst;
            case ProtEntityType.ProtEntityGadget:
                var gadg = new GadgetEntity(info);
                gadg.Owner = GetEntity(info.Gadget.OwnerEntityId);
                return gadg;
            case ProtEntityType.ProtEntityNpc:
                break;
        }

        return null;
    }

    public void EnterScene(int sceneId)
    {
        
    }

    public BaseEntity GetRootEntityOwner(BaseEntity entity)
    {
        var et = entity;
        while (et.Owner is not null)
        {
            //this statement is probably extra, but i dont care enough to check lol
            if (et.Owner is not null)
            {
                et = et.Owner!;
            }
        }

        return et;
    }

    private List<Vector> abilityinvokecreategadget = new List<Vector>();


    #region OnPacketEvent

    public void OnAbilityInvoke(AbilityInvokeEntry entry, bool fromServer = false)
    {
        return;
        BaseEntity? entity = GetEntity(entry.EntityId);
        if (entity is null)
        {
            Console.WriteLine("Missing entity: " + entry.EntityId);
        }
        byte[] data = entry.AbilityData.ToByteArray();
        switch (entry.ArgumentType)
        {
            case AbilityInvokeArgument.AbilityMetaAddNewAbility:
                break;
            case AbilityInvokeArgument.AbilityMetaRemoveAbility:
                break;
            case AbilityInvokeArgument.AbilityActionCreateGadget:
                var g = AbilityActionCreateGadget.Parser.ParseFrom(data);
                if (!g.Pos.Equals(new Vector()))
                {
                    abilityinvokecreategadget.Add(g.Pos);
                }

                break;
            default:
                break;
        }
    }

    public void OnClientAbilityInitFinish(ClientAbilityInitFinishNotify notify)
    {
        foreach (var abilityInvokeEntry in notify.Invokes)
        {
            OnAbilityInvoke(abilityInvokeEntry);
            
        }
    }
    public void OnClientAbilityChange(ClientAbilityChangeNotify notify)
    {
        foreach (var abilityInvokeEntry in notify.Invokes)
        {
            OnAbilityInvoke(abilityInvokeEntry);
        }
    }

    public void OnAbilityChange(AbilityChangeNotify notify)
    {
        
    }

    public void OnCombatInvoke(CombatInvokeEntry entry)
    {
        var bytes = entry.CombatData.ToByteArray()!;
        switch (entry.ArgumentType)
        {
            case CombatTypeArgument.CombatEvtBeingHit:
                EvtBeingHitInfo info = EvtBeingHitInfo.Parser.ParseFrom(bytes);
                var attacker = GetEntity(info.AttackResult.AttackerId);
                var defender = GetEntity(info.AttackResult.DefenseId);
                if (attacker is not null)
                {
                    Console.WriteLine(
                        $"{info.AttackResult.Damage} to {FriendlyNameUtil.FriendlyNames.GetOrDefault(defender?.Id ?? 0, "Defender")} " +
                        $"by {FriendlyNameUtil.FriendlyNames.GetOrDefault(GetRootEntityOwner(attacker).Id, "Attacker")}");
                }
                
                break;
            case CombatTypeArgument.EntityMove:
            {
                var em = EntityMoveInfo.Parser.ParseFrom(bytes);
                var et = GetEntity(em.EntityId);
                if(et is null) return;
                et.CurrentPos = em.MotionInfo.Pos;
            }
                break;
            case CombatTypeArgument.CombatBeingHealedNtf:
                break;
            default:
                //CombatSteerMotionInfo
                // Console.WriteLine(entry.ArgumentType);
                break;
        }
    }

    public void OnSceneTeamUpdate(SceneTeamUpdateNotify notify)
    {
        // Console.WriteLine(JsonFormatter.Default.Format(notify));
        var newTeam = new List<uint>();
        foreach (var sceneTeamAvatar in notify.SceneTeamAvatarList)
        {
            if (sceneTeamAvatar.EntityId == 0) continue;
            if (_entities.ContainsKey(sceneTeamAvatar.EntityId))
            {
                Console.WriteLine("there is already a valid entity, so skipping");
                newTeam.Add(sceneTeamAvatar.EntityId);

            };
            AvatarEntity avatarEntity = FromSceneEntityInfo(sceneTeamAvatar.SceneEntityInfo) as AvatarEntity;
            if (avatarEntity is null)
            {
                Console.WriteLine("wtf?");
            };
            
            RegisterEntity(avatarEntity);
            Console.WriteLine("New Avatar: " + FriendlyNameUtil.FriendlyNames[avatarEntity.Id]);
            newTeam.Add(sceneTeamAvatar.EntityId);
        }
        
        //remove old avatars
        // foreach (var entity in _entities.Where(x=>x.Value is AvatarEntity).Select(x=> (x.Value as AvatarEntity)!).Where(x=>!newTeam.Contains(x.EntityId)))
        // {
        //     Console.WriteLine("Removing avatar: " + FriendlyNameUtil.FriendlyNames[entity.Id]);
        //     DeregisterEntity(entity);
        // }
        foreach (var (key, value) in _entities)
        {
            if (value is AvatarEntity ae)
            {
                if (!newTeam.Contains(ae.EntityId))
                {
                    //remove
                    Console.WriteLine("Removing avatar: " + FriendlyNameUtil.FriendlyNames[ae.Id]);
                    DeregisterEntity(ae.EntityId);
                }
            }
        }
        
        Console.WriteLine($"New Team! ${String.Join(',', newTeam.Select(x=>GetEntity(x) as AvatarEntity).Select(x=> FriendlyNameUtil.FriendlyNames[x.Id]))}");
    }

    public void OnSceneEntityAppear(SceneEntityAppearNotify notify)
    {
        foreach (var sei in notify.EntityList)
        {
            if(sei is null) continue;
            if(sei.EntityType == ProtEntityType.ProtEntityAvatar) continue;
            var et = FromSceneEntityInfo(sei);
            if(et is not null)
                RegisterEntity(et);
            
        }
    }

    public void OnSceneDisappearNotify(SceneEntityDisappearNotify notify)
    {
        foreach (var entityId in notify.EntityList)
        {
            var et = GetEntity(entityId);
            if(et is null) continue;
            if(et is AvatarEntity) continue;
            DeregisterEntity(et);
        }
    }

    public void OnGadgetCreate(EvtCreateGadgetNotify notify)
    {
        GadgetEntity g = new GadgetEntity(notify);
        g.Owner = GetEntity(notify.OwnerEntityId);
        
        RegisterEntity(g);
    }
    public void OnGadgetDestroy(EvtDestroyGadgetNotify notify)
    {
        DeregisterEntity(notify.EntityId);
    }

    public void OnEntityFightPropUpdate(EntityFightPropUpdateNotify notify)
    {
        var et = GetEntity(notify.EntityId);
        if (et is null) return;
        foreach (var (key, value) in notify.FightPropMap)
        {
            et.FightProps[(FightProp)key] = value;
        }
    }

    public void OnEntityFightProp (EntityFightPropNotify notify)
    {
        var et = GetEntity(notify.EntityId);
        if (et is null) return;
        foreach (var (key, value) in notify.FightPropMap)
        {
            et.FightProps[(FightProp)key] = value;
        }
    }
    public void OnAvatarFightPropUpdate()
    {
        
    }

    public void OnAvatarFightProp()
    {
        
    }
    #endregion

}