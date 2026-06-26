using Content.FlagShip.Shared.Surgery.Conditions;
using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Surgery.Steps;

/// <summary>
/// Adds an organ slot the body part when the step is complete.
/// Requires <see cref="SurgeryOrganSlotConditionComponent"/> on
/// the surgery entity in order to specify the organ slot.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryAddOrganSlotStepComponent : Component;
