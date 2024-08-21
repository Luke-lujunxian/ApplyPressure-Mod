using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Applypressure
{
    public class CanCrawlAlternativeActionComp : ThingComp
    {
        public CrawlAlternativeAction crawlAlternativeAction = Applypressure.settings.defaultAction;
        public bool ApplyingPressure;
        public Hediff mostSevere;
        private Hediff_ApplyingPressure currentHediff = null;


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Pawn pawn = parent as Pawn;
            if (!pawn.IsPlayerControlled)
            {
                yield break;
            }

            switch (crawlAlternativeAction)
            {
                case CrawlAlternativeAction.ApplyPressureNow:
                    yield return new Command_Action
                    {
                        defaultLabel = "ApplyPressureNow".Translate(),
                        defaultDesc = "ApplyPressureNowDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/APNow", false),
                        action = delegate
                        {
                            crawlAlternativeAction = CrawlAlternativeAction.ApplyPressureSafe;
                            if (pawn.Downed)
                            {
                                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                            }
                        }
                    };
                    break;
                case CrawlAlternativeAction.ApplyPressureSafe:
                    yield return new Command_Action
                    {
                        defaultLabel = "ApplyPressureSafe".Translate(),
                        defaultDesc = "ApplyPressureSafeDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/APSafe", false),
                        action = delegate
                        {
                            crawlAlternativeAction = CrawlAlternativeAction.ApplyPressureNone;
                            if (pawn.Downed)
                            {
                                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                            }
                        }
                    };
                    break;
                case CrawlAlternativeAction.ApplyPressureNone:
                    yield return new Command_Action
                    {
                        defaultLabel = "ApplyPressureNone".Translate(),
                        defaultDesc = "ApplyPressureNoneDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/APNone", false),
                        action = delegate
                        {
                            crawlAlternativeAction = CrawlAlternativeAction.ApplyPressureNow;
                            if (pawn.Downed)
                            {
                                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                            }
                        }
                    };
                    break;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            Pawn pawn = parent as Pawn;
            if (!pawn.IsPlayerControlled) {
                return;
            }
            if (pawn.health.Downed && ApplyingPressure)
            {

                if (pawn != null && !pawn.Dead)
                {
                    if (mostSevere == null)
                    {
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
                                    mostSevere = hediff;
                                }
                            }
                        }
                    }

                    if (currentHediff == null)
                    {
                        currentHediff = (Hediff_ApplyingPressure)pawn.health.hediffSet.GetFirstHediffOfDef(ApplypressureDefOf.ApplyingPressure);

                    }

                    if (currentHediff == null)
                    {
                        //Hediff hediff = new Hediff_ApplyingPressure(mostSevere);
#if DEBUG
                        Log.Message($"{mostSevere} on {mostSevere.Part}");
#endif
                        Hediff hediff;
                        hediff = HediffMaker.MakeHediff(ApplypressureDefOf.ApplyingPressure, pawn, null);
                        currentHediff = (Hediff_ApplyingPressure)hediff;
                        pawn.health.AddHediff(hediff);
#if DEBUG
                        Log.Message($"Adding Hediff");
#endif

                    }
                    currentHediff.targetHediff = mostSevere;

                }
            }
            else
            {
                currentHediff = null;
                ApplyingPressure = false;
                Hediff oldhediff = currentHediff ?? pawn.health.hediffSet.GetFirstHediffOfDef(ApplypressureDefOf.ApplyingPressure);
                if (oldhediff != null)
                {
                    pawn.health.RemoveHediff(oldhediff);
#if DEBUG
                    Log.Message($"Removing Hediff");
#endif
                }
            }
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            Pawn pawn = parent as Pawn;
            if (!pawn.IsColonistPlayerControlled)
            {
                ApplyingPressure = false;
                return;
            }

#if DEBUG
            Log.Message($"Damaged {pawn}");
#endif
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
                        maxBleeding = hediff;
                    }
                }
            }

            if (maxBleeding == null)
            {
#if DEBUG
                Log.Message($"Damage is done, but no BleedRate>0");
                return;
#endif
            }
#if DEBUG
            Log.Message($"ApplyingPressure is: {ApplyingPressure} ");
#endif
            mostSevere = maxBleeding;
            if (pawn != null && !pawn.Dead)
            {
                if (mostSevere == null || mostSevere != maxBleeding)//if the most severe bleeding has changed
                {
                    
                    if (ApplyingPressure)
                    {
                        Hediff oldhediff = pawn.health.hediffSet.GetFirstHediffOfDef(ApplypressureDefOf.ApplyingPressure);
                        if (oldhediff == null)
                        {

                            Hediff hediff = HediffMaker.MakeHediff(ApplypressureDefOf.ApplyingPressure, pawn, null);
                            ((Hediff_ApplyingPressure)hediff).targetHediff = mostSevere;
                            pawn.health.AddHediff(hediff);

                        }
                    }

                }
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref crawlAlternativeAction, "crawlAlternativeAction", Applypressure.settings.defaultAction);
            Scribe_Values.Look(ref ApplyingPressure, "ApplyingPressure", false);
            Scribe_References.Look(ref mostSevere, "mostSevere");
        }
    }


    public enum CrawlAlternativeAction
    {
        ApplyPressureNone = 0,
        ApplyPressureNow,
        ApplyPressureSafe,
    }


}
