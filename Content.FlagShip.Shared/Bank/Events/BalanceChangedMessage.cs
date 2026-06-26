using Robust.Shared.Player;

namespace Content.FlagShip.Shared.Bank.Events;
public sealed record BalanceChangedEvent(ICommonSession Session, int Amount);
