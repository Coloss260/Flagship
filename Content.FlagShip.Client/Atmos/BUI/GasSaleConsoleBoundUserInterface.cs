using Content.FlagShip.Client.Atmos.UI;
using Content.FlagShip.Shared.Atmos.BUI;
using Content.FlagShip.Shared.Atmos.Events;
using Robust.Client.UserInterface;

namespace Content.FlagShip.Client.Atmos.BUI;

public sealed class GasSaleConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private GasSaleMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<GasSaleMenu>();
        _menu.RefreshRequested += OnRefresh;
        _menu.SellRequested += OnSell;
    }

    private void OnRefresh()
    {
        SendMessage(new GasSaleRefreshMessage());
    }

    private void OnSell()
    {
        SendMessage(new GasSaleSellMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not GasSaleConsoleBoundUserInterfaceState gasState)
            return;

        _menu?.SetEnabled(gasState.Enabled);
        _menu?.SetMixture(gasState.Mixture, gasState.Appraisal);
    }
}
