using Content.FlagShip.Client.Bank.UI;
using Content.FlagShip.Shared.Bank.BUI;
using Content.FlagShip.Shared.Bank.Events;

namespace Content.FlagShip.Client.Bank.BUI;

public sealed class StationAdminConsoleBoundUserInterface : BoundUserInterface
{
    private StationAdminConsoleMenu? _menu;

    public StationAdminConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) {}

    protected override void Open()
    {
        base.Open();

        _menu = new StationAdminConsoleMenu();
        _menu.WithdrawRequest += OnWithdraw;
        _menu.DepositRequest += OnDeposit;
        _menu.OnClose += Close;
        _menu.PopulateReasons();
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

    private void OnWithdraw()
    {
        if (_menu?.WithdrawalAmount is not int amount)
            return;

        SendMessage(new StationBankWithdrawMessage(amount, _menu.WithdrawalReason, _menu.WithdrawalDescription));
    }

    private void OnDeposit()
    {
        if (_menu?.DepositAmount is not int amount)
            return;

        SendMessage(new StationBankDepositMessage(amount, _menu.DepositReason, _menu.DepositDescription));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not StationBankATMMenuInterfaceState bankState)
            return;

        _menu?.SetEnabled(bankState.Enabled);
        _menu?.SetBalance(bankState.Balance);
        _menu?.SetDeposit(bankState.Deposit);
    }
}
