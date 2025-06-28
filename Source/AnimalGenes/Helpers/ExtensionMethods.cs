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

        private static readonly Dictionary<ThingDef, Boolean> IsSapientAnimalCache = [];
        public static bool IsSapientAnimal(this Pawn pawn)
        {
            if (IsSapientAnimalCache.TryGetValue(pawn.def, out bool isSapientAnimal)) {
                return isSapientAnimal;
            }
            isSapientAnimal = HumanlikeAnimalGenerator.humanlikeAnimals.Values.Select(h => h.humanlikeThing).Contains(pawn.def);
            IsSapientAnimalCache.Add(pawn.def, isSapientAnimal);
            return isSapientAnimal;
        }
    }
}
