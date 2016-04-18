using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequenceAligner
{
    public interface IAlignmentResult
    {
        int Score { get; }
        void PrintAlignment(int blockLength);
    }

    
    public interface ISequenceAligner
    {
        // Align and 
        IAlignmentResult Align(Sequence seq1, Sequence seq2);
    }
}
