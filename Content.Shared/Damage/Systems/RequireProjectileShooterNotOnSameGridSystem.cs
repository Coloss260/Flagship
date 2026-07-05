using Content.Shared.Damage.Components;
using Content.Shared.Projectiles;
using Robust.Shared.Physics.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Shared.Damage.Systems;

public sealed partial class RequireProjectileShooterNotOnSameGridSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<RequireProjectileShooterNotOnSameGridComponent, PreventCollideEvent>(OnPreventCollide);
    }

    private void OnPreventCollide(Entity<RequireProjectileShooterNotOnSameGridComponent> ent, ref PreventCollideEvent args)
    {
        if (args.Cancelled)
            return;


        if (!ent.Comp.Active)
            return;

        var other = args.OtherEntity;

        if (TryComp(other, out ProjectileComponent? projectile))
        {
            if (projectile.Shooter.HasValue && Transform(ent.Owner).GridUid == Transform(projectile.Shooter.Value).GridUid)
            {
                args.Cancelled = true;
            }
            // ProjectileGrenade origining projectiles will not have a Shooter value.
            else if (TryComp(other, out ProjectileShooterDataCacheComponent? shooterData))
            {
                if (shooterData.ShooterGridUid == Transform(ent.Owner).GridUid)
                {
                    args.Cancelled = true;
                }
            }
        }
    }
}
