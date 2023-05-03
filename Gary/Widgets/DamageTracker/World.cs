using System.Collections.Concurrent;
using Common.Protobuf;
using DNToolKit.AnimeGame.Models;
using Gary.Extentions;
using Gary.Widgets.DamageTracker.Entity;
using Gary.Widgets.DamageTracker.Util;
using Newtonsoft.Json;
using JsonFormatter = Google.Protobuf.JsonFormatter;

namespace Gary.Widgets.DamageTracker;

public class World
{
    private ConcurrentDictionary<uint, BaseEntity> _entities = new();


    public ulong now = 0;
    

    public World()
    {
        
    }

    //change to a Team class idk

    public Team currentTeam = new Team();


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
            // Task.Delay(5000);
            _entities.Remove(entityId, out var unused);
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
        
        BaseEntity? entity = GetEntity(entry.EntityId);
        if (entity is null)
        {
            // Console.WriteLine("Missing entity: " + entry.EntityId);
            return;
        }
        byte[] data = entry.AbilityData.ToByteArray();
        switch (entry.ArgumentType)
        {
            case AbilityInvokeArgument.AbilityMetaAddNewAbility:
                var metaAddAbility = AbilityMetaAddAbility.Parser.ParseFrom(data);
                switch (metaAddAbility.Ability.AbilityName.TypeCase)
                {
                    case AbilityString.TypeOneofCase.Hash:
                        
                        entity.AddAbility(metaAddAbility.Ability.InstancedAbilityId, metaAddAbility.Ability.AbilityName.Hash, metaAddAbility.Ability.AbilityOverride.Hash);
                        break;
                    case AbilityString.TypeOneofCase.Str:
                        entity.AddAbility(metaAddAbility.Ability.InstancedAbilityId, metaAddAbility.Ability.AbilityName.Str, metaAddAbility.Ability.AbilityOverride.Str);
                        break;
                    default:
                        //cry
                        break;
                }
                break;
            case AbilityInvokeArgument.AbilityMetaRemoveAbility:
                
                entity.RemoveAbility(entry.Head.InstancedAbilityId);
                break;
            case AbilityInvokeArgument.AbilityActionCreateGadget:
                
                var g = AbilityActionCreateGadget.Parser.ParseFrom(data);
                if (!g.Pos.Equals(new Vector()))
                {
                    abilityinvokecreategadget.Add(g.Pos);
                }
                Console.WriteLine($"new gadget spawned by: {entry.EntityId} bc of {JsonFormatter.Default.Format(entry.Head)}");

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
        var ent = GetEntity(notify.EntityId);
        if(ent is null) return;
        
        foreach (var abilityEmbryo in notify.AbilityControlBlock.AbilityEmbryoList)
        {
            //theres an override hash as well? idk if i
            ent.AddAbility(abilityEmbryo.AbilityId, abilityEmbryo.AbilityNameHash, abilityEmbryo.AbilityOverrideNameHash);
        }
    }

    public void OnCombatInvoke(CombatInvokeEntry entry, Sender s)
    {
        var bytes = entry.CombatData.ToByteArray()!;
        
        switch (entry.ArgumentType)
        {
            case CombatTypeArgument.CombatEvtBeingHit:
                if(s == Sender.Client) break;
                EvtBeingHitInfo info = EvtBeingHitInfo.Parser.ParseFrom(bytes);
                var attacker = GetEntity(info.AttackResult.AttackerId);
                var defender = GetEntity(info.AttackResult.DefenseId);
                
                
                if (attacker is not null)
                {
                    var owner = GetRootEntityOwner(attacker);
                    
                    

                    if (info.AttackResult.AbilityIdentifier is null)
                    {
                        //null for sword carrying characters?
                        break;
                    }
                    //
                    // var caster = GetEntity(info.AttackResult.AbilityIdentifier.AbilityCasterId);
                    // owner = caster;
                    var instancedAbilityId = info.AttackResult.AbilityIdentifier.InstancedAbilityId;
                    var abilityName =
                        owner.Abilities.GetOrDefault(instancedAbilityId,
                            "unknown :(");
                    if (abilityName == "unknown :(")
                    {
                        Console.WriteLine(instancedAbilityId);
                        Console.WriteLine(info.AttackResult.AbilityIdentifier.AbilityCasterId);
                        var caster = GetEntity(info.AttackResult.AbilityIdentifier.AbilityCasterId);
                        if (caster is not null)
                        {
                            var casterowner = GetRootEntityOwner(caster);
                        }
                        
                    }
                    Console.WriteLine(
                        $"{info.AttackResult.Damage} to {FriendlyNameUtil.FriendlyNames.GetOrDefault(defender?.Id ?? 0, "Defender")} " +
                        $"by {FriendlyNameUtil.FriendlyNames.GetOrDefault(owner.Id, "Attacker")} with ability {abilityName}");
                    
                    

                    if (owner is AvatarEntity)
                    {
                        currentTeam.AddDamageEvent(new DamageEvent()
                                            {
                                                attackerId = owner.EntityId,
                                                damage =  info.AttackResult.Damage,
                                                healed = 0,
                                                nowMs = now,
                                                abilitySource = abilityName
                                            });
                    }
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
        var newTeam = new List<AvatarEntity>();
        foreach (var sceneTeamAvatar in notify.SceneTeamAvatarList)
        {
            if (sceneTeamAvatar.EntityId == 0) continue;
            if (_entities.ContainsKey(sceneTeamAvatar.EntityId))
            {
                Console.WriteLine("there is already a valid entity, so skipping");
                newTeam.Add(_entities[sceneTeamAvatar.EntityId] as AvatarEntity);
                //todo: check lol
                continue;

            };
            AvatarEntity avatarEntity = FromSceneEntityInfo(sceneTeamAvatar.SceneEntityInfo) as AvatarEntity;
            if (avatarEntity is null)
            {
                Console.WriteLine("wtf?");
            };
            
            RegisterEntity(avatarEntity);
            newTeam.Add(avatarEntity);
        }

        foreach (var (_, value) in _entities)
        {
            if (value is AvatarEntity ae)
            {
                if (!newTeam.Exists(x=>x.EntityId == ae.EntityId))
                {
                    //remove
                    Console.WriteLine("Removing Avatar: " + FriendlyNameUtil.FriendlyNames[ae.Id]);
                    DeregisterEntity(ae.EntityId);
                }
            }
        }
        
        Console.WriteLine($"New Team! {string.Join(", ", newTeam.Select(x=> FriendlyNameUtil.FriendlyNames[x.Id]))}");
        currentTeam.Set(newTeam);
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