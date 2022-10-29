using HarmonyLib;
using RimWorld;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(StatsRecord), "Notify_ColonistKilled")]
    public static class StatsRecord_Notify_ColonistKilled_Patch
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

