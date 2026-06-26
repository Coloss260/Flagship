using Content.Shared.Actions;

namespace Content.FlagShip.Shared.Abilities.Goliath;

internal sealed partial class GoliathTentacleSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actionsSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<GoliathTentacle.GoliathTentacleComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GoliathTentacle.GoliathTentacleComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, GoliathTentacle.GoliathTentacleComponent component, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, ref component.ActionEntity, component.Action);
    }

    private void OnShutdown(EntityUid uid, GoliathTentacle.GoliathTentacleComponent component, ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(uid, component.ActionEntity);
    }
}
