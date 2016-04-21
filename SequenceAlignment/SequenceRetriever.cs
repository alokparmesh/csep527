using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Bio.IO;
using Bio;
using System.IO;

namespace SequenceAligner
{
    /// <summary>
    /// Checks for accession in local directory otherwise tries to download from uniprot site
    /// </summary>
    public static class SequenceRetriever
    {
        private const string folderPath = "Sequences";

        /// <summary>
        /// Get the fasta file and read the sequence from file
        /// </summary>
        /// <param name="accession"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static bool TryRetrieveSequence(string accession, out string sequence)
        {
            sequence = null;
            // Find the parser
            string fileName = string.Format("{0}.fasta", accession);
            string fullFileName = Path.Combine(folderPath, fileName);

            if (!File.Exists(fullFileName))
            {
                try
                {
                    WebClient myWebClient = new WebClient();
                    myWebClient.DownloadFile(string.Format("http://www.uniprot.org/uniprot/{0}", fileName), fullFileName);
                }
                catch(Exception)
                {
                    return false;
                }
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
