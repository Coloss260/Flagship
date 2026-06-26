using Content.Shared.Foldable;
using Robust.Shared.Physics.Systems;

namespace Content.FlagShip.Shared.Foldable;

public sealed partial class FoldableFixtureSystem : EntitySystem
{
    [Dependency] private FixtureSystem _fixtures = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FlagShip.Shared.Foldable.FoldableFixtureComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<FlagShip.Shared.Foldable.FoldableFixtureComponent, FoldedEvent>(OnFolded);
    }

    private void OnMapInit(EntityUid uid, FlagShip.Shared.Foldable.FoldableFixtureComponent component, MapInitEvent args)
    {
        if (TryComp<FoldableComponent>(uid, out var foldable))
            SetFoldedFixtures(uid, foldable.IsFolded, component);
    }

    private void OnFolded(EntityUid uid, FlagShip.Shared.Foldable.FoldableFixtureComponent? component, ref FoldedEvent args)
    {
        SetFoldedFixtures(uid, args.IsFolded, component);
    }

    // Sets all relevant fixtures for the entity to an appropriate hard/soft state.
    private void SetFoldedFixtures(EntityUid uid, bool isFolded, FlagShip.Shared.Foldable.FoldableFixtureComponent? component)
    {
        if (!Resolve(uid, ref component))
            return;

        if (isFolded)
        {
            SetAllFixtureHardness(uid, component.FoldedFixtures, true);
            SetAllFixtureHardness(uid, component.UnfoldedFixtures, false);
        }
        else
        {
            SetAllFixtureHardness(uid, component.FoldedFixtures, false);
            SetAllFixtureHardness(uid, component.UnfoldedFixtures, true);
        }
    }

    // Sets all fixtures on an entity in a list to either be hard or soft.
    void SetAllFixtureHardness(EntityUid uid, List<string> fixtures, bool hard)
    {
        foreach (var fixName in fixtures)
        {
            var fixture = _fixtures.GetFixtureOrNull(uid, fixName);
            if (fixture != null)
                _physics.SetHard(uid, fixture, hard);
        }
    }
}
