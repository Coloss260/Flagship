namespace Content.FlagShip.Shared.ModularShields.Components;

[RegisterComponent]
public sealed partial class ModularShieldShieldComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ModularShieldCoreSource;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid ShieldedEntity;
}
