using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes
{
    public class ANGDefs
    {
        public static GeneDef GrazingGene = DefDatabase<GeneDef>.GetNamed("ANG_Grazing_Behavior");
        public static GeneDef DendrovoreGene = DefDatabase<GeneDef>.GetNamed("ANG_Dendrovore_Behavior");
    }
}
