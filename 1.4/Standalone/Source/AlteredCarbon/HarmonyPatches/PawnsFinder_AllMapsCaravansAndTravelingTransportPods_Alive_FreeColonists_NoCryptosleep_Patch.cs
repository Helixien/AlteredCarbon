using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(PawnsFinder), "AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep", MethodType.Getter)]
    public static class PawnsFinder_AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep_Patch
    {
        public static void Postfix(ref List<Pawn> __result)
        {
            if (Ideo_RecacheColonistBelieverCount_Patch.includeStackPawns)
            {
                var pawns = AlteredCarbonManager.Instance.PawnsWithStacks.Concat(AlteredCarbonManager.Instance.deadPawns ?? Enumerable.Empty<Pawn>()).ToList();
                foreach (var pawn in pawns)
                {
                    if (pawn?.ideo != null && pawn.Ideo != null && !__result.Contains(pawn))
                    {
                        __result.Add(pawn);
                    }
                }
            }
        }
    }
}

