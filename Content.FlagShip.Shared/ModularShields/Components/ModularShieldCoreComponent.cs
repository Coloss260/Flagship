using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Shared.ModularShields.Components;


[RegisterComponent, NetworkedComponent]
public sealed partial class ModularShieldCoreComponent : Component
{
    /// <summary>
    /// Whether this modular shield core is the master core in control of the node group.
    /// </summary>
    public bool IsMasterShieldCore = false;

    /// <summary>
    /// Whether the modular shield core is trying (but possibly not able) to project it's shield at the moment.
    /// </summary>
    public bool ShieldProjectionEnabled = true;

    /// <summary>
    /// The shield entity this modular shield core is currently projecting.
    /// </summary>
    public EntityUid? ShieldProjected = null;

    /// <summary>
    /// The entity that this shield core's shield is projected around.
    /// </summary>
    public EntityUid? ShieldedEntity = null;

    /// <summary>
    /// The cooldown on toggling the shield projection on/off.
    /// </summary>
    public TimeSpan? ShieldProjectionToggleCooldown = null;

    /// <summary>
    /// The amount of flux that has overflown the flux capacity of the system.
    /// </summary>
    public float FluxOverflow = 0;

    /// <summary>
    /// The time at which the current overflow buffer will end and the system with overload if flux is still overflowing.
    /// </summary>
    public TimeSpan? FluxOverflowBufferEnd = default;

    /// <summary>
    /// The audio entity for the looping flux overflow buffer sound, used to stop the sound when overloading.
    /// </summary>
    public EntityUid? FluxOverflowBufferAudioEntity;

    /// <summary>
    /// The time at which the current overload will end and the system returns to normal operation.
    /// </summary>
    public TimeSpan? FluxOverloadEnd = default;

    /// <summary>
    /// The color of the projected shield.
    /// </summary>
    [DataField]
    public Color ShieldColor = Color.White;



    /// <summary>
    /// How much energy is destroyed per damage absorbed by the shield.
    /// Example weapon damage:
    /// Vanilla Lecter: 17 damage
    /// Vanilla Perforator (ship weapon): 55 damage
    /// </summary>
    [DataField]
    public float DamageAbsorbedToEnergyDestructionMultiplier = 1f;

    /// <summary>
    /// How much flux is genereted per damage absorbed by the shield.
    /// </summary>
    [DataField]
    public float DamageAbsorbedToFluxGenerationMultiplier = 1f;

    /// <summary>
    /// How much damage from explosion is scaled before terating it as normal damage.
    /// Damage from explosions is TotalIntensity * DamagePerIntensity (damage types combined). Example weapons:
    /// Vanilla china lake blast grenade = 150 intensity, 15 DamagePerIntensity = 2250
    /// Vanila RPG-7 = 200 intensity, 15 DamagePerIntensity = 3000
    /// </summary>
    [DataField]
    public float ExplosionDamageToNormalDamageMultiplier = 0.25f;

    /// <summary>
    /// How much Emp energy consumption is scaled before treating it as normal damage.
    /// To give a sense of scale, an vanilla emp grenade does 50000 energy consumption.
    /// </summary>
    [DataField]
    public float EmpDamageToNormalDamageMultiplier = 0.01f;

    /// <summary>
    /// The minimum amount of energy that must be stored in order to start projecting the shield.
    /// </summary>
    [DataField]
    public float MinimumEnergyStoredToProjectShield = 100;

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
    public TimeSpan FluxOverflowBufferDuration = TimeSpan.FromSeconds(3.0);

    /// <summary>
    /// How far the flux overflow buffer can go past the system's flux capacity before the shield overloads.
    /// Expressed as a percent of the flux capacity. 
    /// If set to 1, the flux overflow buffer will allow the flux to reach double the system's flux capacity before overloading.
    /// </summary>
    [DataField]
    public float FluxOverFlowBufferLimit = 1f;

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
    public TimeSpan FluxOverloadDuration = TimeSpan.FromSeconds(7.0);

    /// <summary>
    /// How long the cooldown is on shield projection toggling.
    /// </summary>
    [DataField]
    public TimeSpan ShieldProjectionToggleCooldownTime = TimeSpan.FromSeconds(0.5);

    /// <summary>
    /// On shield projection starting.
    /// </summary>
    [DataField]
    public SoundSpecifier ProjectionStartSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    /// <summary>
    /// On shield projection ending under normal conditions.
    /// </summary>
    [DataField]
    public SoundSpecifier ProjectionEndCalmSound = new SoundPathSpecifier("/Audio/Items/Medical/jet_injector.ogg");

    /// <summary>
    /// On shield projectiong ending due to 'violent' means. (e.g. out of energy or overload due to flux).
    /// </summary>
    [DataField]
    public SoundSpecifier ProjectionEndViolentSound = new SoundPathSpecifier("/Audio/Effects/guardian_warn.ogg");

    /// <summary>
    /// When the shield system has started to overflow with flux.
    /// </summary>
    [DataField]
    public SoundSpecifier OverflowBufferStartSound = new SoundPathSpecifier("/Audio/Effects/Grenades/Supermatter/supermatter_loop.ogg");



    /// <summary>
    /// Absorbed damage must be equal to or above this limit to trigger an AbsorbedDamageSound.
    /// </summary>
    [DataField]
    public float AbsorbedDamageSoundMinimumDamage = 3f;

    /// <summary>
    /// Audio played when the shield absorbs a projectile.
    /// </summary>
    [DataField]
    public SoundSpecifier AbsorbedDamageSound = new SoundPathSpecifier("/Audio/Weapons/Guns/Hits/laser_sear_wall.ogg");

    [DataField]
    public float AbsorbedDamageSoundScalingMinimumDamage = 20f;

    [DataField]
    public float AbsorbedDamageSoundScalingMaximumDamage = 200f;

    [DataField]
    public float AbsorbedDamageSoundScalingMinimumVolume = -8f;

    [DataField]
    public float AbsorbedDamageSoundScalingMaximumVolume = 0f;

    [DataField]
    public float AbsorbedDamageSoundScalingMinimumPitch = 0.5f;

    [DataField]
    public float AbsorbedDamageSoundScalingMaximumPitch = 1.5f;
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
