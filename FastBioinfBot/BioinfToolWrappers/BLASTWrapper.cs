using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bio;
using Bio.Web.Blast;


namespace FastBioinfBot.BioinfToolWrappers
{
    public class BLASTWrapper
    {

        public Action<string> Log { get; set; }

        public async Task<string> SearchBlast(string seqString, CancellationToken cancellationToken)
        {


            NcbiBlastWebHandler handler = new NcbiBlastWebHandler()
            {
                LogOutput = Log,
                EndPoint = "https://www.ncbi.nlm.nih.gov/blast/Blast.cgi",
                TimeoutInSeconds = 3600
            };
            string cleanDNASequence = new string(seqString.Where(c => c=='A'||c=='G'||c=='T'||c=='C').ToArray());

            Sequence sequence = new Sequence(DnaAlphabet.Instance, cleanDNASequence);

            List<Bio.ISequence> sequences = new List<Bio.ISequence>();
            sequences.Append(sequence);
            var request = new BlastRequestParameters(sequences)
            {
                Database = "nt",
                Program = BlastProgram.Blastn
            };
            request.Sequences.Add(sequence);
            HttpContent result = handler.BuildRequest(request);
            var executeResult = await handler.ExecuteAsync(request, cancellationToken);

            if (executeResult == null)
            {
                return "Your sequence is not found (hits=0)";
            }

            //Stream stream = await result.ReadAsStreamAsync();
            Bio.Web.Blast.BlastXmlParser parser = new BlastXmlParser();
            var results = parser.Parse(executeResult).ToList();
            var resString = String.Join(Environment.NewLine,
                results.FirstOrDefault()
                .Records.FirstOrDefault()
                .Hits.Take(5)
                .Select(x => $"ID: {x.Id}, Accession: {x.Accession}, Def: {x.Def}")
                .ToArray());
            return resString;
        }
    }
}
