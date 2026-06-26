using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Implants;

/// <summary>
/// Implant to get MimePowers status (to summon walls and take the mime's vow)
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MimePowersImplantComponent : Component;
