using Content.FlagShip.Client.Atmos.UI;
using Content.FlagShip.Shared.Atmos.Components;

namespace Content.FlagShip.Client.Atmos.BUI;

public sealed class GasDepositScannerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private GasDepositScannerWindow? _window;

    public GasDepositScannerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = new GasDepositScannerWindow();
        _window.OnClose += OnClose;
        _window.OpenCenteredLeft();
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (_window == null)
            return;
        if (message is not GasDepositScannerComponent.GasDepositScannerUserMessage cast)
            return;
        _window.Populate(cast);
    }

    /// <summary>
    /// Closes UI and tells the server to disable the analyzer
    /// </summary>
    private void OnClose()
    {
        SendMessage(new GasDepositScannerComponent.GasDepositScannerDisableMessage());
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _window?.Dispose();
    }
}
