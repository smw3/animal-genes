﻿using AnimalGenes.GeneModExtensions;
using AnimalGenes.Genes;
using AnimalGenes.Helpers;
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
        public static List<HumanlikeAnimal> SapientAnimals => [.. HumanlikeAnimalGenerator.humanlikeAnimals.Values];
        public static Dictionary<HumanlikeAnimal, List<GeneDef>> humanLikeGenes = [];
        public static Dictionary<GeneDef, HumanlikeAnimal> affinityGenes = [];

        public static void AddGeneToHumanLikeAnimal(HumanlikeAnimal animal, GeneDef geneDef)
        {
            if (!humanLikeGenes.TryGetValue(animal, out var geneList))
            {
                geneList = [];
                humanLikeGenes[animal] = geneList;
            }
            if (!geneList.Contains(geneDef))
            {
                geneList.Add(geneDef);
            }
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
            GeneGenerator_Wings.GenerateGenes(SapientAnimals);

            // Assign existing genes
            Genegenerator_SkinColor.AssignGenesByLeather(SapientAnimals);
            GeneGenerator_Diet.AssignGenes(SapientAnimals);
            GeneGenerator_Speed.AssignGenes(SapientAnimals);
            GeneGenerator_Temperature.AssignGenes(SapientAnimals);
            GeneGenerator_Health.AssignGenes(SapientAnimals);

            GeneGenerator_Overrides.AssignGenes(SapientAnimals);

            // Must be done last, after all other genes have been generated
            GenerateAffinityGenes();

            RemoveStatDefsThatHaveGenesNow();
        }

        private static void SetBaseToDefault(HumanlikeAnimal sapientAnimal, StatDef statDef)
        {
            ThingDef baseliner = DefDatabase<ThingDef>.GetNamed("Human");
            sapientAnimal.humanlikeThing.SetStatBaseValue(statDef, baseliner.statBases.GetStatValueFromList(statDef, 0.0f));
        }

        private static void RemoveStatDefsThatHaveGenesNow()
        {
            foreach (var sapientAnimal in SapientAnimals)
            {
                // set body size to default, as it is now handled by genes
                sapientAnimal.humanlikeThing.SetStatBaseValue(BSDefs.SM_BodySizeOffset, 1.0f - sapientAnimal.humanlikeThing.race.baseBodySize);

                SetBaseToDefault(sapientAnimal, StatDefOf.MoveSpeed);
                sapientAnimal.humanlikeThing.SetStatBaseValue(StatDefOf.CarryingCapacity, 75.0f);
                SetBaseToDefault(sapientAnimal, StatDefOf.ToxicEnvironmentResistance);
                SetBaseToDefault(sapientAnimal, StatDefOf.ToxicResistance);
                SetBaseToDefault(sapientAnimal, StatDefOf.ComfyTemperatureMin);
                SetBaseToDefault(sapientAnimal, StatDefOf.ComfyTemperatureMax);
                // I thought I might have to remove natural armor here, but they don't get any in the first place.. still, just in case
                SetBaseToDefault(sapientAnimal, StatDefOf.ArmorRating_Sharp);
                SetBaseToDefault(sapientAnimal, StatDefOf.ArmorRating_Blunt);
                SetBaseToDefault(sapientAnimal, StatDefOf.ArmorRating_Heat);
                SetBaseToDefault(sapientAnimal, StatDefOf.MaxHitPoints);
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

                Helpers.GeneTemplate template = DefDatabase<Helpers.GeneTemplate>.GetNamed("ANG_Animal_Affinity_Template");
                string geneDefName = template.GenerateDefName(sapientAnimal.animal);

                GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
                if (newGene == null)
                {
                    newGene = GeneDefFromTemplate.GenerateGeneDef(template, sapientAnimal.animal, []);
                    Check.NotNull(newGene, "Failed to create affinity gene from template");
                    Check.NotNull(newGene, "Failed to create new GeneDef instance for affinity gene");
                    Check.NotNull(sapientAnimal.animal.label, sapientAnimal.animal + " label is null");

                    Check.DebugLog($"Generating affinity gene {newGene.defName} for {sapientAnimal.animal.defName} with {requiredGenes.Count} required genes.");
                    newGene.modExtensions =
                    [
                        new GenePrerequisites
                        {
                            prerequisiteSets = [
                                new PrerequisiteSet
                                {
                                    prerequisites = [.. requiredGenes.Select(g => g.defName)],
                                    type = PrerequisiteSet.PrerequisiteType.AllOf
                                }
                            ]
                        },
                        new PawnExtension // Effectively the same as the "early maturity" gene
                        {
                            babyStartAge = 3,
                            sizeByAgeMult = new SimpleCurve([ new CurvePoint(3, 0.6f), new CurvePoint(13, 1) ])
                        },
                        new TargetAffinity
                        {
                            targetAnimal = sapientAnimal
                        }
                    ];
                    newGene.biostatMet = -requiredGenes.Select(g => g.biostatMet).Sum();
                    newGene.modExtensions.Add(IconHelper.GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f)]));

                    newGene.ResolveReferences();
                    DefDatabase<GeneDef>.Add(newGene);
                }

                affinityGenes[newGene] = sapientAnimal;
            }
        }
    }
}
