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
    public static class SequenceRetriever
    {
        private const string folderPath = "Sequences";
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
