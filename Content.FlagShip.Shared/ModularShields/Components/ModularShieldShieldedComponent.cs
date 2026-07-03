namespace Content.FlagShip.Shared.ModularShields.Components;

[RegisterComponent]
public sealed partial class ModularShieldShieldedComponent : Component
{
    public EntityUid Shield;
    public EntityUid? Source;
}
