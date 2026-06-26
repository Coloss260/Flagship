using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Clothing;

/// <summary>
/// The component toggles the UnremoveableClothingComponent.
/// </summary>
[NetworkedComponent]
[RegisterComponent]
[Access(typeof(UnremovableClothingSystem))]
public sealed partial class UnremovableClothingRemoverComponent : Component
{
    /// <summary>
    /// Whitelist for UnremoveableClothingComponent entities. If null, works on all entities with the component.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist = null;
}
