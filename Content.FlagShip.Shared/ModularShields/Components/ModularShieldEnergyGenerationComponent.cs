using Robust.Shared.GameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Shared.ModularShields.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModularShieldEnergyGenerationComponent : Component
{
    /// <summary>
    /// The maximum rate at which this energy generation component can generate energy per second.
    /// </summary>
    [DataField]
    public int EnergyGenerationRateMaximum = 20;

    /// <summary>
    /// The priority for usage of this energy generation component.
    /// Lower numbers indicate higher priority and will be used first to generate energy.
    /// Equal priority components will be used in an arbitrary order.
    /// </summary>
    [DataField]
    public int EnergyGenerationPriority = 0;

    /// <summary>
    /// The secondary priority for usage of this energy generation component.
    /// Generated and arbitrary, used to have a consistent ordering across identical 
    /// </summary>
    [DataField]
    public int EnergyGenerationPriorityTieBreaker = 0;

    /// <summary>
    /// Multiplier for the energy generation rate while the shield of the system is projected.
    /// A value of 0.25 would mean it would generate 25% as much energy while the shield is up.
    /// This multiplier will just be treated as though the energy generation rate maximum was lower/higher.
    /// As such any costs will be scaled down/up as well.
    /// </summary>
    [DataField]
    public float EnergyGenerationWhileShieldProjectedRateMultiplier = 1f;

    /// <summary>
    /// Multiplier for the 'cost' of the energy generation while the shield of the system is projected.
    /// A value of 2 would mean it would 'cost' twice as much to produce X amount of energy while the shield is up.
    /// This is intended to be used in conjunction with the above multiplier to allow for more complex energy balancing.
    /// e.g. You want the shield to generate 25% of the normal energy, but 'cost' the same amount it does normally.
    /// This would be a RateMultiplier of 0.25 and a CostMultiplier of 4.
    /// </summary>
    [DataField]
    public float EnergyGenerationWhileShieldProjectedCostMultiplier = 1f;

    /// <summary>
    /// See <see cref="EnergyGenerationWhileShieldProjectedRateMultiplier"/>.
    /// Same thing but for overloading.
    /// </summary>
    [DataField]
    public float EnergyGenerationWhileOverloadedRateMultiplier = 0.1f;

    /// <summary>
    /// See <see cref="EnergyGenerationWhileShieldProjectedCostMultiplier"/>.
    /// Same thing but for overloading.
    /// </summary>
    [DataField]
    public float EnergyGenerationWhileOverloadedCostMultiplier = 10f;
}
