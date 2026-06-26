using Content.FlagShip.Server.Tools.Component;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Tools;
using Content.Shared.Tools.Components;

namespace Content.FlagShip.Server.Tools;

public sealed partial class DisableToolUseSystem : EntitySystem
{
    [Dependency] private SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DisableToolUseComponent, ToolUseAttemptEvent>(OnToolUseAttempt);
    }

    private void OnToolUseAttempt(EntityUid uid, DisableToolUseComponent component, ToolUseAttemptEvent args)
    {
        if (!TryComp(args.User, out HandsComponent? handsComponent))
            return;

        if (!_hands.TryGetActiveItem((args.User, handsComponent), out var heldEntity))
            return;

        if (!TryComp(heldEntity, out ToolComponent? tool))
            return;

        foreach (var quality in tool.Qualities)
        {
            if (Disabled(component, quality))
            {
                args.Cancel();
                break;
            }
        }
    }

    private bool Disabled(DisableToolUseComponent component, string quality)
    {
        switch (quality)
        {
            case "Anchoring":
                return component.Anchoring;
            case "Prying":
                return component.Prying;
            case "Screwing":
                return component.Screwing;
            case "Cutting":
                return component.Cutting;
            case "Welding":
                return component.Welding;
            case "Pulsing":
                return component.Pulsing;
            case "Slicing":
                return component.Slicing;
            case "Sawing":
                return component.Sawing;
            case "Honking":
                return component.Honking;
            case "Rolling":
                return component.Rolling;
            case "Brushing":
                return component.Brushing;
            case "Shearing":
                return component.Shearing;
            default:
                return false;
        }
    }
}
