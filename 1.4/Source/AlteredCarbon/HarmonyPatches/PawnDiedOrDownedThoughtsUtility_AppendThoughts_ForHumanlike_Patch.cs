using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike")]
    public class PawnDiedOrDownedThoughtsUtility_AppendThoughts_ForHumanlike_Patch
    {
        public static bool disableKilledEffect = false;
        public static bool Prefix(Pawn victim)
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

