using Content.FlagShip.Shared.Traits.Physical;
using Content.Shared.Body;
using Robust.Shared.Containers;

namespace Content.FlagShip.Server.Traits.Physical;

/// <summary>
/// Replaces the user's liver with a Dwarf liver on spawn.
/// </summary>
public sealed partial class LiquorLifelineSystem : EntitySystem
{
    [Dependency] private SharedBodySystem _body = null!;
    [Dependency] private SharedContainerSystem _containers = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LiquorLifelineComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, LiquorLifelineComponent comp, ComponentStartup args)
    {
        if (!TryComp<BodyComponent>(uid, out var body))
            return;

        if (_body.TryGetBodyOrganEntityComps<LiverComponent>((uid, body), out var livers))
        {
            var old = livers[0].Owner;
            if (_containers.TryGetContainingContainer((old, null, null), out var cont))
            {
                var part = cont.Owner;
                _body.RemoveOrgan(old);
                QueueDel(old);

                var spawn = Spawn("OrganDwarfLiver", Transform(part).Coordinates);
                if (TryComp(spawn, out OrganComponent? organ))
                {
                    _body.InsertOrgan(part, spawn, "liver", null, organ);
                }
                else
                {
                    QueueDel(spawn);
                }
            }
        }
    }
}


