using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(HediffSet), "GetHediffsVerbs")]
    public static class HediffSet_GetHediffsVerbs_Patch
    {
        public static void Postfix(List<Verb> __result)
        {
            __result.RemoveAll(x => x.HediffCompSource is HediffComp_MeleeWeapon);
        }
    }
}

