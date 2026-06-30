using Content.FlagShip.Server.ModularShields.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.NodeContainer;
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
    [Dependency] private IGameTiming _gameTiming = default!;
    [Dependency] private PowerReceiverSystem _power = default!;



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

        var energyGeneration = nodeGroup.GetEnergyGeneration();
        var fluxDestruction = nodeGroup.GetFluxDestruction();

        foreach (var energyGenerator in energyGeneration)
        {
            UpdateEnergyGenerator(frameTime, shieldCore, nodeGroup, energyGenerator.EntityUid, energyGenerator.EnergyGenerationComponent);
        }

        foreach (var fluxDestructor in fluxDestruction)
        {
            UpdateFluxDestructor(frameTime, shieldCore, nodeGroup, fluxDestructor.EntityUid, fluxDestructor.FluxDestructionComponent, nodeContainer);
        }



        // Perform passive shield energy drain.
        if (shieldCore.ShieldProjected != null)
        {
            float excessDestruction = DestroyEnergy(nodeGroup, shieldCore.ShieldProjectionPassiveEnergyDrain);
        }

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

            if (shieldCore.FluxOverflowBufferEnd < curTime)
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
            TryStartModularShieldProjection(shieldCoreUid, shieldCore, nodeGroup);
        }
        else if (!shieldCore.ShieldProjectionEnabled &&
            shieldCore.ShieldProjected != null)
        {
            TryStopModularShieldProjection(shieldCoreUid, shieldCore, nodeGroup, true);
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
        float excessDestruction = DestroyFlux(nodeGroup, destructor.FluxDestructionRateMaximum);

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



    public bool TryStartModularShieldProjection(
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
            shieldCoreComponent.FluxOverflowBufferEnd != null && // Flux overflow locks shield into staying up or down
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



    public bool TryStopModularShieldProjection(
        EntityUid shieldCoreUid,
        ModularShieldCoreComponent? shieldCoreComponent = null,
        ModularShieldNodeGroup? nodeGroup = null,
        bool force = false)
    {
        if (!Resolve(shieldCoreUid, ref shieldCoreComponent))
            return false;

        var parentGridEntityUid = Transform(shieldCoreUid).GridUid;
        bool success = false;

        if (force)
        {
            success = UnshieldEntity(shieldCoreUid);
            shieldCoreComponent.ShieldProjected = null;
            shieldCoreComponent.ShieldedEntity = null;
        }
        // Whether to stop projecting the shield.
        else if (shieldCoreComponent.ShieldProjected != null && // Shield needs to be on
                shieldCoreComponent.ShieldedEntity != null &&   // We need to be shielding something
                shieldCoreComponent.FluxOverflowBufferEnd != null && ( // Flux Overflow locks shield into staying up or down
                    shieldCoreComponent.FluxOverloadEnd != null // Overload disables shields.
                ))
        {
            success = UnshieldEntity((EntityUid)shieldCoreComponent.ShieldedEntity);
            shieldCoreComponent.ShieldProjected = null;
            shieldCoreComponent.ShieldedEntity = null;
        }

        return success;
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
