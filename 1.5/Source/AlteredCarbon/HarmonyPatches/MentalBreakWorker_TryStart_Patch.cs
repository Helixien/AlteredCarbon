using HarmonyLib;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(MentalBreakWorker), "TryStart")]
    public class MentalBreakWorker_TryStart_Patch
    {
        public static bool Prefix(Pawn pawn)
        {
            if (pawn.IsEmptySleeve())
            {
                return false;
            }
            return true;
        }
    }
}

