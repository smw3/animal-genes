using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes.Defs
{
    public class GeneForLeatherDefs : DefModExtension
    {
        public string geneDefName;
        public List<string> thingDefNames = [];
    }

    public class AnimalGeneSettingsDef : Def
    {
        private static List<AnimalGeneSettingsDef> _allSettings = null;
        public static List<AnimalGeneSettingsDef> AllSettings =>
            _allSettings ??= DefDatabase<AnimalGeneSettingsDef>.AllDefsListForReading;

        public GeneForLeatherDefs GeneForLeatherDefs;
    }
}
