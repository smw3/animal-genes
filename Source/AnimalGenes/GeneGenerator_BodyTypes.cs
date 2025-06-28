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
            Dictionary<string, GeneDef> bodyTypeGenes = [];
            foreach (var sapientAnimal in sapientAnimals)
            {
                bodyTypeGenes.TryGetValue(sapientAnimal.animal.race.body.label, out var existingGene);
                if (existingGene == null)
                {
                    var bodyType = sapientAnimal.animal.race.body;
                    string geneDefName = $"AG_{bodyType.defName}_BodyType";
                    GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
                    if (newGene == null)
                    {
                        GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("AG_BodyType_Template");
                        Check.NotNull(templateGene, "AG_BodyType_Template gene template not found");

                        newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                        DefHelper.CopyGeneDefFields(templateGene, newGene);

                        newGene.defName = geneDefName;
                        Check.NotNull(newGene, "Failed to create new GeneDef instance for body type gene");

                        newGene.label = $"{bodyType.label.CapitalizeFirst()} body type";
                        newGene.description += $"{bodyType.label.CapitalizeFirst()}";
                        newGene.generated = true;
                        Log.Message($"Generating body type gene {newGene.defName} for {bodyType.label}.");

                        newGene.modExtensions = [IconHelper.GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f)])];

                        newGene.ResolveReferences();
                        DefDatabase<GeneDef>.Add(newGene);
                    }
                    existingGene = newGene;
                    bodyTypeGenes[bodyType.label] = existingGene;
                }
                GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, existingGene);
            }
        }
    }
}
