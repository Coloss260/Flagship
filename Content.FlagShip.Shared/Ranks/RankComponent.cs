using Content.Shared.Roles.Ranks;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;
using Robust.Shared.ViewVariables;

namespace Content.FlagShip.Shared.Ranks;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedRankSystem))]
public sealed partial class RankComponent : Component
{
    [AutoNetworkedField]
    internal ProtoId<RankPrototype> RankId;

    [ViewVariables(VVAccess.ReadWrite)]
    public RankPrototype? Rank
    {
        get
        {
            if (string.IsNullOrWhiteSpace(RankId.Id))
                return null;

            var prototypes = IoCManager.Resolve<IPrototypeManager>();
            return prototypes.TryIndex(RankId, out RankPrototype? rank) ? rank : null;
        }
        set
        {
            if (value == null)
                RankId = default;
            else
                RankId = value;

            IoCManager.Resolve<IEntityManager>().Dirty(this);
        }
    }
}
