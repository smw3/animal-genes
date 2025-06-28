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
            var targetDef = humanlikeAnimal.animal;

            // Spawn animal
            Pawn aniPawn = PawnGenerator.GeneratePawn(humanlikeAnimal.animalKind, null, null);
            aniPawn.gender = humanPawn.gender;
            GenSpawn.Spawn(aniPawn, humanPawn.Position, humanPawn.Map, WipeMode.VanishOrMoveAside);

            // Tame it
            aniPawn.SetFaction(Faction.OfPlayerSilentFail);

            // Rename the old guy
            string oldName = humanPawn.Name?.ToStringShort;
            humanPawn.Name = new NameSingle(humanPawn.Name.ToStringShort + "_Discard");

            // Make it sapient based on the human pawn.
            Pawn newPawn = SwapAnimalToSapientVersion(aniPawn);

            // Copy over all the relevant data from the human pawn to the new sapient animal pawn.
            newPawn.Name = new NameSingle(oldName);

            // Relations
            Log.Message($"[Big and Small] Copying relations from {humanPawn} to {newPawn}");
            newPawn.relations.ClearAllRelations();
            foreach (var relation in humanPawn.relations.DirectRelations)
            {
                if (relation.otherPawn != null && relation.otherPawn != newPawn)
                {
                    newPawn.relations.AddDirectRelation(relation.def, relation.otherPawn);
                }
            }
            humanPawn.relations.ClearAllRelations();

            // Copy over the skills
            Log.Message($"[Big and Small] Copying skills from {humanPawn} to {newPawn}");
            foreach (var skill in humanPawn.skills.skills)
            {
                var equivalentSkill = newPawn.skills.GetSkill(skill.def);
                equivalentSkill.Level = skill.Level;
                equivalentSkill.passion = skill.passion;
            }

            // Copy over the traits
            Log.Message($"[Big and Small] Copying traits from {humanPawn} to {newPawn}");
            newPawn.story.traits.allTraits.Clear();
            foreach (var trait in humanPawn.story.traits.allTraits)
            {
                newPawn.story.traits.GainTrait(trait);
            }
            newPawn.story.Adulthood = humanPawn.story.Adulthood;
            newPawn.story.Childhood = humanPawn.story.Childhood;

            Log.Message($"[Big and Small] Swapping {humanPawn} to {newPawn} as a sapient animal of type {targetDef.defName}");
            humanPawn.Destroy(DestroyMode.Vanish);
        }

        public static Pawn SwapAnimalToSapientVersion(Pawn aniPawn)
        {
            var targetDef = HumanlikeAnimals.HumanLikeAnimalFor(aniPawn.def);

            // Empty inventory
            if (aniPawn.inventory != null && aniPawn.inventory?.innerContainer != null)
            {
                aniPawn.inventory.DropAllNearPawn(aniPawn.Position);
            }
            bool shouldBeWildman = false;
            var request = new PawnGenerationRequest(PawnKindDefOf.Colonist,
                canGeneratePawnRelations: false,
                allowDead: false, allowDowned: false, allowAddictions: false,
                forbidAnyTitle: true, forceGenerateNewPawn: true,
                forceBaselinerChance: 1,
                forceNoBackstory: true);

            var newPawn = PawnGenerator.GeneratePawn(request);
            newPawn.inventory.DestroyAll(DestroyMode.Vanish);
            newPawn.equipment.DestroyAllEquipment(DestroyMode.Vanish);
            newPawn.apparel.DestroyAll(DestroyMode.Vanish);

            string oldName = aniPawn.Name?.ToStringShort;
            if (oldName == null)
            {
                newPawn.Name = PawnBioAndNameGenerator.GeneratePawnName(newPawn, forceNoNick: true);
            }
            else
            {
                aniPawn.Name = new NameSingle(aniPawn.Name.ToStringShort + "_Discard");
                newPawn.Name = new NameSingle(oldName);
            }
            newPawn.relations.ClearAllRelations(); // Should add a friend relationship to any bonded pawn here later...
            newPawn.story.Adulthood = DefDatabase<BackstoryDef>.GetNamed("Colonist97");
            newPawn.story.Childhood = DefDatabase<BackstoryDef>.GetNamed("TribeChild19");
            if (aniPawn.Faction == null)
            {
                shouldBeWildman = true;
                newPawn.ideo?.SetIdeo(Faction.OfPlayerSilentFail?.ideos?.PrimaryIdeo);
            }
            else
            {
                newPawn.ideo?.SetIdeo(aniPawn.Faction.ideos?.PrimaryIdeo);
                newPawn.SetFaction(aniPawn.Faction);
            }
            newPawn.gender = aniPawn.gender == Gender.None ? newPawn.gender : aniPawn.gender;
            newPawn.ageTracker.AgeChronologicalTicks = aniPawn.ageTracker.AgeChronologicalTicks;
            if (aniPawn.ageTracker.AgeBiologicalYears < 18)
            {
                newPawn.ageTracker.AgeBiologicalTicks = 18 * GenDate.TicksPerYear;
            }
            else
            {
                newPawn.ageTracker.AgeBiologicalTicks = aniPawn.ageTracker.AgeBiologicalTicks;
            }

            if (ModsConfig.BiotechActive)
            {
                if (newPawn.genes.Xenotype != XenotypeDefOf.Baseliner)
                {
                    Log.Message($"[Big and Small] {newPawn} had a xenotype {newPawn.genes.Xenotype.defName} but was supossed to generate as a baseliner." +
                        $"Removing xenotype and genes.");
                    // Somehow they can still end up having a xenotype.
                    for (int idx = newPawn.genes.GenesListForReading.Count - 1; idx >= 0; idx--)
                    {
                        var gene = newPawn.genes.GenesListForReading[idx];
                        newPawn.genes.RemoveGene(gene);
                    }
                    newPawn.genes.SetXenotype(XenotypeDefOf.Baseliner);
                    GeneHelpers.ClearCachedGenes(newPawn);
                }
            }
            BigAndSmall.RaceMorpher.CacheAndRemoveHediffs(aniPawn);
            newPawn.health.hediffSet.hediffs.Clear();
            //foreach (var hediff in pawn.health.hediffSet.hediffs)
            //{
            //    var h = newPawn.health.AddHediff(hediff.def, hediff.Part, null);
            //    h.Severity = hediff.Severity;
            //}


            // Spawn into the same position as the old pawn.
            GenSpawn.Spawn(newPawn, aniPawn.Position, aniPawn.Map, WipeMode.VanishOrMoveAside);



            BigAndSmall.RaceMorpher.SwapThingDef(newPawn, targetDef, true, 9001, force: true, permitFusion: false, clearHediffsToReapply: false);
            BigAndSmall.RaceMorpher.RestoreMatchingHediffs(newPawn, targetDef, aniPawn);
            if (shouldBeWildman)
            {
                newPawn.SetFaction(null);
                newPawn.ChangeKind(PawnKindDefOf.WildMan);
                newPawn.jobs.StopAll();
            }
            if (aniPawn.RaceProps.IsMechanoid && aniPawn.kindDef?.weaponTags?.Any() == true)
            {
                try
                {
                    var weaponTag = aniPawn.kindDef.weaponTags.FirstOrDefault();
                    var weaponFromTag = DefDatabase<ThingDef>.AllDefsListForReading
                        .Where(x => x.IsWeapon && x.weaponTags?.Contains(weaponTag) == true)
                        .OrderByDescending(x => x.BaseMarketValue).FirstOrDefault();
                    var weapon = (ThingWithComps)ThingMaker.MakeThing(weaponFromTag);
                    newPawn.equipment.AddEquipment(weapon);
                }
                catch (Exception e)
                {
                    Log.Error($"[Big and Small] Error trying to equip {newPawn} with a weapon from {aniPawn.kindDef}: {e.Message}");
                }
            }

            aniPawn.Destroy(DestroyMode.Vanish);
            return newPawn;
        }


    }
}
