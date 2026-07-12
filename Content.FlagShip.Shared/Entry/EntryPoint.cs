using Robust.Shared.ContentPack;

namespace Content.FlagShip.Shared.Entry;

public sealed class EntryPoint : GameShared
{
    public override void Init()
    {
        base.Init();

        Dependencies.BuildGraph();
        Dependencies.InjectDependencies(this);
    }
}
