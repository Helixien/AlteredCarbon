﻿using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(FactionGiftUtility), "GetBaseGoodwillChange")]
    public static class FactionGiftUtility_GetBaseGoodwillChange_Patch
    {
        public static void Postfix(ref float __result, Thing anyThing, int count, float singlePrice, Faction theirFaction)
        {
            if (anyThing is CorticalStack corticalStack && corticalStack.PersonaData.ContainsInnerPersona)
            {
                __result = 0;
            }
        }
    }
}
