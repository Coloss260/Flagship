using Content.FlagShip.Common.Projectiles.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Common.Projectiles.Systems;

public sealed partial class ShooterDataCacheSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ShooterDataCacheComponent, ShooterUpdatedEvent>(OnShooterUpdated);
    }

    private void OnShooterUpdated(Entity<ShooterDataCacheComponent> ent, ref ShooterUpdatedEvent args)
    {
        ent.Comp.ShooterGridUid = Transform(args.Shooter).GridUid;
    }

    [ByRefEvent]
    public record struct ShooterUpdatedEvent(EntityUid Shooter)
    {

    }
}
