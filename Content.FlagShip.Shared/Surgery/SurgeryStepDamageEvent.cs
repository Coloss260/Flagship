using Content.Shared.Damage;

namespace Content.FlagShip.Shared.Surgery;

/// <summary>
///     Raised on the target entity.
/// </summary>
[ByRefEvent]
public record struct SurgeryStepDamageEvent(EntityUid User, EntityUid Body, EntityUid Part, EntityUid Surgery, DamageSpecifier Damage, float PartMultiplier);
