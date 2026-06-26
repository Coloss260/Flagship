using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Shared.Claws.Components;

/// <summary>
/// This is claw component used for <see cref="FlagShip.Shared.Claws.SharedClawsSystem"/> System.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ClawsComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<ClawPrototype> ClawStage;

    [DataField, AutoNetworkedField]
    public Dictionary<int, ProtoId<ClawPrototype>> Claws;

    [DataField]
    public LocId? ClawGrowthNotification;

    [DataField]
    public TimeSpan GrowTimer = TimeSpan.Zero;

    [DataField]
    public TimeSpan AccumulatedBonusGrowth = TimeSpan.Zero;

    [DataField]
    public TimeSpan DeclawItemHoldTimer = TimeSpan.Zero;
}
