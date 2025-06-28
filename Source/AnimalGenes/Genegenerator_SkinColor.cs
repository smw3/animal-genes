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
                // If you ignore human skin tones, orange and pale yellow end up being very close to the default skin color
                if (skinColorGene.defName == "Skin_Orange" || skinColorGene.defName == "Skin_PaleYellow") { 
                    continue;
                }
                GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, skinColorGene);
            }
        }
    }
}