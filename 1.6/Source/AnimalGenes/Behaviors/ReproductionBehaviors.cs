using AnimalGenes.GeneModExtensions;
using AnimalGenes.Helpers;
using BigAndSmall;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes.Behaviors
{
    // This file is cursed. But if the goal is "behavior as close to actual animals", then it is inevitable. I am probably going to default all of this to off, and then
    // only the people who turn it on are to blame.
    public class ReproductionBehaviors
    {
        [HarmonyPatch(typeof(HumanlikeAnimalGenerator), nameof(HumanlikeAnimalGenerator.SetAnimalStatDefValues))]
        static class HumanlikeAnimalGenerator_SetAnimalStatDefValues_Patch
        {
            static void Postfix(ThingDef humanThing, ThingDef animalThing, ThingDef newThing, float fineManipulation, PawnExtension pExt)
            {
                if (AnimalGenesModSettings.Settings.EnableCrossbreeding)
                {
                    newThing.SetStatBaseValue(BSDefs.SM_FlirtChance, 1);
                    newThing.SetStatBaseValue(StatDefOf.Fertility, 1);

                    newThing.race.canCrossBreedWith ??= [];
                    newThing.race.canCrossBreedWith.Add(animalThing);

                    animalThing.race.canCrossBreedWith ??= [];
                    newThing.race.canCrossBreedWith.AddRange(animalThing.race.canCrossBreedWith);
                    animalThing.race.canCrossBreedWith.Add(newThing);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnUtility), nameof(PawnUtility.Mated))]
    public static class PawnUtility_Mated
    {
        public static bool Prefix(Pawn male, Pawn female)
        {
            if (!male.IsSapientAnimal() && !female.IsSapientAnimal()) return true;
            if (female.Sterile() || male.Sterile()) return false;

            // Egg fertilisation would go here as well.

            if (Rand.Value > PregnancyUtility.PregnancyChanceForPawn(female)) return false;
            // Guarantee full set of sapient endogenes
            GeneSet geneSet = new();
            foreach (GeneDef geneDef in GeneGenerator.humanLikeGenes[HumanlikeAnimalGenerator.humanlikeAnimals[female.def]])
            {
                geneSet.AddGene(geneDef);
            }
            geneSet.AddGene(GeneGenerator.affinityGenes.Where(ag => ag.Value.humanlikeAnimal == female.def).Select(ag => ag.Key).First());

            if (female.IsSapientAnimal())
            {
                Hediff_Pregnant hediff_Pregnant = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.PregnantHuman, female, null);
                hediff_Pregnant.SetParents(female, male, geneSet);

                female.health.AddHediff(hediff_Pregnant, null, null, null);
            }
            else
            {
                Hediff_Pregnant hediff_Pregnant = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.Pregnant, female, null);
                hediff_Pregnant.SetParents(female, male, null);

                female.health.AddHediff(hediff_Pregnant, null, null, null);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(SocialCardUtility), "ShouldShowPawnRelations")]
    [HarmonyPriority(Priority.First)]
    public static class SocialCardUtility_ShouldShowPawnRelations
    {
        public static void Postfix(ref bool __result, Pawn pawn, Pawn selPawnForSocialInfo)
        {
            if (__result || !AnimalGenesModSettings.Settings.EnableCrossbreeding) return;
            __result = ((!pawn.Dead || pawn.Corpse != null) && pawn.Name != null && !pawn.relations.hidePawnRelations && !selPawnForSocialInfo.relations.hidePawnRelations && pawn.relations.everSeenByPlayer);
        }
    }

    [HarmonyPatch(typeof(RaceProperties), nameof(RaceProperties.ConfigErrors))]
    public static class RaceProperties_ConfigErrors
    {
        public static void Postfix(ref IEnumerable<string> __result, ThingDef thingDef)
        {
            if (__result != null && __result.Any(s => s != null && s.Contains("not an animal")))
            {
                __result = Enumerable.Empty<string>();
            }
        }
    }
}
