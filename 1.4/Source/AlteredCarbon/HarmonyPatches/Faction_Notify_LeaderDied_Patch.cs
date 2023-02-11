using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Faction), "Notify_LeaderDied")]
    public static class Faction_Notify_LeaderDied_Patch
    {
        public static Pawn disableKillEffect;
        public static bool Prefix(Faction __instance)
        {
            if (__instance.leader == disableKillEffect)
            {
                disableKillEffect = null;
                return false;
            }
            return true;
        }
    }
}

