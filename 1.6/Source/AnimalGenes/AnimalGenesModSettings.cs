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
        public bool DebugEnabled = false;

        public bool AllowGrazingBehavior = true;
        public bool AllowDendrovoreBehavior = true;
        public bool AllowPredatorBehavior = true;

        public bool EnableCrossbreeding = false;


        public static AnimalGenesModSettings Settings => LoadedModManager.GetMod<Main_early>().GetSettings<AnimalGenesModSettings>();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref DebugEnabled, "DebugEnabled");
            Scribe_Values.Look(ref AllowGrazingBehavior, "AllowGrazingBehavior");
            Scribe_Values.Look(ref AllowDendrovoreBehavior, "AllowDendrovoreBehavior");
            Scribe_Values.Look(ref AllowPredatorBehavior, "AllowPredatorBehavior");
            Scribe_Values.Look(ref EnableCrossbreeding, "EnableCrossbreeding");
            base.ExposeData();
        }
    }
}
