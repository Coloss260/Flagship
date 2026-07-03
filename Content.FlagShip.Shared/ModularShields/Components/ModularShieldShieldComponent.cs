namespace Content.FlagShip.Shared.ModularShields.Components;

[RegisterComponent]
public sealed partial class ModularShieldShieldComponent : Component
{
    public EntityUid? ModularShieldCoreSource;
    public EntityUid ShieldedEntity;
}
