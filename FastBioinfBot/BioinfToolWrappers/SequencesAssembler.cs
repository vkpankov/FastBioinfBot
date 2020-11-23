using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bio;
using Bio.Algorithms.Assembly;
using Bio.IO.FastQ;
using Bio.IO.FastA;

namespace FastBioinfBot.BioinfToolWrappers
{
    public class SequencesAssembler
    {
        public static async void AssemblySequences(string fastqFileName)
        {
            var parser = new FastQParser();
            List<IQualitativeSequence> sequences = new List<IQualitativeSequence>();
            using (var fileStream = new FileStream(fastqFileName, FileMode.Open))
            {
                sequences = parser.Parse(fileStream).ToList();
            }
            OverlapDeNovoAssembler assembler = new OverlapDeNovoAssembler();
            IDeNovoAssembly assembly = assembler.Assemble(sequences);

            FastAFormatter outputFormatter = new FastAFormatter();
            outputFormatter.Open("assembled_sequences.fasta");
            outputFormatter.Format(assembly.AssembledSequences);
            outputFormatter.Close();

        }
    }
}
