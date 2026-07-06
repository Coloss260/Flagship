using Content.FlagShip.Shared.Ranks;
using Content.Server.Players.PlayTimeTracking;
using Content.Server.Preferences.Managers;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Ranks;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Server.Ranks;

public sealed partial class RankSystem : SharedRankSystem
{
    [Dependency] private PlayTimeTrackingManager _tracking = default!;
    [Dependency] private IServerPreferencesManager _preferences = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RankComponent, TransformSpeakerNameEvent>(OnSpeakerNameTransform);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnSpeakerNameTransform(Entity<RankComponent> ent, ref TransformSpeakerNameEvent args)
    {
        var name = GetSpeakerRankName(ent);
        if (name != null)
            args.VoiceName = name;
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        if (ev.JobId == null || !_prototypes.TryIndex<JobPrototype>(ev.JobId, out var job) || job.Ranks == null)
            return;

        if (!_tracking.TryGetTrackerTimes(ev.Player, out var playTimes))
        {
            Log.Error($"Playtimes were not ready for {ev.Player} during rank assignment.");
            playTimes = new Dictionary<string, TimeSpan>();
        }

        var profile = _preferences.GetPreferences(ev.Player.UserId).SelectedCharacter as HumanoidCharacterProfile ?? ev.Profile;
        profile.RankPreferences.TryGetValue(ev.JobId, out var preferredRankId);

        if (preferredRankId != null &&
            job.Ranks.TryGetValue(preferredRankId.Value, out var preferredRequirements) &&
            RequirementsMet(preferredRequirements, ev.Profile, playTimes) &&
            _prototypes.TryIndex(preferredRankId.Value, out RankPrototype? preferred))
        {
            SetRank(ev.Mob, preferred);
            return;
        }

        foreach (var (rankProtoId, requirements) in job.Ranks)
        {
            if (!RequirementsMet(requirements, ev.Profile, playTimes) ||
                !_prototypes.TryIndex(rankProtoId, out RankPrototype? rank))
            {
                continue;
            }

            SetRank(ev.Mob, rank);
            return;
        }
    }

    private bool RequirementsMet(
        HashSet<JobRequirement>? requirements,
        HumanoidCharacterProfile profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes)
    {
        if (requirements == null)
            return true;

        foreach (var requirement in requirements)
        {
            if (!requirement.Check(EntityManager, _prototypes, profile, playTimes, out _))
                return false;
        }

        return true;
    }
}
