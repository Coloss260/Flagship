using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.PlantAnalyzer;

[Serializable, NetSerializable]
public sealed partial class PlantAnalyzerDoAfterEvent : SimpleDoAfterEvent
{
}
