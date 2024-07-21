using HarmonyLib;
using Verse;
using RimWorld;
using Verse.AI;
namespace Applypressure.HarmonyPatches
{
    [HarmonyPatch]
    public class Patch_ThinkNode_CrawlInterrupt
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ThinkNode_CrawlInterrupt),nameof(ThinkNode_CrawlInterrupt.TryIssueJobPackage))]
        public static void Postfix(Pawn pawn, ref ThinkResult __result)
        {
#if DEBUG
            Log.Message($"{pawn}, have job {__result}");
#endif
            if (__result == ThinkResult.NoJob || __result.Job.def != JobDefOf.Wait_Downed)
            {
                CanCrawlAlternativeActionComp comp = pawn.GetComp<CanCrawlAlternativeActionComp>();
                if (comp != null)
                {
#if DEBUG
                    Log.Message($"Job.def is: {__result} calceling" );
#endif
                    comp.ApplyingPressure = false;
                }
            }
        }
    }
}
