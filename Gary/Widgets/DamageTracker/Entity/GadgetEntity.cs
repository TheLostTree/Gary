using Common.Protobuf;

namespace Gary.Widgets.DamageTracker.Entity;

public class GadgetEntity : BaseEntity
{
    public GadgetEntity(SceneEntityInfo info) : base(info)
    {
        Id = info.Gadget.GadgetId;
    }

    public GadgetEntity(EvtCreateGadgetNotify notify)
    {
        //I THINK!
        this.Id = notify.ConfigId;
        EntityId = notify.EntityId;
        CurrentPos = notify.InitPos;
        
    }
}