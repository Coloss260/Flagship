using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Surgery.Effects.Step;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryStepCavityEffectComponent : Component
{
    [DataField]
    public string Action = "Insert";
}
