using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Pvs;

public sealed partial class GlobalPvsSystem : EntitySystem
{
    [Dependency] private SharedPvsOverrideSystem _pvs = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GlobalPvsComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<GlobalPvsComponent> ent, ref ComponentInit args)
    {
        _pvs.AddGlobalOverride(ent);
    }
}
