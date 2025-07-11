using BigAndSmall;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AnimalGenes.Helpers
{
    public static class GeneDefFromTemplate
    {
        public static string GenerateDefName(this GeneTemplate template, ThingDef thing = null, string extraDefPart = null)
        {
            return $"ANG_{(thing != null ? thing.defName + "_" : "")}{(extraDefPart != null ? extraDefPart + "_" : "")}{template.keyTag}";
        }
        public static GeneDef GenerateGeneDef(this GeneTemplate template, ThingDef thing, List<string> descriptionKeys, string extraDefPart = null)
        {
            // Validate so nothing is null.
            if (template == null || descriptionKeys == null)
            {
                Log.Error($"Animal Genes: GenerateGene: One of the parameters was null." +
                    $"\ntemplate: {template}, descriptionKeys: {descriptionKeys}");
                return null;
            }

            string defName = GenerateDefName(template, thing, extraDefPart);
            var geneDef = new GeneDef
            {
                defName = defName,
                label = $"{(thing != null ? thing.defName + " " : "")}{(extraDefPart != null ? extraDefPart+" " : "")}{template.label}",
                description = template.description,
                customEffectDescriptions = template.customEffectDescriptions,
                iconPath = template.iconPath,
                biostatCpx = 0,
                biostatMet = 0,
                displayCategory = template.displayCategory,
                canGenerateInGeneSet = template.canGenerateInGeneSet,
                selectionWeight = template.selectionWeight,
                exclusionTags = template.exclusionTags,
                geneClass = template.geneClass ?? typeof(Gene),
                renderNodeProperties = template.renderNodeProperties,
                generated = true
            };

            for (int idx = 0; idx < descriptionKeys.Count; idx++)
            {
                geneDef.description = geneDef.description.Replace("{" + idx + "}", descriptionKeys[idx]);
            }

            return geneDef;
        }
    }
}
