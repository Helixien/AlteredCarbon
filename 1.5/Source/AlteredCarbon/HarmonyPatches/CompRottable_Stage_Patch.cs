using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{

    [HarmonyPatch(typeof(CompRottable), "Stage", MethodType.Getter)]
    internal static class CompRottable_Stage_Patch
    {
        public static void Postfix(CompRottable __instance, RotStage __result)
        {
            if (__result == RotStage.Dessicated && __instance.parent is Corpse corpse
                && corpse.InnerPawn.HasNeuralStack(out var hediff))
            {
                hediff.SpawnStack(placeMode: ThingPlaceMode.Direct);
            }
        }
    }
}
