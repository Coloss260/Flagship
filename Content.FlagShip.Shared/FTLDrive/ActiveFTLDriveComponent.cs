using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.FTLDrive;

/// <summary>
/// Used to keep track of "active" FTL drives, e.g. when charging
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveFTLDriveComponent : Component;
