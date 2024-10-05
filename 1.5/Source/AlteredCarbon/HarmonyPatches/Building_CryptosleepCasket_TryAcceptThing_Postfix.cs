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
            if (__result && thing is Pawn pawn && pawn.Faction == Faction.OfPlayer && pawn.HasNeuralStack(out var neural))
            {
                var comp = __instance.GetComp<CompAffectedByFacilities>();
                if (comp != null)
                {
                    var matrix = comp.LinkedFacilitiesListForReading.OfType<Building_NeuralMatrix>().FirstOrDefault();
                    if (matrix != null)
                    {
                        neural.NeuralData.trackedToMatrix = matrix;
                    }
                }
            }
        }
    }
}

