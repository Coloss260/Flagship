using Content.FlagShip.Shared.Research;
using Content.FlagShip.Server.Research.PointDiskPrinter.Components;
using Content.Server.Research.Systems;
using Content.Shared.Research.Components;
using Content.Shared.UserInterface;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.FlagShip.Server.Research.PointDiskPrinter.Systems;

public sealed partial class PointDiskConsoleSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private ResearchSystem _research = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<Components.PointDiskConsoleComponent, PointDiskConsolePrint1KDiskMessage>(OnPrint1KDisk);
        SubscribeLocalEvent<Components.PointDiskConsoleComponent, PointDiskConsolePrint5KDiskMessage>(OnPrint5KDisk);
        SubscribeLocalEvent<Components.PointDiskConsoleComponent, PointDiskConsolePrint10KDiskMessage>(OnPrint10KDisk);
        SubscribeLocalEvent<Components.PointDiskConsoleComponent, ResearchServerPointsChangedEvent>(OnPointsChanged);
        SubscribeLocalEvent<Components.PointDiskConsoleComponent, ResearchRegistrationChangedEvent>(OnRegistrationChanged);
        SubscribeLocalEvent<Components.PointDiskConsoleComponent, BeforeActivatableUIOpenEvent>(OnBeforeUiOpen);

        SubscribeLocalEvent<Components.PointDiskConsolePrintingComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<Components.PointDiskConsolePrintingComponent, Components.PointDiskConsoleComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var printing, out var console, out var xform))
        {
            if (printing.FinishTime > _timing.CurTime)
                continue;

            RemComp(uid, printing);
            if (printing.Disk1K)
                Spawn(console.Disk1KPrototype, xform.Coordinates);

            if (printing.Disk5K)
                Spawn(console.Disk5KPrototype, xform.Coordinates);

            if (printing.Disk10K)
                Spawn(console.Disk10KPrototype, xform.Coordinates);
        }
    }

    private void OnPrint1KDisk(EntityUid uid, Components.PointDiskConsoleComponent component, PointDiskConsolePrint1KDiskMessage args)
    {
        if (HasComp<Components.PointDiskConsolePrintingComponent>(uid))
            return;

        if (!_research.TryGetClientServer(uid, out var server, out var serverComp))
            return;

        if (serverComp.Points < component.PricePer1KDisk)
            return;

        _research.ModifyServerPoints(server.Value, -component.PricePer1KDisk, serverComp);
        _audio.PlayPvs(component.PrintSound, uid);


        var printing = EnsureComp<Components.PointDiskConsolePrintingComponent>(uid);
        printing.Disk1K = true;
        printing.FinishTime = _timing.CurTime + component.PrintDuration;
        UpdateUserInterface(uid, component);
    }

    private void OnPrint5KDisk(EntityUid uid, Components.PointDiskConsoleComponent component, PointDiskConsolePrint5KDiskMessage args)
    {
        if (HasComp<Components.PointDiskConsolePrintingComponent>(uid))
            return;

        if (!_research.TryGetClientServer(uid, out var server, out var serverComp))
            return;

        if (serverComp.Points < component.PricePer5KDisk)
            return;

        _research.ModifyServerPoints(server.Value, -component.PricePer5KDisk, serverComp);
        _audio.PlayPvs(component.PrintSound, uid);

        var printing = EnsureComp<Components.PointDiskConsolePrintingComponent>(uid);
        printing.Disk5K = true;
        printing.FinishTime = _timing.CurTime + component.PrintDuration;
        UpdateUserInterface(uid, component);
    }

    private void OnPrint10KDisk(EntityUid uid, Components.PointDiskConsoleComponent component, PointDiskConsolePrint10KDiskMessage args)
    {
        if (HasComp<Components.PointDiskConsolePrintingComponent>(uid))
            return;

        if (!_research.TryGetClientServer(uid, out var server, out var serverComp))
            return;

        if (serverComp.Points < component.PricePer10KDisk)
            return;

        _research.ModifyServerPoints(server.Value, -component.PricePer10KDisk, serverComp);
        _audio.PlayPvs(component.PrintSound, uid);

        var printing = EnsureComp<Components.PointDiskConsolePrintingComponent>(uid);
        printing.Disk10K = true;
        printing.FinishTime = _timing.CurTime + component.PrintDuration;
        UpdateUserInterface(uid, component);
    }

    private void OnPointsChanged(EntityUid uid, Components.PointDiskConsoleComponent component, ref ResearchServerPointsChangedEvent args)
    {
        UpdateUserInterface(uid, component);
    }

    private void OnRegistrationChanged(EntityUid uid, Components.PointDiskConsoleComponent component, ref ResearchRegistrationChangedEvent args)
    {
        UpdateUserInterface(uid, component);
    }

    private void OnBeforeUiOpen(EntityUid uid, Components.PointDiskConsoleComponent component, BeforeActivatableUIOpenEvent args)
    {
        UpdateUserInterface(uid, component);
    }

    public void UpdateUserInterface(EntityUid uid, Components.PointDiskConsoleComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        var totalPoints = 0;
        if (_research.TryGetClientServer(uid, out _, out var server))
        {
            totalPoints = server.Points;
        }

        var canPrint1K = !(TryComp<Components.PointDiskConsolePrintingComponent>(uid, out var printing1K) && printing1K.FinishTime >= _timing.CurTime) &&
                       totalPoints >= component.PricePer1KDisk;

        var canPrint5K = !(TryComp<Components.PointDiskConsolePrintingComponent>(uid, out var printing5K) && printing5K.FinishTime >= _timing.CurTime) &&
                       totalPoints >= component.PricePer5KDisk;

        var canPrint10K = !(TryComp<Components.PointDiskConsolePrintingComponent>(uid, out var printing10K) && printing10K.FinishTime >= _timing.CurTime) &&
                       totalPoints >= component.PricePer10KDisk;

        var state = new PointDiskConsoleBoundUserInterfaceState(totalPoints, component.PricePer1KDisk, component.PricePer5KDisk, component.PricePer10KDisk, canPrint1K, canPrint5K, canPrint10K);
        _ui.SetUiState(uid, PointDiskConsoleUiKey.Key, state);
    }

    private void OnShutdown(EntityUid uid, Components.PointDiskConsolePrintingComponent component, ComponentShutdown args)
    {
        UpdateUserInterface(uid);
    }
}
