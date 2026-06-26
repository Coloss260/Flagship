using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Atmos.Visuals;

[Serializable, NetSerializable]
public enum GasDepositExtractorVisuals : byte
{
    State,
}

[Serializable, NetSerializable]
public enum GasDepositExtractorState : byte
{
    Off, // Not pumping.
    On, // Actively pumping, lots of gas left.
    Low, // Actively pumping, not much gas left.
    Blocked, // Not pumping, gas left.
    Empty, // Not pumping, no gas left.
}
