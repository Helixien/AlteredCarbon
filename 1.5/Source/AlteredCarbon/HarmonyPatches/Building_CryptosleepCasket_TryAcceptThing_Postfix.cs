using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Building_CryptosleepCasket), "TryAcceptThing")]
    public static class Building_CryptosleepCasket_TryAcceptThing_Postfix
    {
        public static void Postfix(Building_CryptosleepCasket __instance, Thing thing, bool __result)
        {
            if (__result && thing is Pawn pawn && pawn.HasNeuralStack(out var neural))
            {
                var matrix = __instance.GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading
                    .OfType<Building_NeuralMatrix>().FirstOrDefault();
                if (matrix != null)
                {
                    neural.NeuralData.trackedToMatrix = matrix;
                }
            }
        }
    }
}

