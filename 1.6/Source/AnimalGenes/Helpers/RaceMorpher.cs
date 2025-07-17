using BigAndSmall;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace AnimalGenes
{
    public static class RaceMorpher
    {
        public static void SwapPawnToSapientAnimal(Pawn humanPawn, HumanlikeAnimal humanlikeAnimal)
        {
            // Empty inventory
            if (humanPawn.inventory != null && humanPawn.inventory?.innerContainer != null)
            {
                humanPawn.inventory.DropAllNearPawn(humanPawn.Position);
            }
            humanPawn.equipment?.DropAllEquipment(humanPawn.Position);
            humanPawn.apparel?.DropAll(humanPawn.Position);

            var targetDef = humanlikeAnimal.humanlikeThing;
            BigAndSmall.RaceMorpher.SwapThingDef(humanPawn, targetDef, true, 9001, force: true, permitFusion: false, clearHediffsToReapply: false);

            // Turn all affinity prerequisite genes into endogenes
            string geneDefName = $"ANG_{humanlikeAnimal.animal.defName}_Affinity";
            GeneDef affinityGeneDef = DefDatabase<GeneDef>.GetNamed(geneDefName);

            humanPawn.genes.SetXenotypeDirect(DefDatabase<XenotypeDef>.GetNamed("SapientAnimal"));

            if (humanPawn.genes.HasXenogene(affinityGeneDef) || humanPawn.genes.HasEndogene(affinityGeneDef))
            {
                List<string> preReqs = affinityGeneDef.GetModExtension<BigAndSmall.GenePrerequisites>()?.prerequisiteSets
                    .Where(ps => ps.type == BigAndSmall.PrerequisiteSet.PrerequisiteType.AllOf)
                    .SelectMany(ps => ps.prerequisites).ToList();
                preReqs.Add(affinityGeneDef.defName);

                RemoveExistingEndogenes(humanPawn, preReqs);
                Endogenify(preReqs, humanPawn);
            }

            // Remove any other affinity genes that might be present
            RemoveAffinityGenes(humanPawn, affinityGeneDef.defName);
        }

        public static void AddAppropriateAffinityGenes(Pawn pawn)
        {
            HumanlikeAnimalGenerator.humanlikeAnimals.TryGetValue(pawn.def, out HumanlikeAnimal humanlikeAnimal);
            if (humanlikeAnimal != null)
            {
                pawn.genes.SetXenotypeDirect(DefDatabase<XenotypeDef>.GetNamed("SapientAnimal"));

                GeneDef affinityGeneDef = GeneGenerator.affinityGenes.Where(kvp => kvp.Value == humanlikeAnimal).First().Key;

                List<string> preReqDefNames = affinityGeneDef.GetModExtension<GenePrerequisites>().prerequisiteSets.Where(ps => ps.type == PrerequisiteSet.PrerequisiteType.AllOf).First().prerequisites;
                foreach (var preReqDefName in preReqDefNames)
                {
                    GeneDef preReqGene = DefDatabase<GeneDef>.GetNamed(preReqDefName, true);
                    pawn.genes.AddGene(preReqGene, false);
                }
                pawn.genes.AddGene(affinityGeneDef, false);
            } else
            {
                Check.DebugLog($"AddAppropriateAffinityGenes: No humanlike animal found for pawn {pawn.Name} with def {pawn.def.defName}. Cannot add affinity genes.");
                Check.DebugLog($"Available humanlike animals: {string.Join(", ", HumanlikeAnimalGenerator.humanlikeAnimals.Keys.Select(h => h.defName))}");
            }
        }

        public static void RemoveExistingEndogenes(Pawn pawn, List<string> preReqs)
        {
            List<Gene> genesToRemove = [.. pawn.genes.Endogenes.Where(g => !preReqs.Contains(g.def.defName))];
            foreach (var gene in genesToRemove)
            {
                Check.DebugLog($"Removing existing endogene {gene.def.defName} from pawn {pawn.Name}");
                pawn.genes.RemoveGene(gene);
            }
        }

        public static void RemoveAffinityGenes(Pawn pawn, string ExceptDefName)
        {
            List<Gene> affinityGenes = [.. pawn.genes.GenesListForReading
                .Where(g => GeneGenerator.affinityGenes.ContainsKey(g.def) && g.def.defName != ExceptDefName)
                ];
            Check.DebugLog($"Removing extra affinity genes for pawn {pawn.Name}: {string.Join(", ", affinityGenes.Select(g => g.def.defName))}");
            foreach (var gene in affinityGenes)
            {
                Check.DebugLog($"Removing gene {gene.def.defName} for pawn {pawn.Name}");
                pawn.genes.RemoveGene(gene);
            }
        }

        public static void Endogenify(List<string> geneDefNames, Pawn pawn)
        {
            List<Gene> genesToEndogenify = [.. pawn.genes.Xenogenes.Where(g => geneDefNames.Contains(g.def.defName))];
            Check.DebugLog($"Endogenifying genes for pawn {pawn.Name}: {string.Join(", ", genesToEndogenify.Select(g => g.def.defName))}");
            foreach (var gene in genesToEndogenify)
            {
                GeneDef geneDef = gene.def;
                if (!pawn.genes.HasEndogene(geneDef))
                {
                    Check.DebugLog($"Removing gene {gene.def.defName} for pawn {pawn.Name}");
                    pawn.genes.RemoveGene(gene);
                    Check.DebugLog($"Adding gene {geneDef.defName} as endogene for pawn {pawn.Name}");
                    pawn.genes.AddGene(geneDef, false);
                } else
                {
                    Check.DebugLog($"Gene {geneDef.defName} is already an endogene for pawn {pawn.Name}, skipping.");
                }
            }
        }
    }
}
