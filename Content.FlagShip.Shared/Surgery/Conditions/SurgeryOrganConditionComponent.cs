using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Shared.Surgery.Conditions;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryOrganConditionComponent : Component
{
    [DataField]
    public ComponentRegistry? Organ;

    [DataField]
    public bool Inverse;

    [DataField]
    public bool Reattaching;
}
