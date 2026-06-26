using Content.Server.Lightning;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Systems;
using Robust.Shared.Random;

namespace Content.FlagShip.Server.Trigger;

public sealed partial class MonoTriggerSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private LightningSystem _lightning = default!;
    [Dependency] private TriggerSystem _trigger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlagShip.Server.Trigger.LightningOnTriggerComponent, TriggerEvent>(OnTriggerLightning);
        SubscribeLocalEvent<TriggerOnProjectileSpentComponent, ProjectileSpentEvent>(OnProjectileSpent);
    }

    private void OnTriggerLightning(Entity<FlagShip.Server.Trigger.LightningOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (!_random.Prob(ent.Comp.Chance))
            return;

        _lightning.ShootRandomLightnings(ent, ent.Comp.Range, ent.Comp.Count, ent.Comp.LightningProto, ent.Comp.ArcDepth, ent.Comp.LightningEffects);
    }

    private void OnProjectileSpent(Entity<TriggerOnProjectileSpentComponent> ent, ref ProjectileSpentEvent args)
    {
        _trigger.Trigger(ent);
    }
}
