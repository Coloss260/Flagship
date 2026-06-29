using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Shared.ModularShields;

[Virtual]
public partial class SharedModularShieldFluxStorageComponent : Component
{
    /// <summary>
    /// The maximum flux capacity of this storage component.
    /// </summary>
    [DataField]
    public int FluxCapacity = 2000;

    /// <summary>
    /// The amount of flux currently stored in this storage component.
    /// </summary>
    [DataField]
    public float FluxStored = 0;

    /// <summary>
    /// The priority for usage of this flux storage component.
    /// Lower numbers indicate higher priority and will be used first to store flux.
    /// Equal priority components will be filled in arbitrary order. (Filling up one by one looks cooler)
    /// </summary>
    [DataField]
    public int FluxStoragePriority = 0;

    /// <summary>
    /// The secondary priority for usage of this flux storage component.
    /// Generated and arbitrary, used to have a consistent ordering across identical 
    /// </summary>
    [DataField]
    public int FluxStoragePriorityTieBreaker = 0;
}
