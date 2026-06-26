using Content.FlagShip.Shared.Chemistry.Events;
using Content.Shared.Chemistry.Reagent;
using JetBrains.Annotations;

namespace Content.FlagShip.Client.Chemistry.UI
{
    [UsedImplicitly]
    public sealed class ChangeReagentWhitelistBoundUserInterface : BoundUserInterface
    {
        private ChangeReagentWhitelistWindow? _window;
        public ChangeReagentWhitelistBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();
            _window = new ChangeReagentWhitelistWindow(this);

            _window.OnClose += Close;
            _window.OpenCentered();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            _window?.Dispose();
        }
        public void ChangeReagentWhitelist(ProtoId<ReagentPrototype> newReagentProto)
        {
            SendMessage(new ReagentWhitelistChangeMessage(newReagentProto));
        }

        public void ResetReagentWhitelist()
        {
            SendMessage(new ReagentWhitelistResetMessage());
        }
    }
}
