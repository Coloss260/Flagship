using Content.FlagShip.Shared.ModularShields.Components;
using Content.Shared.Shuttles.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Content.FlagShip.Server.ModularShields;

public partial class ModularShieldSystem
{
    /// <summary>
    /// Adds the specified amount of energy to the energy storage components in this node group.
    /// </summary>
    /// <param name="amountToAdd"></param>
    /// <returns>Amount of energy that could not be added due to lack of capacity</returns>
    public float GenerateEnergy(ModularShieldNodeGroup nodeGroup, float amountToAdd)
    {
        ModularShieldCoreComponent? shieldCore = nodeGroup.GetMasterModularShieldCore()?.Comp;
        if (shieldCore == null)
        {
            // No master shield core, disabled.
            return amountToAdd;
        }

        var energyStorage = nodeGroup.OrderEnergyStorageByPriority(nodeGroup.GetEnergyStorage(), highestPriorityFirst: true);

        var amountRemaining = amountToAdd;

        foreach (var energyStorageEntity in energyStorage)
        {
            var energyStorageComponent = energyStorageEntity.Comp;
            var remainingCapacity = energyStorageComponent.EnergyCapacity - energyStorageComponent.EnergyStored;

            if (remainingCapacity < amountRemaining)
            {
                energyStorageComponent.EnergyStored = energyStorageComponent.EnergyCapacity;
                amountRemaining -= remainingCapacity;
            }
            else
            {
                energyStorageComponent.EnergyStored += amountRemaining;
                amountRemaining = 0;
            }

            UpdateEnergyStorageDisplay(energyStorageEntity);

            if (amountRemaining <= 0)
            {
                break;
            }
        }

        return amountRemaining;
    }

    /// <summary>
    /// Destroys the specified amount of energy from the energy storage components in this node group.
    /// </summary>
    /// <param name="amountToDestroy"></param>
    /// <returns>Amount of energy that could not be destroyed due to lack of stored energy</returns>
    public float DestroyEnergy(ModularShieldNodeGroup nodeGroup, float amountToDestroy)
    {

        var shieldCore = nodeGroup.GetMasterModularShieldCore();
        if (shieldCore == null)
        {
            // No master shield core, disabled.
            return amountToDestroy;
        }
        var shieldCoreEntityUid = shieldCore.Value.Owner;

        // We're destroying energy, so we sort in reverse order to remove from the lowest priority storage first.
        var energyStorage = nodeGroup.OrderEnergyStorageByPriority(nodeGroup.GetEnergyStorage(), highestPriorityFirst: false);

        var amountRemaining = amountToDestroy;

        foreach (var energyStorageEntity in energyStorage)
        {
            var energyStorageComponent = energyStorageEntity.Comp;
            if (energyStorageComponent.EnergyStored < amountRemaining)
            {
                amountRemaining -= energyStorageComponent.EnergyStored;
                energyStorageComponent.EnergyStored = 0;
            }
            else
            {
                energyStorageComponent.EnergyStored -= amountRemaining;
                amountRemaining = 0;
            }

            UpdateEnergyStorageDisplay(energyStorageEntity);

            if (amountRemaining <= 0)
            {
                break;
            }
        }

        return amountRemaining;
    }



    /// <summary>
    /// Adds the specified amount of flux to the flux storage components in this node group.
    /// </summary>
    /// <param name="amountToAdd"></param>
    /// <returns>Amount of flux that could not be added due to lack of capacity</returns>
    public float GenerateFlux(ModularShieldNodeGroup nodeGroup, float amountToAdd)
    {
        ModularShieldCoreComponent? shieldCore = nodeGroup.GetMasterModularShieldCore()?.Comp;
        if (shieldCore == null)
        {
            // No master shield core, disabled.
            return amountToAdd;
        }

        var fluxStorage = nodeGroup.OrderFluxStorageByPriority(nodeGroup.GetFluxStorage(), highestPriorityFirst: true);

        var amountRemaining = amountToAdd;

        // Check if flux is overflowing and possibly deny access to normal storage components
        // To force incoming flux to overflow buffer instead.
        if (shieldCore.FluxOverflowBufferEnd == null ||
            shieldCore.FluxOverflowAllowUsingNormalStorageDuringOverflow)
        {
            foreach (var fluxStorageEntity in fluxStorage)
            {
                var fluxStorageComponent = fluxStorageEntity.Comp;
                var remainingCapacity = fluxStorageComponent.FluxCapacity - fluxStorageComponent.FluxStored;

                if (remainingCapacity < amountRemaining)
                {
                    fluxStorageComponent.FluxStored = fluxStorageComponent.FluxCapacity;
                    amountRemaining -= remainingCapacity;
                }
                else
                {
                    fluxStorageComponent.FluxStored += amountRemaining;
                    amountRemaining = 0;
                }

                UpdateFluxStorageDisplay(fluxStorageEntity);

                if (amountRemaining <= 0)
                {
                    break;
                }
            }
        }


        // Master shield core flux overflow.
        if (amountRemaining > 0)
        {
            shieldCore.FluxOverflow += amountRemaining;
        }

        return amountRemaining;
    }

    /// <summary>
    /// Destroys the specified amount of flux from the flux storage components in this node group.
    /// </summary>
    /// <param name="amountToDestroy"></param>
    /// <returns>Amount of flux that could not be destroyed due to lack of stored flux</returns>
    public float DestroyFlux(ModularShieldNodeGroup nodeGroup, float amountToDestroy)
    {
        ModularShieldCoreComponent? masterShieldCore = nodeGroup.GetMasterModularShieldCore()?.Comp;
        if (masterShieldCore == null)
        {
            // No master shield core, disabled.
            return amountToDestroy;
        }

        // Check if the shield is in a situation in which destroying flux is not allowed.
        // FluxOverflow
        if (masterShieldCore.FluxOverflowBufferEnd != null &&
            !masterShieldCore.FluxOverflowFluxDestructionAllowed)
        {
            return amountToDestroy;
        }

        var amountRemaining = amountToDestroy;

        // Handle flux overflow destruction.
        if (masterShieldCore.FluxOverflowFluxDestructionAllowed &&
            masterShieldCore.FluxOverflow > 0)
        {
            if (masterShieldCore.FluxOverflow < amountRemaining)
            {
                amountRemaining -= masterShieldCore.FluxOverflow;
                masterShieldCore.FluxOverflow = 0;
            }
            else
            {
                masterShieldCore.FluxOverflow -= amountRemaining;
                amountRemaining = 0;
            }
        }

        // We're destroying flux, so we sort in reverse order to remove from the lowest priority storage first.
        var fluxStorage = nodeGroup.OrderFluxStorageByPriority(nodeGroup.GetFluxStorage(), highestPriorityFirst: false);

        foreach (var fluxStorageEntity in fluxStorage)
        {
            var fluxStorageComponent = fluxStorageEntity.Comp;
            if (fluxStorageComponent.FluxStored < amountRemaining)
            {
                amountRemaining -= fluxStorageComponent.FluxStored;
                fluxStorageComponent.FluxStored = 0;
            }
            else
            {
                fluxStorageComponent.FluxStored -= amountRemaining;
                amountRemaining = 0;
            }

            UpdateFluxStorageDisplay(fluxStorageEntity);

            if (amountRemaining <= 0)
            {
                break;
            }
        }

        return amountRemaining;
    }
}
