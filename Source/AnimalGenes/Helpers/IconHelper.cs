using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AnimalGenes
{
    public class IconHelper
    {
        public static GeneModExtension_ProceduralIconData GetProceduralIconData(List<Pair<ThingDef, float>> iconThingDefsAndScale)
        {
            return new GeneModExtension_ProceduralIconData
            {
                iconThingDefsAndScale = iconThingDefsAndScale
            };
        }
        public static void GenerateAndCacheIcon(GeneDef geneDef)
        {
            List<Pair<ThingDef, float>> iconThingDefs = geneDef.GetModExtension<GeneModExtension_ProceduralIconData>()?.iconThingDefsAndScale;
            if (iconThingDefs.NullOrEmpty())
            {
                return;
            }
            Check.DebugLog($"Generating icon for gene {geneDef.defName} with {iconThingDefs.Count} thing defs.");
            Texture2D icon = GenerateGeneIcon(iconThingDefs);

            var prop = geneDef.GetType().GetField("cachedIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prop.SetValue(geneDef, icon);
        }
        public static Texture2D RenderTextureToTexture2D(RenderTexture rTex)
        {
            // Save the currently active RenderTexture
            RenderTexture previous = RenderTexture.active;
            // Set the supplied RenderTexture as the active one
            RenderTexture.active = rTex;

            // Create a new Texture2D with the same dimensions and format
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false);

            // Read the pixels from the RenderTexture into the Texture2D
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();

            // Restore the previously active RenderTexture
            RenderTexture.active = previous;

            return tex;
        }
        public static void DrawTextureCentered(Texture2D source, Material sourceMaterial, RenderTexture destination, float scale)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = destination;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, destination.width, destination.height, 0);

            int drawWidth = Mathf.RoundToInt(destination.width * scale);
            int drawHeight = Mathf.RoundToInt(destination.height * scale);
            int x = (destination.width - drawWidth) / 2;
            int y = (destination.height - drawHeight) / 2;

            Graphics.DrawTexture(new Rect(x, y, drawWidth, drawHeight), source, sourceMaterial);

            GL.PopMatrix();
            RenderTexture.active = previous;
        }

        private static Texture2D GenerateGeneIcon(List<Pair<ThingDef, float>> iconThingDefs)
        {
            // This is a placeholder for generating a gene icon.
            // You can implement your own logic to create a texture for the gene icon.
            // For now, we return a simple white texture.
            RenderTexture temporary = RenderTexture.GetTemporary(128, 128, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
            RenderTexture.active = temporary;
            GL.Clear(true, true, Color.clear); // Clear with transparent color
            RenderTexture.active = null;

            foreach (var iconThingDefAndScale in iconThingDefs)
            {
                ThingDef iconThingDef = iconThingDefAndScale.First;
                float iconScale = iconThingDefAndScale.Second;

                Texture2D thingDefTexture = iconThingDef.uiIcon;
                Check.NotNull(thingDefTexture, "Texture for " + iconThingDef.defName + " is null.");

                DrawTextureCentered(thingDefTexture, iconThingDef.uiIconMaterial, temporary, iconScale);
            }

            Texture2D texture = RenderTextureToTexture2D(temporary);
            RenderTexture.ReleaseTemporary(temporary);

            return texture;
        }
    }
}
