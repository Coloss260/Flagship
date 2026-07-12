using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.FTLDrive;

/// <summary>
/// Added to a shuttle when a warp drive fully starts up and passes in its range, removed on warp drive shut down
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShuttleFTLDriveComponent : Component
{
    /// <summary>
    /// The actual FTL drive entity that is on the shuttle
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? FTLDriveEntity;
}
