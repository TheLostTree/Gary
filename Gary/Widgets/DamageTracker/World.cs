using Common.Protobuf;
using Gary.Widgets.DamageTracker.Entity;

namespace Gary.Widgets.DamageTracker;

public class World
{
    private Dictionary<uint, BaseEntity> _entities = new();

    public World()
    {
        
    }

    public BaseEntity? GetEntity(uint id)
    {
        if (_entities.ContainsKey(id))
        {
            return _entities[id];
        }

        return null;
    }

    public void EnterScene(int sceneId)
    {
        
    }


    #region OnPacketEvent
    public void OnAbilityInvoke(AbilityInvokeEntry entry)
    {
        
    }

    public void OnCombatInvoke(CombatInvokeEntry entry)
    {
        
    }

    public void OnSceneTeamUpdate(SceneTeamUpdateNotify notify)
    {
        
    }

    public void OnSceneEntityAppear(SceneEntityAppearNotify notify)
    {
        
    }

    public void OnSceneDisappearNotify(SceneEntityDisappearNotify notify)
    {
        
    }
    #endregion

}