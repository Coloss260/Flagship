using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Shared.ModularShields;


[Virtual]
public partial class SharedModularShieldCoreComponent : Component
{
    /// <summary>
    /// Whether the modular shield core is trying (but possibly not able) to project it's shield at the moment.
    /// </summary>
    [DataField]
    public bool ShieldProjectionEnabled = true;

    /// <summary>
    /// Whether the modular shield core is projecting it's shield at the moment.
    /// </summary>
    [DataField]
    public bool ProjectingShield = false;

    /// <summary>
    /// The amount of flux that has overflown the flux capacity of the system.
    /// </summary>
    [DataField]
    public float FluxOverflow = 0;

    /// <summary>
    /// The time at which the current overflow buffer will end and the system with overload if flux is still overflowing.
    /// </summary>
    [DataField]
    public TimeSpan? FluxOverflowBufferEnd = default;

    /// <summary>
    /// The time at which the current overload will end and the system returns to normal operation.
    /// </summary>
    [DataField]
    public TimeSpan? FluxOverloadEnd = default;



    /// <summary>
    /// How much energy is destroyed per damage absorbed by the shield.
    /// </summary>
    [DataField]
    public float DamageAbsorbedToEnergyDestructionRatio = 100f;

    /// <summary>
    /// How much flux is genereted per damage absorbed by the shield.
    /// </summary>
    [DataField]
    public float DamageAbsorbedToFluxGenerationRatio = 10f;

    /// <summary>
    /// The minimum amount of energy that must be stored in order to start projecting the shield.
    /// </summary>
    [DataField]
    public float MinimumEnergyStoredToProjectShield = 10000;

    /// <summary>
    /// The minimum amount of energy that must be stored in order to start projecting the shield, as a percent of the current storage.
    /// </summary>
    [DataField]
    public float MinimumEnergyStoredToProjectShieldPercent = 0.1f;

    /// <summary>
    /// Amount of energy drained per tick while the shield is projected.
    /// </summary>
    [DataField]
    public int ShieldProjectionPassiveEnergyDrain = 0;

    /// <summary>
    /// The amount of time that flux can overflow the flux capacity of the system before it overloads.
    /// </summary>
    [DataField]
    public TimeSpan FluxOverflowBufferDuration = TimeSpan.FromSeconds(10.0);

    /// <summary>
    /// The maximum amount of flux that can be stored in this system.
    /// </summary>
    /// <summary>
    /// Whether or not overflowing flux is allowed to be destroyed by flux destroyers (thus preventing the shield from overloading if destroyed below max capacity).
    /// </summary>
    [DataField]
    public bool FluxOverflowFluxDestructionAllowed = false;

    [DataField]
    public bool FluxOverflowAllowUsingNormalStorageDuringOverflow = false;

    /// <summary>
    /// How long the shield will be overloaded for if flux overflows capacity for enough time.
    /// </summary>
    [DataField]
    public TimeSpan FluxOverloadDuration = TimeSpan.FromSeconds(30.0);
}
