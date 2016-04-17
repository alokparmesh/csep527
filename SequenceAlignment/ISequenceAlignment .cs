using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequenceAligner
{
    public interface IAlignmentResult
    {
        int Score { get; set; }
        void PrintAlignment();
    }

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
    public interface ISequenceAlignment
    {
        // Align and 
        IAlignmentResult Align(Sequence seq1, Sequence seq2);
    }
}
