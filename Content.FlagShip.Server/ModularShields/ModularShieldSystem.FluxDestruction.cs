using Content.FlagShip.Shared.ModularShields.Components;
using Content.Shared.Examine;
using Content.Shared.NodeContainer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.FlagShip.Server.ModularShields;

public sealed partial class ModularShieldSystem
{
    public void InitializeFluxDestruction()
    {
        SubscribeLocalEvent<ModularShieldFluxDestructionComponent, ExaminedEvent>(OnFluxDestructorExamined);
    }



    /// <summary>
    /// Performs update operations for a single flux destruction component
    /// </summary>
    /// <param name="fluxDestructorUid"></param>
    /// <param name="destructor"></param>
    /// <param name="nodeContainer"></param>
    private void UpdateFluxDestructor(float frameTime, ModularShieldCoreComponent shieldCore, ModularShieldNodeGroup nodeGroup, EntityUid fluxDestructorUid, ModularShieldFluxDestructionComponent destructor, NodeContainerComponent nodeContainer)
    {
        if (!_power.IsPowered(fluxDestructorUid))
        {
            return;
        }

        float fluxToDestroy = destructor.FluxDestructionRateMaximum * frameTime;
        if (shieldCore?.ShieldProjected != null)
        {
            fluxToDestroy = fluxToDestroy * destructor.FluxDestructionWhileShieldProjectedRateMultiplier;
        }

        // Amount of extra flux destruction that wasn't needed to empty the system of flux.
        // To be used to 'scale' the cost of the flux destruction by comparing against the maximum flux destruction rate.
        float excessDestruction = DestroyFlux(nodeGroup, fluxToDestroy);

        float fluxForCosting = destructor.FluxDestructionRateMaximum - excessDestruction;
        if (shieldCore?.ShieldProjected != null)
        {
            fluxForCosting = fluxForCosting * destructor.FluxDestructionWhileShieldProjectedCostMultiplier;
        }
    }



    private void OnFluxDestructorExamined(EntityUid uid, ModularShieldFluxDestructionComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;


        if (!TryGetModularShieldNodeGroup(uid, out var nodeGroup))
        {
            args.PushMarkup(Loc.GetString("modular-shield-examine-disconnected"));
        }

        args.PushMarkup(Loc.GetString("modular-shield-flux-destructor-examine", ("fluxdestruct", component.FluxDestructionRateMaximum)));
    }
}
