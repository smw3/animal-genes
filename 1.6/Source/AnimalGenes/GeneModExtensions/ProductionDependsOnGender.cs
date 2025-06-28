using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes.GeneModExtensions
{
    public class ProductionDependsOnGender : DefModExtension
    {
        public Gender activeIfGender = Gender.None;
    }
}
