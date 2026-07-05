using Content.FlagShip.Shared.ModularShields.Components;
using Content.Shared.Examine;
using Content.Shared.Storage.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Server.ModularShields;

public sealed partial class ModularShieldSystem
{
    public void InitializeFluxStorage()
    {
        SubscribeLocalEvent<ModularShieldFluxStorageComponent, ExaminedEvent>(OnFluxStorageExamined);
    }

    private void OnFluxStorageExamined(EntityUid uid, ModularShieldFluxStorageComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!TryGetModularShieldNodeGroup(uid, out var nodeGroup))
        {
            args.PushMarkup(Loc.GetString("modular-shield-examine-disconnected"));
        }

        args.PushMarkup(Loc.GetString("modular-shield-flux-storage-examine", ("flux", (int)Math.Round(component.FluxStored)), ("fluxmax", component.FluxCapacity)));
    }
    private void UpdateFluxStorageDisplay(Entity<ModularShieldFluxStorageComponent> ent, AppearanceComponent? appComp = null, StorageFillVisualizerComponent? visualizerComponent = null)
    {
        if (!Resolve(ent.Owner, ref appComp, ref visualizerComponent))
            return;

        byte fillLevel = (byte)Math.Ceiling(ent.Comp.FluxStored / ent.Comp.FluxCapacity * visualizerComponent.MaxFillLevels);
        _sharedAppearance.SetData(ent.Owner, StorageFillVisuals.FillLevel, fillLevel, appComp);
    }
}
