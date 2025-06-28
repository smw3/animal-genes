using AnimalGenes.Helpers;
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
                    Helpers.GeneTemplate template = DefDatabase<Helpers.GeneTemplate>.GetNamed("ANG_BodySize_Template");
                    string geneDefName = $"ANG_Animal_Size_{roundedBodySize}";

                    GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
                    if (newGene == null)
                    {
                        string sizeLabel = roundedBodySize < 1.0f ? "BodySizeSmaller".Translate() : "BodySizeLarger".Translate();
                        newGene = GeneDefFromTemplate.GenerateGeneDef(template, null, [sizeLabel]);
                        newGene.defName = geneDefName;
                        newGene.label = "BodySizeLabel".Translate().Replace("{0}", roundedBodySize.ToString());

                        newGene.statFactors = [
                            new StatModifier
                            {
                                stat = BSDefs.SM_BodySizeMultiplier,
                                value = roundedBodySize
                            }];

                        if (roundedBodySize < 1.0f)
                        {
                            newGene.iconPath = "UI/Icons/Genes/Skills/Animals/Poor";
                        }

                        float metMult = roundedBodySize < 1.0f ? 1.0f / roundedBodySize : roundedBodySize;
                        newGene.biostatCpx = 2;
                        newGene.biostatMet = -(int)Math.Abs(metMult);

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