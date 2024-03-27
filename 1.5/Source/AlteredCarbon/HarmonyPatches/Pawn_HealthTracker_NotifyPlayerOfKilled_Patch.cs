using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "NotifyPlayerOfKilled")]
    internal static class Pawn_HealthTracker_NotifyPlayerOfKilled_Patch
    {
        public static Pawn disableKillEffect;
        private static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn, DamageInfo? dinfo, Hediff hediff, Caravan caravan)
        {
            if (disableKillEffect == ___pawn)
            {
                try
                {
                    if (!___pawn.IsEmptySleeve() && ___pawn.HasStackInsideOrOutside())
                    {
                        TaggedString taggedString = "";
                        taggedString = (dinfo.HasValue ? "AC.SleveOf".Translate() + dinfo.Value.Def.deathMessage
                                        .Formatted(___pawn.LabelShortCap, ___pawn.Named("PAWN")) : ((hediff == null)
                                        ? "AC.PawnDied".Translate(___pawn.LabelShortCap, ___pawn.Named("PAWN"))
                                        : "AC.PawnDiedBecauseOf".Translate(___pawn.LabelShortCap, hediff.def.LabelCap,
                                        ___pawn.Named("PAWN"))));
                        taggedString = taggedString.AdjustedFor(___pawn);
                        TaggedString label = "AC.SleeveDeath".Translate() + ": " + ___pawn.LabelShortCap;
                        Find.LetterStack.ReceiveLetter(label, taggedString, LetterDefOf.NegativeEvent, ___pawn);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                disableKillEffect = null;
                return false;
            }
            return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var isSleeveOrHasStack = AccessTools.Method(typeof(Pawn_HealthTracker_NotifyPlayerOfKilled_Patch), "ShouldSkip");
            var pawnField = AccessTools.Field(typeof(Pawn_HealthTracker), "pawn");
            var getIdeo = AccessTools.Method(typeof(Pawn), "get_Ideo");
            bool found = false;
            var codes = instructions.ToList();

            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (!found && i > 2 && codes[i].opcode == OpCodes.Brfalse && codes[i - 1].Calls(getIdeo))
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Brfalse_S, instr.operand);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                    yield return new CodeInstruction(OpCodes.Call, isSleeveOrHasStack);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, instr.operand);
                }
                else
                {
                    yield return instr;
                }
            }
        }

        public static Pawn pawnToSkip;
        public static bool ShouldSkip(Pawn pawn)
        {
            return pawn == pawnToSkip;
        }
    }
}

