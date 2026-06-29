using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Shared.ModularShields;

public abstract partial class SharedModularShieldFluxDestructionComponent : Component
{
    /// <summary>
    /// The maximum rate at which this flux destruction component can destroy flux per second
    /// </summary>
    [DataField]
    public int FluxDestructionRateMaximum = 200;

    /// <summary>
    /// The priority for usage of this flux destruction component.
    /// Lower numbers indicate higher priority and will be used first to destroy flux.
    /// Equal priority components will be used in an arbitrary order.
    /// </summary>
    [DataField]
    public int FluxDestructionPriority = 0;



    /// <summary>
    /// The secondary priority for usage of this flux destruction component.
    /// Generated and arbitrary, used to have a consistent ordering across identical 
    /// </summary>
    [DataField]
    public int FluxDestructionPriorityTieBreaker = 0;

    /// <summary>
    /// Multiplier for the flux destruction rate while the shield of the system is projected.
    /// A value of 0.25 would mean it would generate 25% as much flux while the shield is up.
    /// This multiplier will just be treated as though the flux destruction rate maximum was lower/higher.
    /// As such any costs will be scaled down/up as well.
    /// </summary>
    [DataField]
    public float FluxDestructionWhileShieldProjectedRateMultiplier = 1;

    /// <summary>
    /// Multiplier for the 'cost' of the flux destruction while the shield of the system is projected.
    /// A value of 2 would mean it would 'cost' twice as much to produce X amount of flux while the shield is up.
    /// This is intended to be used in conjunction with the above multiplier to allow for more complex flux balancing.
    /// e.g. You want the shield to generate 25% of the normal flux, but 'cost' the same amount it does normally.
    /// This would be a RateMultiplier of 0.25 and a CostMultiplier of 4.
    /// </summary>
    [DataField]
    public float FluxDestructionWhileShieldProjectedCostMultiplier = 1;


    /// <summary>
    /// See <see cref="FluxDestructionWhileShieldProjectedRateMultiplier"/>.
    /// Same thing but for overloading.
    /// </summary>
    [DataField]
    public float FluxDestructionWhileOverloadedRateMultiplier = 0.1f;

    /// <summary>
    /// See <see cref="FluxDestructionWhileShieldProjectedCostMultiplier"/>.
    /// Same thing but for overloading.
    /// </summary>
    [DataField]
    public float FluxDestructionWhileOverloadedCostMultiplier = 10f;
}
