using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(MapDeiniter), "PassPawnsToWorld")]
    internal static class MapDeiniter_PassPawnsToWorld_Patch
    {
        private static void Prefix(Map map)
        {
            for (int num = map.mapPawns.AllPawns.Count - 1; num >= 0; num--)
            {
                Pawn pawn = map.mapPawns.AllPawns[num];
                if ((pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer) && pawn.HasStack())
                {
                    pawn.DeSpawn(DestroyMode.Vanish);
                    TaggedString label = "Death".Translate() + ": " + pawn.LabelShortCap;
                    TaggedString taggedString = "PawnDied".Translate(pawn.LabelShortCap, pawn.Named("PAWN"));
                    Find.LetterStack.ReceiveLetter(label, taggedString, LetterDefOf.Death, pawn, null, null);
                }
            }
        }
    }
}

