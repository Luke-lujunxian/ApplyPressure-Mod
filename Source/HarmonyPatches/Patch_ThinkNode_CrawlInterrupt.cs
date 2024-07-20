using HarmonyLib;
using Verse;
using RimWorld;
using Verse.AI;
namespace Applypressure.HarmonyPatches
{
    public class Patch_ThinkNode_CrawlInterrupt
    {
        [HarmonyPatch(typeof(ThinkNode_CrawlInterrupt),nameof(ThinkNode_CrawlInterrupt.TryIssueJobPackage))]
        public static void Postfix(Pawn __pawn, ref ThinkResult __result)
        {
#if DEBUG
            Log.Message($"{__pawn}, have job {__result.Job.def}");
#endif
            if (__result.Job.def != JobDefOf.Wait_Downed)
            {
                CanCrawlAlternativeActionComp comp = __pawn.GetComp<CanCrawlAlternativeActionComp>();
                if (comp != null)
                {
#if DEBUG
                    Log.Message($"Job.def is: {__result.Job.def} calceling" );
#endif
                    comp.ApplyingPressure = false;
                }
            }
        }
    }
}
