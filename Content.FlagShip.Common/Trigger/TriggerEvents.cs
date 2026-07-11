using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Common.Trigger;

/// <summary>
/// Raised on the target that an entity with OnTriggerCollide is colliding with
/// Can be used to cancel the triggering of effects while still colliding with the target.
/// </summary>
/// <param name="Origin"></param>
/// <param name="Cancelled"></param>
[ByRefEvent]
public record struct TriggerOnCollideActivationAttemptEvent(EntityUid Origin, bool Cancelled = false)
{

}
