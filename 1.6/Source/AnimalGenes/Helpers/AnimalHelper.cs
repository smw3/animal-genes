using BigAndSmall;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes.Helpers
{
    public static class AnimalHelper
    {
        public static bool IsAnimalOfColony(this Pawn pawn)
        {
            var race = pawn.def.race;
            return (pawn.Faction != null && pawn.Faction.IsPlayer && race != null && race.intelligence == Intelligence.Animal && race.FleshType != FleshTypeDefOf.Mechanoid);
        }

        public static bool IsSapientAnimal(this Pawn pawn)
        {
            return HumanlikeAnimals.IsHumanlikeAnimal(pawn.def);
        }

        public static ThingDef AnimalSourceFor(this Pawn pawn)
        {
            return HumanlikeAnimals.AnimalSourceFor(pawn.def);
        }

        public static ThingDef HumanlikeSourceFor(this Pawn pawn)
        {
            return HumanlikeAnimals.HumanLikeSourceFor(pawn.def);
        }
    }
}
