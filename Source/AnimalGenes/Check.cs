using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalGenes
{
    public static class Check
    {
        public static void Argument(bool condition, string message)
        {
            if (!condition)
                throw new ArgumentException(message);
        }

        public static void NotNull(object obj, string name)
        {
            if (obj == null)
                throw new ArgumentNullException(name);
        }

        public static void State(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        public static void ListNotEmpty<T>(List<T> l, string message)
        {
            if (l.Count == 0)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
