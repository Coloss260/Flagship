using Content.Shared.Inventory;

namespace Content.FlagShip.Shared.Surgery.Steps;

[ByRefEvent]
public record struct SurgeryCanPerformStepEvent(
    EntityUid User,
    EntityUid Body,
    List<EntityUid> Tools,
    SlotFlags TargetSlots,
    string? Popup = null,
    StepInvalidReason Invalid = StepInvalidReason.None,
    Dictionary<EntityUid, float>? ValidTools = null
) : IInventoryRelayEvent;
