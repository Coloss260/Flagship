using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.ModularShields.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModularShieldShieldComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ModularShieldCoreSource;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid ShieldedEntity;
}
