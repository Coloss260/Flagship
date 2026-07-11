using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Common.Explosion.Events;

/// <summary>
/// Raised on an entity with an explosive component to mark it as exploded without actually exploding it.
/// </summary>
/// <param name="Defused">Will be set to true if the entity's explosive component was not yet exploded, but now is.</param>
[ByRefEvent]
public record struct DefuseExplosiveEvent(bool Defused = false)
{

}
