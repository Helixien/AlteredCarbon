using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(TransferableUIUtility), "DoExtraIcons")]
    public static class TransferableUIUtility_DoExtraIcons_Patch
    {
        private static float BondIconWidth = 24f;
        public static void Postfix(Transferable trad, Rect rect, ref float curX)
        {
            var pawn = trad.AnyThing is Corpse corpse ? corpse.InnerPawn : trad.AnyThing as Pawn;
            if (pawn != null && pawn.HasCorticalStack(out var master))
            {
                var iconRect = new Rect(curX - BondIconWidth, (rect.height - BondIconWidth) / 2f, BondIconWidth, BondIconWidth);
                GUI.DrawTexture(iconRect, master.SourceStack.uiIcon);
                curX -= BondIconWidth;
            }
        }
    }
}
