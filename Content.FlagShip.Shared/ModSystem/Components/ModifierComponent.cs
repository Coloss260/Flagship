using Content.FlagShip.Shared.ModSystem.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Shared.ModSystem.Components;

/// <summary>
/// Used to store the modifiers an entity that can be used as a mod has, you must add another component to actually add the modifier.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ModifierComponent : Component
{
    [DataField]
    public List<ProtoId<ModifierPrototype>> Modifiers = [];
}
