using BigAndSmall;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace AnimalGenes
{
    public class GeneGenerator_Temperature
    {
        public static void AssignGenes(List<HumanlikeAnimal> sapientAnimals)
        {
            ThingDef baseliner = DefDatabase<ThingDef>.GetNamed("Human");
            float lowTempBase = baseliner.statBases.GetStatValueFromList(StatDefOf.ComfyTemperatureMin, 0.0f);
            float highTempBase = baseliner.statBases.GetStatValueFromList(StatDefOf.ComfyTemperatureMax, 0.0f);

            lowTempBase -= (4.0f + 3.6f); // Assume t-shirt and pants
            highTempBase += (1.8f + 1.44f); // Assume t-shirt and pants

            foreach (var sapientAnimal in sapientAnimals)
            {
                float animalLowTemp = sapientAnimal.animal.statBases.GetStatValueFromList(StatDefOf.ComfyTemperatureMin, lowTempBase);
                float lowTempDiff = animalLowTemp - lowTempBase;

                float animalHighTemp = sapientAnimal.animal.statBases.GetStatValueFromList(StatDefOf.ComfyTemperatureMax, highTempBase);
                float highTempDiff = animalHighTemp - highTempBase;

                // if animal already has gene with modifier, include that
                foreach (var geneDef in GeneGenerator.GetGenesForHumanLikeAnimal(sapientAnimal))
                {
                    if (geneDef.statOffsets != null)
                    {
                        foreach (var statOffset in geneDef.statOffsets)
                        {
                            if (statOffset.stat == StatDefOf.ComfyTemperatureMin)
                            {
                                lowTempDiff -= statOffset.value;
                            }
                            else if (statOffset.stat == StatDefOf.ComfyTemperatureMax)
                            {
                                highTempDiff -= statOffset.value;
                            }
                        }
                    }
                }

                string lowTempDiffGene = lowTempDiff switch
                {
                    <= -20.0f => "MinTemp_LargeDecrease",
                    <= -10.0f => "MinTemp_SmallDecrease",
                    >= 5.0f => "MinTemp_SmallIncrease",
                    _ => null
                };
                if (lowTempDiffGene != null)
                {
                    GeneDef lowTempGene = DefDatabase<GeneDef>.GetNamed(lowTempDiffGene, true);
                    Check.DebugLog($"Assigning low temp gene {lowTempGene.defName} to {sapientAnimal.animal.defName}, animal min temp {animalLowTemp} vs baseliner {lowTempBase}.");
                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, lowTempGene);

                    // We've now handled _most_ of the temp tolerance via genes, so adjust the base values to account for that
                    float adjustedByGene = lowTempGene.statOffsets.Find(x => x.stat == StatDefOf.ComfyTemperatureMin)?.value ?? 0.0f;
                    sapientAnimal.humanlikeThing.SetStatBaseValue(StatDefOf.ComfyTemperatureMin, animalLowTemp - adjustedByGene);
                }


                string highTempDiffGene = highTempDiff switch
                {
                    >= 20.0f => "MaxTemp_LargeIncrease",
                    >= 10.0f => "MaxTemp_SmallIncrease",
                    <= -5.0f => "MaxTemp_SmallDecrease",
                    _ => null
                };
                if (highTempDiffGene != null)
                {
                    GeneDef highTempGene = DefDatabase<GeneDef>.GetNamed(highTempDiffGene, true);
                    Check.DebugLog($"Assigning high temp gene {highTempGene.defName} to {sapientAnimal.animal.defName}, animal max temp {animalHighTemp} vs baseliner {highTempBase}.");
                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, highTempGene);

                    // We've now handled _most_ of the temp tolerance via genes, so adjust the base values to account for that
                    float adjustedByGene = highTempGene.statOffsets.Find(x => x.stat == StatDefOf.ComfyTemperatureMax)?.value ?? 0.0f;
                    sapientAnimal.humanlikeThing.SetStatBaseValue(StatDefOf.ComfyTemperatureMax, animalHighTemp - adjustedByGene);
                }
            }
        }
    }
}