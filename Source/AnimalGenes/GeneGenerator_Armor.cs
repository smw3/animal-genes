using BigAndSmall;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes
{
    public class GeneGenerator_Armor
    {
        public static void GenerateGenes(List<HumanlikeAnimal> sapientAnimals)
        {
            //Log.Message($"Generating armor genes for {SapientAnimals.Count} sapient animal armors...");
            foreach (var sapientAnimal in sapientAnimals)
            {
                float blunt = sapientAnimal.animal.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Blunt, 0.0f);
                float sharp = sapientAnimal.animal.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Sharp, 0.0f);
                float heat = sapientAnimal.animal.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Heat, 0.0f);

                if (blunt + sharp + heat == 0)
                {
                    //Log.Message($"Sapient animal {sapientAnimal.animal.defName} has no armor to generate genes for.");
                    continue;
                }

                string geneDefName = $"HL_{sapientAnimal.animal.defName}_armor";
                GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
                if (newGene == null)
                {
                    GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("AG_ArmorTemplate");
                    newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                    DefHelper.CopyGeneDefFields(templateGene, newGene);

                    newGene.defName = geneDefName;
                    newGene.label = $"{sapientAnimal.animal.label} skin";
                    newGene.generated = true;

                    newGene.statOffsets =
                    [
                        new() { stat = StatDefOf.ArmorRating_Blunt, value = blunt },
                        new() { stat = StatDefOf.ArmorRating_Sharp, value = sharp },
                        new() { stat = StatDefOf.ArmorRating_Heat, value = heat }
                    ];

                    newGene.modExtensions = new List<DefModExtension>([IconHelper.GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f)])]);

                    newGene.ResolveReferences();
                    DefDatabase<GeneDef>.Add(newGene);
                }

                GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, newGene);
            }
        }
    }
}
