using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Alert_ColonistLeftUnburied), "IsCorpseOfColonist")]
    public static class Alert_ColonistLeftUnburied_IsCorpseOfColonist_Patch
    {
        public static void Postfix(ref bool __result, Corpse corpse)
        {
            if (__result && corpse.InnerPawn.IsEmptySleeve())
            {
                __result = false;
            }
        }
    }
}

