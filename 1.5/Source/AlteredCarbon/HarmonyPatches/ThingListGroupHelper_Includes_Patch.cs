using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(ThingListGroupHelper), "Includes")]
    public static class ThingListGroupHelper_Includes_Patch
    {
        public static void Postfix(ThingDef def, ThingRequestGroup group, ref bool __result)
        {
            if (__result is false && group == ThingRequestGroup.PotentialBillGiver && def == AC_DefOf.AC_NeuralEditor)
            {
                __result = true;
            }
        }
    }
}