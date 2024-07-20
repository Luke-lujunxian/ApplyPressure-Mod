using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Applypressure
{
    public class Hediff_ApplyingPressure: Hediff
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



            if (targetHediff != null) {
                if (targetHediff.IsTended())
                {
                    Severity = 0;
                    return;
                }
                bleedRate = - targetHediff.BleedRate * 0.9f * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation);
                return;
            }

            float tempbr = 0f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if((hediffs[i].Part == Part || hediffs[i].Part.parent == Part) && hediffs[i] != this)
                {
                    tempbr += hediffs[i].BleedRate;
                }
            }
            bleedRate = -tempbr * 0.9f * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation); //resuce bleading
        }

        public override string GetTooltip(Pawn pawn, bool showHediffsDebugInfo)
        {
            return "reduceBleedRate".Translate() +$": {-bleedRate*100}%\n" + base.GetTooltip(pawn, showHediffsDebugInfo);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref targetHediff, "targetHediff");
        }
    }
    
}
