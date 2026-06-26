using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Implants;

/// <summary>
/// Implant to get BibleUser status (to pray, summon familiars, bless with bibles)
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BibleUserImplantComponent : Component;
