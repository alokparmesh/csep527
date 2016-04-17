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
                string sequence1 = GetSequence(options.Accession1);
                string sequence2 = GetSequence(options.Accession2);              
            }
        }

        private static string GetSequence(string accession)
        {
            string seq = null;
            if (SequenceRetriever.TryRetrieveSequence(accession, out seq))
            {
                return seq;
            }
            else
            {
                throw new Exception(string.Format("Could not find or download sequence for accession {0}", accession));
            }
        }
    }
}
