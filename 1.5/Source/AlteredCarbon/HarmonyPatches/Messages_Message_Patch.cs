using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Messages), "Message", new Type[]
    {
                typeof(string),
                typeof(LookTargets),
                typeof(MessageTypeDef),
                typeof(bool)
    })]
    internal static class Messages_Message_Patch
    {
        private static bool Prefix(string text, LookTargets lookTargets, MessageTypeDef def)
        {
            if (def == MessageTypeDefOf.PawnDeath && lookTargets.TryGetPrimaryTarget().Thing is Pawn pawn && (pawn.IsEmptySleeve() 
                || pawn.HasStackInsideOrOutside()))
            {
                return false;
            }
            return true;
        }
    }
}

