using AnimalGenes.GeneModExtensions;
using BigAndSmall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes.Helpers
{
    public static class ExtensionMethods
    {
        public static bool CanGraze(this Pawn pawn)
        {
            return pawn.genes != null &&
                pawn.genes.DontMindRawFood &&
                pawn.genes.GenesListForReading.Where(g => g.def.GetModExtension<EnableBehavior>()?.canGraze ?? false).Any();
        }
        public static bool IsDendrovore(this Pawn pawn)
        {
            return pawn.genes != null &&
                pawn.genes.DontMindRawFood &&
                pawn.genes.GenesListForReading.Where(g => g.def.GetModExtension<EnableBehavior>()?.canEatTrees ?? false).Any();
        }
        public static bool IsPredator(this Pawn pawn)
        {
            return pawn.genes != null &&
                pawn.genes.DontMindRawFood &&
                pawn.genes.GenesListForReading.Where(g => g.def.GetModExtension<EnableBehavior>()?.isPredator ?? false).Any();
        }

        public static void SwapToSapientAnimal(this Pawn humanPawn, HumanlikeAnimal humanlikeAnimal)
        {
            RaceMorpher.SwapPawnToSapientAnimal(humanPawn, humanlikeAnimal);
        }
    }
}
