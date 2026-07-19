using Content.FlagShip.Shared.Projectiles.Components;
using Content.FlagShip.Shared.Projectiles.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Shared.Projectiles.Systems;

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
}
