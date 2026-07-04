using Robust.Shared.Prototypes;

namespace Content.FlagShip.Shared.ModSystem.Prototypes;

/// <summary>
/// Used by <see cref="ModSystem"/> and <see cref="ModifiableComponent"/> to mark what aspects an entity has that can be "modified", basically just it's systems, e.g. WarpDriveRange
/// </summary>
[Prototype]
public sealed partial class ModAspectPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; set; } = default!;
}
