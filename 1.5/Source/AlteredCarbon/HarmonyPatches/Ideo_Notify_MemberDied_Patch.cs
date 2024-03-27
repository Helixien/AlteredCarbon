using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Ideo), "Notify_MemberDied")]
    public static class Ideo_Notify_MemberDied_Patch
    {
        public static Pawn disableKillEffect;
        public static bool Prefix(Pawn member)
        {
            if (disableKillEffect == member)
            {
                disableKillEffect = null;
                return false;
            }
            return true;
        }
    }
}

