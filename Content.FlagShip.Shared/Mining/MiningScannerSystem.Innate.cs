using Content.FlagShip.Shared.Mining.Components;
using Content.Shared.Mining.Components;

namespace Content.FlagShip.Shared.Mining;

public sealed partial class MiningScannerSystem : EntitySystem
{

    /// <inheritdoc/>
    public void NFInitialize()
    {
        SubscribeLocalEvent<Components.InnateMiningScannerViewerComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<Components.InnateMiningScannerViewerComponent> ent, ref ComponentStartup args)
    {
        if (!HasComp<MiningScannerViewerComponent>(ent))
        {
            SetupInnateMiningViewerComponent(ent);
        }
    }

    private void SetupInnateMiningViewerComponent(Entity<Components.InnateMiningScannerViewerComponent> ent)
    {
        var comp = EnsureComp<MiningScannerViewerComponent>(ent);
        comp.ViewRange = ent.Comp.ViewRange;
        comp.PingDelay = ent.Comp.PingDelay;
        comp.PingSound = ent.Comp.PingSound;
        comp.QueueRemoval = false;
        comp.NextPingTime = _timing.CurTime + ent.Comp.PingDelay;
        Dirty(ent.Owner, comp);
    }
}
