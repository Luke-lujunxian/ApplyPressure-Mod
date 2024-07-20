using HarmonyLib;
using RimWorld;
using Verse;

namespace Applypressure
{
    public class Applypressure:Mod
    {
        public Applypressure(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("lke.applypressure");
            harmony.PatchAll();
        }
    }

    [DefOf]
    public class ApplypressureDefOf
    {
        public static HediffDef ApplyingPressure;

        static ApplypressureDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ApplypressureDefOf));
        }
    }
}