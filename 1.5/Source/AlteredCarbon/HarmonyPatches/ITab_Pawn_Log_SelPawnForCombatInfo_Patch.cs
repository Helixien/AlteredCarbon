using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    [HarmonyPatch(typeof(ITab_Pawn_Log), "SelPawnForCombatInfo", MethodType.Getter)]
    public static class ITab_Pawn_Log_SelPawnForCombatInfo_Patch
    {
        public static int lastTimeUpdated;
        public static Pawn lastPawn;
        public static bool Prefix(ref Pawn __result)
        {
            var personaData = TryGetPersonaData();
            if (personaData != null)
            {
                if (__result != lastPawn)
                {
                    lastPawn = __result;
                    personaData.RefreshDummyPawn();
                }
                else if (Time.frameCount - lastTimeUpdated >= 60)
                {
                    lastTimeUpdated = Time.frameCount;
                    personaData.RefreshDummyPawn();
                }
                __result = personaData.GetDummyPawn;
                return false;
            }
            return true;
        }

        public static PersonaData TryGetPersonaData()
        {
            var selectedThing = Find.Selector.SingleSelectedThing;
            if (selectedThing is MindFrame mindFrame)
            {
                return mindFrame.PersonaData;
            }
            return null;
        }
    }
}

