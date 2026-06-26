using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.Chemistry.Events;


/// <summary>
///     Sends a message to change the associated injector component's ReagentWhitelist to null,
///     allowing all Reagents to be drawn by that injector
/// </summary>
[Serializable, NetSerializable]
public sealed class ReagentWhitelistResetMessage : BoundUserInterfaceMessage
{
    public ReagentWhitelistResetMessage()
    {

    }
}
