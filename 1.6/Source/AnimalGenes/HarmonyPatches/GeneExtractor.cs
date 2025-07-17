using AnimalGenes.Helpers;
using BigAndSmall;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static RimWorld.PsychicRitualRoleDef;

namespace AnimalGenes.HarmonyPatches
{

    [HarmonyPatch(typeof(Building_GeneExtractor), nameof(Building_GeneExtractor.CanAcceptPawn))]
    static class Gene_CanAcceptPawn_Patch
    {
        public static void Postfix(ref AcceptanceReport __result, Building_GeneExtractor __instance, Pawn pawn)
        {
            if (pawn.IsAnimalOfColony() && AnimalHelper.HumanlikeSourceFor(pawn) != null)
            {
                if (!__instance.PowerOn)
                {
                    __result = "NoPower".Translate().CapitalizeFirst();
                }
                if (__instance.innerContainer.Count > 0)
                {
                    __result = "Occupied".Translate();
                }
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Building_GeneExtractor), "Finish")]
    static class Gene_Finish_Patch
    {
        private static Lazy<MethodInfo> _ContainedPawn = new(() => AccessTools.Method(typeof(Building_GeneExtractor), "get_ContainedPawn"));
        public static Pawn ContainedPawn(Building_GeneExtractor ge)
        {
            return (Pawn)_ContainedPawn.Value.Invoke(ge, new object[] { });
        }

        private static Lazy<FieldInfo> _StartTick = new(() => AccessTools.Field(typeof(Building_GeneExtractor), "startTick"));
        public static int StartTick(Building_GeneExtractor ge)
        {
            return (int)_StartTick.Value.GetValue(ge);
        }
        public static void SetStartTick(Building_GeneExtractor ge, int val)
        {
            _StartTick.Value.SetValue(ge, val);
        }

        private static Lazy<FieldInfo> _SelectedPawn = new(() => AccessTools.Field(typeof(Building_GeneExtractor), "selectedPawn"));
        private static Lazy<FieldInfo> _SustainerWorking = new(() => AccessTools.Field(typeof(Building_GeneExtractor), "sustainerWorking"));
        private static Lazy<FieldInfo> _PowerCutTicks = new(() => AccessTools.Field(typeof(Building_GeneExtractor), "powerCutTicks"));

        public static bool Prefix(Building_GeneExtractor __instance)
        {
            Pawn pawn = ContainedPawn(__instance);
            if (pawn != null && !pawn.RaceProps.Humanlike)
            {
                HumanlikeAnimal hla = HumanlikeAnimals.GetHumanlikeAnimalFor(pawn.def);
                if (hla == null) return true;

                _SelectedPawn.Value.SetValue(__instance, null);
                _SustainerWorking.Value.SetValue(__instance, null);
                _PowerCutTicks.Value.SetValue(__instance, 0);

                Rand.PushState(pawn.thingIDNumber ^ StartTick(__instance));
                Genepack genepack = AnimalHelper.GenerateGenepackFor(hla);

                IntVec3 intVec = (__instance.def.hasInteractionCell ? __instance.InteractionCell : __instance.Position);
                __instance.innerContainer.TryDropAll(intVec, __instance.Map, ThingPlaceMode.Near, null, null, true);

                if (genepack != null)
				{
                    GenPlace.TryPlaceThing(genepack, intVec, __instance.Map, ThingPlaceMode.Near, null, null, null, 1);
                }

                int ticksForRegrowth = Mathf.RoundToInt(60000f * GeneTuning.GeneExtractorRegrowingDurationDaysRange.RandomInRange);
                pawn.health.AddHediff(HediffDefOf.XenogermLossShock, null, null, null);
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.XenogermReplicating, pawn, null);
                hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = ticksForRegrowth;
                pawn.health.AddHediff(hediff, null, null, null);

                Rand.PopState();
                SetStartTick(__instance, -1);

                return false;
            }
            return true;
        }
    }

    [HarmonyPatch]
    public static class Building_GeneExtractor_GetGizmos_Patch
    {
        static MethodBase TargetMethod()
        {
            return typeof(Building_GeneExtractor).GetMethod("<GetGizmos>b__38_3", BindingFlags.Instance | BindingFlags.NonPublic);

        }

        public static bool AllowSelectionForGeneExtractor(Pawn pawn)
        {
            bool pawnOkForGenes = HumanlikeAnimals.GetHumanlikeAnimalFor(pawn.def) != null;
            return pawnOkForGenes;
        }

        public static string PawnLabel(Pawn pawn)
        {
            return pawn.LabelShortCap;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);
            var myCondition = AccessTools.Method(typeof(Building_GeneExtractor_GetGizmos_Patch), nameof(AllowSelectionForGeneExtractor));
            var pawnLabel = AccessTools.Method(typeof(Building_GeneExtractor_GetGizmos_Patch), nameof(PawnLabel));

            var get_LabelShortCap = AccessTools.Method(typeof(Entity), "get_LabelShortCap");

            bool ifReplacementDone = false;
            bool labelReplacementDone = false;

            Label continueLabel = il.DefineLabel();
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldloc_3 &&
                    codes[i + 1].opcode == OpCodes.Ldfld &&
                    codes[i + 2].opcode == OpCodes.Ldfld &&
                    codes[i + 3].opcode == OpCodes.Brfalse &&
                    !ifReplacementDone)
                {
                    yield return codes[i]; // this
                    yield return codes[i + 1]; // pawn
                    yield return codes[i + 2]; // genes
                    yield return new CodeInstruction(OpCodes.Brtrue, continueLabel);

                    yield return codes[i];     // this
                    yield return codes[i + 1]; // pawn
                    yield return new CodeInstruction(OpCodes.Call, myCondition);
                    yield return new CodeInstruction(OpCodes.Brtrue, continueLabel);

                    yield return new CodeInstruction(OpCodes.Br, codes[i + 3].operand);

                    var continueInstr = new CodeInstruction(OpCodes.Nop);
                    continueInstr.labels.Add(continueLabel);
                    yield return continueInstr;

                    i += 3;
                    ifReplacementDone = true;
                } else if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand as MethodInfo == get_LabelShortCap && !labelReplacementDone) {
                    yield return new CodeInstruction(OpCodes.Call, pawnLabel);

                    i += 6;
                    labelReplacementDone = true;
                } else {
                    yield return codes[i];
                }
            }
        }
    }
}
