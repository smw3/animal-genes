using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes
{
    public class AnimalGenesModSettings : ModSettings
    {
        public bool AllowGrazingBehavior = false;
        public bool AllowDendrovoreBehavior = false;

        public static AnimalGenesModSettings Settings => LoadedModManager.GetMod<Main_early>().GetSettings<AnimalGenesModSettings>();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref AllowGrazingBehavior, "AllowGrazingBehavior");
            Scribe_Values.Look(ref AllowDendrovoreBehavior, "AllowDendrovoreBehavior");
            //Scribe_Values.Look(ref exampleFloat, "exampleFloat", 200f);
            //Scribe_Collections.Look(ref exampleListOfPawns, "exampleListOfPawns", LookMode.Reference);
            base.ExposeData();
        }
    }
}
