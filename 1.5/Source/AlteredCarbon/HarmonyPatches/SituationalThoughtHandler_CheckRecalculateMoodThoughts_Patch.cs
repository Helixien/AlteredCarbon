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
	[HarmonyPatch(typeof(SituationalThoughtHandler), "UpdateAllMoodThoughts")]
	public static class SituationalThoughtHandler_UpdateAllMoodThoughts_Patch
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

