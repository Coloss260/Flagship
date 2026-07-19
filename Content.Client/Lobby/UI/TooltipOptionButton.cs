using System.Collections.Generic;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Lobby.UI;

public sealed class TooltipOptionButton : OptionButton
{
    private readonly List<Button> _itemButtons = new();

    public override void ButtonOverride(Button button)
    {
        _itemButtons.Add(button);
    }

    public void SetItemToolTip(int idx, string toolTip)
    {
        _itemButtons[idx].ToolTip = toolTip;
    }
}
