using HarmonyLib;
using Verse;

namespace AlteredCarbon
{

    [HarmonyPatch(typeof(HediffSet), nameof(HediffSet.DirtyCache))]
    public static class HediffSet_DirtyCache_Patch
    {
        public static bool looking;
        public static void Postfix(HediffSet __instance)
        {
            if (looking) return;
            looking = true;
            if (__instance.pawn.HasRemoteStack(out var remote) && remote.Needlecasted)
            {
                if (remote.CanBeConnected(remote.Source) is false)
                {
                    remote.EndNeedlecasting();
                }
            }
            else if (__instance.pawn.HasNeuralStack(out var neural) && neural.needleCastingInto != null 
                && neural.needleCastingInto.CanBeConnected(neural) is false)
            {
                neural.needleCastingInto.EndNeedlecasting();
            }
            looking = false;
        }
    }
}
