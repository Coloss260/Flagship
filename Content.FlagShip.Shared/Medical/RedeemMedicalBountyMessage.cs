using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Medical;

[Serializable, NetSerializable]
public sealed class RedeemMedicalBountyMessage : BoundUserInterfaceMessage
{
    public RedeemMedicalBountyMessage()
    {
    }
}
