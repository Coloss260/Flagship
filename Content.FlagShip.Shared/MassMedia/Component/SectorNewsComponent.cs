using Content.Shared.MassMedia.Systems;

namespace Content.FlagShip.Shared.MassMedia.Component;

[RegisterComponent]
public sealed partial class SectorNewsComponent : Robust.Shared.GameObjects.Component
{
    public static List<NewsArticle> Articles = new();
}
