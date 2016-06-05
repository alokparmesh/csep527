using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace polyA
{
    public class AlignmentLine
    {
        private AlignmentLine()
        {
            this.ReadLength = 75;
            this.AlignmentScore = int.MinValue;
            this.NumMismatches = int.MaxValue;
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
    }
}
