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
            if (__instance.pawn.HasRemoteStack(out var remote) && remote.source != null 
                && remote.CanBeConnected(remote.source.pawn) is false)
            {
                Log.Message(__instance.pawn + " can't connect to " + remote.source.pawn);
                remote.EndNeedlecasting();
            }
            else if (__instance.pawn.HasNeuralStack(out var neural) && neural.needleCastingInto != null 
                && neural.needleCastingInto.CanBeConnected(__instance.pawn) is false)
            {
                Log.Message(__instance.pawn + " can't connect to " + neural.needleCastingInto.pawn);
                neural.needleCastingInto.EndNeedlecasting();
            }
            looking = false;
        }
    }
}
