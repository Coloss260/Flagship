using Robust.Shared.Prototypes;

namespace Content.FlagShip.Shared.Grid;

/// <summary>
/// This prototypes stores all grid modifiers to process them.
/// </summary>
[Prototype("gridModifier")]
public sealed partial class GridModificationPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public List<GridModifier> Modifiers = [];
}
