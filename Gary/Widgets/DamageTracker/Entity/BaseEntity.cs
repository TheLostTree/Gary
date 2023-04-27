
using Common.Protobuf;
using Vector = Common.Protobuf.Vector;

namespace Gary.Widgets.DamageTracker.Entity;

public class BaseEntity
{
    public uint EntityId;
    public uint Id;
    public Vector CurrentPos;
    
    
    public BaseEntity? Owner;

    //backup, don't rely on etc
    protected SceneEntityInfo _info;

    public Dictionary<FightProp, float> FightProps;

    protected BaseEntity()
    {
        CurrentPos = new Vector();
        this.FightProps = new Dictionary<FightProp, float>();
    }

    protected BaseEntity(SceneEntityInfo info)
    {
        _info = info;
        EntityId = info.EntityId;
        CurrentPos = info.MotionInfo.Pos;
        FightProps = info.FightPropList.ToDictionary(x => (FightProp)x.PropType, pair => pair.PropValue);

    }


}