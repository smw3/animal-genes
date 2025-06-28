using BigAndSmall;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        }
    }

    [HarmonyPatch(typeof(BigAndSmall.RaceMorpher), nameof(BigAndSmall.RaceMorpher.SwapAnimalToSapientVersion))]
    static class RaceMorpher_SwapAnimalToSapientVersion_Patch
    {
        static void Postfix(Pawn __result, Pawn aniPawn)
        {
            Check.DebugLog($"RaceMorpher_SwapAnimalToSapientVersion_Patch: Adding genes to sapient animal {__result.Name}.");
            RaceMorpher.AddAppropriateAffinityGenes(__result);
        }
    }

    [HarmonyPatch(typeof(Gene), nameof(Gene.OverrideBy))]
    [HarmonyAfter("RedMattis.BigAndSmall_Early")]
    static class Gene_OverrideBy_Patch
    {
        public static void Postfix(Gene __instance, Gene overriddenBy)
        {
            Gene gene = __instance;
            if (gene != null && gene.pawn != null && gene.pawn.Spawned)
            {
                if (GeneGenerator.affinityGenes.ContainsKey(gene.def))
                {
                    if (PrerequisiteValidator.Validate(gene.def, gene.pawn) is string pFailReason && pFailReason != "") {
                        Check.DebugLog($"Gene {gene.def.defName} failed prerequisite validation: {pFailReason}");
                    } else { 
                        HumanlikeAnimal target = gene.def.GetModExtension<GeneModExtension_TargetAffinity>()?.targetAnimal;
                        Check.NotNull(target, "target cannot be null in GeneModExtension_TargetAffinity Gene_OverrideBy_Patch");
                        if (gene.pawn.def != target.humanlikeThing)
                        {
                            Check.DebugLog($"Pawn of def {gene.pawn.def} has ACTIVE affinity (Gene {gene.def.defName}) for target: {target.humanlikeThing.defName} but is {gene.pawn.def.defName}");
                            RaceMorpher.SwapPawnToSapientAnimal(gene.pawn, target);
                        }
                    }
                }
            }
        }
    }
}
