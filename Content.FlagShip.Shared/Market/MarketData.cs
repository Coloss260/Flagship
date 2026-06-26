using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Market;

[Virtual, NetSerializable, Serializable]
public class MarketData
{
    [ViewVariables]
    public EntProtoId Prototype { get; set; }

    [ViewVariables]
    public ProtoId<StackPrototype>? StackPrototype { get; set; }

    [ViewVariables]
    public int Quantity { get; set; }

    [ViewVariables]
    public double Price { get; set; }

    public MarketData(EntProtoId prototype, ProtoId<StackPrototype>? stackPrototype, int quantity, double price)
    {
        Prototype = prototype;
        StackPrototype = stackPrototype;
        Quantity = quantity;
        Price = price;
    }
}
