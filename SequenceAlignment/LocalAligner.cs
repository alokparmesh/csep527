﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequenceAligner
{
    /// <summary>
    /// Local alignment implementation using Smith-Waterman local sequence alignment algorithm
    /// </summary>
    public class LocalAligner : ISequenceAligner
    {
        private IScoreProvider scoreProvider;
        private int gapCost;
        private CostFunction[,] costMatrix;
        private char[] seq1chars;
        private char[] seq2chars;

        public LocalAligner(IScoreProvider scoreProvider, int gapCost)
        {
            this.scoreProvider = scoreProvider;
            this.gapCost = gapCost;
        }

        public IAlignmentResult Align(Sequence seq1, Sequence seq2, bool traceBack)
        {
            int m = seq1.SequenceString.Length;
            seq1chars = seq1.SequenceString.ToCharArray();

            int n = seq2.SequenceString.Length;
            seq2chars = seq2.SequenceString.ToCharArray();

            this.costMatrix = new CostFunction[m + 1, n + 1];

            for (int i = 0; i < m + 1; i++)
            {
                this.costMatrix[i, 0] = new CostFunction { MaxScore = 0, Trace = TraceBack.None };
            }

            for (int j = 0; j < n + 1; j++)
            {
                this.costMatrix[0, j] = new CostFunction { MaxScore = 0, Trace = TraceBack.None };
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
                    if (this.costMatrix[i, j].MaxScore > highestCost)
                    {
                        highestCost = this.costMatrix[i, j].MaxScore;
                        firstSequenceEnd = i;
                        secondSequenceEnd = j;
                    }
                }
            }

            if (traceBack)
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
                    alignedSecondSequence,
                    null
                    );
            }
        }

        public void OutpuCostMatrix(string fileName)
        {
            using (var outputWriter = new StreamWriter(fileName))
            {
                int rowLength = this.costMatrix.GetLength(0);
                int colLength = this.costMatrix.GetLength(1);

                outputWriter.Write(',');
                for (int j = 1; j < colLength; j++)
                {
                    outputWriter.Write(',');
                    outputWriter.Write(seq2chars[j - 1]);
                }
                outputWriter.WriteLine();

                for (int i = 0; i < rowLength; i++)
                {
                    bool bFirst = true;
                    for (int j = 0; j < colLength; j++)
                    {
                        if (!bFirst)
                        {
                            outputWriter.Write(',');
                        }
                        else
                        {
                            bFirst = false;
                            if (i == 0)
                            {
                                outputWriter.Write(',');
                            }
                            else
                            {
                                outputWriter.Write(seq1chars[i - 1]);
                                outputWriter.Write(',');
                            }


                        }

                        outputWriter.Write(this.costMatrix[i, j].MaxScore);
                    }
                    outputWriter.WriteLine();
                }
            }
        }

        /// <summary>
        /// Trace the result of alignment by following trace directions from the end
        /// </summary>
        /// <param name="firstSequenceEnd"></param>
        /// <param name="secondSequenceEnd"></param>
        /// <returns></returns>
        private IAlignmentResult Trace(int firstSequenceEnd, int secondSequenceEnd)
        {
            int firstSequenceStart = firstSequenceEnd;
            int secondSequenceStart = secondSequenceEnd;

            CostFunction cost = this.costMatrix[firstSequenceStart, secondSequenceStart];

            int highestCost = cost.MaxScore;

            StringBuilder alignedFirstSequence = new StringBuilder();
            StringBuilder alignedSecondSequence = new StringBuilder();

            while (cost.Trace != TraceBack.None)
            {
                switch (cost.Trace)
                {
                    case TraceBack.Diagonal:
                        alignedFirstSequence.Append(seq1chars[firstSequenceStart - 1]);
                        firstSequenceStart--;
                        alignedSecondSequence.Append(seq2chars[secondSequenceStart - 1]);
                        secondSequenceStart--;
                        cost = this.costMatrix[firstSequenceStart, secondSequenceStart];
                        break;
                    case TraceBack.Left:
                        alignedFirstSequence.Append('-');
                        alignedSecondSequence.Append(seq2chars[secondSequenceStart - 1]);
                        secondSequenceStart--;
                        cost = this.costMatrix[firstSequenceStart, secondSequenceStart];
                        break;
                    case TraceBack.Up:
                        alignedFirstSequence.Append(seq1chars[firstSequenceStart - 1]);
                        firstSequenceStart--;
                        alignedSecondSequence.Append('-');
                        cost = this.costMatrix[firstSequenceStart, secondSequenceStart];
                        break;
                    default:
                        throw new Exception("Unexpected direction");
                }
            }

            return new LocalAlignmentResult(
                    highestCost,
                    firstSequenceStart + 1,
                    firstSequenceEnd,
                    secondSequenceStart + 1,
                    secondSequenceEnd,
                    alignedFirstSequence.ToString().Reverse(),
                    alignedSecondSequence.ToString().Reverse(),
                    this.scoreProvider
                    );
        }

        private CostFunction GetCost(int i, int j)
        {
            CostFunction cost = new CostFunction { MaxScore = 0, Trace = TraceBack.None };

            CostFunction upCost = new CostFunction { MaxScore = this.costMatrix[i - 1, j].MaxScore + this.gapCost, Trace = TraceBack.Up };
            CostFunction leftCost = new CostFunction { MaxScore = this.costMatrix[i, j - 1].MaxScore + this.gapCost, Trace = TraceBack.Left };
            CostFunction diagCost = new CostFunction { MaxScore = this.costMatrix[i - 1, j - 1].MaxScore + this.scoreProvider.GetScore(seq1chars[i - 1], seq2chars[j - 1]), Trace = TraceBack.Diagonal };

            if (upCost.MaxScore > cost.MaxScore)
            {
                cost = upCost;
            }

            if (leftCost.MaxScore > cost.MaxScore)
            {
                cost = leftCost;
            }

            if (diagCost.MaxScore > cost.MaxScore)
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
            public int MaxScore { get; set; }

            public TraceBack Trace { get; set; }
        }

        private class LocalAlignmentResult : IAlignmentResult
        {
            private string alignedFirstSequence;
            private string alignedSecondSequence;
            private IScoreProvider scoreProvider;

            public LocalAlignmentResult(
                int score,
                int firstSequenceStart,
                int firstSequenceEnd,
                int secondSequenceStart,
                int secondSequenceEnd,
                string alignedFirstSequence,
                string alignedSecondSequence,
                IScoreProvider scoreProvider
                )
            {
                this.Score = score;
                this.FirstSequenceStart = firstSequenceStart;
                this.FirstSequenceEnd = firstSequenceEnd;
                this.SecondSequenceStart = secondSequenceStart;
                this.SecondSequenceEnd = secondSequenceEnd;
                this.alignedFirstSequence = alignedFirstSequence;
                this.alignedSecondSequence = alignedSecondSequence;
                this.scoreProvider = scoreProvider;
            }

            public int FirstSequenceStart { get; private set; }

            public int FirstSequenceEnd { get; private set; }

            public int SecondSequenceStart { get; private set; }

            public int SecondSequenceEnd { get; private set; }

            public int Score { get; private set; }

            public void PrintAlignment(int blockLength)
            {
                if (this.scoreProvider != null)
                {
                    if (blockLength <= 0)
                    {
                        throw new Exception("Block length should be positive");
                    }

                    var seq1array = this.alignedFirstSequence.ToCharArray();
                    var seq2array = this.alignedSecondSequence.ToCharArray();

                    StringBuilder firstRow = new StringBuilder();
                    StringBuilder secondRow = new StringBuilder();
                    StringBuilder thirdRow = new StringBuilder();

                    int index = 0;
                    int firstSeqIndex = this.FirstSequenceStart;
                    int secondSeqIndex = this.SecondSequenceStart;

                    // Match the blanks     
                    Tuple<string, string, string> t = StringExtensions.GetIndexStrings(firstSeqIndex, secondSeqIndex);
                    firstRow.Append(t.Item1);
                    secondRow.Append(t.Item2);
                    thirdRow.Append(t.Item3);

                    for (int i = 0; i < seq1array.Length; i++)
                    {
                        if (index >= blockLength)
                        {
                            index = 0;

                            Console.WriteLine(firstRow.ToString());
                            Console.WriteLine(secondRow.ToString());
                            Console.WriteLine(thirdRow.ToString());
                            Console.WriteLine();

                            firstRow.Clear();
                            secondRow.Clear();
                            thirdRow.Clear();

                            // Match the blanks     
                            t = StringExtensions.GetIndexStrings(firstSeqIndex, secondSeqIndex);
                            firstRow.Append(t.Item1);
                            secondRow.Append(t.Item2);
                            thirdRow.Append(t.Item3);
                        }

                        firstRow.Append(seq1array[i]);
                        thirdRow.Append(seq2array[i]);


                        if (seq1array[i] != '-')
                        {
                            firstSeqIndex++;
                        }

                        if (seq2array[i] != '-')
                        {
                            secondSeqIndex++;
                        }

                        if (seq1array[i] == seq2array[i])
                        {
                            secondRow.Append(seq1array[i]);
                        }
                        else
                        {
                            if (seq1array[i] == '-' || seq2array[i] == '-')
                            {
                                secondRow.Append(" ");
                            }
                            else if (this.scoreProvider.GetScore(seq1array[i], seq2array[i]) > 0)
                            {
                                secondRow.Append("+");
                            }
                            else
                            {
                                secondRow.Append(" ");
                            }
                        }

                        index++;
                    }

                    if (firstRow.Length > 0)
                    {
                        Console.WriteLine(firstRow.ToString());
                        Console.WriteLine(secondRow.ToString());
                        Console.WriteLine(thirdRow.ToString());
                        Console.WriteLine();
                    }
                }
                else
                {
                    throw new Exception("Traceback should be enabled to print alignment");
                }
            }
        }
    }
}
