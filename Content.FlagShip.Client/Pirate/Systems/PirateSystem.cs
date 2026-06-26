using Content.FlagShip.Shared.Pirate;
using Robust.Client.GameObjects;

namespace Content.FlagShip.Client.Pirate.Systems;

public sealed partial class PirateSystem : SharedPirateSystem
{
    [Dependency] private AnimationPlayerSystem _player = default!;

    public override void Initialize()
    {
        base.Initialize();
    }
}
