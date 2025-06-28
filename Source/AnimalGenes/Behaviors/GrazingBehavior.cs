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
    public static class GrazingBehavior
    {       
        public static bool IsSapientHerbivore(this Pawn pawn)
        {
            // Check if the pawn is a sapient animal and has the grazing behavior
            if (!pawn.IsSapientAnimal()) return false;
            if (pawn.genes != null && pawn.genes.GenesListForReading.Select(g => g.def.defName).Contains("ANG_Grazing_Behavior")) return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.TryFindBestFoodSourceFor))]
    static class FoodUtility_TryFindBestFoodSourceFor_Patch
    {
        public static void Prefix(Pawn getter, Pawn eater, bool desperate, ref Thing foodSource, ref ThingDef foodDef, bool canRefillDispenser, bool canUseInventory, bool canUsePackAnimalInventory, bool allowForbidden, bool allowCorpse, bool allowSociallyImproper, bool allowHarvest, bool forceScanWholeMap, bool ignoreReservations, bool calculateWantedStackCount, bool allowVenerated, FoodPreferability minPrefOverride)
        {
            Check.DebugLog("TryFindBestFoodSourceFor patch");
            //if (!eater.IsSapientHerbivore() && !getter.IsSapientHerbivore()) return;
            string log = $@"
                TryFindBestFoodSourceFor Postfix:
                  getter: {getter?.ToString() ?? "null"} (defName: {getter?.def?.defName ?? "null"})
                  eater: {eater?.ToString() ?? "null"} (defName: {eater?.def?.defName ?? "null"})
                  desperate: {desperate}
                  foodSource: {foodSource?.ToString() ?? "null"} (defName: {foodSource?.def?.defName ?? "null"})
                  foodDef: {foodDef?.defName ?? "null"}
                  canRefillDispenser: {canRefillDispenser}
                  canUseInventory: {canUseInventory}
                  canUsePackAnimalInventory: {canUsePackAnimalInventory}
                  allowForbidden: {allowForbidden}
                  allowCorpse: {allowCorpse}
                  allowSociallyImproper: {allowSociallyImproper}
                  allowHarvest: {allowHarvest}
                  forceScanWholeMap: {forceScanWholeMap}
                  ignoreReservations: {ignoreReservations}
                  calculateWantedStackCount: {calculateWantedStackCount}
                  allowVenerated: {allowVenerated}
                  minPrefOverride: {minPrefOverride}
                ";
            Check.DebugLog(log);
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap))]
    static class FoodUtility_BestFoodSourceOnMap_Patch
    {
        private static bool FoodValidatorPredicate(Thing t, Pawn getter, Pawn eater)
        {
            if (!eater.WillEat(t, getter, true, false) || !t.IngestibleNow || t.Destroyed || t.IsForbidden(getter))
            {
                return false;
            }

            return !getter.roping.IsRoped || t.PositionHeld.InHorDistOf(getter.roping.RopedTo.Cell, 8f);
        }

        public static void Postfix(ref Thing __result, Pawn getter, Pawn eater, bool desperate, ref ThingDef foodDef, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false, bool ignoreReservations = false, bool calculateWantedStackCount = false, FoodPreferability minPrefOverride = FoodPreferability.Undefined, float? minNutrition = null, bool allowVenerated = false)
        {
            if (__result == null && !eater.IsSapientHerbivore() && AnimalGenesModSettings.Settings.AllowGrazingBehavior) return;
            Check.DebugLog("Extra check for hungry hungry herbiovore");

            int maxRegionsToScan = 100;
            // All things, including plants
            ThingRequest thingRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodSource);

            Thing bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, 
                PathEndMode.OnCell, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false, false, false, true),
                9999f, (Thing t) => FoodValidatorPredicate(t, getter, eater), null, 0, maxRegionsToScan, false, RegionType.Set_Passable, true, false);

            Check.DebugLog($"New best thing: {bestThing.def.defName} {bestThing.def}");
            __result = bestThing;
            foodDef = bestThing.def;
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.WillEat), new Type[] { typeof(Pawn), typeof(ThingDef), typeof(Pawn), typeof(bool), typeof(bool) })]
    static class FoodUtility_FoodIsSuitable_WillEat_Patch
    {
        public static void Postfix(ref bool __result, Pawn p, ThingDef food, Pawn getter, bool careIfNotAcceptableForTitle, bool allowVenerated)
        {
            //Check.DebugLog($"WillEat Postfix: {food.defName} {__result}");
            if (__result == true) return;
            if (food.plant != null && p.IsSapientHerbivore() && AnimalGenesModSettings.Settings.AllowGrazingBehavior)
            {
                //Check.DebugLog($"WillEat Postfix: {food.ingestible.foodType} {food.ingestible.preferability}");
                if (food.ingestible.foodType == FoodTypeFlags.Plant && food.ingestible.preferability >= FoodPreferability.RawBad)
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
            if (!___usingNutrientPasteDispenser && __instance.pawn.IsSapientHerbivore() && __instance.job.GetTarget(TargetIndex.A).Thing is Plant)
            {
                MethodInfo dynMethod = __instance.GetType().GetMethod("PrepareToIngestToils_NonToolUser",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                __result = (IEnumerable<Toil>)dynMethod.Invoke(__instance, new object[] { });

                Check.DebugLog($"Nomnom {__instance.job.GetTarget(TargetIndex.A).Thing} from the floor");

                return;
            }
        }
    }
}
