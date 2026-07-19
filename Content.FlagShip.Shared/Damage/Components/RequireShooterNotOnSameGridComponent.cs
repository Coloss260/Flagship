using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Damage.Components;

/// <summary>
/// Prevent the object from getting hit by projectiles and hitscans if the shooter was on the same grid.
/// Based on Content.Shared.Damage.Components.RequireProjectileTargetComponent
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RequireShooterNotOnSameGridComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Active = true;
}
