using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Claws;

public abstract partial class SharedClawsSystem
{
    private void InitializeNailClippers()
    {
        SubscribeLocalEvent<Components.NailClipperComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<Components.NailClipperComponent, AfterInteractEvent>(OnTargetUse);
        SubscribeLocalEvent<Components.ClawsComponent, NailClipperDoAfterEvent>(ClipNails);
    }

    private void OnUse(Entity<Components.NailClipperComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = TryClipNails(ent, ent, args.User);
    }

    private void OnTargetUse(Entity<Components.NailClipperComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || !args.Target.HasValue)
            return;

        args.Handled = TryClipNails(ent, ent, args.User, args.Target.Value);
    }

    /// <summary>
    /// Used to handle nail clipping action, either from user itself or on the target.
    /// Reduces stage based on <see cref="Components.NailClipperComponent"/>
    /// </summary>
    /// <param name="component"></param>
    /// <param name="nailClipper"></param>
    /// <param name="user"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool TryClipNails(Components.NailClipperComponent component, EntityUid nailClipper, EntityUid user, EntityUid? target = null)
    {
        target ??= user;

        if (!TryComp<Components.ClawsComponent>(target, out var claws))
        {
            return false;
        }

        if (TryGetStage(claws, out var stage) && !stage.CanBeCut)
        {
            _popup.PopupClient(Robust.Shared.Localization.Loc.GetString("has-no-claws-popup"), Transform(user).Coordinates, user);
            return false;
        }

        if ((TryGetStageNumber(claws) <= 0) & (component.DeclawChance < 1))
        {
            _popup.PopupClient(Robust.Shared.Localization.Loc.GetString("claws-too-short-popup"), Transform(user).Coordinates, user);
            return false;
        }

        _popup.PopupClient(Robust.Shared.Localization.Loc.GetString("claws-clipping-doafter"), Transform(user).Coordinates, user);

        var doAfterArgs = new DoAfterArgs(Robust.Shared.GameObjects.EntityManager,
            user,
            component.ClipDoAfter,
            new NailClipperDoAfterEvent(),
            target,
            target,
            nailClipper)
        {
            NeedHand = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
        };

        return _doafter.TryStartDoAfter(doAfterArgs);
    }

    public void ClipNails(EntityUid uid, Components.ClawsComponent component, NailClipperDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (args.Used == null || !TryComp<Components.NailClipperComponent>(args.Used, out var nailClipper))
            return;

        if (nailClipper.DeclawChance > _random.NextFloat())
        {
            Declaw(uid, component);
            return;
        }

        component.ClawStage = component.Claws.GetValueOrDefault(
            Math.Clamp(TryGetStageNumber(component) - nailClipper.StageReduction, 0, int.MaxValue));
        _popup.PopupClient(Robust.Shared.Localization.Loc.GetString("claws-clipping-success"), Transform(uid).Coordinates, uid);

        // Reset current growth progress
        component.GrowTimer = TimeSpan.Zero;

        UpdateClaws(uid, component);
        Dirty(uid, component);
    }
}

[Serializable, NetSerializable]
public sealed partial class NailClipperDoAfterEvent : SimpleDoAfterEvent;
