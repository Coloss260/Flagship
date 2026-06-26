using Content.FlagShip.Server.DelayedDeath;

namespace Content.FlagShip.Server.Body.Organ;

public sealed partial class HeartSystem : EntitySystem
{
    [Dependency] private FlagShip.Shared.Body.Systems.SharedBodySystem _bodySystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeartComponent, OrganAddedToBodyEvent>(HandleAddition);
        SubscribeLocalEvent<HeartComponent, OrganRemovedFromBodyEvent>(HandleRemoval);
    }

    private void HandleRemoval(EntityUid uid, HeartComponent _, ref OrganRemovedFromBodyEvent args)
    {
        if (TerminatingOrDeleted(uid) || TerminatingOrDeleted(args.OldBody))
            return;

        // TODO: Add some form of very violent bleeding effect.
        EnsureComp<DelayedDeathComponent>(args.OldBody);
    }

    private void HandleAddition(EntityUid uid, HeartComponent _, ref OrganAddedToBodyEvent args)
    {
        if (TerminatingOrDeleted(uid) || TerminatingOrDeleted(args.Body))
            return;

        if (_bodySystem.TryGetBodyOrganEntityComps<BrainComponent>(args.Body, out var _))
            RemComp<DelayedDeathComponent>(args.Body);
    }
    // Shitmed-End
}
