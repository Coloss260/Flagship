using System.Linq;
using Content.FlagShip.Shared.ModSystem.Components;
using Content.FlagShip.Shared.ModSystem.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Shared.ModSystem.Systems;

public sealed partial class ModifierSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _protoMan = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<AddModifierOnInsertedComponent, EntGotInsertedIntoContainerMessage>(OnInsert);
        SubscribeLocalEvent<AddModifierOnInsertedComponent, EntGotRemovedFromContainerMessage>(OnRemove);
    }

    private void OnInsert(Entity<AddModifierOnInsertedComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        AddModifiers(ent.Owner, args.Container.Owner);
    }

    private void OnRemove(Entity<AddModifierOnInsertedComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        RemoveModifiers(ent.Owner, args.Container.Owner);
    }

    /// <summary>
    /// Adds modifiers from the source entity to the target entity, returns if the source doesn't have ModifierComponent or the target doesn't have ModifiableComponent.
    /// </summary>
    /// <param name="source">The source of the modifiers (should be the entity with ModifierComponent)</param>
    /// <param name="target">The target to add modifiers to (should be the entity with ModifiableComponent)</param>
    private void AddModifiers(EntityUid source, EntityUid target)
    {
        if (!TryComp<ModifierComponent>(source, out var modifiers) || !TryComp<ModifiableComponent>(target, out var targetMod))
            return;

        foreach (var mod in modifiers.Modifiers)
        {
            if (!_protoMan.TryIndex(mod.Id, out ModifierPrototype? modifierProto))
                return;

            if (targetMod.CurrentModifiers.Contains(modifierProto) || !IsAspectAllowed(target, modifierProto.Aspect))
                continue;

            targetMod.CurrentModifiers.Add(modifierProto);
        }
        Dirty(target, targetMod);
    }

    /// <summary>
    /// Same as <see cref="AddModifiers"/> but removes.
    /// </summary>
    /// <param name="source">The source of the modifiers (should be the entity with ModifierComponent)</param>
    /// <param name="target">The target to add modifiers to (should be the entity with ModifiableComponent)</param>
    private void RemoveModifiers(EntityUid source, EntityUid target)
    {
        if (!TryComp<ModifierComponent>(source, out var modifiers) || !TryComp<ModifiableComponent>(target, out var targetMod))
            return;

        foreach (var mod in modifiers.Modifiers)
        {
            if (!_protoMan.TryIndex(mod.Id, out ModifierPrototype? modifierProto))
                return;

            targetMod.CurrentModifiers.Remove(modifierProto);
        }
        Dirty(target, targetMod);
    }

    [PublicAPI]
    public float GetNumberModified(float value, EntityUid entity, ProtoId<ModAspectPrototype> aspect)
    {
        if (!TryComp<ModifiableComponent>(entity, out var modifiable))
            return value;

        var orderedMods = modifiable.CurrentModifiers
            .OrderBy(mod => mod.ModifierType == ModifierType.Addition)
            .ThenBy(mod => mod.ModifierType == ModifierType.Multiplication)
            .ThenBy(mod => mod.ModifierType == ModifierType.Set);

        foreach (var mod in orderedMods)
        {
            if (aspect != mod.Aspect)
                continue;

            switch (mod.ModifierType)
            {
                case ModifierType.Addition:
                    value += mod.Modifier;
                    continue;
                case ModifierType.Multiplication:
                    value *= mod.Modifier;
                    continue;
                case ModifierType.Set:
                    value = mod.Modifier;
                    continue;
            }
        }

        return value;
    }

    [PublicAPI]
    public bool IsAspectAllowed(EntityUid target, ProtoId<ModAspectPrototype> aspect)
    {
        if (!TryComp<ModifiableComponent>(target, out var modifiable))
            return false;

        if (!modifiable.AllowedAspects.Contains(aspect))
            return false;

        return true;
    }
}
