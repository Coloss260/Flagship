using Content.FlagShip.Client.Pirate.UI;
using Content.FlagShip.Shared.Pirate.BUI;
using Content.FlagShip.Shared.Pirate.Components;
using Content.FlagShip.Shared.Pirate.Events;

namespace Content.FlagShip.Client.Pirate.BUI;

public sealed class PirateBountyRedemptionConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private PirateBountyRedemptionMenu? _menu;
    [ViewVariables]
    private EntityUid uid;

    public PirateBountyRedemptionConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        if (EntMan.TryGetComponent<PirateBountyRedemptionConsoleComponent>(owner, out var console))
            uid = owner;
    }

    protected override void Open()
    {
        base.Open();

        _menu = new PirateBountyRedemptionMenu();
        _menu.SellRequested += OnSell;
        _menu.OnClose += Close;

        _menu.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _menu?.Dispose();
        }
    }

    private void OnSell()
    {
        SendMessage(new PirateBountyRedemptionMessage());
    }

    // TODO: remove this, nothing to update
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not PirateBountyRedemptionConsoleInterfaceState palletState)
            return;
    }
}
