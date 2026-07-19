using System.Collections.Generic;
using System.Threading;
using Content.IntegrationTests.Fixtures;
using Content.Server.Database;
using Content.Server.Preferences.Managers;
using Content.Shared.Body;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Ranks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Robust.Shared.Configuration;
using Robust.Shared.Log;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Tests.Preferences
{
    [TestFixture]
    public sealed class ServerDbSqliteTests : GameTest
    {
        [TestPrototypes]
        private const string Prototypes = @"
- type: dataset
  id: sqlite_test_names_first_male
  values:
  - Aaden

- type: dataset
  id: sqlite_test_names_first_female
  values:
  - Aaliyah

- type: dataset
  id: sqlite_test_names_last
  values:
  - Ackerley

- type: playTimeTracker
  id: TestRankedPlayTimeTracker

- type: rank
  id: TestRankCadet
  name: Test Cadet
  prefix: CDT

- type: rank
  id: TestRankOfficer
  name: Test Officer
  prefix: OFF

- type: job
  id: TestRankedJob
  name: test-ranked-job
  playTimeTracker: TestRankedPlayTimeTracker
  setRankPreference: true
  ranks:
    TestRankOfficer: []
    TestRankCadet: []";

        private static HumanoidCharacterProfile CharlieCharlieson()
        {
            return new()
            {
                Name = "Charlie Charlieson",
                FlavorText = "The biggest boy around.",
                Species = "Human",
                Age = 21,
                Appearance = new(
                    Color.Azure,
                    Color.Beige,
                    new ())
            };
        }

        private static ServerDbSqlite GetDb(RobustIntegrationTest.ServerIntegrationInstance server)
        {
            var cfg = server.ResolveDependency<IConfigurationManager>();
            var serialization = server.ResolveDependency<ISerializationManager>();
            var opsLog = server.ResolveDependency<ILogManager>().GetSawmill("db.ops");
            var builder = new DbContextOptionsBuilder<SqliteServerDbContext>();
            var conn = new SqliteConnection("Data Source=:memory:");
            conn.Open();
            builder.UseSqlite(conn);
            return new ServerDbSqlite(() => builder.Options, true, cfg, true, opsLog, serialization);
        }

        [Test]
        public async Task TestUserDoesNotExist()
        {
            var pair = Pair;
            var db = GetDb(pair.Server);
            // Database should be empty so a new GUID should do it.
            Assert.That(await db.GetPlayerPreferencesAsync(NewUserId()), Is.Null);
        }

        [Test]
        public async Task TestAppearanceValidationAndSave()
        {
            var pair = await PoolManager.GetServerClient();
            var db = GetDb(pair.Server);
            var username = new NetUserId(new Guid("640bd619-fc8d-4fe2-bf3c-4a5fb17d6ddd"));

            var profile = CharlieCharlieson();
            profile.Appearance.Markings["Head"] = new Dictionary<HumanoidVisualLayers, List<Marking>>
            {
                [HumanoidVisualLayers.Hair] = [],
                [HumanoidVisualLayers.FacialHair] = [],
            };
            profile.Appearance.Markings["OrganFake"] = new Dictionary<HumanoidVisualLayers, List<Marking>>();

            await pair.Server.WaitAssertion(() =>
            {
                var updated = HumanoidCharacterAppearance.EnsureValid(profile.Appearance, profile.Species, profile.Sex);
                Assert.That(updated.Markings["Head"], Is.Empty);
                Assert.That(updated.Markings.ContainsKey("OrganFake"), Is.False);
                profile.Appearance = updated;
            });

            Assert.DoesNotThrowAsync(async () => await db.InitPrefsAsync(username, profile));

            var preferences = (ServerPreferencesManager)pair.Server.ResolveDependency<IServerPreferencesManager>();
            var prefs = await db.GetPlayerPreferencesAsync(username);
            var fetchedProfile = preferences.ConvertProfiles(prefs!.Profiles.Find(p => p.Slot == 0));
            Assert.That(fetchedProfile.MemberwiseEquals(profile));

            await pair.CleanReturnAsync();
        }

        [Test]
        public async Task TestInitPrefs()
        {
            var pair = Pair;
            var db = GetDb(pair.Server);
            var preferences = (ServerPreferencesManager)pair.Server.ResolveDependency<IServerPreferencesManager>();
            var username = new NetUserId(new Guid("640bd619-fc8d-4fe2-bf3c-4a5fb17d6ddd"));
            const int slot = 0;
            var originalProfile = CharlieCharlieson();
            await db.InitPrefsAsync(username, originalProfile);
            var prefs = await db.GetPlayerPreferencesAsync(username);
            var profile = preferences.ConvertProfiles(prefs!.Profiles.Find(p => p.Slot == slot));
            Assert.That(profile.MemberwiseEquals(originalProfile));
        }

        [Test]
        public void TestWithRankPreferenceAddReplaceRemove()
        {
            var jobId = new ProtoId<JobPrototype>("TestRankedJob");
            var cadet = new ProtoId<RankPrototype>("TestRankCadet");
            var officer = new ProtoId<RankPrototype>("TestRankOfficer");

            var original = CharlieCharlieson();
            var added = original.WithRankPreference(jobId, cadet);
            var replaced = added.WithRankPreference(jobId, officer);
            var removed = replaced.WithRankPreference(jobId, null);

            Assert.That(original.RankPreferences.ContainsKey(jobId), Is.False);
            Assert.That(added.RankPreferences[jobId], Is.EqualTo(cadet));
            Assert.That(replaced.RankPreferences[jobId], Is.EqualTo(officer));
            Assert.That(added.MemberwiseEquals(replaced), Is.False);
            Assert.That(added.GetHashCode(), Is.Not.EqualTo(replaced.GetHashCode()));
            Assert.That(removed.RankPreferences.ContainsKey(jobId), Is.False);
        }

        [Test]
        public async Task TestRankPreferenceRoundTrip()
        {
            var pair = Pair;
            var db = GetDb(pair.Server);
            var preferences = (ServerPreferencesManager)pair.Server.ResolveDependency<IServerPreferencesManager>();
            var username = NewUserId();
            var jobId = new ProtoId<JobPrototype>("TestRankedJob");
            var rankId = new ProtoId<RankPrototype>("TestRankOfficer");

            var originalProfile = CharlieCharlieson().WithRankPreference(jobId, rankId);

            await db.InitPrefsAsync(username, originalProfile);
            var prefs = await db.GetPlayerPreferencesAsync(username);
            var profile = preferences.ConvertProfiles(prefs!.Profiles.Find(p => p.Slot == 0));

            Assert.That(profile.MemberwiseEquals(originalProfile), Is.True);
            Assert.That(profile.RankPreferences.TryGetValue(jobId, out var loadedRank), Is.True);
            Assert.That(loadedRank, Is.EqualTo(rankId));
        }

        [Test]
        public async Task TestDeleteCharacter()
        {
            var pair = Pair;
            var server = pair.Server;
            var db = GetDb(server);
            var username = new NetUserId(new Guid("640bd619-fc8d-4fe2-bf3c-4a5fb17d6ddd"));
            await db.InitPrefsAsync(username, new HumanoidCharacterProfile());
            await db.SaveCharacterSlotAsync(username, CharlieCharlieson(), 1);
            await db.SaveSelectedCharacterIndexAsync(username, 1);
            await db.SaveCharacterSlotAsync(username, null, 1);
            var prefs = await db.GetPlayerPreferencesAsync(username);
            Assert.That(prefs!.Profiles, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task TestNoPendingDatabaseChanges()
        {
            var pair = Pair;
            var server = pair.Server;
            var db = GetDb(server);
            Assert.That(async () => await db.HasPendingModelChanges(), Is.False,
                "The database has pending model changes. Add a new migration to apply them. See https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations");
        }

        private static NetUserId NewUserId()
        {
            return new(Guid.NewGuid());
        }

        private const string InvalidSpecies = "WingusDingus";

        private static bool[] _trueFalse = [true, false];

        [Test]
        [TestCaseSource(nameof(_trueFalse))]
        public async Task InvalidSpeciesConversion(bool legacy)
        {
            var pair = Pair;
            var server = pair.Server;
            var db = GetDb(pair.Server);
            var preferences = (ServerPreferencesManager)pair.Server.ResolveDependency<IServerPreferencesManager>();

            var proto = server.ResolveDependency<IPrototypeManager>();
            Assert.That(!proto.HasIndex<SpeciesPrototype>(InvalidSpecies), "You should not have added a species called WingusDingus, but change it in this test to something else I guess");

            var bogus = new HumanoidCharacterProfile()
            {
                Species = InvalidSpecies,
            };

            var username = new NetUserId(new Guid("640bd619-fc8d-4fe2-bf3c-4a5fb17d6ddd"));
            await db.InitPrefsAsync(username, new HumanoidCharacterProfile());
            await db.SaveCharacterSlotAsync(username, bogus, 0);
            await db.SaveSelectedCharacterIndexAsync(username, 0);

            if (legacy)
                await db.MakeCharacterSlotLegacyAsync(username, 0);

            var prefs = await db.GetPlayerPreferencesAsync(username, CancellationToken.None);

            Assert.That(prefs, Is.Not.Null);
            await server.WaitAssertion(() =>
            {
                var converted = preferences.ConvertPreferences(prefs);

                Assert.That(converted.Characters, Has.Count.EqualTo(1));
                Assert.That(converted.Characters[0].Species, Is.Not.EqualTo(InvalidSpecies));
                Assert.That(converted.Characters[0].Species, Is.EqualTo(HumanoidCharacterProfile.DefaultSpecies));
            });
        }
    }
}
