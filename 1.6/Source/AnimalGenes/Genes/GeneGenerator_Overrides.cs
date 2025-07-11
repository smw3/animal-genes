using AnimalGenes.Defs;
using BigAndSmall;
using System;
using System.Collections.Generic;
using Verse;

namespace AnimalGenes
{
    public class GeneGenerator_Overrides
    {
        public static void AssignGenes(List<HumanlikeAnimal> sapientAnimals)
        {
            foreach (var sapientAnimal in sapientAnimals)
            {
                ThingDef leather = sapientAnimal.animal.race.leatherDef;
                foreach (var setting in AnimalGeneSettingsDef.AllSettings)
                {
                    // Overrides based on leather
                    if (leather != null && setting.GeneForLeatherDefs != null && setting.GeneForLeatherDefs.thingDefNames.Contains(leather.defName))
                    {
                        foreach (var geneDefName in setting.GeneForLeatherDefs.geneDefNames)
                        {
                            GeneDef geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(geneDefName);
                            // May reference genes of mods that aren't loaded, so just silently ignore those
                            if (geneDef != null) GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, geneDef);
                        }
                    }

                    // Overrides based on specific def
                    if (setting.GeneForAnimalDefs != null && setting.GeneForAnimalDefs.animalDefNames.Contains(sapientAnimal.animal.defName))
                    {
                        foreach (var geneDefName in setting.GeneForAnimalDefs.geneDefNames)
                        {
                            GeneDef geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(geneDefName);
                            // May reference genes of mods that aren't loaded, so just silently ignore those
                            if (geneDef != null) GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, geneDef);
                        }
                    }
                }
            }
        }
    }
}