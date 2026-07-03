using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.ModularShields.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModularShieldShieldVisualsComponent : Component
{
    /// <summary>
    /// The color of this shield.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public Color ShieldColor = Color.White;

    /// <summary>
    /// The extra padding of this shield.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Padding = 50f;
}
