using Common.Protobuf;
using Gary.Extentions;
using Gary.Widgets.DamageTracker.Util;

namespace Gary.Widgets.DamageTracker.Entity;

public class AvatarEntity : BaseEntity
{
    public double totalDamageDealt = 0;
    public double totalHealed = 0;

    public string name;
    
    public AvatarEntity(SceneEntityInfo info) : base(info)
    {
        Id = info.Avatar.AvatarId;
        name = FriendlyNameUtil.FriendlyNames.GetOrDefault(Id, Id.ToString());
    }

    public void Reset()
    {
        totalDamageDealt = 0;
        totalHealed = 0;
    }
}