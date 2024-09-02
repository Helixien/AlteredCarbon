using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(LordJob_BestowingCeremony), "FinishCeremony")]
    public static class LordJob_BestowingCeremony_FinishCeremony_Patch
    {
        public static void Postfix(LordJob_BestowingCeremony __instance, Pawn pawn)
        {
            if (__instance.bestower.kindDef == PawnKindDefOf.Empire_Royal_Bestower)
            {
                ThingOwner<Thing> innerContainer = __instance.bestower.inventory.innerContainer;
                for (int num = innerContainer.Count - 1; num >= 0; num--)
                {
                    if (innerContainer[num].def == AC_DefOf.AC_EmptyNeuralStack)
                    {
                        innerContainer.TryDrop(innerContainer[num], ThingPlaceMode.Near, out Thing lastResultingThing);
                    }
                }
            }
        }
    }
}

