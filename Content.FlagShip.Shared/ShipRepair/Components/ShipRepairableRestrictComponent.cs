using Content.Shared.Whitelist;

namespace Content.FlagShip.Shared.ShipRepair.Components;

/// <summary>
/// Add to entity to restrict tools that can repair it.
/// </summary>
[RegisterComponent]
public sealed partial class ShipRepairableRestrictComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist ToolWhitelist = new();
}
