using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Shared.Surgery.Conditions;

/// <summary>
///     Requires that this surgery is (not) done on one of the provided body prototypes
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryBodyConditionComponent : Component
{
    [DataField(required: true)]
    public HashSet<ProtoId<BodyPrototype>> Accepted = default!;

    [DataField]
    public bool Inverse;
}
