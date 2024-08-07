using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(CharacterCardUtility), "DoLeftSection")]
    public static class CharacterCardUtility_DoLeftSection_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stfld && instruction.operand is FieldInfo info && info.Name == "abilities")
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(CharacterCardUtility_DoLeftSection_Patch), "FilterAbilities"));
                }
                yield return instruction;
            }
        }

        public static List<Ability> FilterAbilities(List<Ability> abilities, Pawn pawn)
        {
            if (PersonaData.dummyPawns.Contains(pawn))
            {
                var filteredAbilities = abilities.Where(x => PersonaData.CanStoreAbility(pawn, x.def)).ToList();
                return filteredAbilities;
            }
            return abilities;
        }
    }
}
