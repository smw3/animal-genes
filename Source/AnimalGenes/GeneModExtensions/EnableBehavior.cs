using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes.GeneModExtensions
{
    public class EnableBehavior : DefModExtension
    {
        public bool canGraze = false;
        public bool canEatTrees = false;
        public bool isPredator = false;
    }
}
