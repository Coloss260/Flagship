using Content.Shared.Examine;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.FlagShip.Server.Weapons.Melee;

public sealed partial class MeleeChargeSystem : EntitySystem
{
    [Dependency] private ItemToggleSystem _toggle = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent, ItemToggledEvent>(OnToggle);
        SubscribeLocalEvent<FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent, ItemToggleActivateAttemptEvent>(OnToggleAttempt);
    }

    private void OnExamined(Entity<FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent> ent, ref ExaminedEvent args)
    {
        if (InCooldown(ent))
            args.PushMarkup(Loc.GetString("melee-charge-weakened", ("cooldown", CooldownToSeconds(ent))));
    }

    private void OnMeleeHit(Entity<FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent> ent, ref MeleeHitEvent args)
    {
        if (InCooldown(ent))
        {
            args.BonusDamage += ent.Comp.CooldownDamagePenalty;
            return;
        }

        if (!IsActive(ent))
            return;

        TryDeactivate(ent, ent.Comp);
    }

    private void OnToggleAttempt(Entity<FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent> ent, ref ItemToggleActivateAttemptEvent args)
    {
        if (!InCooldown(ent))
            return;

        _popup.PopupEntity(Loc.GetString("melee-charge-remaining-cooldown", ("remainingCooldown", CooldownToSeconds(ent))),
            args.User ?? ent);

        args.Cancelled = true;
    }

    private void OnToggle(Entity<FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent> ent, ref ItemToggledEvent args)
    {
        if (args.Activated)
            Activate(ent, ent.Comp);
        else
            TryDeactivate(ent, ent.Comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FlagShip.Server.Weapons.Melee.ActiveWeaponMeleeChargeComponent, FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent>();

        while (query.MoveNext(out var uid, out _, out var charge))
        {
            if (!ActiveTimePassed(charge))
                continue;

            TryDeactivate(uid, charge);
        }
    }

    private void TryDeactivate(EntityUid uid, FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent charge)
    {
        if(!_toggle.TryDeactivate(uid))
            return;

        if (HasComp<FlagShip.Server.Weapons.Melee.ActiveWeaponMeleeChargeComponent>(uid))
            RemComp<FlagShip.Server.Weapons.Melee.ActiveWeaponMeleeChargeComponent>(uid);

        charge.CooldownEndTime = TimeSpan.FromSeconds(charge.Cooldown) + _timing.CurTime;
    }

    private void Activate(EntityUid uid, FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent charge)
    {
        AddComp<FlagShip.Server.Weapons.Melee.ActiveWeaponMeleeChargeComponent>(uid);
        charge.ActiveEndTime = TimeSpan.FromSeconds(charge.ActiveTime) + _timing.CurTime;
    }

    private bool InCooldown(FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent charge)
    {
        return charge.CooldownEndTime > _timing.CurTime;
    }

    private bool IsActive(EntityUid uid)
    {
        return HasComp<FlagShip.Server.Weapons.Melee.ActiveWeaponMeleeChargeComponent>(uid);
    }

    private bool ActiveTimePassed(FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent charge)
    {
        return charge.ActiveEndTime < _timing.CurTime;
    }

    private int CooldownToSeconds(Entity<FlagShip.Server.Weapons.Melee.WeaponMeleeChargeComponent> ent)
    {
        return (int) double.Ceiling((ent.Comp.CooldownEndTime - _timing.CurTime).TotalSeconds);
    }
}
