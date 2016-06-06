using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace polyA
{
    public class WeightMatrixModel
    {
        private double[,] weightDistribution;
        private double[,] baseDistribution;

        private double[,] outputDistribution;

        public double[,] OutputDistribution
        {
            get
            {
                return this.outputDistribution;
            }
        }

        public WeightMatrixModel(double[,] weightDistribution, double[,] baseDistribution)
        {
            this.weightDistribution = weightDistribution;
            this.baseDistribution = baseDistribution;
        }

        public void Match(List<AlignmentLine> alignments)
        {
            PrintRelativeEntropy();
            double hitCount = 0;
            double sumCleavageDistance = 0.0;
            this.outputDistribution = new double[4, 6];

            foreach (AlignmentLine alignment in alignments)
            {
                string sequence = AdjustSequence(alignment.CleavageMarkedSequence.Split('.')[0]);

                double maxLogRatio = double.MinValue;
                int motifPosition = -1;
                for (int i = 0; i <= sequence.Length - 6; i++)
                {
                    string motif = sequence.Substring(i, 6);

                    double logRatio = 0.0;
                    double probability = 1.0 / (sequence.Length - 5);

                    for (int j = 0; j < 6; j++)
                    {
                        int index = GetIndex(motif[j]);
                        logRatio += Math.Log(this.weightDistribution[index, j] / this.baseDistribution[index, j], 2);
                        probability *= this.weightDistribution[index, j];
                    }

                    if (logRatio > 0 && logRatio >= maxLogRatio)
                    {
                        maxLogRatio = logRatio;
                        motifPosition = i;
                    }

                    for (int j = 0; j < 6; j++)
                    {
                        int index = GetIndex(motif[j]);
                        this.outputDistribution[index, j] += probability;
                    }
                }

                if (motifPosition >= 0)
                {
                    hitCount++;
                    sumCleavageDistance += sequence.Length - motifPosition;
                    //Console.WriteLine(motifPosition);
                    //Console.WriteLine(sequence.Length - motifPosition);
                    //Console.WriteLine();
                }
            }

            for (int j = 0; j < 6; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < 4; i++)
                {
                    sum += this.outputDistribution[i, j];
                }

                for (int i = 0; i < 4; i++)
                {
                    this.outputDistribution[i, j] /= sum;
                }
            }

            Console.WriteLine(hitCount);
            Console.WriteLine(sumCleavageDistance / hitCount);
        }

        private void PrintRelativeEntropy()
        {
            double relativeEntropy = 0.0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (this.weightDistribution[i, j] > 0)
                    {
                        relativeEntropy += this.weightDistribution[i, j] * Math.Log(this.weightDistribution[i, j] / this.baseDistribution[i, j], 2);
                    }
                }
            }

            Console.WriteLine(relativeEntropy);
        }

        private int GetIndex(char c)
        {
            switch (c)
            {
                case 'A':
                    return 0;
                case 'C':
                    return 1;
                case 'G':
                    return 2;
                case 'T':
                    return 3;
                default:
                    throw new ArgumentException(string.Format("Invalid argument {0}", c));
            }
        }

        private string AdjustSequence(string sequence)
        {
            StringBuilder adjustedSequence = new StringBuilder();

            foreach (char c in sequence)
            {
                if (c == 'N')
                {
                    adjustedSequence.Append('T');
                }
                else if (c == '*')
                {
                    continue;
                }
                else
                {
                    adjustedSequence.Append(c);
                }
            }

            return adjustedSequence.ToString();

        }
    }
}
