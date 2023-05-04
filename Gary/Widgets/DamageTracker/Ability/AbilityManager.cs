using Common.Protobuf;

namespace Gary.Widgets.DamageTracker.Ability;

public class AbilityManager
{
    public void onAbilitySyncStateInfo(AbilitySyncStateInfo info)
    {
        foreach (var abilityAppliedAbility in info.AppliedAbilities)
        {
            
            
        }

        foreach (var appliedModifier in info.AppliedModifiers)
        {
            
        }
        foreach (var dynamicValue in info.DynamicValueMap)
        {
            
        }
        
        foreach (var mixinRecover in info.MixinRecoverInfos)
        {
            
        }
        foreach (var sgvDynamicValue in info.SgvDynamicValueMap)
        {
            
        }
    }
    
    public Dictionary<uint, String> Abilities = new();

}