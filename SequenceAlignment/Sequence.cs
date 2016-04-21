using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequenceAligner
{
    /// <summary>
    /// Sequence & ID structure
    /// </summary>
    public class Sequence
    {
        public Sequence(string accession, string seq)
        {
            this.Accession = accession;
            this.SequenceString = seq;
        }

        public string Accession { get; private set; }

        public string SequenceString { get; private set; }
    }
}
