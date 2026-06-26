using Content.Shared.Stacks;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.FlagShip.Shared.Contraband.Components;

[RegisterComponent]
[Access(typeof(FlagShip.Shared.Contraband.SharedContrabandTurnInSystem))]
public sealed partial class ContrabandPalletConsoleComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("cashType", serverOnly: true, customTypeSerializer:typeof(PrototypeIdSerializer<StackPrototype>))]
    public string RewardType = "FederationMilitaryCredit";

    [ViewVariables(VVAccess.ReadWrite), DataField(serverOnly: true)]
    public string Faction = "NFSD";

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public string LocStringPrefix = string.Empty;
}
