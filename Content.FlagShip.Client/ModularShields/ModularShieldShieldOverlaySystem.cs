using Content.FlagShip.Client.ModularShields;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Client.ModularShields;

public sealed partial class ModularShieldShieldOverlaySystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlayManager = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IResourceCache _resourceCache = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlayManager.AddOverlay(new ModularShieldShieldOverlay(EntityManager, _prototypeManager, _resourceCache));
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlayManager.RemoveOverlay<ModularShieldShieldOverlay>();
    }
}
