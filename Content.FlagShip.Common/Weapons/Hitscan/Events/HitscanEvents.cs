using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Common.Weapons.Hitscan.Events;

/// <summary>
/// Raised on the target that is the next possible target for a hitscan ray.
/// This is raised before the target is hit and can be cancelled to have the hitscan not hit and proceed onto it's next target instead.
/// </summary>
/// <param name="Origin"></param>
/// <param name="Cancelled"></param>
[ByRefEvent]
public record struct HitscanHitAttemptEvent(EntityUid Origin, bool Cancelled = false)
{

}


/// <summary>
/// Raised on the target that is about to take hitscan damage (regardless of if they have a DamageableComponent).
/// This is raised before the damage is applied and can be cancelled to prevent the damage from being applied.
/// </summary>
/// <param name="Origin">Entity that fired the hitscan.</param>
/// <param name="DamageToTake">Amount of damage to be dealt.</param>
/// <param name="Cancelled"></param>
[ByRefEvent]
public record struct HitscanDamageAttemptEvent(EntityUid Origin, float DamageToTake, bool Cancelled = false)
{

}
