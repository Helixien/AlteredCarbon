using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon;


[HarmonyPatch(typeof(ThoughtWorker_AnnoyingVoice), "CurrentSocialStateInternal")]
public class ThoughtWorker_AnnoyingVoice_CurrentSocialStateInternal_Patch
{
    public static void Postfix(Pawn other, ref ThoughtState __result)
    {
        if (other.health.hediffSet.HasHediff(AC_DefOf.AC_VoiceSynthesizer))
        {
            __result = false;
        }
    }
}