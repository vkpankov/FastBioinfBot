using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastBioinfBot.BioinfToolWrappers
{
    public class TrimmomaticInputParams
    {
        public string Mode { get; set; }
        public int Leading { get; set; }
        public int Trailing { get; set; }
        public int MinLen { get; set; }
        public string ForwardReadsFileName { get; set; }
        public string ReverseReadsFileName { get; set; }

    }
}
