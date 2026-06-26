using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Claws.Components;

/// <summary>
/// This is used for clipping nails (Claws). See <see cref="FlagShip.Shared.Claws.NailClipperDoAfterEvent"/>
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NailClipperComponent : Component
{
    [DataField]
    public TimeSpan ClipDoAfter = TimeSpan.FromSeconds(4);

    /// <summary>
    /// Amount of stages to skip down. Capped to 0.
    /// </summary>
    [DataField]
    public int StageReduction = 1;

    /// <summary>
    /// Chance of instantly reducing stage to -1 (Declawed)
    /// </summary>
    [DataField]
    public float DeclawChance;

    /// <summary>
    /// Claw types you are allowed to cut
    /// </summary>
    [DataField]
    public List<string> AllowedClawTypes = [];
}
