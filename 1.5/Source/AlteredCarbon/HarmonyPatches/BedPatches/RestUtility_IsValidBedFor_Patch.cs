using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(RestUtility), "IsValidBedFor")]
    public static class RestUtility_IsValidBedFor_Patch
    {
        private static bool Prefix(Thing bedThing, Pawn sleeper)
        {
            if (bedThing != null && sleeper != null && !sleeper.IsEmptySleeve() && bedThing is Building_SleeveCasket)
            {
                return false;
            }
            return true;
        }
    }
}

