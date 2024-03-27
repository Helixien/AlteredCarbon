using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn), "AdoptableBy")]
    public static class Pawn_AdoptableBy_Patch
    {
        public static bool Prefix(Pawn __instance)
        {
            if (__instance.IsEmptySleeve())
            {
                return false;
            }
            return true;
        }
    }
}

