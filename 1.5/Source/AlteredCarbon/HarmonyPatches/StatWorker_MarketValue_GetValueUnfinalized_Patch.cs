using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(StatWorker_MarketValue), "GetValueUnfinalized")]
    public static class StatWorker_MarketValue_GetValueUnfinalized_Patch
    {
        public static bool Prefix(StatWorker_MarketValue __instance, ref float __result, StatRequest req, bool applyPostProcess = true)
        {
            if (req.Thing is ThingWithNeuralData thingWithStack)
            {
                __result =  __instance.GetBaseValueFor(req) * PawnQualityPriceFactor(thingWithStack.NeuralData.DummyPawn);
                return false;
            }
            return true;
        }

        public static float PawnQualityPriceFactor(Pawn pawn, StringBuilder explanation = null)
        {
            float num = 1f;
            if (pawn.skills != null && pawn.skills.skills.Count > 1)
            {
                num *= PriceUtility.AverageSkillCurve.Evaluate(((IEnumerable<SkillRecord>)pawn.skills.skills)
                    .Average((Func<SkillRecord, float>)((SkillRecord sk) => sk.Level)));
            }
            num *= pawn.ageTracker.CurLifeStage.marketValueFactor;
            if (pawn.story != null && pawn.story.traits != null)
            {
                for (int j = 0; j < pawn.story.traits.allTraits.Count; j++)
                {
                    Trait trait = pawn.story.traits.allTraits[j];
                    if (!trait.Suppressed)
                    {
                        num += trait.CurrentData.marketValueFactorOffset;
                    }
                }
            }
            explanation?.AppendLine("StatsReport_CharacterQuality".Translate() + ": x" + num.ToStringPercent());
            return num;
        }
    }
}

