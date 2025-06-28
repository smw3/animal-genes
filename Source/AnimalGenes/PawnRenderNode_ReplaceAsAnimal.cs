using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AnimalGenes
{
    public class PawnRenderNode_ReplaceAsAnimal : PawnRenderNode
    {
        private PawnKindDef animal = DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef pk) => pk.label == "thrumbo").RandomElement();

        public PawnRenderNode_ReplaceAsAnimal(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }

        private int GetEquivalentLifeStageIndex(Pawn pawn)
        {
            // If Adult, then just use oldest stage
            if (pawn.ageTracker.Adult)
            {
                return animal.RaceProps.lifeStageAges.Count-1;
            }

            // If not adult, then scale animal minAge with track to adulthood...
            float pawnAge = pawn.ageTracker.AgeBiologicalYearsFloat;
            float adulthoodPerc = Math.Min(1, pawnAge / pawn.ageTracker.AdultMinAge);

            // And scale that by age of oldest lifestage
            float ageInAnimal = adulthoodPerc * animal.RaceProps.lifeStageAges.Last().minAge;

            // And then figure out the lifestage as usual
            for (int i = 0; i < animal.RaceProps.lifeStageAges.Count - 1; i++)
            {
                LifeStageAge lifestage = animal.RaceProps.lifeStageAges[i];
                LifeStageAge nextLifestage = animal.RaceProps.lifeStageAges[i+1];

                if (lifestage.minAge >= ageInAnimal && nextLifestage.minAge < ageInAnimal)
                {
                    return i;
                }
            }

            // Fallback to oldest lifestage
            return animal.RaceProps.lifeStageAges.Count - 1;
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            return this.animal.lifeStages[GetEquivalentLifeStageIndex(pawn)].bodyGraphicData.Graphic;
        }
    }
}
