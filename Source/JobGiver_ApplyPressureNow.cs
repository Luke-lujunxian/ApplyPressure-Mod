using RimWorld;
using Verse;
using Verse.AI;

namespace Applypressure
{
    public class JobGiver_ApplyPressureNow : ThinkNode_JobGiver
    {

        protected override Job TryGiveJob(Pawn pawn)
        {
#if DEBUG
            Log.Message("Trying JobGiver_ApplyPressureNow");
#endif
            if (!HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn))
            {
#if DEBUG

                Log.Message("Not ShouldSeekMedicalRestUrgent");
#endif

                return null;
            }
            CanCrawlAlternativeActionComp comp = pawn.GetComp<CanCrawlAlternativeActionComp>();
            {
#if DEBUG
                Log.Message($"crawlAlternativeAction = {comp.crawlAlternativeAction}");
#endif
                if (comp != null && comp.crawlAlternativeAction == CrawlAlternativeAction.ApplyPressureNow)
                {
                    comp.ApplyingPressure = true;
                    //Same as JobGiver_IdleForever for job system
                    Job job = JobMaker.MakeJob(JobDefOf.Wait_Downed);
                    if (pawn.Deathresting)
                    {
                        job.forceSleep = true;
                    }
                    else
                    {
                        job.expiryInterval = 2500;
                    }
#if DEBUG
                    Log.Message($"JobGiver_ApplyPressureNow for {pawn}, ApplyingPressure is {comp.ApplyingPressure}");
#endif

                    return job;
                }
                return null;
            }
        }
    }

    public class JobGiver_ApplyPressureSafe : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn))
            {
                return null;
            }

            CanCrawlAlternativeActionComp comp = pawn.GetComp<CanCrawlAlternativeActionComp>();
            if (comp != null && comp.crawlAlternativeAction == CrawlAlternativeAction.ApplyPressureSafe)
            {
                comp.ApplyingPressure = true;

                //Same as JobGiver_IdleForever for job system
                Job job = JobMaker.MakeJob(JobDefOf.Wait_Downed);
                if (pawn.Deathresting)
                {
                    job.forceSleep = true;
                }
                else
                {
                    job.expiryInterval = 2500;
                }

                return job;
            }
            return null;

        }
    }
}

