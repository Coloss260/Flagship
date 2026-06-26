using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Shared.Claws
{
    public sealed partial class ClawsGrowth : EntityEffect
    {
        /// <summary>
        /// Bonus Claws growth in seconds. X seconds of additional growth per second.
        /// </summary>
        [DataField]
        public double Growth;

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-claws-growth",
                ("chance", Probability),
                ("amount", Growth));

        public override void Effect(EntityEffectBaseArgs args)
        {
            if (args.EntityManager.TryGetComponent<Components.ClawsComponent>(args.TargetEntity, out var claws))
            {
                var sys = args.EntityManager.EntitySysManager.GetEntitySystem<FlagShip.Shared.Claws.SharedClawsSystem>();
                var growth = Growth;

                if (args is EntityEffectReagentArgs reagentArgs)
                {
                    growth *= reagentArgs.Scale.Float();
                }

                sys.GrowClaws(TimeSpan.FromSeconds(growth), claws);
            }
        }
    }
}
