using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Contraband.Events;

/// <summary>
/// Raised on a client request pallet sale
/// </summary>
[Serializable, NetSerializable]
public sealed class ContrabandPalletSellMessage : BoundUserInterfaceMessage
{

}
