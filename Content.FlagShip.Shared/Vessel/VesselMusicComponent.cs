using Content.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Shared.Vessel;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class VesselMusicComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<AmbientMusicPrototype> AmbientMusicPrototype;
}
