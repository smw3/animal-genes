﻿using System;
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
                if ((geneDef.defName.StartsWith("Skin_") || geneDef.defName.StartsWith("ANG_Skin_")) && geneDef.skinColorOverride.HasValue)
                {
                    float distance = ColorDistance(color, geneDef.skinColorOverride.Value);
                    //Check.DebugLog($"GeneDefForSkinColor: Checking gene {geneDef.defName} with color {geneDef.skinColorOverride.Value} against target color {color}. Distance: {distance}");
                    if (distance < colorDistance)
                    {
                        colorDistance = distance;
                        closestGene = geneDef;
                    }
                }
            }

            if (colorDistance > 40.0)
            {
                Check.DebugLog($"No good color match found for color {color}. Closest match is {closestGene?.defName ?? "none"} with distance {colorDistance}.");
                return null;
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

        // Redmean color distance calculation
        private static float ColorDistance(Color color, Color targetColor)
        {            
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