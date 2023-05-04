using Common.Protobuf;
using Gary.Widgets.DamageTracker.Entity;
using Gary.Widgets.DamageTracker.Util;

namespace Gary.Widgets.DamageTracker;

public partial class World
{
    public void OnSceneTeamUpdate(SceneTeamUpdateNotify notify)
    {
        // Console.WriteLine(JsonFormatter.Default.Format(notify));
        var newTeam = new List<AvatarEntity>();
        foreach (var sceneTeamAvatar in notify.SceneTeamAvatarList)
        {
            if (sceneTeamAvatar.EntityId == 0) continue;
            if (_entities.ContainsKey(sceneTeamAvatar.EntityId))
            {
                // Console.WriteLine("there is already a valid entity, so skipping");
                newTeam.Add(_entities[sceneTeamAvatar.EntityId] as AvatarEntity);
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
        // notify.
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

    public void OnPlayerEnterSceneInfoNotify(PlayerEnterSceneInfoNotify notify)
    {
        MPLevelEntity entity = new MPLevelEntity(notify.MpLevelEntityInfo);
    }
}