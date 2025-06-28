using AnimalGenes.GeneModExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace AnimalGenes
{
    public class GeneIconGeneratorComponent : GameComponent
    {
        private bool initialized = false;

        public GeneIconGeneratorComponent(Game game)
        {

        }
        public override void GameComponentUpdate()
        {
            if (!initialized)
            {
                GenerateIcons();
                initialized = true;
            }
        }

        private void GenerateIcons()
        {
            Check.DebugLog("Generating gene icons...");
            foreach (var geneDef in DefDatabase<GeneDef>.AllDefsListForReading)
            {
                if (geneDef.GetModExtension<ProceduralIconData>() != null)
                {
                    IconHelper.GenerateAndCacheIcon(geneDef);
                }
            }
        }
    }
}
