using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Roles.Ranks;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Shared.Ranks;

public abstract partial class SharedRankSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RankComponent, ExaminedEvent>(OnRankExamined);
    }

    private void OnRankExamined(Entity<RankComponent> ent, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(SharedRankSystem), 1))
        {
            var rank = GetRankString(ent.Owner, hasPaygrade: true);
            if (rank == null)
                return;

            args.PushMarkup(Loc.GetString("rank-component-examine", ("user", ent.Owner), ("rank", rank)));
        }
    }

    public void SetRank(EntityUid uid, RankPrototype from)
    {
        SetRank(uid, from.ID);
    }

    public void SetRank(EntityUid uid, ProtoId<RankPrototype> from)
    {
        var comp = EnsureComp<RankComponent>(uid);
        comp.RankId = from;
        Dirty(uid, comp);
    }

    public RankPrototype? GetRank(EntityUid uid)
    {
        return TryComp<RankComponent>(uid, out var component) ? GetRank(component) : null;
    }

    public RankPrototype? GetRank(RankComponent component)
    {
        if (string.IsNullOrWhiteSpace(component.RankId.Id))
            return null;

        return _prototypes.TryIndex(component.RankId, out RankPrototype? rankProto) ? rankProto : null;
    }

    public string? GetRankString(EntityUid uid, bool isShort = false, bool hasPaygrade = false)
    {
        var rank = GetRank(uid);
        if (rank == null)
            return null;

        if (isShort)
        {
            if (rank.FemalePrefix == null || rank.MalePrefix == null)
                return rank.Prefix;

            if (!TryComp<HumanoidProfileComponent>(uid, out var appearance))
                return rank.Prefix;

            return appearance.Gender switch
            {
                Gender.Female => rank.FemalePrefix,
                Gender.Male => rank.MalePrefix,
                _ => rank.Prefix,
            };
        }

        if (hasPaygrade && rank.Paygrade != null)
            return $"({Loc.GetString(rank.Paygrade)}) {rank.Name}";

        return rank.Name;
    }

    public string? GetSpeakerRankName(EntityUid uid)
    {
        var rank = GetRankString(uid, isShort: true);
        return rank == null ? null : $"{rank} {Name(uid)}";
    }

    public string? GetSpeakerFullRankName(EntityUid uid)
    {
        var rank = GetRankString(uid);
        return rank == null ? null : $"{rank} {Name(uid)}";
    }
}
