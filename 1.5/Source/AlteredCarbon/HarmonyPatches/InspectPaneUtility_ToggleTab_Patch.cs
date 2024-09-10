using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(InspectPaneUtility), "ToggleTab")]
    public static class InspectPaneUtility_ToggleTab_Patch
    {
        public static bool Prefix(InspectTabBase tab, IInspectPane pane)
        {
            if (tab is ITab_StackStorageContents tabStackStorage)
            {
                var matrix = tabStackStorage.CompNeuralCache.GetMatrix();
                if (matrix != null)
                {
                    Find.WindowStack.Add(new Window_NeuralMatrixManagement(matrix));
                    return false;
                }
            }
            return true;
        }
    }
}

