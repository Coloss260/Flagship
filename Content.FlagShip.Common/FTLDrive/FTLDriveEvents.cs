namespace Content.FlagShip.Common.FTLDrive;

/// <summary>
/// Used to get the range a shuttle should be able to FTL <see cref="SharedShuttleSystem"/>
/// </summary>
/// <param name="Range">The max range the shuttle can FTL</param>
[ByRefEvent]
public record struct GetFTLDriveRangeEvent(float Range);
