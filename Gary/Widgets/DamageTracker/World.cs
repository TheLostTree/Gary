using System.Collections.Concurrent;
using Common.Protobuf;
using DNToolKit.AnimeGame.Models;
using Gary.Extentions;
using Gary.Widgets.DamageTracker.Entity;
using Gary.Widgets.DamageTracker.Util;
using Newtonsoft.Json;
using JsonFormatter = Google.Protobuf.JsonFormatter;

namespace Gary.Widgets.DamageTracker;

public partial class World
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
    

    #endregion

}