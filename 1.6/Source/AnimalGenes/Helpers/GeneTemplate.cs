using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes.Helpers
{
    public class GeneTemplate : BigAndSmall.GeneTemplate
    {
        public string iconPath = "";
        public List<string> exclusionTags = [];
        public Type geneClass = null;

        public List<PawnRenderNodeProperties> renderNodeProperties = [];
    }
}
