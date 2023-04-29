using Common.Protobuf;

namespace Gary.Widgets.DamageTracker.Entity;

public class AvatarEntity : BaseEntity
{
    public double totalDamageDealt = 0;
    public double totalHealed = 0;
    
    public AvatarEntity(SceneEntityInfo info) : base(info)
    {
        Id = info.Avatar.AvatarId;
    }

    public void Reset()
    {
        totalDamageDealt = 0;
        totalHealed = 0;
    }
}