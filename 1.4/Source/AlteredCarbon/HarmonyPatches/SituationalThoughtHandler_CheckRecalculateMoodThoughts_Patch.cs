using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
	[HarmonyPatch(typeof(SituationalThoughtHandler), "CheckRecalculateMoodThoughts")]
	public static class SituationalThoughtHandler_CheckRecalculateMoodThoughts_Patch
    {
		public static bool Prefix(Pawn ___pawn)
		{
			if (___pawn.IsEmptySleeve())
			{
				return false;
			}
			return true;
		}
	}
}

