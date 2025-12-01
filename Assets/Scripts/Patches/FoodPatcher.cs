using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using HarmonyLib;
using Objects.Items;
using SealedSustenance.Interfaces;
using static Assets.Scripts.Objects.Thing;

namespace SealedSustenance.Patchers
{
    [HarmonyPatch(typeof(Food))]
    [HarmonyPatch("OnUseSecondary")]
    public static class Food_OnUseSecondary_Patch_2
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var canEatMethod = AccessTools.Method(typeof(Entity), nameof(Entity.CanEat));
            var myHelperMethod = AccessTools.Method(typeof(Food_OnUseSecondary_Patch_2), nameof(CheckEvaOrCanEat));

            foreach (var instruction in instructions)
            {
                yield return instruction;

                if (instruction.Calls(canEatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, myHelperMethod);
                }
            }
        }

        public static bool CheckEvaOrCanEat(bool originalCanEat, object item)
        {
            if (originalCanEat) return true;
            if (item is IEVAConsumable) return true;

            return false;
        }
    }
}
