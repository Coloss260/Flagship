using Content.FlagShip.Shared.Blocking.Components;

namespace Content.FlagShip.Shared.Blocking;

public abstract class SharedBlockingSystem : EntitySystem
{
    public virtual void SetEnabled(EntityUid uid, bool value, Components.BlockingVisualsComponent? component = null)
    {
        if (!Resolve(uid, ref component, false) || component.Enabled == value)
            return;

        component.Enabled = value;
        Dirty(uid, component);
    }
}
