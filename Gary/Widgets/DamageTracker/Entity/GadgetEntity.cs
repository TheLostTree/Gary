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
        EntityId = notify.EntityId;
        CurrentPos = notify.InitPos;
    }
}