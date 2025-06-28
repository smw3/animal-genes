using AnimalGenes.Defs;
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
                if (leather == null)
                {
                    continue;
                }

                foreach (var setting in AnimalGeneSettingsDef.AllSettings) {
                    if (setting.GeneForLeatherDefs.thingDefNames.Contains(leather.defName))
                    {
                        foreach (var geneDefName in setting.GeneForLeatherDefs.geneDefNames)
                        {
                            GeneDef geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(geneDefName);
                            if (geneDef != null)
                            {   
                                // May reference genes of mods that aren't loaded, so just silently ignore those
                                GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, geneDef);
                            }
                        }
                    }
                }

                if (leather.graphicData == null || leather.graphicData.color == null)
                {
                    Check.DebugLog($"{sapientAnimal.animal.defName} has no valid leather definition or color.");
                    continue;
                }
                Check.DebugLog($"Looking for good color gene for {sapientAnimal.animal.defName} with leather {leather.defName}.");
                GeneDef skinColorGene = ColorHelper.GeneDefForSkinColor(leather.graphicData.color);
                // If you ignore human skin tones, orange and pale yellow end up being very close to the default skin color
                if (skinColorGene == null || skinColorGene.defName == "Skin_Orange" || skinColorGene.defName == "Skin_PaleYellow") { 
                    continue;
                }
                Check.DebugLog($"Assigning color gene {skinColorGene.defName} for {sapientAnimal.animal.defName}.");
                GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, skinColorGene);
            }
        }
    }
}