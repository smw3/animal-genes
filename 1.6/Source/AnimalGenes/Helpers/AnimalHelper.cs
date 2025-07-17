using BigAndSmall;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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

        private static readonly SimpleCurve GeneCountChanceCurve = new SimpleCurve
        {
            {
                new CurvePoint(1f, 0.7f),
                true
            },
            {
                new CurvePoint(2f, 0.2f),
                true
            },
            {
                new CurvePoint(3f, 0.08f),
                true
            },
            {
                new CurvePoint(4f, 0.02f),
                true
            }
        };

        public static Genepack GenerateGenepackFor(HumanlikeAnimal hla)
        {
            Genepack genepack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack, null);

            List<GeneDef> geneDefs = GeneGenerator.GetGenesForHumanLikeAnimal(hla);
            GeneDef affinityGene = GeneGenerator.affinityGenes.Where(kvp => kvp.Value == hla).First().Key;
            geneDefs.Add(affinityGene);

            Check.DebugLog($"GeneDefs possible for genepack: {geneDefs.Select(g => g.defName).Join()}");

            int num = Mathf.Min(
                (int)GeneCountChanceCurve.RandomElementByWeight((CurvePoint p) => p.y).x,
                geneDefs.Count);

            Func<GeneDef, float> geneWeightDelegate(List<GeneDef> genesToAdd)
            {
                return (GeneDef g) => {
                    if (genesToAdd.Contains(g)) return 0.0f;
                    return 1f;
                };
            }

            List<GeneDef> genesToAdd = [];
            int genesGenerated = 0;
            while (genesGenerated < num &&
                geneDefs.TryRandomElementByWeight(geneWeightDelegate(genesToAdd), out GeneDef gene))
            {
                genesToAdd.Add(gene);
                genesGenerated++;
            }

            if (genesToAdd.Any<GeneDef>())
            {
                genepack.Initialize(genesToAdd);
                return genepack;
            }
            return null;
        }
    }
}
