using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Surgery;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryTargetComponent : Component
{
    [DataField]
    public bool CanOperate = true;
}
