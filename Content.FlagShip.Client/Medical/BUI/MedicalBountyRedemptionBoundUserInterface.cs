using Content.FlagShip.Client.Medical.UI;
using Content.FlagShip.Shared.Medical;
using JetBrains.Annotations;

namespace Content.FlagShip.Client.Medical.BUI;

[UsedImplicitly]
public sealed class MedicalBountyRedemptionBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private MedicalBountyRedemptionMenu? _menu;

    public MedicalBountyRedemptionBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = new();

        _menu.OnClose += Close;

        _menu.SellRequested += SendBountyMessage;

        _menu.OpenCentered();
    }

    private void SendBountyMessage()
    {
        SendMessage(new RedeemMedicalBountyMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState message)
    {
        base.UpdateState(message);

        if (message is not MedicalBountyRedemptionUIState state)
            return;

        _menu?.UpdateState(state);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _menu?.Dispose();
    }
}
