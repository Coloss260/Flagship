namespace Content.FlagShip.Shared.ShipShields;

[RegisterComponent]
public sealed partial class ShipShieldComponent : Component
{
    public EntityUid? Source;
    public EntityUid Shielded;
}
