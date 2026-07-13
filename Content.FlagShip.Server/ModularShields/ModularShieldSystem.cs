using Content.FlagShip.Common.Explosion.Events;
using Content.FlagShip.Shared.ModularShields.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.Destructible;
using Content.Shared.EntityEffects.Effects.StatusEffects;
using Content.Shared.Examine;
using Content.Shared.Explosion.Components;
using Content.Shared.Interaction;
using Content.Shared.NodeContainer;
using Content.Shared.Projectiles;
using Content.Shared.Trigger.Components.Effects;
using Content.Shared.Trigger.Systems;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Core;

namespace Content.FlagShip.Server.ModularShields;

public sealed partial class ModularShieldSystem : EntitySystem
{
    [Dependency] private TriggerSystem _trigger = default!;
    [Dependency] private IGameTiming _gameTiming = default!;
    [Dependency] private PowerReceiverSystem _power = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private ExplosionSystem _explosion = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private SharedAppearanceSystem _sharedAppearance = default!;

    private EntityQuery<ProjectileComponent> _projectileQuery;

    public override void Initialize()
    {
        base.Initialize();
        _projectileQuery = GetEntityQuery<ProjectileComponent>();

        SubscribeLocalEvent<ModularShieldCoreComponent, ModularShieldAbsorbedProjectileEvent>(OnModularShieldProjectileAbsorbed);
        SubscribeLocalEvent<ModularShieldCoreComponent, ModularShieldAbsorbedDamageEvent>(OnModularShieldDamageAbsorbed);
        SubscribeLocalEvent<ModularShieldCoreComponent, DestructionEventArgs>(OnModularShieldCoreDestroyed);
        SubscribeLocalEvent<ModularShieldCoreComponent, ComponentShutdown>(OnModularShieldCoreShutdown);
        SubscribeLocalEvent<ModularShieldCoreComponent, ActivateInWorldEvent>(OnShieldCoreActivateInWorld);

        SubscribeLocalEvent<ModularShieldCoreComponent, ExaminedEvent>(OnShieldCoreExamined);

        InitializeShield();
        InitializeEnergyGeneration();
        InitializeEnergyStorage();
        InitializeFluxStorage();
        InitializeFluxDestruction();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ModularShieldCoreComponent, NodeContainerComponent>();

        while (query.MoveNext(out var uid, out var core, out var nodeContainer))
        {
            UpdateCore(frameTime, uid, core, nodeContainer);
        }
    }



    /// <summary>
    /// Performs update operations for a single shield core component
    /// </summary>
    /// <param name="shieldCoreUid"></param>
    /// <param name="shieldCore"></param>
    /// <param name="nodeContainer"></param>
    private void UpdateCore(float frameTime, EntityUid shieldCoreUid, ModularShieldCoreComponent shieldCore, NodeContainerComponent nodeContainer)
    {
        var curTime = _gameTiming.CurTime;

        if (!TryGetModularShieldNodeGroup(shieldCoreUid, out var nodeGroup, nodeContainer))
            return;

        if (!_power.IsPowered(shieldCoreUid))
            return;

        // Exit early if we're not the master shield core, performing minor tasks that don't require.
        if (!shieldCore.IsMasterShieldCore)
        {
            UpdateShieldCoreDisplay((shieldCoreUid, shieldCore));
            StopModularShieldProjection((shieldCoreUid, shieldCore));
            return;
        }

        // Perform passive shield energy drain.
        if (shieldCore.ShieldProjected != null)
        {
            DestroyEnergy(nodeGroup, shieldCore.ShieldProjectionPassiveEnergyDrain);
        }

        var fluxStorageStats = nodeGroup.GetFluxStorageStatistics();

        // Check whether to start shield core overflow.
        if (shieldCore.FluxOverflow > 0 &&
            shieldCore.FluxOverflowBufferEnd == null)
        {
            shieldCore.FluxOverflowBufferEnd = curTime + shieldCore.FluxOverflowBufferDuration;

            var audioEntity = _audio.PlayGlobal(
                shieldCore.OverflowBufferStartSound,
                GetShieldSoundPlayerFilter((shieldCoreUid, shieldCore)),
                true,
                shieldCore.OverflowBufferStartSound.Params.WithLoop(true));

            if (audioEntity != null)
            {
                shieldCore.FluxOverflowBufferAudioEntity = audioEntity.Value.Entity;
            }
        }

        // Check an ongoing shield core overflow buffer
        if (shieldCore.FluxOverflowBufferEnd != null)
        {
            if (shieldCore.FluxOverflow == 0)
            {
                // Flux overflow cleared.
                shieldCore.FluxOverflowBufferEnd = null;
            }

            if (shieldCore.FluxOverflowBufferEnd < curTime ||
                shieldCore.FluxOverflow > fluxStorageStats.FluxCapacity * shieldCore.FluxOverFlowBufferLimit)
            {
                // Overload the shield.
                PerformShieldCoreOverloadPunishments(shieldCoreUid, shieldCore, shieldCore.FluxOverflow);

                if (shieldCore.FluxOverflowBufferAudioEntity != null)
                {
                    _audio.Stop(shieldCore.FluxOverflowBufferAudioEntity.Value);
                }

                shieldCore.FluxOverflow = 0;
                shieldCore.FluxOverflowBufferEnd = null;
                shieldCore.FluxOverloadEnd = curTime + shieldCore.FluxOverloadDuration;
            }
        }

        // Check an ongoing shield core overload is finished.
        if (shieldCore.FluxOverloadEnd != null)
        {
            if (shieldCore.FluxOverloadEnd < curTime)
            {
                shieldCore.FluxOverloadEnd = null;
            }
        }

        // Check whether to start or stop shield projection.
        if (shieldCore.ShieldProjectionEnabled &&
            shieldCore.ShieldProjected == null)
        {
            CheckIfStartModularShieldProjection(shieldCoreUid, shieldCore, nodeGroup);
        }
        else if (shieldCore.ShieldProjected != null)
        {
            CheckIfStopModularShieldProjection(shieldCoreUid, shieldCore, nodeGroup);
        }

        UpdateShieldCoreDisplay((shieldCoreUid, shieldCore));

        // Do energy generation and flux destruction after we do shield core checks
        // So we can check for energy hitting 0 before we generate energy.
        var energyGeneration = nodeGroup.GetEnergyGeneration();
        var fluxDestruction = nodeGroup.GetFluxDestruction();

        foreach (var energyGenerator in energyGeneration)
        {
            UpdateEnergyGenerator(frameTime, shieldCore, nodeGroup, energyGenerator.Owner, energyGenerator.Comp);
        }

        foreach (var fluxDestructor in fluxDestruction)
        {
            UpdateFluxDestructor(frameTime, shieldCore, nodeGroup, fluxDestructor.Owner, fluxDestructor.Comp, nodeContainer);
        }
    }



    /// <summary>
    /// Handles the 'punishments' for overflowing the shield core with flux
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="core"></param>
    /// <param name="overloadStrength"></param>
    public void PerformShieldCoreOverloadPunishments(EntityUid uid, ModularShieldCoreComponent core, float overloadStrength)
    {

    }



    public bool CheckIfStartModularShieldProjection(
        EntityUid shieldCoreUid,
        ModularShieldCoreComponent? shieldCoreComponent = null,
        ModularShieldNodeGroup? nodeGroup = null)
    {
        if (!Resolve(shieldCoreUid, ref shieldCoreComponent))
            return false;

        if (nodeGroup == null && !TryGetModularShieldNodeGroup(shieldCoreUid, out nodeGroup))
            return false;

        var parentGridEntityUid = Transform(shieldCoreUid).GridUid;

        var energyStorageStats = nodeGroup.GetEnergyStorageStatistics();

        bool success = false;

        // Whether to start projecting the shield.
        if (shieldCoreComponent.ShieldProjected == null && // Shield needs to be off
            shieldCoreComponent.ShieldedEntity == null && // Can't be shielding something
            parentGridEntityUid != null && // Need to be on a grid.
            shieldCoreComponent.FluxOverloadEnd == null && // Overload disables shields
            shieldCoreComponent.MinimumEnergyStoredToProjectShield <= energyStorageStats.EnergyStored &&
            shieldCoreComponent.MinimumEnergyStoredToProjectShieldPercent <= (energyStorageStats.EnergyStored / energyStorageStats.EnergyCapacity))
        {
            success = StartModularShieldProjection((shieldCoreUid, shieldCoreComponent));
        }

        return success;
    }



    public bool CheckIfStopModularShieldProjection(
        EntityUid shieldCoreUid,
        ModularShieldCoreComponent? shieldCoreComponent = null,
        ModularShieldNodeGroup? nodeGroup = null)
    {
        var parentGridEntityUid = Transform(shieldCoreUid).GridUid;


        if (!Resolve(shieldCoreUid, ref shieldCoreComponent))
            return false;

        if (nodeGroup == null && !TryGetModularShieldNodeGroup(shieldCoreUid, out nodeGroup))
            return false;


        var energyStorageStats = nodeGroup.GetEnergyStorageStatistics();

        bool success = false;

        // Whether to stop projecting the shield.
        if (shieldCoreComponent.ShieldProjected != null && // Shield needs to be on
            shieldCoreComponent.ShieldedEntity != null) // We need to be shielding something)
        {
            bool shutdown = false;
            bool violent = false;
            if (energyStorageStats.EnergyStored == 0 || // No energy disables shields.
                shieldCoreComponent.FluxOverloadEnd != null) // Overload disables shields.
            {
                // Violent shutdown of shields (for aesthetics)
                shutdown = true;
                violent = true;
            }
            else if (!shieldCoreComponent.ShieldProjectionEnabled) // Shield core has been turned off.
            {
                // Calm shutdown of projection (for aesthetics)
                shutdown = true;
            }

            if (shutdown)
            {
                StopModularShieldProjection((shieldCoreUid, shieldCoreComponent), violent);
            }
        }

        return success;
    }



    private void OnModularShieldCoreDestroyed(Entity<ModularShieldCoreComponent> ent, ref DestructionEventArgs args)
    {
        if (ent.Comp.ShieldProjected != null && ent.Comp.ShieldedEntity != null)
        {
            StopModularShieldProjection(ent, violent: true);
        }
    }



    private void OnModularShieldCoreShutdown(EntityUid uid, ModularShieldCoreComponent component, ComponentShutdown args)
    {
        if (component.ShieldProjected != null && component.ShieldedEntity != null)
        {
            StopModularShieldProjection((uid, component), silent: true);
        }
    }



    private bool StartModularShieldProjection(Entity<ModularShieldCoreComponent> ent)
    {
        if (ent.Comp.ShieldedEntity != null || ent.Comp.ShieldProjected != null)
            return false;

        bool success = false;
        var parentGridEntityUid = Transform(ent.Owner).GridUid;

        if (parentGridEntityUid != null)
        {
            EntityUid shieldEntityUid = ShieldEntity((EntityUid)parentGridEntityUid, ent.Owner);
            if (shieldEntityUid != EntityUid.Invalid)
            {
                success = true;
                ent.Comp.ShieldProjected = shieldEntityUid;
                ent.Comp.ShieldedEntity = parentGridEntityUid;

                _audio.PlayGlobal(ent.Comp.ProjectionStartSound, GetShieldSoundPlayerFilter(ent), true, ent.Comp.ProjectionStartSound.Params);
            }
        }

        return success;
    }

    private bool StopModularShieldProjection(Entity<ModularShieldCoreComponent> shieldCore, bool violent = false, bool silent = false)
    {
        if (shieldCore.Comp.ShieldedEntity == null || shieldCore.Comp.ShieldProjected == null)
            return false;

        bool success = UnshieldEntity((EntityUid)shieldCore.Comp.ShieldedEntity);

        // Shield core may be terminating at this point so don't use it.
        if (silent)
        {
            // Test will fail if we create an audio entity when the shield core is deleted.
        }
        else if (violent)
        {
            _audio.PlayGlobal(shieldCore.Comp.ProjectionEndViolentSound, GetShieldSoundPlayerFilter(shieldCore), true, shieldCore.Comp.ProjectionEndViolentSound.Params);
        }
        else
        {
            _audio.PlayGlobal(shieldCore.Comp.ProjectionEndCalmSound, GetShieldSoundPlayerFilter(shieldCore), true, shieldCore.Comp.ProjectionEndCalmSound.Params);
        }


        shieldCore.Comp.ShieldProjected = null;
        shieldCore.Comp.ShieldedEntity = null;

        return success;
    }



    private void UpdateShieldCoreDisplay(Entity<ModularShieldCoreComponent> ent, AppearanceComponent? appComp = null)
    {
        ;
        if (!Resolve(ent.Owner, ref appComp))
            return;

        if (!ent.Comp.IsMasterShieldCore)
        {
            _sharedAppearance.SetData(ent.Owner, ModularShieldCoreVisuals.DisplayState, ModularShieldCoreState.Off, appComp);
        }
        else if (ent.Comp.FluxOverloadEnd != null)
        {
            _sharedAppearance.SetData(ent.Owner, ModularShieldCoreVisuals.DisplayState, ModularShieldCoreState.FluxOverload, appComp);
        }
        else if (ent.Comp.FluxOverflowBufferEnd != null)
        {
            _sharedAppearance.SetData(ent.Owner, ModularShieldCoreVisuals.DisplayState, ModularShieldCoreState.FluxOverflow, appComp);
        }
        else if (ent.Comp.ShieldProjected != null)
        {
            _sharedAppearance.SetData(ent.Owner, ModularShieldCoreVisuals.DisplayState, ModularShieldCoreState.Projecting, appComp);
        }
        else if (!ent.Comp.ShieldProjectionEnabled)
        {
            _sharedAppearance.SetData(ent.Owner, ModularShieldCoreVisuals.DisplayState, ModularShieldCoreState.Off, appComp);
        }
        else
        {
            _sharedAppearance.SetData(ent.Owner, ModularShieldCoreVisuals.DisplayState, ModularShieldCoreState.UnableToProject, appComp);
        }
    }




    private void OnModularShieldProjectileAbsorbed(EntityUid uid, ModularShieldCoreComponent component, ModularShieldAbsorbedProjectileEvent args)
    {
        var calculatedDamage = 0f;
        if (TryComp<EmpOnTriggerComponent>(args.AbsorbedProjectile, out var emp))
        {
            calculatedDamage += emp.EnergyConsumption * component.EmpDamageToNormalDamageMultiplier;
        }

        var defuseEvent = new DefuseExplosiveEvent();
        RaiseLocalEvent(args.AbsorbedProjectile, ref defuseEvent);
        if (defuseEvent.Defused && TryComp<ExplosiveComponent>(args.AbsorbedProjectile, out var exp) && _prototypeManager.TryIndex(exp.ExplosionType, out var type))
        {
            calculatedDamage += exp.TotalIntensity * (float)type.DamagePerIntensity.GetTotal() * component.ExplosionDamageToNormalDamageMultiplier;
        }

        calculatedDamage += (float)args.Projectile.Damage.GetTotal();
        args.Projectile.ProjectileSpent = true;

        var ev = new ModularShieldAbsorbedDamageEvent(calculatedDamage);
        RaiseLocalEvent(uid, ref ev);

        QueueDel(args.AbsorbedProjectile);
    }


    private void OnModularShieldDamageAbsorbed(Entity<ModularShieldCoreComponent> ent, ref ModularShieldAbsorbedDamageEvent args)
    {
        if (args.DamageDealt > 0 && TryGetModularShieldNodeGroup(ent.Owner, out var nodeGroup))
        {
            DestroyEnergy(nodeGroup, args.DamageDealt * ent.Comp.DamageAbsorbedToEnergyDestructionMultiplier);
            GenerateFlux(nodeGroup, args.DamageDealt * ent.Comp.DamageAbsorbedToFluxGenerationMultiplier);

            if (args.DamageDealt >= ent.Comp.AbsorbedDamageSoundMinimumDamage)
            {
                var filter = GetShieldSoundPlayerFilter(ent);

                float soundDamageScale = Math.Clamp((args.DamageDealt - ent.Comp.AbsorbedDamageSoundScalingMinimumDamage) / (ent.Comp.AbsorbedDamageSoundScalingMaximumDamage - ent.Comp.AbsorbedDamageSoundScalingMinimumDamage), 0, 1);
                // Sound gets loader the more damage is absorbed.
                float soundVolumeScale = ent.Comp.AbsorbedDamageSoundScalingMinimumVolume + (soundDamageScale * (ent.Comp.AbsorbedDamageSoundScalingMaximumVolume - ent.Comp.AbsorbedDamageSoundScalingMinimumVolume));
                // Sound gets lower pitched the more dmage is absorbed.
                float soundPitchScale = ent.Comp.AbsorbedDamageSoundScalingMinimumPitch + ((1 - soundDamageScale) * (ent.Comp.AbsorbedDamageSoundScalingMaximumPitch - ent.Comp.AbsorbedDamageSoundScalingMinimumPitch));

                var audioParams = ent.Comp.AbsorbedDamageSound.Params;

                _audio.PlayGlobal(
                    ent.Comp.AbsorbedDamageSound,
                    filter,
                    true,
                    audioParams
                        .WithVolume(audioParams.Volume + soundVolumeScale)
                        .WithPitchScale(audioParams.Pitch * soundPitchScale)
                        .WithVariation(audioParams.Variation * soundPitchScale));
            }
        }
    }



    private void OnShieldCoreActivateInWorld(Entity<ModularShieldCoreComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Complex && ent.Comp.ShieldProjectionToggleCooldown == null || ent.Comp.ShieldProjectionToggleCooldown < _gameTiming.CurTime)
        {
            ent.Comp.ShieldProjectionEnabled = !ent.Comp.ShieldProjectionEnabled;
            ent.Comp.ShieldProjectionToggleCooldown = _gameTiming.CurTime + ent.Comp.ShieldProjectionToggleCooldownTime;
            args.Handled = true;
        }
    }



    private void OnShieldCoreExamined(EntityUid uid, ModularShieldCoreComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!TryGetModularShieldNodeGroup(uid, out var nodeGroup))
        {
            args.PushMarkup(Loc.GetString("modular-shield-examine-disconnected"));
            return;
        }
        else if (!component.IsMasterShieldCore)
        {
            args.PushMarkup(Loc.GetString("modular-shield-core-examine-not-master"));
            return;
        }

        var energyStorageStats = nodeGroup.GetEnergyStorageStatistics();
        var fluxStorageStats = nodeGroup.GetFluxStorageStatistics();

        var minEnergyToProject = Math.Max(component.MinimumEnergyStoredToProjectShield, component.MinimumEnergyStoredToProjectShieldPercent * energyStorageStats.EnergyCapacity);

        if (component.ShieldProjected != null)
        {
            args.PushMarkup(Loc.GetString("modular-shield-core-examine-projecting"));
        }
        else
        {
            args.PushMarkup(Loc.GetString("modular-shield-core-examine-not-projecting"));
        }

        args.PushMarkup(Loc.GetString("modular-shield-core-examine-storage",
            ("energy", (int)Math.Round(energyStorageStats.EnergyStored)),
            ("energymax", energyStorageStats.EnergyCapacity),
            ("energymin", (int)Math.Round(minEnergyToProject)),
            ("flux", (int)Math.Round(fluxStorageStats.FluxStored)),
            ("fluxmax", fluxStorageStats.FluxCapacity)
        ));

        if (component.FluxOverflowBufferEnd != null)
        {
            args.PushMarkup(Loc.GetString("modular-shield-core-examine-flux-overflow", ("fluxoverflow", (int)Math.Round(component.FluxOverflow))));
        }

        if (component.FluxOverloadEnd != null)
        {
            args.PushMarkup(Loc.GetString("modular-shield-core-examine-flux-overload"));
        }
    }




    private bool TryGetModularShieldNodeGroup(EntityUid uid, [MaybeNullWhen(false)] out ModularShieldNodeGroup group, NodeContainerComponent? nodes = null)
    {
        if (!Resolve(uid, ref nodes))
        {
            group = null;
            return false;
        }


        group = nodes.Nodes.Values
            .Select(node => node.NodeGroup)
            .OfType<ModularShieldNodeGroup>()
            .FirstOrDefault();

        return group != null;
    }


    private Filter GetShieldSoundPlayerFilter(Entity<ModularShieldCoreComponent> shieldCore)
    {
        // Prefer using the shielded entity (the grid we're on) in case the shield core is gone.
        // We can also base the distance for hearing it on the grid's size.
        if (shieldCore.Comp.ShieldedEntity != null)
        {
            return _station.GetInOwningStation((EntityUid)shieldCore.Comp.ShieldedEntity);
        }
        return _station.GetInOwningStation(shieldCore.Owner);
    }





    [ByRefEvent]
    public record struct ModularShieldAbsorbedProjectileEvent(EntityUid AbsorbedProjectile, ProjectileComponent Projectile)
    {

    }
    [ByRefEvent]
    public record struct ModularShieldAbsorbedDamageEvent(float DamageDealt)
    {

    }
}
