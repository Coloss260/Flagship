namespace Content.FlagShip.Shared.ModularShields.Components;

[RegisterComponent]
public sealed partial class ModularShieldShieldedComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid Shield;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Source;
}
