using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Pirate.Events;

/// <summary>
/// Raised on a client request pallet sale
/// </summary>
[Serializable, NetSerializable]
public sealed class PirateBountyRedemptionMessage : BoundUserInterfaceMessage
{
    public PirateBountyRedemptionMessage()
    {
    }
}
