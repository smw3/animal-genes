using BigAndSmall;
using System;
using System.Collections.Generic;
using Verse;

namespace AnimalGenes
{
    public class Genegenerator_SkinColor
    {
        public static void AssignGenes(List<HumanlikeAnimal> sapientAnimals)
        {
            foreach (var sapientAnimal in sapientAnimals)
            {
                ThingDef leather = sapientAnimal.animal.race.leatherDef;
                if (leather == null || leather.graphicData == null || leather.graphicData.color == null)
                {
                    Check.DebugLog($"{sapientAnimal.animal.defName} has no valid leather definition or color.");
                    continue;
                }
                GeneDef skinColorGene = ColorHelper.GeneDefForSkinColor(leather.graphicData.color);
                GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, skinColorGene);
            }
        }
    }
}