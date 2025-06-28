using BigAndSmall;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AnimalGenes
{
    public class GeneGenerator_Diet
    {
        private static readonly List<FoodTypeFlags> flagsThatRequireRobustStomach =
        [
            FoodTypeFlags.Corpse,
            FoodTypeFlags.VegetarianRoughAnimal,
            FoodTypeFlags.CarnivoreAnimalStrict,
            FoodTypeFlags.CarnivoreAnimal,
            FoodTypeFlags.OmnivoreRoughAnimal,
            FoodTypeFlags.OvivoreAnimal
        ];
        public static void AssignGenes(List<HumanlikeAnimal> sapientAnimals)
        {
            // Genes depending on what the animal eats
            foreach (var sapientAnimal in sapientAnimals)
            {
                FoodTypeFlags foodType = sapientAnimal.animal.race.foodType;
                bool requiresRobustStomach = flagsThatRequireRobustStomach.Where(f => foodType.HasFlag(f)).Any();

                if (requiresRobustStomach)
                {
                    GeneDef robustDigestion = DefDatabase<GeneDef>.GetNamed("RobustDigestion");
                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, robustDigestion);

                    GeneDef strongStomach = DefDatabase<GeneDef>.GetNamed("StrongStomach");
                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, strongStomach);
                }

                // Diet genes if B&S Genes is loaded
                if (ModsConfig.IsActive("RedMattis.BigSmall.Core"))
                {
                    if (foodType.HasFlag(FoodTypeFlags.VegetarianRoughAnimal) || foodType.HasFlag(FoodTypeFlags.VegetarianAnimal))
                    {
                        GeneDef herbivoreGene = DefDatabase<GeneDef>.GetNamed("BS_Diet_Herbivore");
                        GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, herbivoreGene);
                    }

                    if (foodType.HasFlag(FoodTypeFlags.CarnivoreAnimal) || foodType.HasFlag(FoodTypeFlags.CarnivoreAnimalStrict))
                    {
                        GeneDef herbivoreGene = DefDatabase<GeneDef>.GetNamed("BS_Diet_Carnivore");
                        GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, herbivoreGene);
                    }
                }

                if (foodType.HasFlag(FoodTypeFlags.Plant)) {
                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, ANGDefs.GrazingGene);
                }

                if (foodType.HasFlag(FoodTypeFlags.Tree))
                {
                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, ANGDefs.DendrovoreGene);
                }

                if (sapientAnimal.animal.race.predator)
                {
                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, ANGDefs.PredatorGene);
                }
            }                    
        }
    }
}