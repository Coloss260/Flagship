
using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Atmos.Piping.Binary.Messages;

[Serializable, NetSerializable]
public sealed class GasPressurePumpChangePumpDirectionMessage(bool inwards) : BoundUserInterfaceMessage
{
    public bool Inwards { get; } = inwards;
}
