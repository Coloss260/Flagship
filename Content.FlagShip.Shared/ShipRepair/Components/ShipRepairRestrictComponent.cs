using Content.Shared.Whitelist;

namespace Content.FlagShip.Shared.ShipRepair.Components;

/// <summary>
/// Add to grid to restrict tools that can repair it.
/// </summary>
[RegisterComponent]
public sealed partial class ShipRepairRestrictComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist ToolWhitelist = new();
}
