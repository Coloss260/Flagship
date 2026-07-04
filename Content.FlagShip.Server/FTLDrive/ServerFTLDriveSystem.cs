using Content.FlagShip.Shared.FTLDrive;
using Content.Server.Power.EntitySystems;
using Content.Server.Shuttles.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.FlagShip.Server.FTLDrive;

public sealed partial class ServerFTLDriveSystem : EntitySystem
{
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private FTLDriveSystem _ftlDrive = default!;
    [Dependency] private PowerReceiverSystem _power = default!;

    private TimeSpan _nextUiUpdate = TimeSpan.Zero;
    private readonly TimeSpan _uiUpdateInterval = TimeSpan.FromMilliseconds(500); // every half a second

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShuttleFTLDriveComponent, FTLStartedEvent>(OnFTLStarted);
        SubscribeLocalEvent<ShuttleFTLDriveComponent, FTLCompletedEvent>(OnFTLCompleted);
        SubscribeLocalEvent<FTLDriveComponent, FTLChargeButtonPressedMessage>(OnButtonPressed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ActiveFTLDriveComponent, FTLDriveComponent>();
        while (query.MoveNext(out var uid, out _, out var drive))
        {
            if (drive.NextPowerCheckTime != TimeSpan.Zero && curTime > drive.NextPowerCheckTime)
            {
                if (!_power.IsPowered(uid))
                    _ftlDrive.ShutDownFTLDrive((uid, drive), drive.CoolDownTimeBreakDown);

                drive.NextPowerCheckTime = TimeSpan.Zero;
                Dirty(uid, drive);
            }

            if (drive.State == FTLDriveState.Engaged && curTime > drive.EngagedBreakdownTime)
                _ftlDrive.BreakDownFTLDrive((uid, drive));

            if (drive.State == FTLDriveState.Charging && curTime > drive.StartUpFinishTime)
                _ftlDrive.FinishChargingFTLDrive((uid, drive));
        }

        if (curTime < _nextUiUpdate)
            return;

        _nextUiUpdate = curTime + _uiUpdateInterval;

        // Set the UI status
        var query2 = EntityQueryEnumerator<FTLDriveComponent>();
        while (query2.MoveNext(out var uid, out _))
        {
            var data = new FTLDriveBuiState(_ftlDrive.GetDriveStatus(uid));
            _ui.SetUiState(uid, FTLDriveUiKey.Key, data);
        }
    }

    private void OnButtonPressed(Entity<FTLDriveComponent> ent, ref FTLChargeButtonPressedMessage args)
    {
        _ftlDrive.TryToggleFTLDrive(ent);
    }

    private void OnFTLCompleted(Entity<ShuttleFTLDriveComponent> ent, ref FTLCompletedEvent args)
    {
        var ftlDrive = _ftlDrive.GetFTLDriveEnt(ent.Comp.FTLDriveEntity);

        if (ftlDrive is null)
            return;

        var drive = ftlDrive.Value;
        var ftlComponents = ftlDrive.Value.Comp.FTLComponents;

        _ftlDrive.ShutDownFTLDrive(drive, drive.Comp.CoolDownTime);

        if (ftlComponents is not null)
            EntityManager.RemoveComponents(drive.Owner, ftlComponents);
    }

    private void OnFTLStarted(Entity<ShuttleFTLDriveComponent> ent, ref FTLStartedEvent args)
    {
        var ftlDrive = _ftlDrive.GetFTLDriveEnt(ent.Comp.FTLDriveEntity);

        if (ftlDrive is null)
            return;

        var drive = ftlDrive.Value;
        var ftlComponents = ftlDrive.Value.Comp.FTLComponents;

        if (ftlComponents is not null)
            EntityManager.AddComponents(drive.Owner, ftlComponents);

        drive.Comp.State = FTLDriveState.InWarp;
        Dirty(drive);
    }
}
