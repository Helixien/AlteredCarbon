using HarmonyLib;
using RimWorld;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Faction), "Notify_LeaderDied")]
    public static class Faction_Notify_LeaderDied_Patch
    {
        public static bool disableKilledEffect = false;
        public static bool Prefix()
        {
            if (disableKilledEffect)
            {
                disableKilledEffect = false;
                return false;
            }
            return true;
        }
    }
}

