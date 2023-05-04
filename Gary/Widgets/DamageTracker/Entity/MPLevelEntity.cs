using Common.Protobuf;

namespace Gary.Widgets.DamageTracker.Entity;

public class MPLevelEntity : BaseEntity
{

    public string name = "MPLevelEntity";

    public MPLevelEntity(MPLevelEntityInfo info)
    {
        this.EntityId = info.EntityId;
        this.CurrentPos = new Vector();
        this.Id = 0;
    }
}