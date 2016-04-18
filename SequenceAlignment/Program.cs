using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequenceAligner
{
    class Program
    {
        static void Main(string[] args)
        {
            SequenceAlignerOption options = new SequenceAlignerOption();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Sequence sequence1 = null;
                Sequence sequence2 = null;

                if (options.Sequences != null)
                {
                    if (options.Sequences.Count > 0 && options.Sequences.Count != 2)
                    {
                        throw new Exception("Exactly 2 sequences supported");
                    }
                    else
                    {
                        sequence1 = new Sequence("Sequence 1", options.Sequences[0].Trim().ToUpper());
                        sequence2 = new Sequence("Sequence 2", options.Sequences[1].Trim().ToUpper());
                    }
                }

                if (options.Accessions != null)
                {
                    if (options.Accessions.Count > 0 && options.Accessions.Count != 2)
                    {
                        throw new Exception("Exactly 2 accession supported");
                    }
                    else
                    {
                        sequence1 = GetSequence(options.Accessions[0].Trim());
                        sequence2 = GetSequence(options.Accessions[1].Trim());
                    }
                }

                if(sequence1 == null || sequence2 == null)
                {
                    throw new Exception("Missing sequence or accession");
                }

                IScoreProvider subProvider = new SubstitutionScoreProvider(SubstitutionScoreProvider.BLOSUM62);
                LocalAligner aligner = new LocalAligner(subProvider, options.GapInitiationCost, true);
                IAlignmentResult result = aligner.Align(sequence1, sequence2);
                result.PrintAlignment(60);
                Console.WriteLine(result.Score);
            }
        }

        private static Sequence GetSequence(string accession)
        {
            string seq = null;
            if (SequenceRetriever.TryRetrieveSequence(accession, out seq))
            {
                return new Sequence(accession, seq);
            }
            else
            {
                throw new Exception(string.Format("Could not find or download sequence for accession {0}", accession));
            }
        }
    }
}
