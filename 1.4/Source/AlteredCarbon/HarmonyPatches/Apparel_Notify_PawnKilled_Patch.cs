﻿using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Apparel), "Notify_PawnKilled")]
    public static class Apparel_Notify_PawnKilled_Patch
    {
        public static void Postfix(Apparel __instance)
        {
            if (__instance.Wearer is not null)
            {
                if (__instance.Wearer.HasStackInsideOrOutside() || __instance.Wearer.IsEmptySleeve())
                {
                    if (ACUtils.generalSettings.sleeveDeathDoesNotCauseGearTainting)
                    {
                        __instance.wornByCorpseInt = false;
                    }
                }
            }

        }
    }
}
