using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Shipyard.Components;

/// <summary>
/// Tied to an ID card when a ship is purchased. 1 ship per captain.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedShipyardSystem), typeof(FlagShip.Shared.ShuttleRecords.SharedShuttleRecordsSystem), typeof(SharedShuttleConsoleLockSystem))]
public sealed partial class ShuttleDeedComponent : Component
{
    public const int MaxNameLength = 30;
    public const int MaxSuffixLength = 3 + 1 + 4; // 3 digits, dash, up to 4 letters - should be enough

    [DataField]
    public EntityUid? ShuttleUid = null;

    [DataField]
    public string? ShuttleName = "Unknown";

    [DataField("shuttleSuffix")]
    public string? ShuttleNameSuffix;

    [DataField]
    public string? ShuttleOwner = "Unknown";

    [DataField]
    public bool PurchasedWithVoucher;

    /// <summary>
    /// The EntityUid of the voucher used to purchase this ship, stored as a string.
    /// Only relevant if PurchasedWithVoucher is true.
    /// </summary>
    [DataField]
    public string? PurchaseVoucherUid;

    /// <summary>
    /// The ID card entity that holds this deed
    /// </summary>
    [DataField]
    public EntityUid? DeedHolder;
}
