using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Applypressure
{
    public class Hediff_ApplyingPressure : Hediff
    {
        private float bleedRate = 0f;
        public override float BleedRate => bleedRate;

        public Hediff targetHediff;

        public Hediff_ApplyingPressure() : base()
        {
            targetHediff = null;
        }

        public Hediff_ApplyingPressure(Hediff targetHediff) : base()
        {
            this.targetHediff = targetHediff;
        }

        public override void Tick()
        {
            base.Tick();

            if (pawn == null || pawn.Dead || pawn.IsWorldPawn())
            {
                return;
            }



            if (targetHediff == null)
            {
                Log.Warning("Apply pressure hediff is add with no targetHediff, calculating most severe bleeding");
                HediffSet hediffs = pawn.health.hediffSet;
                Hediff maxBleeding = null;
                foreach (Hediff hediff in hediffs.hediffs)
                {
                    if (hediff is Hediff_Injury && hediff.BleedRate > 0)
                    {
#if DEBUG
                        Log.Message($"hediff {hediff} has {hediff.BleedRate}");
#endif
                        if (maxBleeding == null || (maxBleeding.BleedRate < hediff.BleedRate))
                        {
                            targetHediff = hediff;
                        }
                    }
                }

                if (targetHediff == null)
                {
                    Log.Warning("Apply pressure hediff is add with no bleading, removing");
                    this.Severity = 0;
                }

            }

            if (targetHediff.IsTended())//Treated by a doctor
            {
                Severity = 0;
                return;
            }
            bleedRate = -targetHediff.BleedRate * 0.9f * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation);
            return;


        }

        public override string GetTooltip(Pawn pawn, bool showHediffsDebugInfo)
        {
            return $"{Label}\n"
                + "reduceBleedRate".Translate() + $": {-bleedRate * 100}%\n" +
                "Holding".Translate() + $": {targetHediff.Part.Label} {targetHediff.Label}\n\n" +
                $"{Description}" +
                (showHediffsDebugInfo?
                $"\n\n{bleedRate} = -{targetHediff.BleedRate} * 0.9f * {pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation)}" :"");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref targetHediff, "targetHediff");
        }
    }

}
