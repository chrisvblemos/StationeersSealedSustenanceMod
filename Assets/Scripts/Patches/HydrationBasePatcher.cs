using System;
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
    [HarmonyPatch(typeof(HydrationBase))]
    [HarmonyPatch("OnUseSecondary")]
    public static class HydrationBase_OnUseSecondary_Patch_2
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var canDrinkMethod = AccessTools.Method(typeof(Entity), nameof(Entity.CanDrink));
            var myHelperMethod = AccessTools.Method(typeof(HydrationBase_OnUseSecondary_Patch_2), nameof(CheckEvaOrCanDrink));

            foreach (var instruction in instructions)
            {
                yield return instruction;

                if (instruction.Calls(canDrinkMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, myHelperMethod);
                }
            }
        }

        public static bool CheckEvaOrCanDrink(bool originalCanDrink, object item)
        {
            if (originalCanDrink) return true;
            if (item is IEVAConsumable) return true;

            return false;
        }
    }
}
