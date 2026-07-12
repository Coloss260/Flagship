using Robust.Shared.ContentPack;

namespace Content.FlagShip.Server.Entry;

public sealed class EntryPoint : GameServer
{
    public override void Init()
    {
        base.Init();

        Dependencies.BuildGraph();
        Dependencies.InjectDependencies(this);
    }
}
