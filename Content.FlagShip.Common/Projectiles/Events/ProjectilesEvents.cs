using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Shared.Projectiles.Events;

[ByRefEvent]
public record struct ShooterUpdatedEvent(EntityUid Shooter, bool WillShooterEntityBeDeletedSoon = false)
{

}
