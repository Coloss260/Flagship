using Content.FlagShip.Shared.Atmos.Systems;
using Content.FlagShip.Shared.Atmos.Visuals;
using Content.Shared.Atmos;
using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Atmos.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedGasDepositSystem))]
public sealed partial class GasDepositExtractorComponent : Component
{
    /// <summary>
    /// Whether or not the extractor is on and extracting gas.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled;

    /// <summary>
    /// The amount of gas to extract per second, in mol/s.
    /// </summary>
    [DataField]
    public float ExtractionRate;

    /// <summary>
    /// The maximum pressure output, in kPa.
    /// </summary>
    [DataField]
    public float MaxTargetPressure = Atmospherics.MaxOutputPressure;

    [DataField, AutoNetworkedField]
    public float TargetPressure = Atmospherics.OneAtmosphere;

    /// <summary>
    /// The entity to be extracted from.
    /// </summary>
    [DataField]
    public EntityUid? DepositEntity;

    [DataField("port")]
    public string PortName { get; set; } = "port";

    // Storing the last known extraction state.
    [ViewVariables]
    public GasDepositExtractorState LastState { get; set; } = GasDepositExtractorState.Off;
}
