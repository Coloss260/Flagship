using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Species.Systems;

[Serializable, NetSerializable]
public sealed partial class CoolOffDoAfterEvent : SimpleDoAfterEvent;
