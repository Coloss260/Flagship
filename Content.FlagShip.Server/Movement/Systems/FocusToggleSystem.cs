using Content.FlagShip.Shared.Movement.Systems;
using Content.Server.Movement.Components;

namespace Content.FlagShip.Server.Movement.Systems;

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

