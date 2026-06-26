using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Kitchen;
[Serializable, NetSerializable]
public sealed partial class ContainerDoAfterEvent : SimpleDoAfterEvent { }
