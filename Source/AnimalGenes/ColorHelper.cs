using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AnimalGenes
{
    public class ColorHelper
    {
        public static GeneDef GeneDefForSkinColor(Color color)
        {
            List<GeneDef> geneDefs = DefDatabase<GeneDef>.AllDefsListForReading;
            // Iterate through all gene definitions to find one that matches the color most closely
            GeneDef closestGene = null;
            float colorDistance = float.MaxValue;
            foreach (var geneDef in geneDefs)
            {
                if (geneDef.defName.StartsWith("Skin_") && geneDef.skinColorOverride != null)
                {
                    float distance = ColorDistance(color, geneDef.skinColorOverride);
                    if (distance < colorDistance)
                    {
                        colorDistance = distance;
                        closestGene = geneDef;
                    }
                }
            }
            return closestGene;
        }

        private static float ColorDistance(Color color, Color? skinColorOverride)
        {
            return Mathf.Pow(color.r - skinColorOverride.Value.r, 2) +
                   Mathf.Pow(color.g - skinColorOverride.Value.g, 2) +
                   Mathf.Pow(color.b - skinColorOverride.Value.b, 2);
        }
    }
}