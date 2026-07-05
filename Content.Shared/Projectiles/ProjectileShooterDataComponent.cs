using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Shared.Projectiles;

/// <summary>
/// A class for storing data about a projectile's shooter, in the instance that the shooter is deleted as the projectile is shot.
/// Currently intended to assist <see cref="Content.Shared.Damage.Components.RequireProjectileShooterNotOnSameGridComponent"/> in dealing with projectile grenades.
/// </summary>
[RegisterComponent]
public sealed partial class ProjectileShooterDataCacheComponent : Component
{
    /// <summary>
    /// Grid Uid the shooter was on when firing.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ShooterGridUid;
}

