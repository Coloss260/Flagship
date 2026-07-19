using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Projectiles.Components;

/// <summary>
/// A class for storing data about a projectile's shooter at the time of shooting, in the instance that the shooter is deleted as the projectile is shot.
/// Currently intended to assist <see cref="Damage.Components.RequireShooterNotOnSameGridComponent"/> in dealing with projectile grenades and hitscans.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShooterDataCacheComponent : Component
{
    /// <summary>
    /// Grid Uid the shooter was on when firing.
    /// </summary>
    [AutoNetworkedField]
    public EntityUid? ShooterGridUid;
}

