using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.NoDeconstruct;

/// <summary>
/// Prevents construction/deconstruction interactions when present on an entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NoDeconstructComponent : Component
{
}
