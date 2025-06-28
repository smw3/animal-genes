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
                if (geneDef.defName.StartsWith("Skin_") && geneDef.skinColorOverride.HasValue)
                {
                    float distance = ColorDistance(color, geneDef.skinColorOverride.Value);
                    if (distance < colorDistance)
                    {
                        colorDistance = distance;
                        closestGene = geneDef;
                    }
                }
            }
            return closestGene;
        }

        public static GeneDef GeneDefForHairColor(Color color)
        {
            List<GeneDef> geneDefs = DefDatabase<GeneDef>.AllDefsListForReading;
            // Iterate through all gene definitions to find one that matches the color most closely
            GeneDef closestGene = null;
            float colorDistance = float.MaxValue;
            foreach (var geneDef in geneDefs)
            {
                if (geneDef.defName.StartsWith("Hair_") && geneDef.hairColorOverride.HasValue)
                {
                    float distance = ColorDistance(color, geneDef.hairColorOverride.Value);
                    if (distance < colorDistance)
                    {
                        colorDistance = distance;
                        closestGene = geneDef;
                    }
                }
            }
            return closestGene;
        }

        private static float ColorDistance(Color color, Color targetColor)
        {
            // Redmean color distance calculation
            float red_bar = (color.r * 255.0f + targetColor.r * 255.0f) / 2.0f;
            float red_delta = color.r * 255.0f - targetColor.r * 255.0f;
            float green_delta = color.g * 255.0f - targetColor.g * 255.0f;
            float blue_delta = color.b * 255.0f - targetColor.b * 255.0f;

            return Mathf.Sqrt(
                (2.0f + red_bar / 256.0f) * Mathf.Pow(red_delta, 2.0f) + 
                4.0f * Mathf.Pow(green_delta, 2.0f) + 
                (2.0f + (255.0f - red_bar) / 256.0f) * Mathf.Pow(blue_delta, 2.0f));
        }
    }
}