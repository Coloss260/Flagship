using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Trade;

/// <summary>
/// Stores the visuals for trade crates.
/// </summary>
[Serializable, NetSerializable]
public enum TradeCrateVisuals : byte
{
    IsPriority,
    IsPriorityInactive,
    DestinationIcon,
}
