using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.ScuttleDevice;

[Serializable, NetSerializable]
public sealed partial class ScuttleArmDoAfterEvent : SimpleDoAfterEvent {}

[Serializable, NetSerializable]
public sealed partial class ScuttleDisarmDoAfterEvent : SimpleDoAfterEvent {}
