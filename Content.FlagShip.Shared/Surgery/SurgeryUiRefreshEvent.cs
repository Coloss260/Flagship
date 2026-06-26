using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Surgery;

[Serializable, NetSerializable]
public sealed class SurgeryUiRefreshEvent : EntityEventArgs
{
    public NetEntity Uid { get; }

    public SurgeryUiRefreshEvent(NetEntity uid)
    {
        Uid = uid;
    }
}
