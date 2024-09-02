using HarmonyLib;
using System.Reflection;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch]
    public static class GeneRipper_Building_GeneRipper_KillOccupant_Patch
    {
        public static MethodInfo targetMethod;
        public static bool Prepare()
        {
            targetMethod = AccessTools.Method("GeneRipper.Building_GeneRipper:KillOccupant");
            return targetMethod != null;
        }
        public static MethodBase TargetMethod() => targetMethod;
        public static void Postfix(Building __instance, Pawn occupant)
        {
            if (occupant.MapHeld is null && occupant.HasNeuralStack(out var stack))
            {
                stack.SpawnStack(mapToSpawn: __instance.Map);
            }
        }
    }
}
