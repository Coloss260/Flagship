using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.FTLDrive;

[Serializable, NetSerializable]
public record struct FTLDriveStatsData(FTLDriveState State, float Range, TimeSpan CoolDown, TimeSpan StableTime, TimeSpan StartUp, float PowerDraw, TimeSpan CoolDownFinishedTime, TimeSpan CoolDownFailureTime);

[Serializable, NetSerializable]
public record struct FTLDriveData(FTLDriveState State, float PowerDraw, TimeSpan CoolDownFinishedTime, TimeSpan CoolDownFailureTime);

[Serializable, NetSerializable]
public sealed class FTLDriveBuiState(FTLDriveData driveData) : BoundUserInterfaceState
{
    public readonly FTLDriveData DriveData = driveData;
}

[Serializable, NetSerializable]
public sealed class FTLChargeButtonPressedMessage : BoundUserInterfaceMessage;

