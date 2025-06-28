using BigAndSmall;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace AnimalGenes
{
    public static class RaceMorpher
    {
        public static void SwapPawnToSapientAnimal(this Pawn humanPawn, HumanlikeAnimal humanlikeAnimal)
        {
            var targetDef = humanlikeAnimal.humanlikeThing;
            BigAndSmall.RaceMorpher.SwapThingDef(humanPawn, targetDef, true, 9001, force: true, permitFusion: false, clearHediffsToReapply: false);

            // Empty inventory
            if (humanPawn.inventory != null && humanPawn.inventory?.innerContainer != null)
            {
                humanPawn.inventory.DropAllNearPawn(humanPawn.Position);
            }
        }
    }
}
