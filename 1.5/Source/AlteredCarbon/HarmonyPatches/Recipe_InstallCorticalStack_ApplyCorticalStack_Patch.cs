using HarmonyLib;
using Verse;

namespace AlteredCarbon;

[HarmonyPatch(typeof(Recipe_InstallCorticalStack), "ApplyCorticalStack")]
internal static class Recipe_InstallCorticalStack_ApplyCorticalStack_Patch
{
    private static void Postfix(Pawn pawn)
    {
        if (pawn.HasCorticalStack(out var hediff) && (hediff.PersonaData.ideo?.HasPrecept(AC_DefOf.AC_CrossSleeving_DontCare) ?? false))
        {
            hediff.PersonaData.thoughts?.RemoveAll(x => x.def == AC_DefOf.AC_WrongGender);
            hediff.PersonaData.thoughts?.RemoveAll(x => x.def == AC_DefOf.AC_WrongGenderDouble);
            hediff.PersonaData.thoughts?.RemoveAll(x => x.def == AC_DefOf.AC_WrongGenderPregnant);
        }
    }
    
}
