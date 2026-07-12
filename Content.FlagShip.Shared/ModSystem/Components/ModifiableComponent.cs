using Content.FlagShip.Shared.ModSystem.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Shared.ModSystem.Components;

/// <summary>
/// Holds all the current modifiers that an entity has, also allows it to BE modified.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ModifiableComponent : Component
{
    public List<ModifierPrototype> CurrentModifiers = [];

    [DataField]
    public List<ProtoId<ModAspectPrototype>> AllowedAspects = [];
}
