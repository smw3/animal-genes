using BigAndSmall;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace AnimalGenes
{
    public static class GeneGenerator
    {
        public static List<HumanlikeAnimal> SapientAnimals => HumanlikeAnimalGenerator.humanlikeAnimals.Values.ToList();
        public static Dictionary<HumanlikeAnimal, List<GeneDef>> humanLikeGenes = new Dictionary<HumanlikeAnimal, List<GeneDef>>();

        private static readonly Dictionary<string, string> toolDefaultLocations = new Dictionary<string, string> {
            { "claw", "Hand" },
            { "hoof", "Leg" },
            { "paw", "Hand" },
            { "horn", "Head" },
            { "tail", "Spine" },
            { "wing", "Torso" },
            { "mandible", "Jaw" },
            { "tusk", "Jaw" },
            { "beak", "Jaw" },
            { "fang", "Jaw" }
        };

        private static readonly Dictionary<string, string> aliases = new Dictionary<string, string> {
            { "claws", "claw" },
            { "fangs", "fang" },
            { "horns", "horn" },
            { "hooves", "hoof" },
            { "paws", "paw" },
            { "wings", "wing" }
        };

        private static void AddGeneToHumanLikeAnimal(HumanlikeAnimal animal, GeneDef geneDef)
        {
            if (!humanLikeGenes.TryGetValue(animal, out var geneList))
            {
                geneList = [];
                humanLikeGenes[animal] = geneList;
            }
            geneList.Add(geneDef);
        }

        public static void GenerateGenes()
        {
            GenerateGeneForComps();
            GenerateGenesForArmor();
            GenerateGenesForTools();

            RemoveStatDefsThatHaveGenesNow();
        }

        private static void RemoveStatDefsThatHaveGenesNow()
        {
            foreach (var sapientAnimal in SapientAnimals)
            {
               // I thought I might have to remove natural armor here, but they don't get any in the first place..
            }
        }
        private static string RemoveWordsFromLabel(string label, List<string> wordsToRemove)
        {
            var wordSet = new HashSet<string>(wordsToRemove, StringComparer.OrdinalIgnoreCase);
            var result = string.Join(
                " ",
                label.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                     .Where(word => !wordSet.Contains(word))
            );
            return result;
        }

        private static void GenerateGenesForTools()
        {
            List<String> toolLabelBlackList = [ "head", "fist", "teeth", "foot" ];
            List<String> stripLabels = ["left", "right", "front", "back", "hind"];
            HashSet<String> createdTools = [];

            foreach (var sapientAnimal in SapientAnimals)
            {
                foreach (Tool t in sapientAnimal.animal.tools)
                {
                    if (t.label.NullOrEmpty())
                    {
                        //Log.Message($"Skipping tool with empty label for {sapientAnimal.animal.defName}");
                        continue;
                    }

                    var cleanedLabel = RemoveWordsFromLabel(t.label, stripLabels);
                    if (aliases.TryGetValue(cleanedLabel.ToLower(), out string alias))
                    {
                        cleanedLabel = alias;
                    }

                    if (createdTools.Contains(cleanedLabel))
                    {
                        //Log.Message($"Skipping tool {t.label} for {sapientAnimal.animal.defName} as it has already been created.");
                        continue;
                    }

                    if (cleanedLabel.NullOrEmpty() || toolLabelBlackList.Any(x => cleanedLabel.ToLower().Contains(x)))
                    {
                        //Log.Message($"Skipping tool {t.label} for {sapientAnimal.animal.defName} due to blacklisted label.");
                        continue;
                    }

                    string geneDefName = $"HL_{sapientAnimal.animal.defName}_{cleanedLabel}";
                    GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
                    GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("AG_ToolTemplate");
                    if (newGene == null)
                    {
                        newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                        CopyGeneDefFields(templateGene, newGene);
                    }

                    newGene.defName = geneDefName;
                    newGene.label = $"{sapientAnimal.animal.label} {cleanedLabel}";
                    newGene.generated = true;
                    
                    HediffDef toolHediff = CreateHediffDefForTool(t, cleanedLabel);
                    string? toolLocation = GetToolLocationForLabel(cleanedLabel);

                    if (toolLocation == null)
                    {
                        newGene.modExtensions = new List<DefModExtension>
                        {
                            new BigAndSmall.PawnExtension
                            {
                                applyBodyHediff = [
                                    new HediffToBody
                                    {
                                        hediff = toolHediff
                                    }
                                ]
                            }
                        };
                    } else {
                        BodyPartDef targetBodyPart = DefDatabase<BodyPartDef>.GetNamed(toolLocation, true);
                        newGene.modExtensions = new List<DefModExtension>
                        {
                            new BigAndSmall.PawnExtension
                            {
                                applyPartHediff = [
                                    new HediffToBodyparts
                                    {
                                        hediff = toolHediff,
                                        bodyparts = [ targetBodyPart, targetBodyPart, targetBodyPart, targetBodyPart, targetBodyPart, targetBodyPart ]
                                    }
                                ],
                            }
                        };
                    }

                    newGene.ResolveReferences();
                    DefDatabase<GeneDef>.Add(newGene);
                    AddGeneToHumanLikeAnimal(sapientAnimal, newGene);
                    createdTools.Add(cleanedLabel);
                    Log.Message($"Generated new tool gene {newGene.defName} for {sapientAnimal.animal.defName} with tool {cleanedLabel}");
                }
            }
        }

        private static String? GetToolLocationForLabel(string cleanedLabel)
        {
            foreach (var kvp in toolDefaultLocations)
            {
                if (cleanedLabel.ToLower().Contains(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        private static HediffDef CreateHediffDefForTool(Tool t, string cleanedLabel)
        {
            string gediffDefDefName = $"AG_natural_{cleanedLabel}";
            HediffDef newHediff = gediffDefDefName.TryGetExistingDef<HediffDef>();
            HediffDef hediffTemplate = DefDatabase<HediffDef>.GetNamed("AG_NaturalWeapon_Template_Hediff");
            if (newHediff == null)
            {
                newHediff = typeof(HediffDef).GetConstructor([]).Invoke([]) as HediffDef;
                CopyHediffDefFields(hediffTemplate, newHediff);
            }
            newHediff.defName = gediffDefDefName;
            newHediff.generated = true;
            newHediff.label = $"{cleanedLabel.CapitalizeFirst()}";

            newHediff.comps = [ 
                new HediffCompProperties_VerbGiver {
                    tools = [
                        new Tool {
                            id = $"AG_tool_{cleanedLabel}",
                            label = cleanedLabel.CapitalizeFirst(),
                            power = t.power,
                            chanceFactor = t.chanceFactor,
                            armorPenetration = t.armorPenetration,
                            capacities = [.. t.capacities],
                            cooldownTime = t.cooldownTime
                        }
                    ]
                }
            ];

            DefDatabase<HediffDef>.Add(newHediff);
            return newHediff;
        }
        public static void CopyHediffDefFields(HediffDef sThing, HediffDef newThing)
        {
            foreach (var field in sThing.GetType().GetFields().Where(x => !x.IsLiteral && !x.IsStatic))
            {
                try
                {
                    field.SetValue(newThing, field.GetValue(sThing));
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to copy field {field.Name} from HediffDef.");
                    Log.Error(e.ToString());
                }
            }
        }

        public static void GenerateGenesForArmor()
        {
            //Log.Message($"Generating armor genes for {SapientAnimals.Count} sapient animal armors...");
            foreach (var sapientAnimal in SapientAnimals)
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
                GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("AG_ArmorTemplate");

                if (newGene == null)
                {
                    newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                    CopyGeneDefFields(templateGene, newGene);
                }
                newGene.defName = geneDefName;
                newGene.label = $"{sapientAnimal.animal.label} skin";
                newGene.generated = true;

                newGene.statOffsets = new List<StatModifier>
                {
                    new StatModifier { stat = StatDefOf.ArmorRating_Blunt, value = blunt },
                    new StatModifier { stat = StatDefOf.ArmorRating_Sharp, value = sharp },
                    new StatModifier { stat = StatDefOf.ArmorRating_Heat, value = heat }
                };

                newGene.ResolveReferences();
                DefDatabase<GeneDef>.Add(newGene);
                AddGeneToHumanLikeAnimal(sapientAnimal, newGene);
            }
        }

        public static void GenerateGeneForComps()
        {
            //Log.Message($"Generating production genes for {SapientAnimals.Count} sapient animal components...");
            foreach (var sapientAnimal in SapientAnimals)
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
                            if (comp.GetType() != typeof(CompProperties)) { 
                                 Log.Message($"Unhandled comp type {comp.GetType()} on {sapientAnimal.animal.defName}");
                            }
                            break;
                    }
                }
            }
        }

        private static void CreateShearableGene(HumanlikeAnimal sapientAnimal, CompProperties_Shearable shearableComp)
        {
            //Log.Message($"Generating gene for shearable comp on {sapientAnimal.animal.defName}");

            string geneDefName = $"HL_{sapientAnimal.animal.defName}_shearable";
            GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
            GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("AG_ProductionTemplate");

            if (newGene == null) {
                newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                CopyGeneDefFields(templateGene, newGene);
            }
            newGene.defName = geneDefName;
            newGene.label = $"Shearable {sapientAnimal.animal.label}";
            newGene.generated = true;

            BigAndSmall.ProductionGeneSettings settings = typeof(BigAndSmall.ProductionGeneSettings).GetConstructor([]).Invoke([]) as BigAndSmall.ProductionGeneSettings;
            settings.product = shearableComp.woolDef;
            settings.baseAmount = (int)Math.Ceiling(shearableComp.woolAmount / sapientAnimal.animal.race.baseBodySize);
            settings.frequencyInDays = shearableComp.shearIntervalDays;
            settings.progressName = "Growing";
            settings.saveKey = newGene.defName;

            newGene.modExtensions = new List<DefModExtension>( [settings] );

            newGene.ResolveReferences();
            DefDatabase<GeneDef>.Add(newGene);
            AddGeneToHumanLikeAnimal(sapientAnimal, newGene);

            //Log.Message($"Generated new gene {DefDatabase<GeneDef>.GetNamed(geneDefName).defName} for {sapientAnimal.animal.defName}");
            //Log.Message($"Product: {settings.product}, Amount: {settings.baseAmount}, Interval: {settings.frequencyInDays} days");
        }

        public static void CopyGeneDefFields(GeneDef sThing, GeneDef newThing)
        {
            foreach (var field in sThing.GetType().GetFields().Where(x => !x.IsLiteral && !x.IsStatic))
            {
                try
                {
                    field.SetValue(newThing, field.GetValue(sThing));
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to copy field {field.Name} from GeneDef.");
                    Log.Error(e.ToString());
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
            //Log.Message($"Generating gene for milkable comp on {sapientAnimal.animal.defName}");

            string geneDefName = $"HL_{sapientAnimal.animal.defName}_milkable";
            GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
            GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("AG_ProductionTemplate");

            if (newGene == null)
            {
                newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                CopyGeneDefFields(templateGene, newGene);
            }
            newGene.defName = geneDefName;
            newGene.label = $"Milkable {sapientAnimal.animal.label}";
            newGene.generated = true;

            BigAndSmall.ProductionGeneSettings settings = typeof(BigAndSmall.ProductionGeneSettings).GetConstructor([]).Invoke([]) as BigAndSmall.ProductionGeneSettings;
            settings.product = milkableComp.milkDef;
            settings.baseAmount = (int)Math.Ceiling(milkableComp.milkAmount / sapientAnimal.animal.race.baseBodySize);
            settings.frequencyInDays = milkableComp.milkIntervalDays;
            settings.progressName = "Filling";
            settings.saveKey = newGene.defName;

            newGene.modExtensions = new List<DefModExtension>([settings]);

            newGene.ResolveReferences();
            DefDatabase<GeneDef>.Add(newGene);

            //Log.Message($"Generated new gene {DefDatabase<GeneDef>.GetNamed(geneDefName).defName} for {sapientAnimal.animal.defName}");
            //Log.Message($"Product: {settings.product}, Amount: {settings.baseAmount}, Interval: {settings.frequencyInDays} days");
        }
    }
}
