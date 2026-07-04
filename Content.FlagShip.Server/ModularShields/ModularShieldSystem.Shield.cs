using Content.FlagShip.Shared.ModularShields.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Content.Shared.Trigger.Components.Triggers;
using Content.Shared.Weapons.Hitscan.Components;
using Content.Shared.Weapons.Hitscan.Events;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Content.FlagShip.Server.ModularShields;

public partial class ModularShieldSystem
{
    private const string ModularShieldShieldPrototype = "ModularShieldShield";

    [Dependency] private SharedTransformSystem _transformSystem = default!;
    [Dependency] private FixtureSystem _fixtureSystem = default!;
    [Dependency] private PhysicsSystem _physicsSystem = default!;
    [Dependency] private PvsOverrideSystem _pvsSys = default!;



    public void InitializeShield()
    {
        SubscribeLocalEvent<ModularShieldShieldComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<ModularShieldShieldComponent, HitscanDamageAttemptEvent>(OnHitscanDamageAttemptEvent);
    }



    /// <summary>
    /// Produces a shield around a grid entity, if it doesn't already exist.
    /// </summary>
    /// <param name="entity">The entity being shielded.</param>
    /// <param name="source">A modular shield core providing the shield for the entity</param>
    /// <param name="mapGrid">The map grid component of the entity being shielded.</param>
    /// <returns>The shield entity.</returns>
    private EntityUid ShieldEntity(EntityUid entity, EntityUid? source = null, MapGridComponent? mapGrid = null)
    {
        if (TryComp<ModularShieldShieldedComponent>(entity, out var existingShielded))
            return EntityUid.Invalid;

        if (!Resolve(entity, ref mapGrid, false))
            return EntityUid.Invalid;

        var prototype = ModularShieldShieldPrototype;

        var shield = Spawn(prototype, Transform(entity).Coordinates);
        var shieldPhysics = EnsureComp<PhysicsComponent>(shield);
        var shieldComp = EnsureComp<ModularShieldShieldComponent>(shield);
        shieldComp.ShieldedEntity = entity;
        shieldComp.ModularShieldCoreSource = source;

        // Copy shield color from the generator to the shield visuals
        var shieldVisuals = EnsureComp<ModularShieldShieldVisualsComponent>(shield);
        if (source != null && TryComp<ModularShieldCoreComponent>(source.Value, out var shieldCore))
        {
            shieldVisuals.ShieldColor = shieldCore.ShieldColor;
            Dirty(shield, shieldVisuals);
        }

        var gridCenter = new EntityCoordinates(entity, mapGrid.LocalAABB.Center);
        _transformSystem.SetCoordinates(shield, gridCenter);
        _transformSystem.SetWorldRotation(shield, _transformSystem.GetWorldRotation(entity));

        // If you're wondering what the fuck is up with these fixtures:
        // Don't think it's entirely a bug, it's optimisations on fixture handling preventing the fixture from extending too far from the grid's limits.
        // It makes more sense if you put the shield generator on the small ship, the actual fixture you can hover over with "physics shapeinfo" in the console is very tight around the ship compared to the fixture displayed with "physics shapes".
        // Projectiles only hit the tight fixture.
        // Hitscans will hit the full size fixture only if they were fired from inside the full size fixture. Hitscans fired from outside the full size fixture will only hit the tight fixture.
        var chain = GenerateOvalFixture(shield, "shield", shieldPhysics, mapGrid, shieldVisuals.Padding);

        List<Vector2> roughPoly = new();

        var interval = chain.Count / PhysicsConstants.MaxPolygonVertices;

        int i = 0;

        while (i < PhysicsConstants.MaxPolygonVertices)
        {
            roughPoly.Add(chain.Vertices[i * interval]);
            i++;
        }

        var internalPoly = new PolygonShape();
        internalPoly.Set(roughPoly);

        _fixtureSystem.TryCreateFixture(shield, internalPoly, "internalShield",
            hard: true,
            collisionLayer: (int)(CollisionGroup.BulletImpassable | CollisionGroup.ModularShield),
            body: shieldPhysics);


        _physicsSystem.WakeBody(shield, body: shieldPhysics);
        _physicsSystem.SetSleepingAllowed(shield, shieldPhysics, false);

        _pvsSys.AddGlobalOverride(shield);

        var shieldedComp = EnsureComp<ModularShieldShieldedComponent>(entity);
        shieldedComp.Shield = shield;
        shieldedComp.Source = source;

        return shield;
    }

    private bool UnshieldEntity(EntityUid uid, ModularShieldShieldedComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        TryQueueDel(component.Shield);
        RemComp<ModularShieldShieldedComponent>(uid);
        return true;
    }



    private ChainShape GenerateOvalFixture(EntityUid uid, string name, PhysicsComponent physics, MapGridComponent mapGrid, float padding)
    {
        float radius;
        float scale;
        var scaleX = true;

        var height = mapGrid.LocalAABB.Height + padding;
        var width = mapGrid.LocalAABB.Width + padding;

        if (width > height)
        {
            radius = 0.5f * height;
            scale = width / height;
        }
        else
        {
            radius = 0.5f * width;
            scale = height / width;
            scaleX = false;
        }

        var chain = new ChainShape();

        chain.CreateLoop(Vector2.Zero, radius);

        for (int i = 0; i < chain.Vertices.Length; i++)
        {
            if (scaleX)
            {
                chain.Vertices[i].X *= scale;
            }
            else
            {
                chain.Vertices[i].Y *= scale;
            }
        }

        _fixtureSystem.TryCreateFixture(uid, chain, name,
            hard: false,
            // Do not try to make the outer shield block bullets/lasers. Because the whole thing is off the edge of the parent grid it's inconsistent as hell.
            collisionLayer: (int)CollisionGroup.None,
            body: physics);

        return chain;
    }


    private void OnPreventCollide(EntityUid uid, ModularShieldShieldComponent component, ref PreventCollideEvent args)
    {
        if (!_projectileQuery.TryGetComponent(args.OtherEntity, out var projectile) || projectile.ProjectileSpent)
        {
            args.Cancelled = true;
            return;
        }

        if (component.ModularShieldCoreSource is { } source)
        {
            var ev = new ModularShieldAbsorbedProjectileEvent(args.OtherEntity, projectile);
            RaiseLocalEvent(source, ref ev);
        }
    }


    private void OnHitscanDamageAttemptEvent(Entity<ModularShieldShieldComponent> ent, ref HitscanDamageAttemptEvent args)
    {
        if (ent.Comp.ModularShieldCoreSource != null)
        {
            var ev = new ModularShieldAbsorbedDamageEvent((float)args.DamageToTake.GetTotal());
            RaiseLocalEvent((EntityUid)ent.Comp.ModularShieldCoreSource, ref ev);

            args.Cancelled = true;
        }
    }
}
