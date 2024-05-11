using HarmonyLib;
using RimWorld;
using System.Text;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    [HarmonyPatch(typeof(CompRottable), "CompInspectStringExtra")]
    public static class CompRottable_CompInspectStringExtra_Patch
    {
        public static bool Prefix(CompRottable __instance, ref string __result)
        {
            if (__instance.parent is Corpse && __instance.parent.ParentHolder is Building_SleeveGestator)
            {
                __result = CompInspectStringExtra(__instance);
                return false;
            }
            return true;
        }

        public static string CompInspectStringExtra(CompRottable __instance)
        {
            if (!__instance.Active)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder();
            switch (__instance.Stage)
            {
                case RotStage.Fresh:
                    stringBuilder.Append("RotStateFresh".Translate() + ".");
                    break;
                case RotStage.Rotting:
                    stringBuilder.Append("RotStateRotting".Translate() + ".");
                    break;
                case RotStage.Dessicated:
                    stringBuilder.Append("RotStateDessicated".Translate() + ".");
                    break;
            }
            return stringBuilder.ToString();
        }
    }
}

