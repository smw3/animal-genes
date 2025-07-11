using AnimalGenes.Helpers;
using BigAndSmall;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AnimalGenes.Genes
{
    public class GeneGenerator_Wings
    {
        public static void GenerateGenes(List<HumanlikeAnimal> sapientAnimals)
        {
            foreach (var sapientAnimal in sapientAnimals)
            {
                float flightTime = sapientAnimal.animal.GetStatValueAbstract(StatDefOf.MaxFlightTime);
                Check.DebugLog($"{sapientAnimal.animal.defName} flight time: {flightTime}");
                if (flightTime > 0)
                {
                    AddWingsGene(sapientAnimal);
                }
            }
        }

        public static void AddWingsGene(HumanlikeAnimal sapientAnimal)
        {
            Helpers.GeneTemplate template = DefDatabase<Helpers.GeneTemplate>.GetNamed("ANG_WingsTemplate");
            string geneDefName = template.GenerateDefName(sapientAnimal.animal);
            Check.DebugLog($"Creating Wings gene: {geneDefName}");

            GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
            if (newGene == null)
            {
                newGene = template.GenerateGeneDef(sapientAnimal.animal, [sapientAnimal.animal.label]);
                newGene.modExtensions = new List<DefModExtension>([
                    IconHelper.GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f)])
                ]);

                newGene.ResolveReferences();
                DefDatabase<GeneDef>.Add(newGene);
            }
            GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, newGene);
        }
    }
}
