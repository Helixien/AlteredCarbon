using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(TransferableUtility), "CanStack")]
    public static class TransferableUtility_CanStack_Patch
    {
        public static void Postfix(ref bool __result, Thing thing)
        {
            if (thing is ThingWithPersonaData personaStack && personaStack.PersonaData.ContainsPersona)
            {
                __result = false;
            }
        }
    }
}
