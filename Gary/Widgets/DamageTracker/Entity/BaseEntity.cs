
using Common.Protobuf;
using Gary.Extentions;
using Gary.Widgets.DamageTracker.Util;
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
    
    public Dictionary<ulong, String> Abilities = new Dictionary<ulong, string>();

    protected BaseEntity(SceneEntityInfo info)
    {
        _info = info;
        EntityId = info.EntityId;
        CurrentPos = info.MotionInfo.Pos;
        FightProps = new Dictionary<FightProp, float>();
        foreach (var fightPropPair in info.FightPropList)
        {
            FightProps[(FightProp)fightPropPair.PropType] = fightPropPair.PropValue;
        }

    }

    public void AddAbility(uint instancedAbilityId, uint hash, uint overrideHash = 0)
    {
        string abilityName = EmbryoUtil.Embryos.GetOrDefault(hash, $"Unknown_{hash}");
        Abilities[instancedAbilityId] = abilityName;    

    }
    
    public void AddAbility(uint instancedAbilityId, string abilityName, string abilityOverrideName = "")
    {
        Abilities[instancedAbilityId] = abilityName;
    }

    public void RemoveAbility(uint instancedAbilityId)
    {
        Abilities.Remove(instancedAbilityId);
    }


}