using BigAndSmall;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AnimalGenes
{
    public static class GeneGenerator
    {
        public static List<HumanlikeAnimal> SapientAnimals => HumanlikeAnimalGenerator.humanlikeAnimals.Values.ToList();
        private static Dictionary<HumanlikeAnimal, List<GeneDef>> humanLikeGenes = new Dictionary<HumanlikeAnimal, List<GeneDef>>();
        public static Dictionary<GeneDef, HumanlikeAnimal> affinityGenes = new Dictionary<GeneDef, HumanlikeAnimal>();

        public static void AddGeneToHumanLikeAnimal(HumanlikeAnimal animal, GeneDef geneDef)
        {
            if (!humanLikeGenes.TryGetValue(animal, out var geneList))
            {
                geneList = [];
                humanLikeGenes[animal] = geneList;
            }
            geneList.Add(geneDef);
        }

        public static List<GeneDef> GetGenesForHumanLikeAnimal(HumanlikeAnimal animal)
        {
            if (humanLikeGenes.TryGetValue(animal, out var genes))
            {
                return genes;
            }
            return [];
        }

        public static void GenerateGenes()
        {
            GeneGenerator_ProductionComps.GenerateGenes(SapientAnimals);
            GeneGenerator_Armor.GenerateGenes(SapientAnimals);
            GeneGenerator_Tools.GenerateGenes(SapientAnimals);
            GeneGenerator_BodyTypes.GenerateGenes(SapientAnimals);
            GeneGenerator_BodySizes.GenerateGenes(SapientAnimals);  

            // Assign existing genes
            Genegenerator_SkinColor.AssignGenes(SapientAnimals);
            GeneGenerator_Diet.AssignGenes(SapientAnimals);
            GeneGenerator_Speed.AssignGenes(SapientAnimals);
            GeneGenerator_Temperature.AssignGenes(SapientAnimals);
            GeneGenerator_Health.AssignGenes(SapientAnimals);

            // Must be done last, after all other genes have been generated
            GenerateAffinityGenes();

            RemoveStatDefsThatHaveGenesNow();
        }
        private static void RemoveStatDefsThatHaveGenesNow()
        {
            ThingDef baseliner = DefDatabase<ThingDef>.GetNamed("Human");

            void setBaseToDefault(HumanlikeAnimal sapientAnimal, StatDef statDef)
            {
                sapientAnimal.humanlikeThing.statBases.GetStatValueFromList(statDef, baseliner.statBases.GetStatValueFromList(statDef,0.0f));
            }

            foreach (var sapientAnimal in SapientAnimals)
            {
                // I thought I might have to remove natural armor here, but they don't get any in the first place..

                sapientAnimal.humanlike.race.baseBodySize = 1.0f; // Reset body size to default, as it is now handled by genes
                setBaseToDefault(sapientAnimal, StatDefOf.MoveSpeed);
                setBaseToDefault(sapientAnimal, StatDefOf.CarryingCapacity);
            }
        }

        private static void GenerateAffinityGenes()
        {
            foreach (var sapientAnimal in SapientAnimals)
            {
                humanLikeGenes.TryGetValue(sapientAnimal, out var requiredGenes);
                if (requiredGenes.NullOrEmpty())
                {
                    Log.Error("Failed to find any genes for sapient animal " + sapientAnimal.animal.defName + ", cannot generate affinity gene.");
                    continue;
                }
                if (requiredGenes.Count == 1)
                {
                    Check.DebugLog($"Sapient animal {sapientAnimal.animal.defName} has only one gene, that's quite few.");
                }

                string geneDefName = $"ANG_{sapientAnimal.animal.defName}_Affinity";
                GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
                if (newGene == null)
                {
                    GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("ANG_Animal_Affinity_Template");
                    Check.NotNull(templateGene, "ANG_Animal_Affinity_Template gene template not found");

                    newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                    DefHelper.CopyGeneDefFields(templateGene, newGene);

                    newGene.defName = geneDefName;
                    Check.NotNull(newGene, "Failed to create new GeneDef instance for affinity gene");
                    Check.NotNull(sapientAnimal.animal.label, sapientAnimal.animal + " label is null");
                    newGene.label = $"{sapientAnimal.animal.label.CapitalizeFirst()} Affinity";
                    newGene.generated = true;

                    Check.DebugLog($"Generating affinity gene {newGene.defName} for {sapientAnimal.animal.defName} with {requiredGenes.Count} required genes.");
                    newGene.modExtensions =
                    [
                        new BigAndSmall.GenePrerequisites
                        {
                            prerequisiteSets = [
                                new BigAndSmall.PrerequisiteSet
                                {
                                    prerequisites = [.. requiredGenes.Select(g => g.defName)],
                                    type = PrerequisiteSet.PrerequisiteType.AllOf
                                }
                            ]
                        },
                        new GeneModExtension_TargetAffinity
                        {
                            targetAnimal = sapientAnimal
                        }
                    ];

                    newGene.modExtensions.Add(IconHelper.GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f)]));

                    newGene.ResolveReferences();
                    DefDatabase<GeneDef>.Add(newGene);
                }

                affinityGenes[newGene] = sapientAnimal;
            }
        }
    }
}
