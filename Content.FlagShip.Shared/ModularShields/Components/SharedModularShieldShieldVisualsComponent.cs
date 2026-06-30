using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.ModularShields.Components;

[Virtual, NetworkedComponent, AutoGenerateComponentState]
public partial class SharedModularShieldShieldVisualsComponent : Component
{
    /// <summary>
    /// The color of this shield.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color ShieldColor = Color.White;

    /// <summary>
    /// The extra padding of this shield.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Padding = 50f;
}
