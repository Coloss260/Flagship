using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.CrateMachine;

[Serializable, NetSerializable]
public enum CrateMachineVisualState : byte
{
    Open,
    Closed,
    Opening,
    Closing,
}

[Serializable, NetSerializable]
public enum CrateMachineVisuals : byte
{
    VisualState,
}
