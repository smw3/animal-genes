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

        public static void GenerateGenes(List<HumanlikeAnimal> sapientAnimals)
        {
            List<String> toolLabelBlackList = ["head", "fist", "teeth", "foot"];
            List<String> stripLabels = ["left", "right", "front", "back", "hind"];
            HashSet<String> createdTools = [];

            foreach (var sapientAnimal in sapientAnimals)
            {
                foreach (Tool t in sapientAnimal.animal.tools)
                {
                    if (t.label.NullOrEmpty())
                    {
                        //Log.Message($"Skipping tool with empty label for {sapientAnimal.animal.defName}");
                        continue;
                    }

                    var cleanedLabel = TextHelper.RemoveWordsFromLabel(t.label, stripLabels);
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
                        DefHelper.CopyGeneDefFields(templateGene, newGene);

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

                        newGene.modExtensions.Add(IconHelper.GetProceduralIconData([new Pair<ThingDef, float>(sapientAnimal.animal, 0.95f)]));

                        newGene.ResolveReferences();
                        DefDatabase<GeneDef>.Add(newGene);
                    }

                    GeneGenerator.AddGeneToHumanLikeAnimal(sapientAnimal, newGene);
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
                DefHelper.CopyHediffDefFields(hediffTemplate, newHediff);

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
    }
}
