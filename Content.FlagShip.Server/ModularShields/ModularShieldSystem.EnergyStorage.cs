using Content.FlagShip.Shared.ModularShields.Components;
using Content.Shared.Examine;
using Content.Shared.Storage.Components;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Content.FlagShip.Server.ModularShields;

public sealed partial class ModularShieldSystem
{
    public void InitializeEnergyStorage()
    {
        SubscribeLocalEvent<ModularShieldEnergyStorageComponent, ExaminedEvent>(OnEnergyStorageExamined);
    }

    private void OnEnergyStorageExamined(EntityUid uid, ModularShieldEnergyStorageComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!TryGetModularShieldNodeGroup(uid, out var nodeGroup))
        {
            args.PushMarkup(Loc.GetString("modular-shield-examine-disconnected"));
        }

        args.PushMarkup(Loc.GetString("modular-shield-energy-storage-examine", ("energy", (int)Math.Round(component.EnergyStored)), ("energymax", component.EnergyCapacity)));
    }

    private void UpdateEnergyStorageDisplay(Entity<ModularShieldEnergyStorageComponent> ent, AppearanceComponent? appComp = null, StorageFillVisualizerComponent? visualizerComponent = null)
    {
        if (!Resolve(ent.Owner, ref appComp, ref visualizerComponent))
            return;

        var fillLevel = (int)Math.Ceiling(ent.Comp.EnergyStored / ent.Comp.EnergyCapacity * visualizerComponent.MaxFillLevels);
        _sharedAppearance.SetData(ent.Owner, StorageFillVisuals.FillLevel, fillLevel, appComp);
    }
}
