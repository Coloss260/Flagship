using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Shared.ModularShields.Components;

[RegisterComponent]
public sealed partial class ModularShieldFluxStorageComponent : Component
{
    /// <summary>
    /// The maximum flux capacity of this storage component.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int FluxCapacity = 200;

    /// <summary>
    /// The amount of flux currently stored in this storage component.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float FluxStored = 0;

    /// <summary>
    /// The priority for usage of this flux storage component.
    /// Lower numbers indicate higher priority and will be used first to store flux.
    /// Equal priority components will be filled in arbitrary order. (Filling up one by one looks cooler)
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int FluxStoragePriority = 0;

    /// <summary>
    /// The secondary priority for usage of this flux storage component.
    /// Generated and arbitrary, used to have a consistent ordering across identical 
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int FluxStoragePriorityTieBreaker = 0;
}
