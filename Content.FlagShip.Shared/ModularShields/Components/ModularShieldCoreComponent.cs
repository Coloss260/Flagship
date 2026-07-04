using Robust.Shared.Audio;
using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Shared.ModularShields.Components;


[RegisterComponent]
public sealed partial class ModularShieldCoreComponent : Component
{
    /// <summary>
    /// Whether the modular shield core is trying (but possibly not able) to project it's shield at the moment.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool ShieldProjectionEnabled = true;

    /// <summary>
    /// The shield entity this modular shield core is currently projecting.
    /// </summary>
    [DataField, ViewVariables]
    public EntityUid? ShieldProjected = null;

    /// <summary>
    /// The entity that this shield core's shield is projected around.
    /// </summary>
    [DataField, ViewVariables]
    public EntityUid? ShieldedEntity = null;

    /// <summary>
    /// The cooldown on toggling the shield projection on/off.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan? ShieldProjectionToggleCooldown = null;

    /// <summary>
    /// The amount of flux that has overflown the flux capacity of the system.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float FluxOverflow = 0;

    /// <summary>
    /// The time at which the current overflow buffer will end and the system with overload if flux is still overflowing.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan? FluxOverflowBufferEnd = default;

    /// <summary>
    /// The time at which the current overload will end and the system returns to normal operation.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan? FluxOverloadEnd = default;

    /// <summary>
    /// The color of the projected shield.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Color ShieldColor = Color.White;



    /// <summary>
    /// How much energy is destroyed per damage absorbed by the shield.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float DamageAbsorbedToEnergyDestructionRatio = 100f;

    /// <summary>
    /// How much flux is genereted per damage absorbed by the shield.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float DamageAbsorbedToFluxGenerationRatio = 10f;

    /// <summary>
    /// How much Emp energy consumption is scaled before treating it as normal damage and using those absorption ratios.
    /// To give a sense of scale, an vanilla emp grenade does 50000 energy consumption.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float EmpDamageToNormalDamageRatio = 0.01f;

    /// <summary>
    /// The minimum amount of energ y that must be stored in order to start projecting the shield.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MinimumEnergyStoredToProjectShield = 100;

    /// <summary>
    /// The minimum amount of energy that must be stored in order to start projecting the shield, as a percent of the current storage.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MinimumEnergyStoredToProjectShieldPercent = 0.1f;

    /// <summary>
    /// Amount of energy drained per tick while the shield is projected.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int ShieldProjectionPassiveEnergyDrain = 0;

    /// <summary>
    /// The amount of time that flux can overflow the flux capacity of the system before it overloads.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan FluxOverflowBufferDuration = TimeSpan.FromSeconds(3.0);

    /// <summary>
    /// How far the flux overflow buffer can go past the system's flux capacity before the shield overloads.
    /// Expressed as a percent of the flux capacity. 
    /// If set to 1, the flux overflow buffer will allow the flux to reach double the system's flux capacity before overloading.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float FluxOverFlowBufferLimit = 1f;

    /// <summary>
    /// The maximum amount of flux that can be stored in this system.
    /// </summary>
    /// <summary>
    /// Whether or not overflowing flux is allowed to be destroyed by flux destroyers (thus preventing the shield from overloading if destroyed below max capacity).
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool FluxOverflowFluxDestructionAllowed = false;

    /// <summary>
    /// If disabled, flux generated during flux overflow buffer is forced into the overflow buffer regardless of remaining flux capacity in the system.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool FluxOverflowAllowUsingNormalStorageDuringOverflow = false;

    /// <summary>
    /// How long the shield will be overloaded for if flux overflows capacity for enough time.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan FluxOverloadDuration = TimeSpan.FromSeconds(7.0);

    /// <summary>
    /// How long the cooldown is on shield projection toggling.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ShieldProjectionToggleCooldownTime = TimeSpan.FromSeconds(1.0);

    /// <summary>
    /// On shield projection starting.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier ProjectionStartSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    /// <summary>
    /// On shield projection ending under normal conditions.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier ProjectionEndCalmSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");

    /// <summary>
    /// When the shield system has started to overflow with flux.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier? OverflowBufferStartSoundSpecifier = null;

    /// <summary>
    /// When the shield system has overloaded due to flux.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier OverloadSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}

[Serializable, NetSerializable]
public enum ModularShieldCoreVisuals
{
    DisplayState,
}

[Serializable, NetSerializable]
public enum ModularShieldCoreState
{
    Off,
    UnableToProject,
    Projecting,
    FluxOverflow,
    FluxOverload,
}
