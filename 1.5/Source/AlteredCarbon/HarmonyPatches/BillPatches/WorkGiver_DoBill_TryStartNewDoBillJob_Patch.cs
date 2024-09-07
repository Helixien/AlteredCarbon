using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryStartNewDoBillJob")]
    public static class WorkGiver_DoBill_TryStartNewDoBillJob_Patch
    {
        public static bool Prefix(Pawn pawn, Bill bill)
        {
            if (bill is Bill_OperateOnStack operateOnStack)
            {

            }
            return true;
        }
    }
}