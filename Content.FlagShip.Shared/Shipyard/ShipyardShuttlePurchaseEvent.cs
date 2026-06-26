namespace Content.FlagShip.Shared.Shipyard;

public sealed class ShipyardShuttlePurchaseEvent(EntityUid shuttle, EntityUid purchaser)
{
    public EntityUid Shuttle { get;  } = shuttle;
    public EntityUid Purchaser { get; } = purchaser;
}
