using BigAndSmall;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace AnimalGenes
{
    public class GeneGenerator_Health
    {
        public static void AssignGenes(List<HumanlikeAnimal> sapientAnimals)
        {
            foreach (var sapientAnimal in sapientAnimals)
            {
                float toxEnvRes = sapientAnimal.animal.GetStatValueAbstract(StatDefOf.ToxicEnvironmentResistance);
                if (toxEnvRes >= 1.0f)
                {
                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, DefDatabase<GeneDef>.GetNamed("ToxicEnvironmentResistance_Total"));
                }
                else if (toxEnvRes >= 0.5f)
                {
                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, DefDatabase<GeneDef>.GetNamed("ToxicEnvironmentResistance_Partial"));
                }

                float toxRes = sapientAnimal.animal.GetStatValueAbstract(StatDefOf.ToxicResistance);
                if (toxRes >= 1.0f)
                {
                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, DefDatabase<GeneDef>.GetNamed("ToxResist_Total"));
                }
                else if (toxRes >= 0.5f)
                {
                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, DefDatabase<GeneDef>.GetNamed("ToxResist_Partial"));
                }
            }
        }
    }
}