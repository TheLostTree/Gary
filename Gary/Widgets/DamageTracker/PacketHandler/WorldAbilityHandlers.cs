#define FIXLATER
using Common.Protobuf;
using DNToolKit.AnimeGame.Models;
using Gary.Extentions;
using Gary.Widgets.DamageTracker.Ability;
using Gary.Widgets.DamageTracker.Entity;
using Gary.Widgets.DamageTracker.Util;
using Google.Protobuf;

namespace Gary.Widgets.DamageTracker;

public partial class World
{
    public void OnAbilityInvoke(AbilityInvokeEntry entry, bool fromServer = false)
    {
        
        BaseEntity? entity = GetEntity(entry.EntityId);
        if (entity is null)
        {
            Console.WriteLine("Missing entity: " + entry.EntityId);
            return;
        }

        var localId = entry.Head.LocalId;
        var targetId = entry.Head.TargetId;
        var instancedAbilityId = entry.Head.InstancedAbilityId;
        var instancedModifierId = entry.Head.InstancedModifierId;
        var configLocalId = entry.Head.ModifierConfigLocalId;
        
        
        byte[] data = entry.AbilityData.ToByteArray();
        //meta modifier change, meta global float, reinit override map
        switch (entry.ArgumentType)
        {
            case AbilityInvokeArgument.None:
                if (entity.AbilityManager.Abilities.TryGetValue(instancedAbilityId, out var abilityName))
                {
                    // undo localId = (uint)Type + (ModifierIndex << 3) + (MixinIndex << 9) + (ConfigIndex << 15) + (ActionIndex << 21)  
                    var type = (int)localId & 0b111;
                    var modifierIndex = (int)(localId >> 3) & 0b111111111;
                    var mixinIndex = (int)(localId >> 9) & 0b111111111;
                    var configIndex = (int)(localId >> 15) & 0b111111111;
                    var actionIndex = (int)(localId >> 21) & 0b111111111;
                    //Console.WriteLine($"log: {abilityName} {type} {modifierIndex} {mixinIndex} {configIndex} {actionIndex}");
                }
                break;
            // case AbilityInvokeArgument.AbilityMetaAddNewAbility:
            case AbilityInvokeArgument.MetaAddNewAbility:
                var metaAddAbility = AbilityMetaAddAbility.Parser.ParseFrom(data);
                switch (metaAddAbility.Ability.AbilityName.TypeCase)
                {
                    case AbilityString.TypeOneofCase.Hash:
                        
                        entity.AddAbility(metaAddAbility.Ability.InstancedAbilityId, metaAddAbility.Ability.AbilityName.Hash, metaAddAbility.Ability.AbilityOverride.Hash);
                        break;
                    case AbilityString.TypeOneofCase.Str:
                        entity.AddAbility(metaAddAbility.Ability.InstancedAbilityId, metaAddAbility.Ability.AbilityName.Str, metaAddAbility.Ability.AbilityOverride.Str);
                        break;
                }
                break;
            case AbilityInvokeArgument.MetaRemoveAbility:
                
                entity.RemoveAbility(entry.Head.InstancedAbilityId);
                break;
            case AbilityInvokeArgument.ActionCreateGadget:
                var g = AbilityActionCreateGadget.Parser.ParseFrom(data);
                if (!g.Pos.Equals(new Vector()))
                {
                    abilityinvokecreategadget.Add(g.Pos);
                }
                Console.WriteLine($"new gadget spawned by: {entry.EntityId} bc of {JsonFormatter.Default.Format(entry.Head)}");

                break;
            case AbilityInvokeArgument.MetaTriggerElementReaction:
                var er = AbilityMetaTriggerElementReaction.Parser.ParseFrom(data);
                var sourcetype = er.ElementSourceType;
                var reactortype = er.ElementReactorType;
                var triggerEntity = GetEntity(er.TriggerEntityId);

                var owner = GetRootEntityOwner(triggerEntity);
                if (triggerEntity != owner)
                {
                    Console.WriteLine("trigger entity is not owner");
                }
                Console.WriteLine($"{owner.GetFriendlyName()} triggered {((ElementalReactionType)er.ElementReactionType).ToString()}");
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
            case CombatTypeArgument.EvtBeingHit:
                if(s == Sender.Client) break;
                EvtBeingHitInfo info = EvtBeingHitInfo.Parser.ParseFrom(bytes);

                if (info.AttackResult is null) break;
                var attacker = GetEntity(info.AttackResult.AttackerId);
                var defender = GetEntity(info.AttackResult.DefenseId);
                
                
                if (attacker is not null)
                {
                    var owner = GetRootEntityOwner(attacker);

                    if (info.AttackResult.AbilityIdentifier is not null)
                    {
                        var instancedAbilityId = info.AttackResult.AbilityIdentifier?.InstancedAbilityId;
                        var instancedModifierId = info.AttackResult.AbilityIdentifier?.InstancedModifierId;

                        var modifierOwnerId = info.AttackResult.AbilityIdentifier?.ModifierOwnerId;
                        BaseEntity? modifierOwner = null;
                        if (modifierOwnerId is not null)
                        {
                            modifierOwner = GetEntity(modifierOwnerId.Value);
                            
                        }
                        
                        
                        var abilityName = "normal attack or something idk";

                        if (instancedAbilityId is not null)
                        {
                            abilityName = attacker.AbilityManager.Abilities.GetOrDefault(instancedAbilityId.Value,
                                null!);
                            if (abilityName is null)
                            {
                                Console.WriteLine(string.Join("\n", attacker.AbilityManager.Abilities));
                                Console.WriteLine(instancedAbilityId);
                                Console.WriteLine(instancedModifierId);
                                Console.WriteLine(attacker.Id);
                                Console.WriteLine($"attacker entity id: {attacker.EntityId}");
                                abilityName = "unknown ability";
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
            case CombatTypeArgument.BeingHealedNtf:
                break;
            default:
                //CombatSteerMotionInfo
                // Console.WriteLine(entry.ArgumentType);
                break;
        }
    }

}