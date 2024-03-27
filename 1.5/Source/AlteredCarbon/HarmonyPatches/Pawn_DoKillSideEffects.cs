using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn), "DoKillSideEffects")]
    public static class Pawn_DoKillSideEffects
    {
        public static Pawn disableKillEffect;
        public static bool Prefix(Pawn __instance)
        {
            if (disableKillEffect == __instance)
            {
                disableKillEffect = null;
                return false;
            }
            return true;
        }
    }
}

