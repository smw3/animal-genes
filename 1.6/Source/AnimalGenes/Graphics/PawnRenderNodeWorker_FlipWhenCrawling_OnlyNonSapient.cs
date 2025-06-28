using BigAndSmall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes
{
    public class PawnRenderNodeWorker_FlipWhenCrawling_OnlyNonSapient : PawnRenderNodeWorker_FlipWhenCrawling
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            // This might be slow and might need to be cached.
            if (HumanlikeAnimals.IsHumanlikeAnimal(parms.pawn.def))
            {
                return false;
            }
            return base.CanDrawNow(node, parms);
        }   
    }
}
