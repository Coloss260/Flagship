using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.ModularShields.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModularShieldShieldedComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid Shield;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Source;
}
