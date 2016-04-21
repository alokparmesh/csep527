using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequenceAligner
{
    /// <summary>
    /// Interface for result of alignment
    /// </summary>
    public interface IAlignmentResult
    {
        int Score { get; }
        void PrintAlignment(int blockLength);
    }


    /// <summary>
    /// Interface for 
    /// </summary>
    public interface ISequenceAligner
    {
        // Align sequences 
        IAlignmentResult Align(Sequence seq1, Sequence seq2, bool traceBack);

        /// <summary>
        /// Output cost matrix to file
        /// </summary>
    }
}
