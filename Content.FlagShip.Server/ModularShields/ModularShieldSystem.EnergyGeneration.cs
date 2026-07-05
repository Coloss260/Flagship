using Content.FlagShip.Shared.ModularShields.Components;
using Content.Shared.Examine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Server.ModularShields;

public sealed partial class ModularShieldSystem
{
    public void InitializeEnergyGeneration()
    {
        SubscribeLocalEvent<ModularShieldEnergyGenerationComponent, ExaminedEvent>(OnEnergyGeneratorExamined);
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

    private void OnEnergyGeneratorExamined(EntityUid uid, ModularShieldEnergyGenerationComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;


        if (!TryGetModularShieldNodeGroup(uid, out var nodeGroup))
        {
            args.PushMarkup(Loc.GetString("modular-shield-examine-disconnected"));
        }

        args.PushMarkup(Loc.GetString("modular-shield-energy-generator-examine", ("energygen", component.EnergyGenerationRateMaximum)));
    }
}
