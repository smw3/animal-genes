using AnimalGenes.Defs;
using BigAndSmall;
using System;
using System.Collections.Generic;
using Verse;

namespace AnimalGenes
{
    public class Genegenerator_SkinColor
    {
        public static void AssignGenesByLeather(List<HumanlikeAnimal> sapientAnimals)
        {
            foreach (var sapientAnimal in sapientAnimals)
            {
                ThingDef leather = sapientAnimal.animal.race.leatherDef;
                if (leather == null) continue;
                if (leather.graphicData == null || leather.graphicData.color == null) continue;

                if (leather.label.Contains("fur")) // sometimes leather is fur, so we can assign hair color..
                {
                    GeneDef hairColorGene = ColorHelper.GeneDefForHairColor(leather.graphicData.color);
                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, hairColorGene);
                    Check.DebugLog($"Added hair color gene {hairColorGene.defName} for leather/fur color {leather.graphicData.color} to {sapientAnimal.animal.defName}");
                }

                Check.DebugLog($"Looking for good color gene for {sapientAnimal.animal.defName} with leather {leather.defName}.");
                GeneDef skinColorGene = ColorHelper.GeneDefForSkinColor(leather.graphicData.color);
                // If you ignore human skin tones, orange and pale yellow end up being very close to the default skin color, so we just skip those
                if (skinColorGene == null || skinColorGene.defName == "Skin_Orange" || skinColorGene.defName == "Skin_PaleYellow") { 
                    continue;
                }

                Check.DebugLog($"Assigning color gene {skinColorGene.defName} for {sapientAnimal.animal.defName}.");
                GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, skinColorGene);
            }
        }
    }
}