using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Shared.Clothing.Components;

/// <summary>
/// This is used for clothing that makes an entity weightless when worn.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(FlagShip.Shared.Clothing.EntitySystems.SharedNFMoonBootsSystem))]
public sealed partial class NFMoonBootsComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> MoonBootsAlert = "MoonBoots";

    /// <summary>
    /// Slot the clothing has to be worn in to work.
    /// </summary>
    [DataField]
    public string Slot = "shoes";
}
