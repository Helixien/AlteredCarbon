using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_Relations")]
    public class PawnDiedOrDownedThoughtsUtility_AppendThoughts_Relations_Patch
    {
        public static Pawn disableKillEffect;
        public static bool Prefix(Pawn victim)
        {
            if (disableKillEffect == victim)
            {
                disableKillEffect = null;
                return false;
            }
            return true;
        }
    }
}

