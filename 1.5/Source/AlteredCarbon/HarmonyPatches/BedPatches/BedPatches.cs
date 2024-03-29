using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [StaticConstructorOnStartup]
	public static class BedPatches
    {
		static BedPatches()
        {
			MethodInfo method = typeof(BedPatches).GetMethod("Prefix");
			foreach (Type type in GenTypes.AllSubclasses(typeof(Need)))
			{
				MethodInfo method2 = type.GetMethod("NeedInterval");
				try
                {
					AC_Utils.harmony.Patch(method2, new HarmonyMethod(method), null, null);
				}
				catch
				{
				};
			}
		}

		public static bool Prefix(Need __instance, Pawn ___pawn)
        {
			if (___pawn != null && ___pawn.CurrentBed() is Building_SleeveCasket)
            {
				return false;
            }
			return true;
        }
    }

}

