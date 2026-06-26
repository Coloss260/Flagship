using Content.Shared.Containers.ItemSlots;
using JetBrains.Annotations;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Shipyard;

// Note: when adding a new ui key, don't forget to modify the dictionary in SharedShipyardSystem
[NetSerializable, Serializable]
public enum ShipyardConsoleUiKey : byte
{
    Shipyard,
    Security,
    Syndicate,
    BlackMarket,
    Expedition,
    Scrap,
    Sr,
    Medical,
    // Mono start
    Ussp,
    SHM,
    DrakeIndustries,
    // Add ships to this key if they are only available from mothership consoles. Shipyards using it are inherently empty and are populated using the ShipyardListingComponent.
    Custom
}

public abstract partial class SharedShipyardSystem : EntitySystem
{
    [Dependency] private ItemSlotsSystem _itemSlotsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Components.ShipyardConsoleComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<Components.ShipyardConsoleComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<Components.ShipyardConsoleComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<Components.ShipyardConsoleComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, Components.ShipyardConsoleComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not ShipyardConsoleComponentState state) return;

    }

    private void OnGetState(EntityUid uid, Components.ShipyardConsoleComponent component, ref ComponentGetState args)
    {

    }

    private void OnComponentInit(EntityUid uid, Components.ShipyardConsoleComponent component, ComponentInit args)
    {
        _itemSlotsSystem.AddItemSlot(uid, Components.ShipyardConsoleComponent.TargetIdCardSlotId, component.TargetIdSlot);
    }

    private void OnComponentRemove(EntityUid uid, Components.ShipyardConsoleComponent component, ComponentRemove args)
    {
        _itemSlotsSystem.RemoveItemSlot(uid, component.TargetIdSlot);
    }

    [Serializable, NetSerializable]
    private sealed class ShipyardConsoleComponentState : ComponentState
    {
        public List<string> AccessLevels;

        public ShipyardConsoleComponentState(List<string> accessLevels)
        {
            AccessLevels = accessLevels;
        }
    }

}
