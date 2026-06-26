using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.Market.Components;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class MarketItemSpawnerComponent : Component
{

    [NonSerialized]
    public List<MarketData> ItemsToSpawn;
}
