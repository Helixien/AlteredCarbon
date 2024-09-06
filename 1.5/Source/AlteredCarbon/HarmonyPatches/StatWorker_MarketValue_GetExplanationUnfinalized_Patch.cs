using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using System.Text;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(StatWorker_MarketValue), "GetExplanationUnfinalized")]
    public static class StatWorker_MarketValue_GetExplanationUnfinalized_Patch
    {
        public static bool Prefix(StatWorker_MarketValue __instance, ref string __result, StatRequest req, ToStringNumberSense numberSense)
        {
            if (req.Thing is ThingWithNeuralData thingWithStack)
            {
                Pawn pawn = thingWithStack.NeuralData.DummyPawn;
                StringBuilder stringBuilder = new StringBuilder();
                float baseValueFor = __instance.GetBaseValueFor(req);
                if (baseValueFor != 0f || __instance.stat.showZeroBaseValue)
                {
                    stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " + __instance.stat.ValueToString(baseValueFor, numberSense));
                }
                StatWorker_MarketValue_GetValueUnfinalized_Patch.PawnQualityPriceFactor(pawn, stringBuilder);
                __result = stringBuilder.ToString();
                return false;
            }
            return true;
        }
    }
}

