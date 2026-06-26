using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Traits.Physical;

/// <summary>
/// Adds a self-examine verb for the Self-Aware trait.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SelfAwareComponent : Component;
