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
        public static List<HumanlikeAnimal> SapientAnimals => HumanlikeAnimalGenerator.humanlikeAnimals.Values.ToList();
        private static Dictionary<HumanlikeAnimal, List<GeneDef>> humanLikeGenes = new Dictionary<HumanlikeAnimal, List<GeneDef>>();
        public static Dictionary<GeneDef, HumanlikeAnimal> affinityGenes = new Dictionary<GeneDef, HumanlikeAnimal>();

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
            GenerateGenesForBodyTypes();

            // Must be done last, after all other genes have been generated
            GenerateAffinityGenes();

            RemoveStatDefsThatHaveGenesNow();
        }
        private static void RemoveStatDefsThatHaveGenesNow()
        {
            foreach (var sapientAnimal in SapientAnimals)
            {
                // I thought I might have to remove natural armor here, but they don't get any in the first place..
            }
        }

        private static void GenerateGenesForBodyTypes()
        {
            Dictionary<String, GeneDef> bodyTypeGenes = new Dictionary<String, GeneDef>();
            foreach (var sapientAnimal in SapientAnimals)
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
                        CopyGeneDefFields(templateGene, newGene);

                        newGene.defName = geneDefName;
                        Check.NotNull(newGene, "Failed to create new GeneDef instance for body type gene");

                        newGene.label = $"{bodyType.label.CapitalizeFirst()} Body Type";
                        newGene.description += $"{bodyType.label.CapitalizeFirst()}";
                        newGene.generated = true;
                        Log.Message($"Generating body type gene {newGene.defName} for {bodyType.label}.");

                        newGene.modExtensions = [GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f)])];

                        newGene.ResolveReferences();
                        DefDatabase<GeneDef>.Add(newGene);
                    }
                    existingGene = newGene;
                    bodyTypeGenes[bodyType.label] = existingGene;
                }
                AddGeneToHumanLikeAnimal(sapientAnimal, existingGene);
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
                    Log.Message($"Sapient animal {sapientAnimal.animal.defName} has only one gene, that's quite few.");
                }

                string geneDefName = $"AG_{sapientAnimal.animal.defName}_Affinity";
                GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
                if (newGene == null)
                {
                    GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("AG_Animal_Affinity_Template");
                    Check.NotNull(templateGene, "AG_Animal_Affinity_Template gene template not found");

                    newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                    CopyGeneDefFields(templateGene, newGene);

                    newGene.defName = geneDefName;
                    Check.NotNull(newGene, "Failed to create new GeneDef instance for affinity gene");
                    Check.NotNull(sapientAnimal.animal.label, sapientAnimal.animal + " label is null");
                    newGene.label = $"{sapientAnimal.animal.label.CapitalizeFirst()} Affinity";
                    newGene.generated = true;

                    Log.Message($"Generating affinity gene {newGene.defName} for {sapientAnimal.animal.defName} with {requiredGenes.Count} required genes.");
                    newGene.modExtensions =
                    [
                        new BigAndSmall.GenePrerequisites
                        {
                            prerequisiteSets = [
                                new BigAndSmall.PrerequisiteSet
                                {
                                    prerequisites = [.. requiredGenes.Select(g => g.defName)],
                                    type = PrerequisiteSet.PrerequisiteType.AllOf
                                }
                            ]
                        },
                        new GeneModExtension_TargetAffinity
                        {
                            targetAnimal = sapientAnimal
                        }
                    ];

                    newGene.modExtensions.Add(GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f)]));

                    newGene.ResolveReferences();
                    DefDatabase<GeneDef>.Add(newGene);
                }

                affinityGenes[newGene] = sapientAnimal;
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

                    string geneDefName = $"AG_{sapientAnimal.animal.defName}_{cleanedLabel}";
                    GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
                    
                    if (newGene == null)
                    {
                        GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("AG_ToolTemplate");
                        newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                        CopyGeneDefFields(templateGene, newGene);

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
                        }
                        else
                        {
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

                        newGene.modExtensions.Add(GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f)]));

                        newGene.ResolveReferences();
                        DefDatabase<GeneDef>.Add(newGene);
                    }


                    AddGeneToHumanLikeAnimal(sapientAnimal, newGene);
                    createdTools.Add(cleanedLabel);
                    Log.Message($"Generated new tool gene {newGene.defName} for {sapientAnimal.animal.defName} with tool {cleanedLabel}");
                }
            }
        }

        private static string GetToolLocationForLabel(string cleanedLabel)
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
            
            if (newHediff == null)
            {
                HediffDef hediffTemplate = DefDatabase<HediffDef>.GetNamed("AG_NaturalWeapon_Template_Hediff");
                newHediff = typeof(HediffDef).GetConstructor([]).Invoke([]) as HediffDef;
                CopyHediffDefFields(hediffTemplate, newHediff);

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
            }

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
                if (newGene == null)
                {
                    GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("AG_ArmorTemplate");
                    newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                    CopyGeneDefFields(templateGene, newGene);

                    newGene.defName = geneDefName;
                    newGene.label = $"{sapientAnimal.animal.label} skin";
                    newGene.generated = true;

                    newGene.statOffsets =
                    [
                        new() { stat = StatDefOf.ArmorRating_Blunt, value = blunt },
                        new() { stat = StatDefOf.ArmorRating_Sharp, value = sharp },
                        new() { stat = StatDefOf.ArmorRating_Heat, value = heat }
                    ];

                    newGene.modExtensions = new List<DefModExtension>([GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f)])]);

                    newGene.ResolveReferences();
                    DefDatabase<GeneDef>.Add(newGene);
                }

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
            string geneDefName = $"HL_{sapientAnimal.animal.defName}_shearable";
            GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
            if (newGene == null) {
                GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("AG_ProductionTemplate");

                newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                CopyGeneDefFields(templateGene, newGene);

                newGene.defName = geneDefName;
                newGene.label = $"{sapientAnimal.animal.label} {shearableComp.woolDef.label} producer";
                newGene.generated = true;

                BigAndSmall.ProductionGeneSettings settings = typeof(BigAndSmall.ProductionGeneSettings).GetConstructor([]).Invoke([]) as BigAndSmall.ProductionGeneSettings;
                settings.product = shearableComp.woolDef;
                settings.baseAmount = (int)Math.Ceiling(shearableComp.woolAmount / sapientAnimal.animal.race.baseBodySize);
                settings.frequencyInDays = shearableComp.shearIntervalDays;
                settings.progressName = "Growing";
                settings.saveKey = newGene.defName;

                newGene.modExtensions = new List<DefModExtension>([settings, GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f), new Pair<ThingDef, float>(shearableComp.woolDef, 0.4f)])]);

                newGene.ResolveReferences();
                DefDatabase<GeneDef>.Add(newGene);
            }
            AddGeneToHumanLikeAnimal(sapientAnimal, newGene);
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
            string geneDefName = $"HL_{sapientAnimal.animal.defName}_milkable";
            GeneDef newGene = geneDefName.TryGetExistingDef<GeneDef>();
            if (newGene == null)
            {
                GeneDef templateGene = DefDatabase<GeneDef>.GetNamed("AG_ProductionTemplate");
                newGene = typeof(GeneDef).GetConstructor([]).Invoke([]) as GeneDef;
                CopyGeneDefFields(templateGene, newGene);

                newGene.defName = geneDefName;
                newGene.label = $"{sapientAnimal.animal.label} {milkableComp.milkDef.label} producer";
                newGene.generated = true;

                BigAndSmall.ProductionGeneSettings settings = typeof(BigAndSmall.ProductionGeneSettings).GetConstructor([]).Invoke([]) as BigAndSmall.ProductionGeneSettings;
                settings.product = milkableComp.milkDef;
                settings.baseAmount = (int)Math.Ceiling(milkableComp.milkAmount / sapientAnimal.animal.race.baseBodySize);
                settings.frequencyInDays = milkableComp.milkIntervalDays;
                settings.progressName = "Filling";
                settings.saveKey = newGene.defName;

                newGene.modExtensions = new List<DefModExtension>([settings, GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f), new Pair<ThingDef, float>(milkableComp.milkDef, 0.4f)])]);
                newGene.ResolveReferences();
                DefDatabase<GeneDef>.Add(newGene);
            }
        }

        private static GeneModExtension_ProceduralIconData GetProceduralIconData(List<Pair<ThingDef, float>> iconThingDefsAndScale)
        {
            return new GeneModExtension_ProceduralIconData {
                iconThingDefsAndScale = iconThingDefsAndScale
            };
        }
    }
}
