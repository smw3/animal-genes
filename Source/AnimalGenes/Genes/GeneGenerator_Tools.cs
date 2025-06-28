using BigAndSmall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes
{
    public class GeneGenerator_Tools
    {

        private static readonly Dictionary<string, string> toolDefaultLocations = new()
        {
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

        private static readonly Dictionary<string, string> aliases = new()
        {
            { "claws", "claw" },
            { "fangs", "fang" },
            { "horns", "horn" },
            { "hooves", "hoof" },
            { "paws", "paw" },
            { "wings", "wing" }
        };

        public delegate void AddCosmetic(string label, HumanlikeAnimal animal, GeneDef toolGene);
        private static readonly Dictionary<string, AddCosmetic> cosmeticAdditions = new()
        {
            { "horn", (string label, HumanlikeAnimal animal, GeneDef toolGene) => AddRenderNodePropertiesFromOtherGene(label, animal, toolGene, DefDatabase<GeneDef>.GetNamed("Headbone_CenterHorn", false)) }
        };

        public static void GenerateGenes(List<HumanlikeAnimal> sapientAnimals)
        {
            List<String> toolLabelBlackList = ["head", "fist", "teeth", "foot"];
            List<String> stripLabels = ["left", "right", "front", "back", "hind"];
            Dictionary<String, GeneDef> createdTools = [];

            foreach (var sapientAnimal in sapientAnimals)
            {
                HashSet<string> toolsHandledForCurrentAnimal = [];
                foreach (Tool t in sapientAnimal.animal.tools)
                {
                    if (t.label.NullOrEmpty()) continue;

                    var cleanedLabel = TextHelper.RemoveWordsFromLabel(t.label, stripLabels);
                    cleanedLabel = cleanedLabel.Replace(" ", "_");

                    if (aliases.TryGetValue(cleanedLabel.ToLower(), out string alias))
                    {
                        cleanedLabel = alias;
                    }
                    if (toolsHandledForCurrentAnimal.Contains(cleanedLabel)) continue;

                    if (createdTools.TryGetValue(cleanedLabel, out var toolGene))
                    {
                        Check.DebugLog($"Assigning already created tool {cleanedLabel} for {sapientAnimal.animal.defName}.");
                        toolsHandledForCurrentAnimal.Add(cleanedLabel);
                        GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, toolGene);
                        continue;
                    }

                    if (cleanedLabel.NullOrEmpty() || toolLabelBlackList.Any(x => cleanedLabel.ToLower().Contains(x))) continue;

                    // See if there is a specific, pre-made gene
                    string specificGeneDefName = $"ANG_Animal_Tool_{sapientAnimal.animal.label}_{cleanedLabel}";
                    GeneDef newGene = specificGeneDefName.TryGetExistingDef<GeneDef>();

                    // See if there is a generic gene for this tool
                    string geneDefName = $"ANG_Animal_Tool_{cleanedLabel}";
                    newGene ??= geneDefName.TryGetExistingDef<GeneDef>();

                    if (newGene == null)
                    {
                        Helpers.GeneTemplate template = DefDatabase<Helpers.GeneTemplate>.GetNamed("ANG_ToolTemplate");
                        newGene = Helpers.GeneDefFromTemplate.GenerateGeneDef(template, null, []);

                        newGene.defName = geneDefName;
                        newGene.label = $"Natural Weapon ({cleanedLabel})";
                        newGene.generated = true;

                        HediffDef toolHediff = CreateHediffDefForTool(t, cleanedLabel);
                        string toolLocation = GetToolLocationForLabel(cleanedLabel);

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

                        newGene.modExtensions.Add(IconHelper.GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f)]));

                        if (cosmeticAdditions.ContainsKey(cleanedLabel))
                        {
                            cosmeticAdditions[cleanedLabel].Invoke(cleanedLabel, sapientAnimal, newGene);
                        }

                        newGene.ResolveReferences();
                        DefDatabase<GeneDef>.Add(newGene);
                    }

                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, newGene);
                    toolsHandledForCurrentAnimal.Add(cleanedLabel);
                    createdTools.Add(cleanedLabel, newGene);
                    Check.DebugLog($"Generated new tool gene {newGene.defName} for {sapientAnimal.animal.defName} with tool {cleanedLabel}");
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
            string gediffDefDefName = $"ANG_natural_{cleanedLabel}";
            HediffDef newHediff = gediffDefDefName.TryGetExistingDef<HediffDef>();

            if (newHediff == null)
            {
                HediffDef hediffTemplate = DefDatabase<HediffDef>.GetNamed("ANG_NaturalWeapon_Template_Hediff");
                newHediff = typeof(HediffDef).GetConstructor([]).Invoke([]) as HediffDef;
                DefHelper.CopyHediffDefFields(hediffTemplate, newHediff);

                newHediff.defName = gediffDefDefName;
                newHediff.generated = true;
                newHediff.label = $"{cleanedLabel.CapitalizeFirst()}";

                newHediff.comps = [
                    new HediffCompProperties_VerbGiver {
                    tools = [
                        new Tool {
                            id = $"ANG_tool_{cleanedLabel}",
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

        static void AddRenderNodePropertiesFromOtherGene(string label, HumanlikeAnimal animal, GeneDef toolGene, GeneDef graphicGene)
        {
            if (graphicGene == null)
            {
                // This is fine, we can reference genes from other mods that may not be loaded
                return;
            }
            toolGene.renderNodeProperties ??= [];
            foreach (var renderNodeProperty in graphicGene.renderNodeProperties)
            {
                var newRenderNodeProperty = typeof(PawnRenderNodeProperties).GetConstructor([]).Invoke([]) as PawnRenderNodeProperties;
                DefHelper.CopyRenderNodePropertiesDefFields(renderNodeProperty, newRenderNodeProperty);

                newRenderNodeProperty.workerClass = typeof(PawnRenderNodeWorker_FlipWhenCrawling_OnlyNonSapient);

                toolGene.renderNodeProperties.Add(newRenderNodeProperty);
            }
        }
    }
}
