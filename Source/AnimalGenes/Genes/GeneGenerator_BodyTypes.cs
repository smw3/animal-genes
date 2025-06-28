using AnimalGenes.Helpers;
using BigAndSmall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes
{
    public class GeneGenerator_BodyTypes
    {
        public static void GenerateGenes(List<HumanlikeAnimal> sapientAnimals)
        {
            foreach (var sapientAnimal in sapientAnimals)
            {
                var bodyType = sapientAnimal.animal.race.body;              
                Helpers.GeneTemplate template = DefDatabase<Helpers.GeneTemplate>.GetNamed("ANG_BodyType_Template");
                string geneDefName = template.GenerateDefName(null, bodyType.label);

                GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
                if (newGene == null)
                {
                    newGene = GeneDefFromTemplate.GenerateGeneDef(template, null, [bodyType.label], bodyType.label);
                    newGene.generated = true;
                    Check.DebugLog($"Generating body type gene {newGene.defName} for {bodyType.label}.");

                    newGene.modExtensions = [IconHelper.GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f)])];

                    newGene.ResolveReferences();
                    DefDatabase<GeneDef>.Add(newGene);
                }                
                GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, newGene);
            }
        }
    }
}
