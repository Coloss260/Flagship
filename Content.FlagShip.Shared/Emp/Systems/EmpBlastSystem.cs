using Content.FlagShip.Shared.Emp.Components;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.FlagShip.Shared.Emp.Systems;

public sealed partial class EmpBlastSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Components.EmpBlastComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, Components.EmpBlastComponent component, ComponentStartup args)
    {
        component.StartTime = _timing.RealTime;

        // try to get despawn time or keep default duration time
        if (TryComp<TimedDespawnComponent>(uid, out var despawn))
        {
            component.VisualDuration = despawn.Lifetime;
        }
    }
}
