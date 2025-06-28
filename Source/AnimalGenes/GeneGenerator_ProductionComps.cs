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
    public class GeneGenerator_ProductionComps
    {
        public static void GenerateGenes(List<HumanlikeAnimal> sapientAnimals)
        {
            //Log.Message($"Generating production genes for {SapientAnimals.Count} sapient animal components...");
            foreach (var sapientAnimal in sapientAnimals)
            {
                if (sapientAnimal.animal.comps == null || sapientAnimal.animal.comps.Count == 0)
                {
                    // Log.Message($"Sapient animal {sapientAnimal.animal.defName} has no comps to generate genes for.");
                    continue;
                }
                foreach (var comp in sapientAnimal.animal.comps)
                {
                    switch (comp)
                    {
                        case CompProperties_Shearable shearableComp:
                            CreateShearableGene(sapientAnimal, shearableComp);
                            break;

                        case CompProperties_Milkable milkableComp:
                            CreateMilkableGene(sapientAnimal, milkableComp);
                            break;

                        case CompProperties_EggLayer eggLayerComp:
                            CreateEggGene(sapientAnimal, eggLayerComp);
                            break;

                        case CompProperties_HoldingPlatformTarget c:
                        case CompProperties_Studiable d:
                        case CompProperties_CanBeDormant e:
                        case CompProperties_WakeUpDormant f:
                            break; // These comps do not require gene generation

                        default:
                            if (comp.GetType() != typeof(CompProperties))
                            {
                                Log.Message($"Unhandled comp type {comp.GetType()} on {sapientAnimal.animal.defName}");
                            }
                            break;
                    }
                }
            }
        }

        private static void CreateShearableGene(HumanlikeAnimal sapientAnimal, CompProperties_Shearable shearableComp)
        {
            string geneDefName = $"HL_{sapientAnimal.animal.defName}_shearable";
            GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
            if (newGene == null)
            {
                GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("AG_ProductionTemplate");

                newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                DefHelper.CopyGeneDefFields(templateGene, newGene);

                newGene.defName = geneDefName;
                newGene.label = $"{sapientAnimal.animal.label} {shearableComp.woolDef.label} producer";
                newGene.generated = true;

                BigAndSmall.ProductionGeneSettings settings = typeof(BigAndSmall.ProductionGeneSettings).GetConstructor([]).Invoke([]) as BigAndSmall.ProductionGeneSettings;
                settings.product = shearableComp.woolDef;
                settings.baseAmount = (int)Math.Ceiling(shearableComp.woolAmount / sapientAnimal.animal.race.baseBodySize);
                settings.frequencyInDays = shearableComp.shearIntervalDays;
                settings.progressName = "Growing";
                settings.saveKey = newGene.defName;

                newGene.modExtensions = new List<DefModExtension>([settings, IconHelper.GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f), new Pair<ThingDef, float>(shearableComp.woolDef, 0.4f)])]);

                newGene.ResolveReferences();
                DefDatabase<GeneDef>.Add(newGene);
            }
            GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, newGene);
        }

        private static void CreateEggGene(HumanlikeAnimal sapientAnimal, CompProperties_EggLayer eggLayerComp)
        {
            // TODO Eggs are a whole different beast, they need to be handled differently
            //Log.Message($"Generating gene for egg layer comp on {sapientAnimal.animal.defName}");
        }

        private static void CreateMilkableGene(HumanlikeAnimal sapientAnimal, CompProperties_Milkable milkableComp)
        {
            string geneDefName = $"HL_{sapientAnimal.animal.defName}_milkable";
            GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
            if (newGene == null)
            {
                GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("AG_ProductionTemplate");
                newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                DefHelper.CopyGeneDefFields(templateGene, newGene);

                newGene.defName = geneDefName;
                newGene.label = $"{sapientAnimal.animal.label} {milkableComp.milkDef.label} producer";
                newGene.generated = true;

                BigAndSmall.ProductionGeneSettings settings = typeof(BigAndSmall.ProductionGeneSettings).GetConstructor([]).Invoke([]) as BigAndSmall.ProductionGeneSettings;
                settings.product = milkableComp.milkDef;
                settings.baseAmount = (int)Math.Ceiling(milkableComp.milkAmount / sapientAnimal.animal.race.baseBodySize);
                settings.frequencyInDays = milkableComp.milkIntervalDays;
                settings.progressName = "Filling";
                settings.saveKey = newGene.defName;

                newGene.modExtensions = new List<DefModExtension>([settings, IconHelper.GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f), new Pair<ThingDef, float>(milkableComp.milkDef, 0.4f)])]);
                newGene.ResolveReferences();
                DefDatabase<GeneDef>.Add(newGene);
            }
            GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, newGene);
        }
    }
}
