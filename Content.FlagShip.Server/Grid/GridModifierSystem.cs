using Content.FlagShip.Shared.Grid;
using Content.FlagShip.Shared.ShipRepair;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Server.Grid;

/// <summary>
/// This handles grid modification on initialization.
/// </summary>
public sealed partial class GridModifierSystem : SharedGridModifierSystem
{
    [Dependency] private IPrototypeManager _protoMan = default!;
    [Dependency] private IComponentFactory _factory = default!;
    [Dependency] private SharedShipRepairSystem _repair = default!;

    private List<EntityUid> _snapQueue = [];

    public override void Initialize()
    {
        SubscribeLocalEvent<FlagShip.Server.Grid.GridModifierComponent, MapInitEvent>(OnInit);
    }

    public override void Update(float frameTime)
    {
        foreach (var uid in _snapQueue)
        {
            _repair.GenerateRepairData(uid);
        }
        _snapQueue.Clear();
    }

    private void OnInit(EntityUid uid, FlagShip.Server.Grid.GridModifierComponent component, MapInitEvent args)
    {
        ModifyGrid(uid, component.Modifications);
    }

    public void ModifyGrid(EntityUid uid, List<ProtoId<GridModificationPrototype>> modifiers)
    {
        if (!HasComp<MapGridComponent>(uid))
            return;

        foreach (var modProto  in modifiers)
        {
            if (!_protoMan.TryIndex(modProto, out var mod))
                continue;

            foreach (var modifier in mod.Modifiers)
            {
                modifier.Modify(uid, EntityManager, _factory);
            }
        }

        _snapQueue.Add(uid);
    }
}
