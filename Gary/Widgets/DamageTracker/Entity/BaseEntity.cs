using System.Numerics;
using Common.Protobuf;

namespace Gary.Widgets.DamageTracker.Entity;

public class BaseEntity
{
    public uint EntityId;
    public Vector3 CurrentPos;
    
    
    public BaseEntity? Owner;

    //backup, don't rely on etc
    protected SceneEntityInfo _info;

    public Dictionary<FightProp, double> FightProps;

    protected BaseEntity()
    {
        
    }

    protected BaseEntity(SceneEntityInfo info)
    {
        _info = info;
        EntityId = info.EntityId;
        CurrentPos = new Vector3(info.MotionInfo.Pos.X, info.MotionInfo.Pos.Y, info.MotionInfo.Pos.Z);

    }

    

    public static BaseEntity? FromSceneEntityInfo(SceneEntityInfo info)
    {
        switch (info.EntityType)
        {
            case ProtEntityType.ProtEntityAvatar:
                return new AvatarEntity(info);
            case ProtEntityType.ProtEntityMonster:
                return new MonsterEntity(info);
            case ProtEntityType.ProtEntityGadget:
                return new GadgetEntity(info);
            case ProtEntityType.ProtEntityNpc:
                break;
            default:
                throw new NotImplementedException();
        }

        return null;
    }
    
    
    

}