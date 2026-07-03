using Content.FlagShip.Shared.ModularShields.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Explosion.Components;
using Content.Shared.NodeContainer;
using Content.Shared.Projectiles;
using Content.Shared.Trigger.Components.Effects;
using Content.Shared.Trigger.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System;
using System.Collections.Generic;
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

    private EntityQuery<ProjectileComponent> _projectileQuery;

    public override void Initialize()
    {
        base.Initialize();
        _projectileQuery = GetEntityQuery<ProjectileComponent>();

        SubscribeLocalEvent<ModularShieldShieldComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<ModularShieldCoreComponent, ModularShieldAbsorbedEvent>(OnModularShieldAbsorbed);
        SubscribeLocalEvent<ModularShieldCoreComponent, ComponentShutdown>(OnModularShieldCoreShutdown);

        SubscribeLocalEvent<ModularShieldCoreComponent, ExaminedEvent>(OnShieldCoreExamined);
        SubscribeLocalEvent<ModularShieldEnergyGenerationComponent, ExaminedEvent>(OnEnergyGeneratorExamined);
        SubscribeLocalEvent<ModularShieldEnergyStorageComponent, ExaminedEvent>(OnEnergyStorageExamined);
        SubscribeLocalEvent<ModularShieldFluxStorageComponent, ExaminedEvent>(OnFluxStorageExamined);
        SubscribeLocalEvent<ModularShieldFluxDestructionComponent, ExaminedEvent>(OnFluxDestructorExamined);
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
        {
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

        if (shieldCore.ShieldProjectionEnabled &&
            shieldCore.ShieldProjected == null)
        {
            CheckIfStartModularShieldProjection(shieldCoreUid, shieldCore, nodeGroup);
        }
        else if (shieldCore.ShieldProjected != null)
        {
            CheckIfStopModularShieldProjection(shieldCoreUid, shieldCore, nodeGroup);
        }



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
    /// Performs update operations for a single energy generation component
    /// </summary>
    /// <param name="energyGeneratorUid"></param>
    /// <param name="generator"></param>
    /// <param name="nodeContainer"></param>
    private void UpdateEnergyGenerator(float frameTime, ModularShieldCoreComponent shieldCore, ModularShieldNodeGroup nodeGroup, EntityUid energyGeneratorUid, ModularShieldEnergyGenerationComponent generator)
    {
        if (!_power.IsPowered(energyGeneratorUid))
        {
            return;
        }

        float energyToGenerate = generator.EnergyGenerationRateMaximum * frameTime;
        if (shieldCore?.ShieldProjected != null)
        {
            energyToGenerate = energyToGenerate * generator.EnergyGenerationWhileShieldProjectedRateMultiplier;
        }

        // Amount of extra energy generation that wasn't needed to fill the system with flux.
        // To be used to 'scale' the cost of the energy generation by comparing against the maximum energy generation rate.
        float excessGeneration = GenerateEnergy(nodeGroup, energyToGenerate);

        float energyForCosting = generator.EnergyGenerationRateMaximum - excessGeneration;
        if (shieldCore?.ShieldProjected != null)
        {
            energyForCosting = energyForCosting * generator.EnergyGenerationWhileShieldProjectedCostMultiplier;
        }
    }



    /// <summary>
    /// Performs update operations for a single flux destruction component
    /// </summary>
    /// <param name="fluxDestructorUid"></param>
    /// <param name="destructor"></param>
    /// <param name="nodeContainer"></param>
    private void UpdateFluxDestructor(float frameTime, ModularShieldCoreComponent shieldCore, ModularShieldNodeGroup nodeGroup, EntityUid fluxDestructorUid, ModularShieldFluxDestructionComponent destructor, NodeContainerComponent nodeContainer)
    {
        if (!_power.IsPowered(fluxDestructorUid))
        {
            return;
        }

        float fluxToDestroy = destructor.FluxDestructionRateMaximum * frameTime;
        if (shieldCore?.ShieldProjected != null)
        {
            fluxToDestroy = fluxToDestroy * destructor.FluxDestructionWhileShieldProjectedRateMultiplier;
        }

        // Amount of extra flux destruction that wasn't needed to empty the system of flux.
        // To be used to 'scale' the cost of the flux destruction by comparing against the maximum flux destruction rate.
        float excessDestruction = DestroyFlux(nodeGroup, fluxToDestroy);

        float fluxForCosting = destructor.FluxDestructionRateMaximum - excessDestruction;
        if (shieldCore?.ShieldProjected != null)
        {
            fluxForCosting = fluxForCosting * destructor.FluxDestructionWhileShieldProjectedCostMultiplier;
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
            EntityUid shieldEntityUid = ShieldEntity((EntityUid)parentGridEntityUid, shieldCoreUid);
            if (shieldEntityUid != EntityUid.Invalid)
            {
                success = true;
                shieldCoreComponent.ShieldProjected = shieldEntityUid;
                shieldCoreComponent.ShieldedEntity = parentGridEntityUid;
            }
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
                shieldCoreComponent.ShieldedEntity != null && (  // We need to be shielding something
                    !shieldCoreComponent.ShieldProjectionEnabled || // Shield core has been turned off.
                    energyStorageStats.EnergyStored == 0 || // No energy disables shields.
                    shieldCoreComponent.FluxOverloadEnd != null // Overload disables shields.
                ))
        {
            success = UnshieldEntity((EntityUid)shieldCoreComponent.ShieldedEntity);
            shieldCoreComponent.ShieldProjected = null;
            shieldCoreComponent.ShieldedEntity = null;
        }

        return success;
    }



    private void OnModularShieldAbsorbed(EntityUid uid, ModularShieldCoreComponent component, ModularShieldAbsorbedEvent args)
    {
        var calculatedDamage = 0f;
        if (TryComp<EmpOnTriggerComponent>(args.AbsorbedProjectile, out var emp))
        {
            calculatedDamage += emp.EnergyConsumption * component.EmpDamageToNormalDamageRatio;
            _trigger.Trigger(args.AbsorbedProjectile);
        }

        if (TryComp<ExplosiveComponent>(args.AbsorbedProjectile, out var exp) && _prototypeManager.TryIndex(exp.ExplosionType, out var type))
        {
            calculatedDamage += exp.TotalIntensity * (float)type.DamagePerIntensity.GetTotal();
        }

        calculatedDamage += (float)args.Projectile.Damage.GetTotal();
        args.Projectile.ProjectileSpent = true;

        if (calculatedDamage > 0 && TryGetModularShieldNodeGroup(uid, out var nodeGroup))
        {
            DestroyEnergy(nodeGroup, calculatedDamage * component.DamageAbsorbedToEnergyDestructionRatio);
            GenerateFlux(nodeGroup, calculatedDamage * component.DamageAbsorbedToFluxGenerationRatio);
        }

        QueueDel(args.AbsorbedProjectile);
    }



    private void OnModularShieldCoreShutdown(EntityUid uid, ModularShieldCoreComponent component, ComponentShutdown args)
    {
        if (component.ShieldProjected != null && component.ShieldedEntity != null)
        {
            UnshieldEntity((EntityUid)component.ShieldedEntity);
            component.ShieldProjected = null;
            component.ShieldedEntity = null;
        }
    }



    private void OnShieldCoreExamined(EntityUid uid, ModularShieldCoreComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!TryGetModularShieldNodeGroup(uid, out var nodeGroup))
        {
            args.PushMarkup(Loc.GetString("modular-shield-disconnected"));
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
            args.PushMarkup(Loc.GetString("modular-shield-core-examine-flux-overflow", ("fluxoverflow", component.FluxOverflow)));
        }

        if (component.FluxOverloadEnd != null)
        {
            args.PushMarkup(Loc.GetString("modular-shield-core-examine-flux-overload"));
        }
    }

    private void OnEnergyGeneratorExamined(EntityUid uid, ModularShieldEnergyGenerationComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;


        if (!TryGetModularShieldNodeGroup(uid, out var nodeGroup))
        {
            args.PushMarkup(Loc.GetString("modular-shield-disconnected"));
        }

        args.PushMarkup(Loc.GetString("modular-shield-energy-generator-examine", ("energygen", component.EnergyGenerationRateMaximum)));
    }

    private void OnEnergyStorageExamined(EntityUid uid, ModularShieldEnergyStorageComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!TryGetModularShieldNodeGroup(uid, out var nodeGroup))
        {
            args.PushMarkup(Loc.GetString("modular-shield-disconnected"));
        }

        args.PushMarkup(Loc.GetString("modular-shield-energy-storage-examine", ("energy", (int)Math.Round(component.EnergyStored)), ("energymax", component.EnergyCapacity)));
    }

    private void OnFluxStorageExamined(EntityUid uid, ModularShieldFluxStorageComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!TryGetModularShieldNodeGroup(uid, out var nodeGroup))
        {
            args.PushMarkup(Loc.GetString("modular-shield-disconnected"));
        }

        args.PushMarkup(Loc.GetString("modular-shield-flux-storage-examine", ("flux", (int)Math.Round(component.FluxStored)), ("fluxmax", component.FluxCapacity)));
    }

    private void OnFluxDestructorExamined(EntityUid uid, ModularShieldFluxDestructionComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;


        if (!TryGetModularShieldNodeGroup(uid, out var nodeGroup))
        {
            args.PushMarkup(Loc.GetString("modular-shield-disconnected"));
        }

        args.PushMarkup(Loc.GetString("modular-shield-flux-destructor-examine", ("fluxdestruct", component.FluxDestructionRateMaximum)));
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
}
