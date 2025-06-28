using BigAndSmall;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnimalGenes
{
    public class GeneGenerator_BodySizes
    {
        public static void GenerateGenes(List<HumanlikeAnimal> sapientAnimals)
        {
            Dictionary<float, GeneDef> bodySizeGenes = [];
            foreach (var sapientAnimal in sapientAnimals)
            {
                float roundedBodySize = Mathf.Round(sapientAnimal.animal.race.baseBodySize * 5.0f) / 5.0f; // Round bodysize to nearest 0.2
                roundedBodySize = Mathf.Clamp(roundedBodySize, 0.2f, 5.0f); // Ensure it is within a reasonable range

                if (roundedBodySize == 1.0f) continue; // Skip size 1.0 as it is the default and doesn't need a gene

                if (!bodySizeGenes.ContainsKey(roundedBodySize)) {
                    string geneDefName = $"ANG_Animal_Size_{roundedBodySize}";
                    GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
                    if (newGene == null)
                    {
                        GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("ANG_Animal_Size_Template");
                        Check.NotNull(templateGene, "ANG_BodySize_Template gene template not found");
                        newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                        DefHelper.CopyGeneDefFields(templateGene, newGene);
                        newGene.defName = geneDefName;
                        newGene.label = $"x{roundedBodySize} body size";
                        newGene.generated = true;
                        newGene.statFactors = [
                            new StatModifier
                            {
                                stat = BSDefs.SM_BodySizeMultiplier,
                                value = roundedBodySize
                            }];

                        if (roundedBodySize < 1.0f)
                        {
                            newGene.description = "Carriers of this gene are noticably smaller.";
                            newGene.iconPath = "UI/Icons/Genes/Skills/Animals/Poor";
                        }
                        else
                        {
                            newGene.description = "Carriers of this gene are noticably larger.";
                        }

                        newGene.ResolveReferences();
                        DefDatabase<GeneDef>.Add(newGene);
                    }
                    bodySizeGenes[roundedBodySize] = newGene;
                    Check.DebugLog($"Generated body size genes for size {roundedBodySize}.");
                }

                GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, bodySizeGenes[roundedBodySize]);
            }
        }
    }
}