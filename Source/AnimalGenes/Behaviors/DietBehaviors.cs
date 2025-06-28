using AnimalGenes.Genes;
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
        public static bool CanGraze(this Pawn pawn)
        {
            return pawn.genes != null &&
                pawn.genes.DontMindRawFood &&
                pawn.genes.GenesListForReading.Where(g => g.def.GetModExtension<GeneModExtension_EnableBehavior>()?.canGraze ?? false).Any();
        }
        public static bool IsDendrovore(this Pawn pawn)
        {
            return pawn.genes != null &&
                pawn.genes.DontMindRawFood &&
                pawn.genes.GenesListForReading.Where(g => g.def.GetModExtension<GeneModExtension_EnableBehavior>()?.canEatTrees ?? false).Any();
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap))]
    static class FoodUtility_BestFoodSourceOnMap_Patch
    {
        private static bool FoodValidatorPredicate(Thing t, Pawn getter, Pawn eater)
        {
            if (t == null || !eater.WillEat(t, getter, true, false) || !t.IngestibleNow || t.Destroyed || t.IsForbidden(getter))
            {
                return false;
            }

            return !getter.roping.IsRoped || t.PositionHeld.InHorDistOf(getter.roping.RopedTo.Cell, 8f);
        }

        public static void Postfix(ref Thing __result, Pawn getter, Pawn eater, bool desperate, ref ThingDef foodDef, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false, bool allowSociallyImproper = false, bool allowHarvest = false, bool forceScanWholeMap = false, bool ignoreReservations = false, bool calculateWantedStackCount = false, FoodPreferability minPrefOverride = FoodPreferability.Undefined, float? minNutrition = null, bool allowVenerated = false)
        {
            if (__result != null) return;
            if (!(eater.CanGraze() && AnimalGenesModSettings.Settings.AllowGrazingBehavior) && !(eater.IsDendrovore() && AnimalGenesModSettings.Settings.AllowDendrovoreBehavior)) return;

            int maxRegionsToScan = 100;
            // All things, including plants
            ThingRequest thingRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodSource);

            Thing bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, 
                PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false, false, false, true),
                9999f, (Thing t) => FoodValidatorPredicate(t, getter, eater), null, 0, maxRegionsToScan, false, RegionType.Set_Passable, true, false);

            __result = bestThing;
            foodDef = bestThing?.def;
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
