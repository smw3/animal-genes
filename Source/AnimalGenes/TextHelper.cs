using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalGenes
{
    public class TextHelper
    {
        public static string RemoveWordsFromLabel(string label, List<string> wordsToRemove)
        {
            var wordSet = new HashSet<string>(wordsToRemove, StringComparer.OrdinalIgnoreCase);
            var result = string.Join(
                " ",
                label.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                     .Where(word => !wordSet.Contains(word))
            );
            return result;
        }
    }
}
