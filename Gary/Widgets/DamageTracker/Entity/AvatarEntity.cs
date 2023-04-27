using Common.Protobuf;

namespace Gary.Widgets.DamageTracker.Entity;

public class AvatarEntity : BaseEntity
{
    public AvatarEntity(SceneEntityInfo info) : base(info)
    {
        Id = info.Avatar.AvatarId;
    }
}