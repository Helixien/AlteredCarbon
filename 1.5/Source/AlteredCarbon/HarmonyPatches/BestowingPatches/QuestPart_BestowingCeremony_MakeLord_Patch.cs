using System;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(QuestPart_BestowingCeremony), "MakeLord")]
    public static class QuestPart_BestowingCeremony_MakeLord_Patch
    {
        public static void Postfix(QuestPart_BestowingCeremony __instance, Lord __result)
        {
            if (__instance.bestower.kindDef == PawnKindDefOf.Empire_Royal_Bestower)
            {
                RoyalTitleDef titleAwardedWhenUpdating = __instance.target.royalty.GetTitleAwardedWhenUpdating(__instance.bestower.Faction,
                    __instance.target.royalty.GetFavor(__instance.bestower.Faction));
                if (titleAwardedWhenUpdating != null && (titleAwardedWhenUpdating.defName == "Baron"
                    || titleAwardedWhenUpdating.defName == "Count"))
                {
                    ThingOwner<Thing> innerContainer = __instance.bestower.inventory.innerContainer;
                    innerContainer.TryAdd(ThingMaker.MakeThing(AC_DefOf.AC_EmptyNeuralStack), 1);
                }
            }
        }
    }
}

