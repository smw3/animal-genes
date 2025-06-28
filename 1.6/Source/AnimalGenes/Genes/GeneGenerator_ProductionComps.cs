using AnimalGenes.GeneModExtensions;
using AnimalGenes.Helpers;
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
                                Check.DebugLog($"Unhandled comp type {comp.GetType()} on {sapientAnimal.animal.defName}");
                            }
                            break;
                    }
                }
            }
        }

        private static void CreateShearableGene(HumanlikeAnimal sapientAnimal, CompProperties_Shearable shearableComp)
        {
            Helpers.GeneTemplate template = DefDatabase<Helpers.GeneTemplate>.GetNamed("ANG_ProductionTemplate");
            string geneDefName = template.GenerateDefName(sapientAnimal.animal, "Shearable");

            GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
            if (newGene == null)
            {
                string productName = shearableComp.woolDef.label;
                newGene = template.GenerateGeneDef(sapientAnimal.animal, ["ProductionShearedVerb".Translate(), productName], "Shearable");
                newGene.label = "ProductionLabel".Translate().Replace("{0}",  productName).Replace("{1}", sapientAnimal.animal.label);

                BigAndSmall.ProductionGeneSettings settings = new()
                {
                    product = shearableComp.woolDef,
                    baseAmount = (int)Math.Ceiling(shearableComp.woolAmount / sapientAnimal.animal.race.baseBodySize),
                    frequencyInDays = shearableComp.shearIntervalDays,
                    progressName = "ProductionShearedGrowing".Translate(),
                    saveKey = newGene.defName
                };

                newGene.modExtensions = new List<DefModExtension>([settings, IconHelper.GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f), new Pair<ThingDef, float>(shearableComp.woolDef, 0.4f)])]);

                newGene.ResolveReferences();
                DefDatabase<GeneDef>.Add(newGene);
                Check.DebugLog($"Generated shearable gene {newGene.defName} for {sapientAnimal.animal.defName} with wool {shearableComp.woolDef.label}");
            }
            GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, newGene);

            // Also add appropriate color genes if they exist, based on the wool color
            ThingDef wool = newGene.GetModExtension<BigAndSmall.ProductionGeneSettings>()?.product;
            if (wool != null)
            {
                GeneDef hairColorGene = ColorHelper.GeneDefForHairColor(wool.stuffProps.color);
                GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, hairColorGene);
                Check.DebugLog($"Added hair color gene {hairColorGene.defName} for wool color {wool.stuffProps.color} to {sapientAnimal.animal.defName}");

                // If the wool is a textile, add furry gene as well
                Check.DebugLog($"Wool has categories: {string.Join(", ", wool.thingCategories.Select(c => c.defName))}");
                if (wool.thingCategories.Contains(ThingCategoryDefOf.Textiles) || wool.thingCategories.Contains(ThingCategoryDefOf.Wools))
                {                    
                    GeneDef furryGene = DefDatabase<GeneDef>.GetNamed("Furskin");
                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, furryGene);
                    Check.DebugLog($"Added furry gene {furryGene.defName} for wool {wool.defName} to {sapientAnimal.animal.defName}");
                }
            }
        }

        private static void CreateEggGene(HumanlikeAnimal sapientAnimal, CompProperties_EggLayer eggLayerComp)
        {
            // TODO Eggs are a whole different beast, they need to be handled differently
            //Log.Message($"Generating gene for egg layer comp on {sapientAnimal.animal.defName}");
        }

        private static void CreateMilkableGene(HumanlikeAnimal sapientAnimal, CompProperties_Milkable milkableComp)
        {
            Helpers.GeneTemplate template = DefDatabase<Helpers.GeneTemplate>.GetNamed("ANG_ProductionTemplate");
            string geneDefName = template.GenerateDefName(sapientAnimal.animal, "Milkable");

            GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
            if (newGene == null)
            {
                string productName = milkableComp.milkDef.label;
                newGene = template.GenerateGeneDef(sapientAnimal.animal, ["ProductionMilkedVerb".Translate(), productName], "Milkable");
                newGene.label = "ProductionLabel".Translate().Replace("{0}", productName).Replace("{1}", sapientAnimal.animal.label);

                ProductionGeneSettings settings = new()
                {
                    product = milkableComp.milkDef,
                    baseAmount = (int)Math.Ceiling(milkableComp.milkAmount / sapientAnimal.animal.race.baseBodySize),
                    frequencyInDays = milkableComp.milkIntervalDays,
                    progressName = "ProductionMilkedGrowing".Translate(),
                    saveKey = newGene.defName
                };

                ProductionDependsOnGender productionDependsOnGender = new()
                {
                    activeIfGender = Gender.Female
                };

                newGene.modExtensions = new List<DefModExtension>([
                    settings, productionDependsOnGender,
                    IconHelper.GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f), new Pair<ThingDef, float>(milkableComp.milkDef, 0.5f)])]);
                newGene.ResolveReferences();
                DefDatabase<GeneDef>.Add(newGene);
            }
            GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, newGene);
        }
    }
}
