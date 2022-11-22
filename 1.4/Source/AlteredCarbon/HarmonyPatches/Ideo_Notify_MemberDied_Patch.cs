using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Ideo), "Notify_MemberDied")]
    public static class Ideo_Notify_MemberDied_Patch
    {
        public static bool disableKilledEffect = false;
        public static bool Prefix()
        {
            Log.Message("disableKilledEffect: " + disableKilledEffect);
            if (disableKilledEffect)
            {
                disableKilledEffect = false;
                return false;
            }
            return true;
        }
    }
}

