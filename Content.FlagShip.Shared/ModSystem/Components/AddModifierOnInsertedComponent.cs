using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.ModSystem.Components;

/// <summary>
/// Adds modifiers in <see cref="ModifierComponent"/> if the container has <see cref="ModifiableComponent"/> when inserted.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AddModifierOnInsertedComponent : Component;
