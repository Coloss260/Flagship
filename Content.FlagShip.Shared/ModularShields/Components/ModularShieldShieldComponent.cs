using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.ModularShields.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModularShieldShieldComponent : Component
{
    public EntityUid? ModularShieldCoreSource;

    public EntityUid ShieldedEntity;
}
