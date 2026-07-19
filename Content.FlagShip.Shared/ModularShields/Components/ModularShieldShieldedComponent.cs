using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.ModularShields.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModularShieldShieldedComponent : Component
{
    public EntityUid Shield;

    public EntityUid? Source;
}
