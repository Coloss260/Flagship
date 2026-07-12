using Robust.Shared.ContentPack;

namespace Content.FlagShip.Client.Entry;

public sealed class EntryPoint : GameClient
{
    public override void Init()
    {
        Dependencies.BuildGraph();
        Dependencies.InjectDependencies(this);
    }
}
