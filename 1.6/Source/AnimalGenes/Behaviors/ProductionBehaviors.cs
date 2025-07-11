using AnimalGenes.GeneModExtensions;
using BigAndSmall;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using static HarmonyLib.Code;

namespace AnimalGenes.Behaviors
{
    public class ProductionBehaviors
    {
        [HarmonyPatch(typeof(ProductionGene), nameof(ProductionGene.TickEvent))]
        static class FoodUtility_ProductionGene_TickEvent_Patch
        {

            public static bool Prefix(Gene __instance)
            {
                ProductionDependsOnGender props = __instance.def.GetModExtension<ProductionDependsOnGender>();
                if (props == null || props.activeIfGender == Gender.None) return true;
                if (__instance.pawn.gender == props.activeIfGender) return true;

                return false;
            }
        }

        [HarmonyPatch(typeof(GeneDef), "GetDescriptionFull")]
        public static class GeneDef_GetDescriptionFull
        {
            public static void Postfix(ref string __result, GeneDef __instance)
            {
                if (__instance.HasModExtension<ProductionDependsOnGender>())
                {
                    var geneExtension = __instance.GetModExtension<ProductionDependsOnGender>();
                    if (geneExtension.activeIfGender == Gender.None) return; // Maybe there's a situation where none gender is valid, but not likely..
                    StringBuilder stringBuilder = new();

                    stringBuilder.AppendLine(__result);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine($"Only pawns of the {geneExtension.activeIfGender} gender produce resources.");

                    __result = stringBuilder.ToString();
                }
            }
        }
    }
}
