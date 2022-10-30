using HarmonyLib;
using RimWorld;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Ideo), "RecacheColonistBelieverCount")]
    public static class Ideo_RecacheColonistBelieverCount_Patch
    {
        public static bool includeStackPawns = false;
        public static void Prefix()
        {
            includeStackPawns = true;
        }
        public static void Postfix()
        {
            includeStackPawns = false;
        }
    }
}

