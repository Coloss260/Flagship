using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;

namespace Content.FlagShip.Client.Hands.UI
{
    public sealed class HandPlaceholderStatus : Control
    {
        public HandPlaceholderStatus()
        {
            RobustXamlLoader.Load(this);
        }
    }
}
