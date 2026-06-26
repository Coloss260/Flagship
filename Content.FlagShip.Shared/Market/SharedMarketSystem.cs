using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Market;

public abstract class SharedMarketSystem : EntitySystem
{
};

[NetSerializable, Serializable]
public enum MarketConsoleUiKey : byte
{
    Default
}
