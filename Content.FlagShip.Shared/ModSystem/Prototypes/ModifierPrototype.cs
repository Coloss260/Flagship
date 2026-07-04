using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.ModSystem.Prototypes;

/// <summary>
/// Prototype used by the ModSystem to find the current mod of an aspect of an entity.
/// </summary>
[Prototype]
public sealed partial class ModifierPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField(required: true)]
    public float Modifier;

    [DataField(required: true)]
    public required ProtoId<ModAspectPrototype> Aspect;

    [DataField(required: true)]
    public ModifierType ModifierType;
}

[Serializable, NetSerializable]
public enum ModifierType : byte
{
    Addition, // Adds value (also use for subtraction)
    Multiplication, // Multiples by value (also use for division)
    Set, // Sets to value
}
