using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevenshteinDistanceAlgorithm
{
    public class ItemCodeMatch
    {
        public ItemCode? OriginalCode { get; set; }
        public ItemCode? MatchedCode { get; set; }
        public short MatchStrength { get; set; }
    }
}
