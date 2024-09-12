using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Building_CryptosleepCasket), "GetGizmos")]
    public static class Building_CryptosleepCasket_GetGizmos_Patch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building_CryptosleepCasket __instance)
        {
            foreach (var gizmo in __result)
            {
                yield return gizmo;
            }
            var pawn = __instance.innerContainer.OfType<Pawn>().FirstOrDefault();
            if (pawn != null && pawn.HasNeuralStack(out var neuralStack))
            {
                foreach (var g in neuralStack.GetNeedleCastingGizmos())
                {
                    yield return g;
                }
            }
        }
    }
}
