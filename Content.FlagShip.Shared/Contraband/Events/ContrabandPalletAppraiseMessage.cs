using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Contraband.Events;

/// <summary>
/// Raised on a client request to refresh the pallet console
/// </summary>
[Serializable, NetSerializable]
public sealed class ContrabandPalletAppraiseMessage : BoundUserInterfaceMessage
{

}
