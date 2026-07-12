using Content.FlagShip.Shared.FTLDrive;
using Robust.Client.UserInterface;

namespace Content.FlagShip.Client.FTLDrive;

public sealed partial class FTLDriveBoundUserInterface : BoundUserInterface
{
    [Dependency] private IEntityManager _entMan = default!;
    private FTLDriveSystem _ftlDrive = default!;
    private FTLDriveUI? _menu;

    public FTLDriveBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _ftlDrive = _entMan.System<FTLDriveSystem>();

        _menu = this.CreateWindow<FTLDriveUI>();

        var stats = _ftlDrive.GetDriveStats(Owner);

        _menu.OnFTLDriveChargeButtonPressed += () =>
        {
            SendMessage(new FTLChargeButtonPressedMessage());
        };

        var formattedBreakDown = GetFormatted(stats.CoolDownFailureTime.Seconds, stats.CoolDownFailureTime.Minutes);
        var formattedCoolDown = GetFormatted(stats.CoolDownFinishedTime.Seconds, stats.CoolDownFinishedTime.Minutes);

        _menu.StatusText.Text = Loc.GetString("ftl-menu-status", ("Status", stats.State.ToString().ToUpper()));
        _menu.StatsRange.Text = Loc.GetString("ftl-menu-range", ("Range", stats.Range));
        _menu.StatsCooldown.Text = Loc.GetString("ftl-menu-cooldown", ("CoolTime", stats.CoolDown));
        _menu.StatsStartup.Text = Loc.GetString("ftl-menu-startup", ("StartTime", stats.StartUp));
        _menu.StatsStableTime.Text = Loc.GetString("ftl-menu-stable", ("StableTime", stats.StableTime));
        _menu.CoolDownTimeLeft.Text = Loc.GetString("ftl-menu-cooldown-time", ("FinishedTime", formattedCoolDown));
        _menu.TimeTillCoolFailure.Text = Loc.GetString("ftl-menu-cooling-failure", ("FailTime", formattedBreakDown));
        _menu.CurrentPowerDraw.Text = Loc.GetString("ftl-menu-power-draw", ("Draw", stats.PowerDraw));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_menu is null || state is not FTLDriveBuiState status)
            return;

        // Do rounding and clamp and formatting.
        var formattedBreakDown = GetFormatted(status.DriveData.CoolDownFailureTime.Seconds, status.DriveData.CoolDownFailureTime.Minutes);
        var formattedCoolDown = GetFormatted(status.DriveData.CoolDownFinishedTime.Seconds, status.DriveData.CoolDownFinishedTime.Minutes);

        // Set the UI text
        _menu.StatusText.Text = Loc.GetString("ftl-menu-status", ("Status", status.DriveData.State.ToString().ToUpper()));
        _menu.CoolDownTimeLeft.Text = Loc.GetString("ftl-menu-cooldown-time", ("FinishedTime", formattedCoolDown));
        _menu.TimeTillCoolFailure.Text = Loc.GetString("ftl-menu-cooling-failure", ("FailTime", formattedBreakDown));
        _menu.CurrentPowerDraw.Text = Loc.GetString("ftl-menu-power-draw", ("Draw", status.DriveData.PowerDraw));
    }

    private string GetFormatted(float seconds, float minutes)
    {
        var breakDownSeconds = Math.Clamp(seconds, 0, 999);
        var breakDownMinutes = Math.Clamp(minutes, 0, 999);

        return string.Format("{0:00}:{1:00}", breakDownMinutes, breakDownSeconds);
    }
}
