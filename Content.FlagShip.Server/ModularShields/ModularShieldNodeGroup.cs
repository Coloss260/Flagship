using Content.FlagShip.Server.ModularShields.Components;
using Content.Server.NodeContainer.NodeGroups;
using Content.Shared.NodeContainer;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Content.FlagShip.Server.ModularShields;

public sealed partial class ModularShieldNodeGroup : BaseNodeGroup
{
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IGameTiming _gameTiming = default!;

    /// <summary>
    /// The core of the modular shield system in control of this node group.
    /// </summary>
    [ViewVariables]
    private EntityUid? _masterShieldCore;

    public EntityUid? MasterShieldCore => _masterShieldCore;

    private HashSet<EntityUid> _energyGenerationEntities = [];
    private HashSet<EntityUid> _energyStorageEntities = [];

    private HashSet<EntityUid> _fluxStorageEntities = [];
    private HashSet<EntityUid> _fluxDestructionEntities = [];

    private int _tieBreakerCount = 0;



    public override void LoadNodes(List<Node> groupNodes)
    {
        _masterShieldCore = null;

        _energyGenerationEntities.Clear();
        _energyStorageEntities.Clear();

        _fluxStorageEntities.Clear();
        _fluxDestructionEntities.Clear();


        var shieldCoreQuery = _entMan.GetEntityQuery<ModularShieldCoreComponent>();
        var energyGenerationQuery = _entMan.GetEntityQuery<ModularShieldEnergyGenerationComponent>();
        var energyStorageQuery = _entMan.GetEntityQuery<ModularShieldEnergyStorageComponent>();
        var fluxStorageQuery = _entMan.GetEntityQuery<ModularShieldFluxStorageComponent>();
        var fluxDestructionQuery = _entMan.GetEntityQuery<ModularShieldFluxDestructionComponent>();

        base.LoadNodes(groupNodes);

        foreach (var node in groupNodes)
        {
            var nodeOwner = node.Owner;
            if (shieldCoreQuery.TryGetComponent(nodeOwner, out var shieldCore))
            {
                if (_masterShieldCore == null)
                    _masterShieldCore = nodeOwner;
            }

            if (energyGenerationQuery.TryGetComponent(nodeOwner, out var energyGen))
            {
                energyGen.EnergyGenerationPriorityTieBreaker = _tieBreakerCount++;
                _energyGenerationEntities.Add(nodeOwner);
            }

            if (energyStorageQuery.TryGetComponent(nodeOwner, out var energyStore))
            {
                energyStore.EnergyStoragePriorityTieBreaker = _tieBreakerCount++;
                _energyStorageEntities.Add(nodeOwner);
            }

            if (fluxStorageQuery.TryGetComponent(nodeOwner, out var fluxStore))
            {
                fluxStore.FluxStoragePriorityTieBreaker = _tieBreakerCount++;
                _fluxStorageEntities.Add(nodeOwner);
            }

            if (fluxDestructionQuery.TryGetComponent(nodeOwner, out var fluxDestr))
            {
                fluxDestr.FluxDestructionPriorityTieBreaker = _tieBreakerCount++;
                _fluxDestructionEntities.Add(nodeOwner);
            }
        }
    }



    public (float EnergyStored, int EnergyCapacity) GetEnergyStorageStatistics()
    {
        var totalEnergyStored = 0f;
        var totalEnergyCapacity = 0;
        foreach (var energyStorage in GetEnergyStorage())
        {
            var energyStorageComponent = energyStorage.EnergyStorageComponent;
            totalEnergyStored += energyStorageComponent.EnergyStored;
            totalEnergyCapacity += energyStorageComponent.EnergyCapacity;
        }

        return (totalEnergyStored, totalEnergyCapacity);
    }



    public (float FluxStored, int FluxCapacity) GetFluxStorageStatistics()
    {
        var totalFluxStored = 0f;
        var totalFluxCapacity = 0;
        foreach (var fluxStorage in GetFluxStorage())
        {
            var fluxStorageComponent = fluxStorage.FluxStorageComponent;
            totalFluxStored += fluxStorageComponent.FluxStored;
            totalFluxCapacity += fluxStorageComponent.FluxCapacity;
        }

        return (totalFluxStored, totalFluxCapacity);
    }




    public (EntityUid EntityUid, ModularShieldCoreComponent ShieldCoreComponent)? GetMasterModularShieldCore()
    {
        if (_masterShieldCore == null)
            return null;
        var shieldCoreQuery = _entMan.GetEntityQuery<ModularShieldCoreComponent>();

        ModularShieldCoreComponent? shieldCore = null;

        if (shieldCoreQuery.Resolve(_masterShieldCore.Value, ref shieldCore))
        {
            return ((EntityUid)_masterShieldCore, shieldCore);
        }
        return null;
    }


    public IEnumerable<(EntityUid EntityUid, ModularShieldEnergyGenerationComponent EnergyGenerationComponent)> GetEnergyGeneration()
    {
        var energyGenerationQuery = _entMan.GetEntityQuery<ModularShieldEnergyGenerationComponent>();

        foreach (var uid in _energyGenerationEntities)
        {
            ModularShieldEnergyGenerationComponent? energyGenerationComponent = null;
            if (energyGenerationQuery.Resolve(uid, ref energyGenerationComponent))
            {
                yield return (uid, energyGenerationComponent);
            }
        }
    }

    public IEnumerable<(EntityUid EntityUid, ModularShieldEnergyStorageComponent EnergyStorageComponent)> GetEnergyStorage()
    {
        var energyStorageQuery = _entMan.GetEntityQuery<ModularShieldEnergyStorageComponent>();

        foreach (var uid in _energyStorageEntities)
        {
            ModularShieldEnergyStorageComponent? energyStorageComponent = null;
            if (energyStorageQuery.Resolve(uid, ref energyStorageComponent))
            {
                yield return (uid, energyStorageComponent);
            }
        }
    }

    public IEnumerable<(EntityUid EntityUid, ModularShieldFluxStorageComponent FluxStorageComponent)> GetFluxStorage()
    {
        var fluxStorageQuery = _entMan.GetEntityQuery<ModularShieldFluxStorageComponent>();

        foreach (var uid in _fluxStorageEntities)
        {
            ModularShieldFluxStorageComponent? fluxStorageComponent = null;
            if (fluxStorageQuery.Resolve(uid, ref fluxStorageComponent))
            {
                yield return (uid, fluxStorageComponent);
            }
        }
    }

    public IEnumerable<(EntityUid EntityUid, ModularShieldFluxDestructionComponent FluxDestructionComponent)> GetFluxDestruction()
    {
        var fluxDestructionQuery = _entMan.GetEntityQuery<ModularShieldFluxDestructionComponent>();

        foreach (var uid in _fluxDestructionEntities)
        {
            ModularShieldFluxDestructionComponent? fluxDestructionComponent = null;
            if (fluxDestructionQuery.Resolve(uid, ref fluxDestructionComponent))
            {
                yield return (uid, fluxDestructionComponent);
            }
        }
    }

    public IEnumerable<(EntityUid EntityUid, ModularShieldEnergyGenerationComponent EnergyGenerationComponent)> OrderEnergyGenerationByPriority(IEnumerable<(EntityUid EntityUid, ModularShieldEnergyGenerationComponent EnergyGenerationComponent)> components, bool highestPriorityFirst = true)
    {
        return components
            .OrderByDescending(component => component.EnergyGenerationComponent.EnergyGenerationPriority) // Order by priority (lower numbers = higher priority).
            .ThenByDescending(component => component.EnergyGenerationComponent.EnergyGenerationPriorityTieBreaker); // Arbitrary ordering
    }

    public IEnumerable<(EntityUid EntityUid, ModularShieldEnergyStorageComponent EnergyStorageComponent)> OrderEnergyStorageByPriority(IEnumerable<(EntityUid EntityUid, ModularShieldEnergyStorageComponent EnergyStorageComponent)> components, bool highestPriorityFirst = true)
    {
        return components
            .OrderByDescending(storage => storage.EnergyStorageComponent.EnergyStoragePriority) // Order by priority (lower numbers = higher priority).
            .ThenByDescending(storage => storage.EnergyStorageComponent.EnergyStoragePriorityTieBreaker); // Arbitrary ordering
    }

    public IEnumerable<(EntityUid EntityUid, ModularShieldFluxStorageComponent FluxStorageComponent)> OrderFluxStorageByPriority(IEnumerable<(EntityUid EntityUid, ModularShieldFluxStorageComponent FluxStorageComponent)> components, bool highestPriorityFirst = true)
    {
        return components
            .OrderByDescending(storage => storage.FluxStorageComponent.FluxStoragePriority) // Order by priority (lower numbers = higher priority).
            .ThenByDescending(storage => storage.FluxStorageComponent.FluxStoragePriorityTieBreaker); // Arbitrary ordering
    }

    public IEnumerable<(EntityUid EntityUid, ModularShieldFluxDestructionComponent FluxDestructionComponent)> OrderFluxDestructionByPriority(IEnumerable<(EntityUid EntityUid, ModularShieldFluxDestructionComponent FluxDestructionComponent)> components, bool highestPriorityFirst = true)
    {
        return components
            .OrderByDescending(component => component.FluxDestructionComponent.FluxDestructionPriority) // Order by priority (lower numbers = higher priority).
            .ThenByDescending(component => component.FluxDestructionComponent.FluxDestructionPriorityTieBreaker); // Arbitrary ordering
    }
}
