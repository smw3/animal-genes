using BigAndSmall;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnimalGenes
{
    public class GeneGenerator_Speed
    {
        public static void AssignGenes(List<HumanlikeAnimal> sapientAnimals)
        {
            ThingDef baseliner = DefDatabase<ThingDef>.GetNamed("Human");
            float baseSpeed = baseliner.statBases.GetStatValueFromList(StatDefOf.MoveSpeed, 0.0f);

            // Genes depending on what the animal eats
            foreach (var sapientAnimal in sapientAnimals)
            {
                float moveSpeedFactor = sapientAnimal.animal.statBases.GetStatValueFromList(StatDefOf.MoveSpeed, 0.0f) / baseSpeed;
                bool BSGenesActive = ModsConfig.IsActive("RedMattis.BigSmall.Core");
                string MoveSpeedGeneDefName =
                    moveSpeedFactor switch
                    {
                        <= 0.6f when BSGenesActive => "BS_Very_Slow",
                        <= 0.8f => "MoveSpeed_Slow",
                        >= 1.4f => "MoveSpeed_VeryQuick",
                        >= 1.2f => "MoveSpeed_Quick",
                        _ => null
                    };

                if (MoveSpeedGeneDefName == null) continue;

                GeneDef moveSpeedGene = DefDatabase<GeneDef>.GetNamed(MoveSpeedGeneDefName, true);
                Check.DebugLog($"Assigning move speed gene {moveSpeedGene.defName} to {sapientAnimal.animal.defName} with factor {moveSpeedFactor}.");
                GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, moveSpeedGene);
            }
        }
    }
}