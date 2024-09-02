using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(HediffComp_DissolveGearOnDeath), "Notify_PawnDied")]
    public static class HediffComp_DissolveGearOnDeath_Notify_PawnDied_Patch
    {
        public static bool Prefix(HediffComp_DissolveGearOnDeath __instance)
        {
            if (Pawn_HealthTracker_NotifyPlayerOfKilled_Patch.pawnToSkip != null && __instance.Pawn == Pawn_HealthTracker_NotifyPlayerOfKilled_Patch.pawnToSkip
                || Recipe_InstallNeuralStack.pawnToInstallStack != null && __instance.Pawn == Recipe_InstallNeuralStack.pawnToInstallStack)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(HediffComp_DissolveGearOnDeath), "Notify_PawnKilled")]
    public static class HediffComp_DissolveGearOnDeath_Notify_PawnKilled_Patch
    {
        public static bool Prefix(HediffComp_DissolveGearOnDeath __instance)
        {
            if (Pawn_HealthTracker_NotifyPlayerOfKilled_Patch.pawnToSkip != null && __instance.Pawn == Pawn_HealthTracker_NotifyPlayerOfKilled_Patch.pawnToSkip
                || Recipe_InstallNeuralStack.pawnToInstallStack != null && __instance.Pawn == Recipe_InstallNeuralStack.pawnToInstallStack)
            {
                return false;
            }
            return true;
        }
    }
}

