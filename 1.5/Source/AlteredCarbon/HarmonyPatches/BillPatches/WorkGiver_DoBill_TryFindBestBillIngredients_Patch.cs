using HarmonyLib;
using RimWorld;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
    public static class WorkGiver_DoBill_TryFindBestBillIngredients_Patch
    {
        public static Bill curBill;
        public static void Prefix(Bill bill)
        {
            curBill = bill;
        }
        public static void Postfix()
        {
            curBill = null;
        }
    }
}

