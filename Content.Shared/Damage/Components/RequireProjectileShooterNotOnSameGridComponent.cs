using Content.Shared.Damage.Systems;
using Robust.Shared.GameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Shared.Damage.Components;

/// <summary>
/// Prevent the object from getting hit by projectiles and hitscans if the shooter was on the same grid.
/// Based on Content.Shared.Damage.Components.RequireProjectileTargetComponent
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(RequireProjectileShooterNotOnSameGridSystem))]
public sealed partial class RequireProjectileShooterNotOnSameGridComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Active = true;
}
