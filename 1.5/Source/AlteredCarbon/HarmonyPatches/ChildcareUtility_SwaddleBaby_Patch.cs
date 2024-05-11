using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(ChildcareUtility), "SwaddleBaby")]
    public static class ChildcareUtility_SwaddleBaby_Patch
    {
        public static void Postfix(ref bool __result, Pawn baby)
        {
            if (__result && baby.ParentHolder is Building_SleeveGestator)
            {
                __result = false;
            }
        }
    }
}

