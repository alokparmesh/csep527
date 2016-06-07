using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace polyA
{
    /// <summary>
    /// Class for each read
    /// </summary>
    public class AlignmentLine
    {
        private const string regexCigar = @"[0-9]+[MIDNSHPX=]";
        private const string regexMD = @"([0-9]+[A-Z])|([0-9]+\^[A-Z]+)|([0-9]+$)";

        private AlignmentLine()
        {
            this.ReadLength = 75;
            this.AlignmentScore = int.MinValue;
            this.NumMismatches = int.MaxValue;
            this.CleavageSite = int.MinValue;
        }

        public static AlignmentLine GetAlignmentLine(string alignmentEntry, Func<AlignmentLine, bool> skip = null)
        {
            AlignmentLine alignmentLine = new AlignmentLine();
            string[] columns = alignmentEntry.Split('\t');

            alignmentLine.QName = columns[0];
            alignmentLine.Flag = int.Parse(columns[1]);
            alignmentLine.RName = columns[2];
            alignmentLine.Pos = int.Parse(columns[3]);
            alignmentLine.MapQ = int.Parse(columns[4]);
            alignmentLine.Cigar = columns[5];
            alignmentLine.RNext = columns[6];
            alignmentLine.PNext = int.Parse(columns[7]);
            alignmentLine.TLen = int.Parse(columns[8]);
            alignmentLine.Sequence = columns[9];
            alignmentLine.Quality = columns[10];

            // if (!this.Cigar.Equals("*"))
            if (skip == null || !skip(alignmentLine))
            {
                for (int i = 10; i < columns.Length; i++)
                {
                    if (columns[i].StartsWith("AS"))
                    {
                        alignmentLine.AlignmentScore = int.Parse(columns[i].Split(':')[2]);
                    }
                    else if (columns[i].StartsWith("NM"))
                    {
                        alignmentLine.NumMismatches = int.Parse(columns[i].Split(':')[2]);
                    }
                    else if (columns[i].StartsWith("MD"))
                    {
                        alignmentLine.MismatchString = columns[i].Split(':')[2];
                    }
                }
            }
            else
            {
                return null;
            }

            return alignmentLine;
        }

        public int ReadLength { get; private set; }

        public string QName { get; private set; }

        public int Flag { get; private set; }

        public string RName { get; private set; }

        public int Pos { get; private set; }

        public int MapQ { get; private set; }

        public string Cigar { get; private set; }

        public string RNext { get; private set; }

        public int PNext { get; private set; }

        public int TLen { get; private set; }

        public string Sequence { get; private set; }

        public string Quality { get; private set; }

        public int AlignmentScore { get; private set; }

        public int NumMismatches { get; private set; }

        public string MismatchString { get; private set; }

        public int CleavageSite { get; private set; }

        public string CleavageMarkedSequence { get; private set; }

        public string ReadSequence { get; private set; }

        public string ReferenceSequence { get; private set; }

        /// <summary>
        /// Adjust alignment scores by ignoring Ns
        /// </summary>
        public void FixUnidentifiedReads()
        {
            char[] sequence = this.Sequence.ToCharArray();

            for (int i = sequence.Length - 1; i >= 0; i--)
            {
                if (sequence[i] == 'N')
                {
                    this.AlignmentScore += 1;
                    this.NumMismatches -= 1;
                    this.ReadLength -= 1;
                }
            }
        }

        /// <summary>
        /// Find the cleavage site by first reconstructing read and reference
        /// Use Cigar and MD string to reconstruct
        /// </summary>
        public void FindCleavageSite()
        {
            StringBuilder referenceSequence = new StringBuilder();
            StringBuilder readSequence = new StringBuilder();
            int readIndex = 0;
            char[] sequence = this.Sequence.ToCharArray();

            if (!this.Cigar.Equals("75M"))
            {
                foreach (Match m in Regex.Matches(this.Cigar, regexCigar))
                {
                    char cigarStep = m.Value[m.Value.Length - 1];
                    int stepCount = int.Parse(m.Value.Substring(0, m.Value.Length - 1));
                    if (cigarStep == 'M')
                    {
                        for (int i = 0; i < stepCount; i++, readIndex++)
                        {
                            referenceSequence.Append(sequence[readIndex]);
                            readSequence.Append(sequence[readIndex]);
                        }
                    }
                    else if (cigarStep == 'I')
                    {
                        for (int i = 0; i < stepCount; i++, readIndex++)
                        {
                            referenceSequence.Append('*');
                            readSequence.Append(sequence[readIndex]);
                        }
                    }
                    else if (cigarStep == 'D')
                    {
                        for (int i = 0; i < stepCount; i++)
                        {
                            referenceSequence.Append('#');
                            readSequence.Append('*');
                        }
                    }
                    else
                    {
                        // In current assignment we did not have anything other than M,I,D after filters
                    }
                }
            }
            else
            {
                readSequence.Append(this.Sequence);
                referenceSequence.Append(this.Sequence);
            }

            sequence = referenceSequence.ToString().ToCharArray();
            readIndex = 0;
            foreach (Match m in Regex.Matches(this.MismatchString, regexMD))
            {
                int stepCount;
                if (char.IsLetter(m.Value[m.Value.Length - 1]) && int.TryParse(m.Value.Substring(0, m.Value.Length - 1), out stepCount))
                {
                    readIndex = Advance(sequence, readIndex, stepCount);
                    sequence[readIndex] = m.Value[m.Value.Length - 1];
                    readIndex++;
                }
                else if (char.IsLetter(m.Value[m.Value.Length - 1]))
                {
                    string[] splits = m.Value.Split('^');
                    readIndex = Advance(sequence, readIndex, int.Parse(splits[0]));

                    foreach (char c in splits[1])
                    {
                        sequence[readIndex] = c;
                        readIndex++;
                    }
                }
            }

            this.ReadSequence = readSequence.ToString();
            this.ReferenceSequence = new string(sequence);

            this.CalculatePolyA();
        }

        /// <summary>
        /// Find cleavage site by checking
        /// Gene match ratio is above threshold
        /// Tail mismatch ratio is above threshold
        /// Minimum length and density of As
        /// </summary>
        private void CalculatePolyA()
        {
            var readSequenceArray = this.ReadSequence.ToCharArray();
            var referenceSequenceArray = this.ReferenceSequence.ToCharArray();

            Tuple<int, int>[] forwardMatch = DoForwardMatch(readSequenceArray, referenceSequenceArray);
            Tuple<int, int>[] backwardMatch = DoBackwardMatch(readSequenceArray, referenceSequenceArray);

            int totalCount = 0;
            int countOfA = 0;
            int aTailStart = int.MinValue;

            for (int i = readSequenceArray.Length - 1; i >= 0; i--)
            {
                if (readSequenceArray[i] == 'N')
                {
                    continue;
                }

                totalCount++;

                if (readSequenceArray[i] == 'A')
                {
                    countOfA++;
                    double currentTailRatioOfA = ((double)countOfA) / totalCount;
                    double geneMatchRatio = forwardMatch[i].Item2 > 0 ? ((double)forwardMatch[i].Item1 / forwardMatch[i].Item2) : 0.0;
                    double tailMatchRatio = backwardMatch[i].Item2 > 0 ? ((double)backwardMatch[i].Item1 / backwardMatch[i].Item2) : 0.0;

                    if (geneMatchRatio > Program.RnaAccuracyRate
                        && tailMatchRatio < Program.TailMatchRatio
                        && currentTailRatioOfA > Program.RnaAccuracyRate
                        && countOfA >= Program.MinimumPolyATailLength)
                    {
                        aTailStart = i;
                    }
                }

                if (totalCount - countOfA > (1 - Program.RnaAccuracyRate) * 75)
                {
                    break;
                }
            }

            if (aTailStart >= 0)
            {
                this.CleavageSite = aTailStart;
                this.CleavageMarkedSequence = string.Concat(this.ReadSequence.Substring(0, aTailStart), ".", this.ReadSequence.Substring(aTailStart, this.ReadSequence.Length - aTailStart));
            }
        }

        /// <summary>
        /// Calculate matches in backward direction
        /// </summary>
        /// <param name="readSequenceArray"></param>
        /// <param name="referenceSequenceArray"></param>
        /// <returns></returns>
        private static Tuple<int, int>[] DoBackwardMatch(char[] readSequenceArray, char[] referenceSequenceArray)
        {
            Tuple<int, int>[] backwardMatch = new Tuple<int, int>[readSequenceArray.Length];

            int currentMatch = 0;
            int currentLength = 0;
            for (int i = readSequenceArray.Length - 1; i >= 0; i--)
            {
                if (readSequenceArray[i] == 'N' || referenceSequenceArray[i] == 'N')
                {
                    backwardMatch[i] = Tuple.Create(currentMatch, currentLength);
                    continue;
                }

                if (readSequenceArray[i] == referenceSequenceArray[i])
                {
                    currentMatch++;
                    currentLength++;
                    backwardMatch[i] = Tuple.Create(currentMatch, currentLength);
                }
                // Treat inserts and deletes as single mismatch
                else if (readSequenceArray[i] == '*' || referenceSequenceArray[i] == '*')
                {
                    currentLength++;
                    while ((readSequenceArray[i] == '*' || referenceSequenceArray[i] == '*') && i >= 0)
                    {
                        backwardMatch[i] = Tuple.Create(currentMatch, currentLength);
                        i--;
                    }
                    i++;
                }
                else
                {
                    currentLength++;
                    backwardMatch[i] = Tuple.Create(currentMatch, currentLength);
                }
            }

            return backwardMatch;
        }

        /// <summary>
        /// Calculate matches in forward direction
        /// </summary>
        /// <param name="readSequenceArray"></param>
        /// <param name="referenceSequenceArray"></param>
        /// <returns></returns>
        private Tuple<int, int>[] DoForwardMatch(char[] readSequenceArray, char[] referenceSequenceArray)
        {
            Tuple<int, int>[] forwardMatch = new Tuple<int, int>[readSequenceArray.Length];

            int currentMatch = 0;
            int currentLength = 0;
            for (int i = 0; i < readSequenceArray.Length; i++)
            {
                if (readSequenceArray[i] == 'N' || referenceSequenceArray[i] == 'N')
                {
                    forwardMatch[i] = Tuple.Create(currentMatch, currentLength);
                    continue;
                }

                if (readSequenceArray[i] == referenceSequenceArray[i])
                {
                    currentMatch++;
                    currentLength++;
                    forwardMatch[i] = Tuple.Create(currentMatch, currentLength);
                }
                // Treat inserts and deletes as single mismatch
                else if (readSequenceArray[i] == '*' || referenceSequenceArray[i] == '*')
                {
                    currentLength++;
                    while ((readSequenceArray[i] == '*' || referenceSequenceArray[i] == '*') && i < readSequenceArray.Length)
                    {
                        forwardMatch[i] = Tuple.Create(currentMatch, currentLength);
                        i++;
                    }
                    i--;
                }
                else
                {
                    currentLength++;
                    forwardMatch[i] = Tuple.Create(currentMatch, currentLength);
                }
            }

            return forwardMatch;
        }

        /// <summary>
        /// Advance read position while skipping inserts
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="readIndex"></param>
        /// <param name="stepCount"></param>
        /// <returns></returns>
        private int Advance(char[] sequence, int readIndex, int stepCount)
        {
            int i = 0;

            while (i < stepCount)
            {
                if (sequence[readIndex] == '*')
                {
                    readIndex++;
                    continue;
                }

                readIndex++;
                i++;
            }

            while (sequence[readIndex] == '*')
            {
                readIndex++;
            }

            return readIndex;
        }
    }
}
