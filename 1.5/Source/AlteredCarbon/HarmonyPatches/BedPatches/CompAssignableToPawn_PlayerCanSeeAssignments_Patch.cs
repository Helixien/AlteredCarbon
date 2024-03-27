using HarmonyLib;
using RimWorld;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(CompAssignableToPawn), "PlayerCanSeeAssignments", MethodType.Getter)]
    public static class CompAssignableToPawn_PlayerCanSeeAssignments_Patch
    {
        public static void Postfix(CompAssignableToPawn __instance, ref bool __result)
        {
            if (__instance.parent is Building_SleeveCasket)
            {
                __result = false;
            }
        }
    }
}

