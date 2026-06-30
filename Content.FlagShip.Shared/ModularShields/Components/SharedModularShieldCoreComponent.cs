using Robust.Shared.Audio;
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
    /// The shield entity this modular shield core is currently projecting.
    /// </summary>
    [DataField]
    public EntityUid? ShieldProjected = null;

    /// <summary>
    /// The entity that this shield core's shield is projected around.
    /// </summary>
    [DataField]
    public EntityUid? ShieldedEntity = null;

    /// <summary>
    /// The cooldown on toggling the shield projection on/off.
    /// </summary>
    [DataField]
    public TimeSpan? ShieldProjectionToggleCooldown = null;

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
    /// The color of the projected shield.
    /// </summary>
    [DataField]
    public Color ShieldColor = Color.White;

    /// <summary>
    /// Whether the energy storage of this shield core's node group's energy storage has hit zero.
    /// Used to disable the shield if disabling is allowed under the current circumstances and reset whether or not 
    /// </summary>
    [DataField]
    public bool EnergyEmptied = false;



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

    /// <summary>
    /// If disabled, flux generated during flux overflow buffer is forced into the overflow buffer regardless of remaining flux capacity in the system.
    /// </summary>
    [DataField]
    public bool FluxOverflowAllowUsingNormalStorageDuringOverflow = false;

    /// <summary>
    /// How long the shield will be overloaded for if flux overflows capacity for enough time.
    /// </summary>
    [DataField]
    public TimeSpan FluxOverloadDuration = TimeSpan.FromSeconds(30.0);

    /// <summary>
    /// How long the cooldown is on shield projection toggling.
    /// </summary>
    [DataField]
    public TimeSpan ShieldProjectionToggleCooldownTime = TimeSpan.FromSeconds(1.0);

    /// <summary>
    /// On shield projection starting.
    /// </summary>
    [DataField]
    public SoundSpecifier ProjectionStartSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    /// <summary>
    /// On shield projection ending under normal conditions.
    /// </summary>
    [DataField]
    public SoundSpecifier ProjectionEndCalmSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");

    /// <summary>
    /// When the shield system has started to overflow with flux.
    /// </summary>
    [DataField]
    public SoundSpecifier? OverflowBufferStartSoundSpecifier = null;

    /// <summary>
    /// When the shield system has overloaded due to flux.
    /// </summary>
    [DataField]
    public SoundSpecifier OverloadSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");]
}
