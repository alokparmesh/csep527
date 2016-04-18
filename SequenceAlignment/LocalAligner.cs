using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequenceAligner
{
    public class LocalAligner : ISequenceAligner
    {
        private IScoreProvider scoreProvider;
        private int gapCost;
        private bool traceBack;
        private CostFunction[,] costMatrix;
        private char[] seq1chars;
        private char[] seq2chars;

        public LocalAligner(IScoreProvider scoreProvider, int gapCost, bool traceBack)
        {
            this.scoreProvider = scoreProvider;
            this.gapCost = gapCost;
            this.traceBack = traceBack;
        }

        public IAlignmentResult Align(Sequence seq1, Sequence seq2)
        {
            int m = seq1.SequenceString.Length;
            seq1chars = seq1.SequenceString.ToCharArray();

            int n = seq2.SequenceString.Length;
            seq2chars = seq2.SequenceString.ToCharArray();

            this.costMatrix = new CostFunction[m + 1, n + 1];

            for (int i = 0; i < m + 1; i++)
            {
                this.costMatrix[i, 0] = new CostFunction { Cost = 0, Trace = TraceBack.None };
            }

            for (int j = 0; j < n + 1; j++)
            {
                this.costMatrix[0, j] = new CostFunction { Cost = 0, Trace = TraceBack.None };
            }

            for (int i = 1; i < m + 1; i++)
            {
                for (int j = 1; j < n + 1; j++)
                {
                    this.costMatrix[i, j] = this.GetCost(i, j);
                }
            }

            int highestCost = 0;
            int firstSequenceStart = -1;
            int firstSequenceEnd = -1;
            int secondSequenceStart = -1;
            int secondSequenceEnd = -1;
            string alignedFirstSequence = string.Empty;
            string alignedSecondSequence = string.Empty;

            for (int i = 0; i < m + 1; i++)
            {
                for (int j = 0; j < n + 1; j++)
                {
                   if(this.costMatrix[i,j].Cost > highestCost)
                    {
                        highestCost = this.costMatrix[i, j].Cost;
                        firstSequenceEnd = i-1;
                        secondSequenceEnd = j - 1;
                    }
                }
            }

            if (this.traceBack)
            {
                return this.Trace(firstSequenceEnd, secondSequenceEnd);
            }
            else
            {

                return new LocalAlignmentResult(
                    highestCost,
                    firstSequenceStart,
                    firstSequenceEnd,
                    secondSequenceStart,
                    secondSequenceEnd,
                    alignedFirstSequence,
                    alignedSecondSequence
                    );
            }
        }

        private IAlignmentResult Trace(int firstSequenceEnd, int secondSequenceEnd)
        {
            int firstSequenceStart = firstSequenceEnd;
            int secondSequenceStart = secondSequenceEnd;

            CostFunction cost = this.costMatrix[firstSequenceStart + 1, secondSequenceStart + 1];

            int highestCost = cost.Cost;

            StringBuilder alignedFirstSequence = new StringBuilder();
            StringBuilder alignedSecondSequence = new StringBuilder();

            while (cost.Trace != TraceBack.None)
            {
                switch (cost.Trace)
                {
                    case TraceBack.Diagonal:
                        alignedFirstSequence.Append(seq1chars[firstSequenceStart]);
                        firstSequenceStart--;
                        alignedSecondSequence.Append(seq2chars[secondSequenceStart]);
                        secondSequenceStart--;
                        cost = this.costMatrix[firstSequenceStart + 1, secondSequenceStart + 1];
                        break;
                    case TraceBack.Left:
                        alignedFirstSequence.Append('-');
                        alignedSecondSequence.Append(seq2chars[secondSequenceStart]);
                        secondSequenceStart--;
                        cost = this.costMatrix[firstSequenceStart + 1, secondSequenceStart + 1];
                        break;
                    case TraceBack.Up:
                        alignedFirstSequence.Append(seq1chars[firstSequenceStart]);
                        firstSequenceStart--;
                        alignedSecondSequence.Append('-');
                        cost = this.costMatrix[firstSequenceStart + 1, secondSequenceStart + 1];
                        break;
                    default:
                        throw new Exception("Unexpected direction");
                }
            }

            return new LocalAlignmentResult(
                    highestCost,
                    firstSequenceStart,
                    firstSequenceEnd,
                    secondSequenceStart,
                    secondSequenceEnd,
                    alignedFirstSequence.ToString().Reverse(),
                    alignedSecondSequence.ToString().Reverse()
                    );
        }

        private CostFunction GetCost(int i, int j)
        {
            CostFunction cost = new CostFunction { Cost = 0, Trace = TraceBack.None };

            CostFunction upCost = new CostFunction { Cost = this.costMatrix[i - 1, j].Cost + this.gapCost, Trace = TraceBack.Up };
            CostFunction leftCost = new CostFunction { Cost = this.costMatrix[i, j - 1].Cost + this.gapCost, Trace = TraceBack.Left };
            CostFunction diagCost = new CostFunction { Cost = this.costMatrix[i - 1, j - 1].Cost + this.scoreProvider.GetScore(seq1chars[i - 1], seq2chars[j - 1]), Trace = TraceBack.Diagonal };

            if (upCost.Cost > cost.Cost)
            {
                cost = upCost;
            }

            if (leftCost.Cost > cost.Cost)
            {
                cost = leftCost;
            }

            if (diagCost.Cost > cost.Cost)
            {
                cost = diagCost;
            }

            return cost;
        }

        private enum TraceBack
        {
            Diagonal,
            Up,
            Left,
            None
        }

        private class CostFunction
        {
            public int Cost { get; set; }

            public TraceBack Trace { get; set; }
        }

        private class LocalAlignmentResult : IAlignmentResult
        {
            private string alignedFirstSequence;
            private string alignedSecondSequence;

            public LocalAlignmentResult(
                int score,
                int firstSequenceStart,
                int firstSequenceEnd,
                int secondSequenceStart,
                int secondSequenceEnd,
                string alignedFirstSequence,
                string alignedSecondSequence
                )
            {
                this.Score = score;
                this.FirstSequenceStart = firstSequenceStart;
                this.FirstSequenceEnd = firstSequenceEnd;
                this.SecondSequenceStart = secondSequenceStart;
                this.SecondSequenceEnd = secondSequenceEnd;
                this.alignedFirstSequence = alignedFirstSequence;
                this.alignedSecondSequence = alignedSecondSequence;
            }

            public int FirstSequenceStart { get; private set; }

            public int FirstSequenceEnd { get; private set; }

            public int SecondSequenceStart { get; private set; }

            public int SecondSequenceEnd { get; private set; }

            public int Score { get; private set; }

            public void PrintAlignment(int blockLength)
            {
                Console.WriteLine(this.alignedFirstSequence);
                Console.WriteLine(this.alignedSecondSequence);
            }
        }
    }
}
