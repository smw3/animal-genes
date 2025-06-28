using BigAndSmall;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static System.Net.Mime.MediaTypeNames;

namespace AnimalGenes
{
    class Main_early : Mod
    {
        public Main_early(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("ingendum.animalgenes");
            harmony.PatchAll(); 
        }
    }

    [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
    [HarmonyAfter("RedMattis.BigAndSmall_Early")]
    static class DefGenerator_GenerateImpliedDefs_PreResolve_Patch
    {
        public static void Prefix(bool hotReload)
        {
            GeneGenerator.GenerateGenes();
        }
    }

    [HarmonyPatch(typeof(HumanlikeAnimalGenerator), nameof(HumanlikeAnimalGenerator.SetAnimalStatDefValues))]
    static class HumanlikeAnimalGenerator_SetAnimalStatDefValues_Patch
    {
        static void Postfix(ThingDef humanThing, ThingDef animalThing, ThingDef newThing, float fineManipulation, PawnExtension pExt)
        {
            newThing.SetStatBaseValue(BSDefs.SM_FlirtChance, 1);
            newThing.SetStatBaseValue(StatDefOf.Fertility, 1);

            Log.Message($"Set flirt chance for {newThing.defName} to {newThing.GetStatValueAbstract(BSDefs.SM_FlirtChance)}");
        }
    }

    [HarmonyPatch(typeof(HumanlikeAnimalGenerator), nameof(HumanlikeAnimalGenerator.GenerateAndRegisterHumanlikeAnimal))]
    static class HumanlikeAnimalGenerator_GenerateAndRegisterHumanlikeAnimal_Patch
    {
        static void Postfix(PawnKindDef aniPawnKind, ThingDef humThing, bool hotReload)
        {
            Log.Message($"Postfix for GenerateAndRegisterHumanlikeAnimal called for {aniPawnKind.defName} with ThingDef {humThing.defName}");
        }
    }
}
