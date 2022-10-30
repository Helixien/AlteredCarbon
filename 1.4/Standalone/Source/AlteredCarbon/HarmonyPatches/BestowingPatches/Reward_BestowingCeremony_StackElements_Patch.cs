using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Reward_BestowingCeremony), "StackElements", MethodType.Getter)]
    public static class Reward_BestowingCeremony_StackElements_Patch
    {
        public static void Postfix(Reward_BestowingCeremony __instance, ref IEnumerable<GenUI.AnonymousStackElement> __result)
        {
            if (ModCompatibility.HelixienAlteredCarbonIsActive)
            {
                if (__instance.royalTitle.defName == "Baron" || __instance.royalTitle.defName == "Count")
                {
                    var list = __result.ToList();
                    var item = QuestPartUtility.GetStandardRewardStackElement(AC_DefOf.VFEU_EmptyCorticalStack.label.CapitalizeFirst(), AC_DefOf.VFEU_EmptyCorticalStack.uiIcon, () => AC_DefOf.VFEU_EmptyCorticalStack.description, delegate
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(AC_DefOf.VFEU_EmptyCorticalStack));
                    });
                    list.Insert(1, item);
                    __result = list;
                }
            }

        }
    }
}

