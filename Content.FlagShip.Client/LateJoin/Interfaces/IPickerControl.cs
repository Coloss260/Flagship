using Robust.Client.UserInterface.Controls;

namespace Content.FlagShip.Client.LateJoin.Interfaces;

public abstract class PickerControl: PanelContainer
{
    public abstract void UpdateUi(IReadOnlyDictionary<NetEntity, StationJobInformation> obj);
}
