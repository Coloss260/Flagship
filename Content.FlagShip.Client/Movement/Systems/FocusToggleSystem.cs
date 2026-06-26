using Content.Client.Movement.Components;
using Content.FlagShip.Shared.Movement.Systems;

namespace Content.FlagShip.Client.Movement.Systems;

public sealed class FocusToggleSystem : SharedFocusToggleSystem
{
    protected override bool HasCompEyeCursorOffset(EntityUid uid)
    {
        return HasComp<EyeCursorOffsetComponent>(uid);
    }

    protected override void AddCompEyeCursorOffset(EntityUid uid)
    {
        EnsureComp<EyeCursorOffsetComponent>(uid);
    }

    protected override void RemCompEyeCursorOffset(EntityUid uid)
    {
        RemComp<EyeCursorOffsetComponent>(uid);
    }
}

