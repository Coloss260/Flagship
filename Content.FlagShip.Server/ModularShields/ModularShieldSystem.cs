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
    /// <param name="uid"></param>
    /// <param name="core"></param>
    /// <param name="nodeContainer"></param>
    private void UpdateCore(float frameTime, EntityUid uid, ModularShieldCoreComponent core, NodeContainerComponent nodeContainer)
    {
        var curTime = _gameTiming.CurTime;

        if (!TryGetModularShieldNodeGroup(uid, out var nodeGroup, nodeContainer))
            return;

        if (!_power.IsPowered(uid))
        {
            return;
        }

        var energyGeneration = nodeGroup.GetEnergyGeneration();
        var fluxDestruction = nodeGroup.GetFluxDestruction();

        foreach (var energyGenerator in energyGeneration)
        {
            UpdateEnergyGenerator(frameTime, core, nodeGroup, energyGenerator.Item1, energyGenerator.Item2);
        }

        foreach (var fluxDestructor in fluxDestruction)
        {
            UpdateFluxDestructor(frameTime, core, nodeGroup, fluxDestructor.Item1, fluxDestructor.Item2, nodeContainer);
        }



        // Perform passive shield energy drain.
        if (core.ProjectingShield)
        {
            float excessDestruction = DestroyEnergy(nodeGroup, core.ShieldProjectionPassiveEnergyDrain);
        }

        // Check an ongoing shield core overload is finished.
        if (core.FluxOverloadEnd != null)
        {
            if (core.FluxOverloadEnd < curTime)
            {
                core.FluxOverloadEnd = null;
            }
        }

        // Check an ongoing shield core overflow buffer
        if (core.FluxOverflowBufferEnd != null)
        {
            if (core.FluxOverflow == 0)
            {
                // Flux overflow cleared.
                core.FluxOverflowBufferEnd = null;
            }

            if (core.FluxOverflowBufferEnd < curTime)
            {
                // Overload the shield.
                PerformShieldCoreOverloadPunishments(uid, core, core.FluxOverflow);
                core.FluxOverflow = 0;
                core.FluxOverflowBufferEnd = null;
                core.FluxOverloadEnd = curTime + core.FluxOverloadDuration;
                core.ProjectingShield = false;
            }
        }
        // Check whether to start shield  core overflow.
        else if (core.FluxOverflow > 0)
        {
            core.FluxOverflowBufferEnd = curTime + core.FluxOverflowBufferDuration;
        }

        // Check whether to start projecting the shield
    }



    /// <summary>
    /// Performs update operations for a single energy generation component
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="generator"></param>
    /// <param name="nodeContainer"></param>
    private void UpdateEnergyGenerator(float frameTime, ModularShieldCoreComponent shieldCore, ModularShieldNodeGroup nodeGroup, EntityUid uid, ModularShieldEnergyGenerationComponent generator)
    {
        if (!_power.IsPowered(uid))
        {
            return;
        }

        float energyToGenerate = generator.EnergyGenerationRateMaximum * frameTime;
        if (shieldCore?.ProjectingShield ?? false)
        {
            energyToGenerate = energyToGenerate * generator.EnergyGenerationWhileShieldProjectedRateMultiplier;
        }

        // Amount of extra energy generation that wasn't needed to fill the system with flux.
        // To be used to 'scale' the cost of the energy generation by comparing against the maximum energy generation rate.
        float excessGeneration = GenerateEnergy(nodeGroup, energyToGenerate);

        float energyForCosting = generator.EnergyGenerationRateMaximum - excessGeneration;
        if (shieldCore?.ProjectingShield ?? false)
        {
            energyForCosting = energyForCosting * generator.EnergyGenerationWhileShieldProjectedCostMultiplier;
        }
    }



    /// <summary>
    /// Performs update operations for a single flux destruction component
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="destructor"></param>
    /// <param name="nodeContainer"></param>
    private void UpdateFluxDestructor(float frameTime, ModularShieldCoreComponent shieldCore, ModularShieldNodeGroup nodeGroup, EntityUid uid, ModularShieldFluxDestructionComponent destructor, NodeContainerComponent nodeContainer)
    {
        if (!_power.IsPowered(uid))
        {
            return;
        }

        float fluxToDestroy = destructor.FluxDestructionRateMaximum * frameTime;
        if (shieldCore?.ProjectingShield ?? false)
        {
            fluxToDestroy = fluxToDestroy * destructor.FluxDestructionWhileShieldProjectedRateMultiplier;
        }

        // Amount of extra flux destruction that wasn't needed to empty the system of flux.
        // To be used to 'scale' the cost of the flux destruction by comparing against the maximum flux destruction rate.
        float excessDestruction = DestroyFlux(nodeGroup, destructor.FluxDestructionRateMaximum);

        float fluxForCosting = destructor.FluxDestructionRateMaximum - excessDestruction;
        if (shieldCore?.ProjectingShield ?? false)
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
