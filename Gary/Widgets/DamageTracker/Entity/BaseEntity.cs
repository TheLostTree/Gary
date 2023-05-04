
using Common.Protobuf;
using Gary.Extentions;
using Gary.Widgets.DamageTracker.Ability;
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

    public AbilityManager AbilityManager = new AbilityManager();

    protected BaseEntity()
    {
        CurrentPos = new Vector();
        FightProps = new Dictionary<FightProp, float>();
    }
    
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

    public string GetFriendlyName()
    {
        return FriendlyNameUtil.FriendlyNames.GetOrDefault(Id, Id.ToString());
    }

    public void AddAbility(uint instancedAbilityId, uint hash, uint overrideHash = 0)
    {
        string abilityName = EmbryoUtil.Embryos.GetOrDefault(hash, $"Unknown_{hash}");
        Console.WriteLine($"Adding ability {abilityName} to {GetFriendlyName()}");
        AbilityManager.Abilities[instancedAbilityId] = abilityName;    

    }
    
    public void AddAbility(uint instancedAbilityId, string abilityName, string abilityOverrideName = "")
    {
        Console.WriteLine($"Adding ability {abilityName} to {GetFriendlyName()}");
        AbilityManager.Abilities[instancedAbilityId] = abilityName;
    }

    public void RemoveAbility(uint instancedAbilityId)
    {
        Console.WriteLine($"Removing ability {AbilityManager.Abilities[instancedAbilityId]} from {GetFriendlyName()}");
        AbilityManager.Abilities.Remove(instancedAbilityId);
    }


}