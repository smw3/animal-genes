using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AnimalGenes
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("ingendum.animalgenes");
            harmony.PatchAll();
        }
    }

    public static class PawnRenderNodeExtensions
    {
        public static bool RenderWhileAnimalGeneActive(this PawnRenderNodeWorker nodeWorker)
        {
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker), nameof(PawnRenderNodeWorker.CanDrawNow))]
    class Patch01
    {

        static bool Postfix(bool result, PawnRenderNode node, PawnDrawParms parms)
        {
            if (!result) return false;
            if (node == node.tree.rootNode) {
                return result;
            }

            if (node == null) throw new ArgumentNullException(nameof(node));

            if (node.GetType() != typeof(PawnRenderNode_ReplaceAsAnimal))
            {
                Pawn pawn = parms.pawn;
                if (pawn == null || pawn.genes == null) return result;

                if (pawn.genes.Xenogenes.Where((Gene g) => g.Label == "animal kind").Any())
                {
                    return false;
                }
                if (pawn.genes.Endogenes.Where((Gene g) => g.Label == "animal kind").Any())
                {
                    return false;
                }
            }

            return result;
        }
    }
}
