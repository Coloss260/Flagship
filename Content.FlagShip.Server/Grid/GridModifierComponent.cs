using Content.FlagShip.Shared.Grid;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Server.Grid;

[RegisterComponent]
public sealed partial class GridModifierComponent : Component
{
    [DataField]
    public List<ProtoId<GridModificationPrototype>> Modifications = [];
}
