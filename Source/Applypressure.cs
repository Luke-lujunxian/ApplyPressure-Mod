using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Applypressure
{
    public class Applypressure:Mod
    {
        public Applypressure(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("lke.applypressure");
            harmony.PatchAll();
            settings = GetSettings<ApplypressureSettings>();

        }

        public static ApplypressureSettings settings;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label($"bleed rate reduced = - targetHediff.BleedRate * stopBleedFactor * max(pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation),manipulationCap)");
            listingStandard.Label($"stopBleedFactor");
            settings.stopBleedFactor = listingStandard.SliderLabeled($"{(int)(settings.stopBleedFactor * 100)}%", (int)(settings.stopBleedFactor*100), 0, 100,0.2f)/100;
            //settings.stopBleedFactor = listingStandard.Slider(settings.stopBleedFactor, 0.0f, 1f);
            listingStandard.Label($"manipulationCap");
            settings.manipulationCap = listingStandard.SliderLabeled($"{(int)(settings.manipulationCap * 100)}%", (int)(settings.manipulationCap*100), 0, 100,0.2f)/100;
            //settings.manipulationCap = listingStandard.Slider(settings.manipulationCap, 0.0f, 1f);
            listingStandard.Label("defaultAction".Translate());
            if (Widgets.ButtonText(new Rect(inRect.width / 2, listingStandard.CurHeight, inRect.width/2, 30), settings.defaultAction.ToString().Translate()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (CrawlAlternativeAction action in Enum.GetValues(typeof(CrawlAlternativeAction)))
                {
                    list.Add(new FloatMenuOption(action.ToString().Translate(), delegate
                    {
                        settings.defaultAction = action;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings.
        /// Using .Translate() is optional, but does allow for localisation.
        /// </summary>
        /// <returns>The (translated) mod name.</returns>
        public override string SettingsCategory()
        {
            return "Applypressure".Translate();
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

    public class ApplypressureSettings : ModSettings
    {
        /// <summary>
        /// The three settings our mod has.
        /// </summary>
        public float stopBleedFactor = 0.9f;
        public float manipulationCap = 0.0f;
        public CrawlAlternativeAction defaultAction;

        /// <summary>
        /// The part that writes our settings to file. Note that saving is by ref.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref stopBleedFactor, "stopBleedFactor", 0.9f);
            Scribe_Values.Look(ref manipulationCap, "manipulationCap", 0.0f);
            Scribe_Values.Look(ref defaultAction, "defaultAction", CrawlAlternativeAction.ApplyPressureNone);
            base.ExposeData();
        }
    }
}