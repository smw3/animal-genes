using AnimalGenes.GeneModExtensions;
using AnimalGenes.Helpers;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using static RimWorld.Dialog_EditIdeoStyleItems;

namespace AnimalGenes.Behaviors
{
    public static class DietBehaviors
    {       

    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap))]
    static class FoodUtility_BestFoodSourceOnMap_Patch
    {
        public static bool IsAcceptablePreyForSapientPredator(Pawn predator, Pawn prey)
        {
            if (!prey.RaceProps.canBePredatorPrey)
            {
                //Check.DebugLog($"Prey: {prey} race can't be prey");
                return false;
            }
            if (!prey.RaceProps.IsFlesh)
            {
                return false;
            }
            if (prey.RaceProps.Humanlike) // Maybe check for traits that would make pawn not care about killing people
            {
                //Check.DebugLog($"Prey: {prey} humanlike");
                return false;
            }
            if (prey.BodySize > predator.RaceProps.maxPreyBodySize)
            {
                //Check.DebugLog($"Prey: {prey} too large");
                return false;
            }
            if (!prey.Downed)
            {
                if (prey.kindDef.combatPower > 2f * predator.kindDef.combatPower)
                {
                    //Check.DebugLog($"Prey: {prey} {prey.kindDef.combatPower} vs {predator.kindDef.combatPower}");
                    return false;
                }
                float adjustedPreyPower = prey.kindDef.combatPower * prey.health.summaryHealth.SummaryHealthPercent * prey.BodySize;
                float adjustedPredatorPower = predator.kindDef.combatPower * predator.health.summaryHealth.SummaryHealthPercent * predator.BodySize;
                if (adjustedPreyPower >= adjustedPredatorPower)
                {
                    //Check.DebugLog($"Prey: {prey} adjusted {adjustedPreyPower} vs {adjustedPredatorPower}");
                    return false;
                }
            }
            return (prey.Faction == null || predator.HostileTo(prey)) && 
                !prey.IsHiddenFromPlayer() && !prey.IsPsychologicallyInvisible() && 
                (!ModsConfig.AnomalyActive || !prey.IsMutant || prey.mutant.Def.canBleed);
        }

        private static bool FoodValidatorPredicate(Thing t, Pawn getter, Pawn eater)
        {
            if (t == null || !t.IngestibleNow || t.Destroyed || !eater.WillEat(t, getter, true, false) || t.IsForbidden(getter))
            {
                return false;
            }

            return !getter.roping.IsRoped || t.PositionHeld.InHorDistOf(getter.roping.RopedTo.Cell, 8f);
        }

        private static Pawn BestPawnToHuntForSapientPredator(Pawn predator)
        {
            List<Pawn> tmpPredatorCandidates = [];
            if (predator.meleeVerbs.TryGetMeleeVerb(null) == null)
            {
                return null;
            }
            bool predatorInjured = predator.health.summaryHealth.SummaryHealthPercent < 0.25f;

            tmpPredatorCandidates.AddRange(predator.Map.mapPawns.AllPawnsSpawned);

            Pawn pawn = null;
            float num = 0f;

            for (int i = 0; i < tmpPredatorCandidates.Count; i++)
            {
                Pawn potentialPrey = tmpPredatorCandidates[i];
                if (predator != potentialPrey && 
                    (!predatorInjured || potentialPrey.Downed) && 
                    IsAcceptablePreyForSapientPredator(predator, potentialPrey) && 
                    predator.CanReach(potentialPrey, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn) && 
                    !potentialPrey.IsForbidden(predator))
                {
                    float preyScoreFor = FoodUtility.GetPreyScoreFor(predator, potentialPrey);
                    if (preyScoreFor > num || pawn == null)
                    {
                        num = preyScoreFor;
                        pawn = potentialPrey;
                        //Check.DebugLog($"Potential prey: {pawn} {num}");
                    }
                }
            }

            return pawn;
        }

        public static void Postfix(ref Thing __result, Pawn getter, Pawn eater, bool desperate, ref ThingDef foodDef, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false, bool ignoreReservations = false, bool calculateWantedStackCount = false, FoodPreferability minPrefOverride = FoodPreferability.Undefined, float? minNutrition = null, bool allowVenerated = false)
        {
            if (__result != null) return;
            // Plant-eaters
            if ((eater.CanGraze() && AnimalGenesModSettings.Settings.AllowGrazingBehavior) || (eater.IsDendrovore() && AnimalGenesModSettings.Settings.AllowDendrovoreBehavior))
            {

                int maxRegionsToScan = 100;
                // All things, including plants
                ThingRequest thingRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodSource);

                Thing bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest,
                    PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false, false, false, true),
                    9999f, (Thing t) => FoodValidatorPredicate(t, getter, eater), null, 0, maxRegionsToScan, false, RegionType.Set_Passable, true, false);

                __result = bestThing;
                foodDef = bestThing?.def;
            }
            if (__result != null) return;

            // Predators
            if (eater.IsPredator() && AnimalGenesModSettings.Settings.AllowPredatorBehavior)
            {
                Thing bestPrey = BestPawnToHuntForSapientPredator(eater);
                //Check.DebugLog($"Predator best prey: {bestPrey}");

                __result = bestPrey;
                foodDef = bestPrey?.def;
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.WillEat), new Type[] { typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool), typeof(bool) })]
    static class FoodUtility_FoodIsSuitable_WillEat_Patch
    {
        public static void Postfix(ref bool __result, Pawn p, ThingDef food, Pawn getter, bool careIfNotAcceptableForTitle, bool allowVenerated)
        {
            if (__result == true) return;
            if (food.plant != null)
            {
                if (p.CanGraze() && AnimalGenesModSettings.Settings.AllowGrazingBehavior && food.ingestible.foodType == FoodTypeFlags.Plant && food.ingestible.preferability >= FoodPreferability.RawBad)
                {
                    __result = true;
                }
                if (p.IsDendrovore() && AnimalGenesModSettings.Settings.AllowDendrovoreBehavior && food.ingestible.foodType == FoodTypeFlags.Tree && food.ingestible.preferability >= FoodPreferability.RawBad)
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(JobDriver_Ingest), "PrepareToIngestToils")]
    static class JobDriver_Ingest_Patch
    {
        public static void Postfix(ref IEnumerable<Toil> __result, JobDriver_Ingest __instance, bool ___usingNutrientPasteDispenser, Toil chewToil)
        {
            if (!___usingNutrientPasteDispenser && (__instance.pawn.CanGraze() || __instance.pawn.IsDendrovore()) && __instance.job.GetTarget(TargetIndex.A).Thing is Plant)
            {
                MethodInfo dynMethod = __instance.GetType().GetMethod("PrepareToIngestToils_NonToolUser",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                __result = (IEnumerable<Toil>)dynMethod.Invoke(__instance, new object[] { });

                return;
            }
        }
    }
}
