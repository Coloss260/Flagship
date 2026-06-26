using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.FlagShip.Shared.ForceParent;

[RegisterComponent, NetworkedComponent]
public sealed partial class ForceParentComponent : Component
{
    [DataField]
    public EntityCoordinates Position;
}
