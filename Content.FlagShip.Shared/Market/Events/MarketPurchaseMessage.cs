using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Market.Events;

/// <summary>
///     When the player purchases an item from the market, this message is sent.
/// </summary>
[Serializable, NetSerializable]
public sealed class MarketPurchaseMessage : BoundUserInterfaceMessage
{
};

