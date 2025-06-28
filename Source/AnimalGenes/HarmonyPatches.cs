using BigAndSmall;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static BigAndSmall.RomanceTags;
using static System.Net.Mime.MediaTypeNames;

namespace AnimalGenes
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony.DEBUG = true; // Enable Harmony debug logging
            var harmony = new Harmony("ingendum.animalgenes");
            Log.Message("Patching");
            harmony.PatchAll();
            Log.Message("Patched");

            Log.Message("Setting romance tags for humanlike animals");
            Log.Message($"Total humanlike animals: {HumanlikeAnimalGenerator.humanlikeAnimals.Count}");
            // Iterate over all values in the dictionary HumanlikeAnimalGenerator.humanlikeAnimals
            foreach (var kvp in HumanlikeAnimalGenerator.humanlikeAnimals)
            {
                ThingDef newThing = kvp.Value.humanlikeThing;
                // Check if the animalThing is not null
                if (newThing != null)
                {
                    RaceExtension rExt = newThing.modExtensions.Where(x => x is RaceExtension).FirstOrDefault() as RaceExtension;
                    List<HediffDef> raceHediffs = rExt.RaceHediffs;

                    foreach(HediffDef hediff in raceHediffs)
                    {
                        PawnExtension pExt = hediff.GetAllPawnExtensionsOnHediff().FirstOrDefault();
                        if (pExt != null)
                        {
                            pExt.romanceTags.compatibilities.Add("Humanlike", new Compatibility
                            {
                                chance = 0.75f,
                                factor = 0.75f
                            });
                            pExt.romanceTags.compatibilities.Add("Human", new Compatibility
                            {
                                chance = 0.5f,
                                factor = 0.5f
                            });
                            Log.Message($"Set romance tags for {newThing.defName}");
                        }
                    }

                    newThing.SetStatBaseValue(RomancePatches.FlirtChanceDef, 1);
                    newThing.SetStatBaseValue(StatDefOf.Fertility, 1);

                    Log.Message($"Set flirt chance for {newThing.defName} to {newThing.GetStatValueAbstract(RomancePatches.FlirtChanceDef)}");
                }
            }
        }
    }
}
