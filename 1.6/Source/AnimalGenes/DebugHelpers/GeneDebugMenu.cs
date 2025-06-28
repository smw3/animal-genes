using BigAndSmall;
using HarmonyLib;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace AnimalGenes.Debugging
{

    [HarmonyPatch]
    public static class DebugUIPatches
    {
        [HarmonyPatch(typeof(GeneUIUtility), "DoDebugButton")]
        [HarmonyPostfix]
        public static void DoDebugButton_Postfix(ref Rect buttonRect, Thing target, GeneSet genesOverride)
        {
            DoGeneDebugButton(ref buttonRect, target);
        }

        public static void DoGeneDebugButton(ref Rect buttonRect, Thing target, string title = "Animal Genes")
        {
            if (target is not Pawn)
            {
                return;
            }
            Pawn pawn = target as Pawn;
            float widthOfPrevious = buttonRect.size.x;
            buttonRect = new Rect(buttonRect.x - widthOfPrevious - 10, buttonRect.y, buttonRect.width, buttonRect.height);
            if (!Widgets.ButtonText(buttonRect, title))
            {
                return;
            }

            List<FloatMenuOption> list =
            [
                new FloatMenuOption("Add only prereqs...", delegate
                {
                    List<DebugMenuOption> list = [];
                    foreach (var def in DefDatabase<GeneDef>.AllDefs.Where(x => x.defName.StartsWith("ANG_") && x.defName.EndsWith("Affinity")))
                    {
                        list.Add(new DebugMenuOption($"{def.defName,-0}\t ({def.LabelCap})", DebugMenuOptionMode.Action, delegate
                        {
                            List<string> preReqDefNames = def.GetModExtension<GenePrerequisites>().prerequisiteSets.Where(ps => ps.type == PrerequisiteSet.PrerequisiteType.AllOf).First().prerequisites;
                            foreach (var preReqDefName in preReqDefNames)
                            {
                                GeneDef preReqGene = DefDatabase<GeneDef>.GetNamed(preReqDefName, true);
                                pawn.genes.AddGene(preReqGene, true);
                            }
                        }));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
                }),
                new FloatMenuOption("Add affinity and prereqs...", delegate
                {
                    List<DebugMenuOption> list = [];
                    foreach (var def in DefDatabase<GeneDef>.AllDefs.Where(x => x.defName.StartsWith("ANG_") && x.defName.EndsWith("Affinity")))
                    {
                        list.Add(new DebugMenuOption($"{def.defName,-0}\t ({def.LabelCap})", DebugMenuOptionMode.Action, delegate
                        {
                            List<string> preReqDefNames = def.GetModExtension<GenePrerequisites>().prerequisiteSets.Where(ps => ps.type == PrerequisiteSet.PrerequisiteType.AllOf).First().prerequisites;
                            foreach (var preReqDefName in preReqDefNames)
                            {
                                GeneDef preReqGene = DefDatabase<GeneDef>.GetNamed(preReqDefName, true);
                                pawn.genes.AddGene(preReqGene, true);
                            }
                            pawn.genes.AddGene(def, true);
                        }));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
                }),
            ];
            Find.WindowStack.Add(new FloatMenu(list));
        }
    }
}
