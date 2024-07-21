using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Applypressure
{
    public class CanCrawlAlternativeActionComp : ThingComp
    {
        public CrawlAlternativeAction crawlAlternativeAction;
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
                case CrawlAlternativeAction.pressureNow:
                    yield return new Command_Action
                    {
                        defaultLabel = "ApplyPressureNow".Translate(),
                        defaultDesc = "ApplyPressureNowDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/ApplyPressureNow", false),
                        action = delegate
                        {
                            crawlAlternativeAction = CrawlAlternativeAction.pressureSafe;
                        }
                    };
                    break;
                case CrawlAlternativeAction.pressureSafe:
                    yield return new Command_Action
                    {
                        defaultLabel = "ApplyPressureSafe".Translate(),
                        defaultDesc = "ApplyPressureSafeDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/ApplyPressureSafe", false),
                        action = delegate
                        {
                            crawlAlternativeAction = CrawlAlternativeAction.none;
                        }
                    };
                    break;
                case CrawlAlternativeAction.none:
                    yield return new Command_Action
                    {
                        defaultLabel = "ApplyPressureNone".Translate(),
                        defaultDesc = "ApplyPressureNoneDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/ApplyPressureNone", false),
                        action = delegate
                        {
                            crawlAlternativeAction = CrawlAlternativeAction.pressureNow;
                        }
                    };
                    break;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            Pawn pawn = parent as Pawn;
            if (pawn.health.Downed && ApplyingPressure && pawn.IsPlayerControlled)
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

            if (pawn != null && !pawn.Dead)
            {
                if (mostSevere == null || mostSevere != maxBleeding)//if the most severe bleeding has changed
                {
                    mostSevere = maxBleeding;
                    if (ApplyingPressure)
                    {
                        Hediff oldhediff = pawn.health.hediffSet.GetFirstHediffOfDef(ApplypressureDefOf.ApplyingPressure);
                        if (oldhediff == null)
                        {

                            Hediff hediff = HediffMaker.MakeHediff(ApplypressureDefOf.ApplyingPressure, pawn, mostSevere.Part);
                            ((Hediff_ApplyingPressure)hediff).targetHediff = mostSevere;
                            pawn.health.AddHediff(hediff);

                        }
                    }

                }
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref crawlAlternativeAction, "crawlAlternativeAction", CrawlAlternativeAction.none);
            Scribe_Values.Look(ref ApplyingPressure, "ApplyingPressure", false);
            Scribe_References.Look(ref mostSevere, "mostSevere");
        }
    }


    public enum CrawlAlternativeAction
    {
        none = 0,
        pressureNow,
        pressureSafe,
    }


}
