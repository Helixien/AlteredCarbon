using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(SymbolResolver_Interior_AncientTemple), "Resolve")]
    public static class SymbolResolver_Interior_AncientTemple_Resolve_Patch
    {
        public static void Postfix(SymbolResolver_Interior_AncientTemple __instance, ResolveParams rp)
        {
            if (AC_Utils.generalSettings.enableMindFramesInAncientDangers)// && Rand.Chance(0.25f))
            {
                ResolveParams resolveParams = rp;
                var mindFrame = ThingMaker.MakeThing(AC_DefOf.AC_MindFrame) as MindFrame;
                var podContentsType = rp.podContentsType;
                var faction = podContentsType is null || podContentsType.Value != PodContentsType.AncientHostile ? Faction.OfAncients : Faction.OfAncientsHostile;
                Log.Message(podContentsType + " - " + faction.def);
                mindFrame.GeneratePersona(faction);
                resolveParams.singleThingToSpawn = mindFrame;
                BaseGen.symbolStack.Push("thing", resolveParams);
            }
        }
    }
}
