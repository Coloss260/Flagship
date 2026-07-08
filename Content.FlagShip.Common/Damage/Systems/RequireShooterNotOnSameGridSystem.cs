using Content.FlagShip.Common.Damage.Components;
using Content.FlagShip.Common.Projectiles.Components;
using Content.FlagShip.Common.Weapons.Hitscan.Events;
using Robust.Shared.Physics.Events;

namespace Content.FlagShip.Common.Damage.Systems;

public sealed partial class RequireShooterNotOnSameGridSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<RequireShooterNotOnSameGridComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<RequireShooterNotOnSameGridComponent, HitscanHitAttemptEvent>(OnPreventHitscan);
    }

    private void OnPreventCollide(Entity<RequireShooterNotOnSameGridComponent> ent, ref PreventCollideEvent args)
    {
        if (args.Cancelled)
            return;


        if (!ent.Comp.Active)
            return;

        var other = args.OtherEntity;

        // ProjectileGrenade origining projectiles will not have a Shooter value.
        if (TryComp(other, out ShooterDataCacheComponent? shooterData))
        {
            if (shooterData.ShooterGridUid == Transform(ent.Owner).GridUid)
            {
                args.Cancelled = true;
            }
        }
    }



    private void OnPreventHitscan(Entity<RequireShooterNotOnSameGridComponent> ent, ref HitscanHitAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.Active)
            return;

        if (Transform(args.Origin).GridUid == Transform(ent.Owner).GridUid)
        {
            args.Cancelled = true;
        }
    }
}


