using Content.FlagShip.Common.FTLDrive;
using Content.FlagShip.Shared.ModSystem.Prototypes;
using Content.Shared.Audio;
using Content.Shared.Explosion.EntitySystems;
using Content.Shared.Power;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.FlagShip.Shared.ModSystem.Systems;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Shared.FTLDrive;

public sealed partial class FTLDriveSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedExplosionSystem _explosion = default!;
    [Dependency] private SharedPowerStateSystem _powerState = default!;
    [Dependency] private ModifierSystem _modSystem = default!;

    private static readonly ProtoId<ModAspectPrototype> FTLRangeAspect = "FTLDriveRange";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShuttleFTLDriveComponent, GetFTLDriveRangeEvent>(OnGetRange);
        SubscribeLocalEvent<FTLDriveComponent, AnchorStateChangedEvent>(OnAnchored);
        SubscribeLocalEvent<FTLDriveComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnAnchored(Entity<FTLDriveComponent> ent, ref AnchorStateChangedEvent args)
    {
        var shuttleUid = Transform(ent.Owner).GridUid;

        if (shuttleUid is null)
            return;

        if (!args.Anchored)
        {
            RemComp<ShuttleFTLDriveComponent>(shuttleUid.Value);
            return;
        }
        var shuttleDriveComponent = EnsureComp<ShuttleFTLDriveComponent>(shuttleUid.Value);

        shuttleDriveComponent.FTLDriveEntity = ent.Owner;
        Dirty(shuttleUid.Value, shuttleDriveComponent);
    }

    private void OnGetRange(Entity<ShuttleFTLDriveComponent> ent, ref GetFTLDriveRangeEvent args)
    {
        var driveEntity = GetFTLDriveEnt(ent.Comp.FTLDriveEntity);

        if (driveEntity is null || driveEntity.Value.Comp.State is not FTLDriveState.Engaged)
            return;

        var drive = driveEntity.Value;

        args.Range = _modSystem.GetNumberModified(drive.Comp.Range, drive.Owner, FTLRangeAspect);
    }

    private void OnPowerChanged(Entity<FTLDriveComponent> ent, ref PowerChangedEvent args)
    {
        if (args.Powered)
            return;

        ent.Comp.NextPowerCheckTime = _timing.CurTime + ent.Comp.PowerGracePeriod;
    }

    /// <summary>
    /// Tries to start the ftl drive fails if the state is incorrect or the drive is still cooling down, turns off the drive if it's already on.
    /// </summary>
    /// <param name="ent">FTL Drive entity</param>
    public void TryToggleFTLDrive(Entity<FTLDriveComponent> ent)
    {
        if (ent.Comp.State is not FTLDriveState.Idle && ent.Comp.State is not FTLDriveState.Engaged)
            return;

        if  (_timing.CurTime < ent.Comp.CoolDownFinishedTime)
            return;

        if (ent.Comp.State == FTLDriveState.Engaged)
        {
            ShutDownFTLDrive(ent, ent.Comp.CoolDownTime);
            return;
        }

        StartUpFTLDrive(ent);
    }

    private void StartUpFTLDrive(Entity<FTLDriveComponent> ent)
    {
        if (ent.Comp.SoundEntity is not null)
            _audio.Stop(ent.Comp.SoundEntity);

        ent.Comp.SoundEntity = _audio.PlayPredicted(ent.Comp.SpoolUpSound, ent.Owner, ent.Owner)?.Entity;
        ent.Comp.State = FTLDriveState.Charging;

        ent.Comp.StartUpFinishTime = _timing.CurTime + ent.Comp.StartUpTime;

        _appearance.SetData(ent.Owner, FTLDriveVisuals.Active, true);

        _powerState.SetWorkingState(ent.Owner, true);

        EnsureComp<ActiveFTLDriveComponent>(ent.Owner);
        Dirty(ent);
    }

    /// <summary>
    /// Shuts down the FTL drive
    /// </summary>
    /// <param name="ent">The drive entity</param>
    /// <param name="coolDownTime">How long should the cooldown be</param>
    public void ShutDownFTLDrive(Entity<FTLDriveComponent> ent, TimeSpan coolDownTime)
    {
        _powerState.SetWorkingState(ent.Owner, false);

        if (ent.Comp.SoundEntity is not null)
            _audio.Stop(ent.Comp.SoundEntity);

        if (TryComp<AmbientSoundComponent>(ent.Owner, out var ambient))
        {
            ambient.Sound = ent.Comp.LoopSound;
            Dirty(ent.Owner, ambient);
        }

        ent.Comp.SoundEntity = _audio.PlayPredicted(ent.Comp.SpoolDownSound, ent.Owner, ent.Owner)?.Entity;

        ent.Comp.State = FTLDriveState.Idle;

        ent.Comp.CoolDownFinishedTime = _timing.CurTime + coolDownTime;

        _appearance.SetData(ent.Owner, FTLDriveVisuals.Active, false);

        if (ent.Comp.FTLComponents is not null)
            EntityManager.RemoveComponents(ent.Owner, ent.Comp.FTLComponents);

        if (ent.Comp.EngagedComponents is not null)
            EntityManager.RemoveComponents(ent.Owner, ent.Comp.EngagedComponents);

        var shuttleUid = Transform(ent.Owner).GridUid;

        if (shuttleUid is not null)
        {
            RemComp<ShuttleFTLDriveComponent>(shuttleUid.Value);
        }

        ent.Comp.EngagedBreakdownTime = _timing.CurTime;
        RemComp<ActiveFTLDriveComponent>(ent.Owner);
        Dirty(ent);
    }

    /// <summary>
    /// Called when the FTL drive finishes charging, switches the state and adds <see cref="FTLDriveComponent.EngagedComponents"/>
    /// </summary>
    /// <param name="ent">Drive entity</param>
    public void FinishChargingFTLDrive(Entity<FTLDriveComponent> ent)
    {
        ent.Comp.State = FTLDriveState.Engaged;

        if (ent.Comp.EngagedComponents is not null)
            EntityManager.AddComponents(ent.Owner, ent.Comp.EngagedComponents);

        if (TryComp<AmbientSoundComponent>(ent.Owner, out var ambient))
        {
            ambient.Sound = ent.Comp.EngagedLoopSound;
            Dirty(ent.Owner, ambient);
        }

        ent.Comp.EngagedBreakdownTime = _timing.CurTime + ent.Comp.StableEngagedTime;
        Dirty(ent);
    }

    /// <summary>
    /// Shuts down the FTL drive with <see cref="FTLDriveComponent.CoolDownTimeBreakDown"/> cooldown time and causes an explosion.
    /// </summary>
    /// <param name="ent">Drive entity</param>
    public void BreakDownFTLDrive(Entity<FTLDriveComponent> ent)
    {
        if (ent.Comp.State is not FTLDriveState.Engaged)
            return;

        ShutDownFTLDrive(ent, ent.Comp.CoolDownTimeBreakDown);
        _explosion.QueueExplosion(ent.Owner, ent.Comp.ExplosionType, ent.Comp.TotalIntensity, ent.Comp.IntensitySlope, ent.Comp.MaxTileBreak, canCreateVacuum:true);
    }

    /// <summary>
    /// Returns the current stats of an FTL drive, returns new FTLDriveStatsData if the entity has no  FTLDriveComponent.
    /// </summary>
    /// <param name="uid">The entity to check</param>
    /// <returns></returns>
    public FTLDriveStatsData GetDriveStats(EntityUid uid)
    {
        var data = new FTLDriveStatsData();

        if (!TryComp<FTLDriveComponent>(uid, out var drive))
            return data;

        data.State = drive.State;
        data.CoolDown = drive.CoolDownTime;
        data.Range = _modSystem.GetNumberModified(drive.Range, uid, FTLRangeAspect);
        data.StableTime = drive.StableEngagedTime;
        data.StartUp = drive.StartUpTime;
        data.CoolDownFinishedTime = drive.CoolDownFinishedTime - _timing.CurTime;
        data.CoolDownFailureTime = drive.EngagedBreakdownTime - _timing.CurTime;

        if (TryComp<PowerStateComponent>(uid, out var power))
        {
            if (power.IsWorking)
                data.PowerDraw = power.WorkingPowerDraw;
            else
                data.PowerDraw = power.IdlePowerDraw;
        }

        return data;
    }

    /// <summary>
    /// Returns the current status of a FTL drive, this is used for the UI
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public FTLDriveData GetDriveStatus(EntityUid uid)
    {
        var data = new FTLDriveData();

        if (!TryComp<FTLDriveComponent>(uid, out var drive))
            return data;

        data.State = drive.State;
        data.CoolDownFinishedTime = drive.CoolDownFinishedTime - _timing.CurTime;
        data.CoolDownFailureTime = drive.EngagedBreakdownTime - _timing.CurTime;

        // I can't find how to get the power it's actually using in shared soo I guess I'll just use this
        if (TryComp<PowerStateComponent>(uid, out var power))
        {
            if (power.IsWorking)
                data.PowerDraw = power.WorkingPowerDraw;
            else
                data.PowerDraw = power.IdlePowerDraw;
        }

        return data;
    }

    public Entity<FTLDriveComponent>? GetFTLDriveEnt(EntityUid? entity)
    {
        if (entity is null)
            return null;

        if (!TryComp<FTLDriveComponent>(entity, out var drive))
            return null;

        return (entity.Value, drive);
    }

}
