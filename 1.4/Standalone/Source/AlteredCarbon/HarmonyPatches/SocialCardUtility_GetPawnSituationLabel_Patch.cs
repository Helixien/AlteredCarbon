using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(SocialCardUtility), "GetPawnSituationLabel")]
    public class SocialCardUtility_GetPawnSituationLabel_Patch
    {
        public static bool Prefix(Pawn pawn, Pawn fromPOV, ref string __result)
        {
            if (AlteredCarbonManager.Instance.deadPawns.Contains(pawn) && AlteredCarbonManager.Instance.StacksIndex.ContainsKey(pawn.thingIDNumber))
            {
                __result = "AC.NoSleeve".Translate();
                return false;
            }
            var stackHediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) =>
                    x.def == AC_DefOf.VFEU_CorticalStack);
            if (stackHediff != null && pawn.Dead)
            {
                __result = "AC.Sleeve".Translate();
                return false;
            }
            return true;
        }
    }
}

