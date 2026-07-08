using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Shared.ModularShields.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModularShieldEnergyStorageComponent : Component
{
    /// <summary>
    /// The maximum energy capacity of this storage component.
    /// </summary>
    [DataField]
    public int EnergyCapacity = 2000;

    /// <summary>
    /// The current amount of energy stored in this storage component.
    /// </summary>
    [DataField]
    public float EnergyStored = 0;

    /// <summary>
    /// The priority for usage of this energy storage component.
    /// Lower numbers indicate higher priority and will be used first to store energy.
    /// Equal priority components will be filled in arbitrary order. (Filling up one by one looks cooler)
    /// </summary>
    [DataField]
    public int EnergyStoragePriority = 0;

    /// <summary>
    /// The secondary priority for usage of this energy storage component.
    /// Generated and arbitrary, used to have a consistent ordering across identical 
    /// </summary>
    [DataField]
    public int EnergyStoragePriorityTieBreaker = 0;

}
