// New Frontiers - This file is licensed under AGPLv3
// Copyright (c) 2024 New Frontiers Contributors
// See AGPLv3.txt for details.

using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Shuttles.Events
{
    /// <summary>
    /// Raised on the client when it wishes to change the maximum speed of a ship.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class SetMaxShuttleSpeedRequest : BoundUserInterfaceMessage
    {
        public float? MaxSpeed;
    }
}
