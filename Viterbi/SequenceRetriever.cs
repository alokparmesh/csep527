using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Bio.IO;
using Bio;
using System.IO;

namespace Viterbi
{
    /// <summary>
    /// Checks for accession in local directory otherwise tries to download from uniprot site
    /// </summary>
    public static class SequenceRetriever
    {
        /// <summary>
        /// Get the fasta file and read the sequence from file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static bool TryRetrieveSequence(string fileName, out string sequence)
        {
            sequence = null;
            string fullFileName = fileName;
            // Find the parser
            if (!File.Exists(fullFileName))
            {
                return false;
            }

            ISequenceParser parser = SequenceParsers.FindParserByFileName(fullFileName);
            if (parser == null)
            {
                return false;
            }

            // Parse the file.
            List<ISequence> sequences;
            using (parser.Open(fullFileName))
            {
                sequences = parser.Parse().ToList();
            }

            Bio.Sequence bioSequence = sequences.First() as Bio.Sequence;
            sequence = bioSequence.ConvertToString();
            return true;
        }
    }
}
